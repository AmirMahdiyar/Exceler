using Exceler.Configuration;
using Exceler.Core;
using Exceler.Core.OpenXml;
using FluentAssertions;
using System;
using System.Drawing;
using Xunit;

namespace Exceler.Tests.Unit.Configuration
{
    public class ConditionalStyleTests
    {
        private class TestModel
        {
            public int Id { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public bool IsUrgent { get; set; }
        }

        private class TestProfile : ExcelProfile<TestModel>
        {
            public TestProfile()
            {
                WithConditionalRowStyle(
                    x => x.IsUrgent,
                    s => s.WithBackgroundColor(ExcelColor.SoftYellow)
                );

                Map(x => x.Id)
                    .ToColumn(1)
                    .WithHeader("ID");

                Map(x => x.Status)
                    .ToColumn(2)
                    .WithHeader("Status")
                    .WithConditionalStyle(
                        status => status == "Failed",
                        s => s.WithBackgroundColor(ExcelColor.SoftRed).WithFontColor(ExcelColor.DarkRed)
                    );

                Map(x => x.Amount)
                    .ToColumn(3)
                    .WithHeader("Amount")
                    .WithFormat("$#,##0.00")
                    .WithConditionalStyle(
                        x => x.Amount < 0,
                        s => s.WithBackgroundColor(ExcelColor.SoftRed).SetBold(true)
                    )
                    .WithConditionalStyle(
                        x => x.Amount > 1000,
                        s => s.WithBackgroundColor(ExcelColor.SoftGreen)
                    );
            }
        }

        [Fact]
        public void Profile_Should_Register_Row_And_Column_ConditionalStyles()
        {
            var profile = new TestProfile();
            profile.EnsureBuilt();

            profile.RowConditionalStyles.Should().HaveCount(1);
            profile.RowConditionalStyles[0].Style.BackgroundColorHex.Should().Be(ColorHelper.ToHex(ExcelColor.SoftYellow));

            var statusCol = profile.ColumnStyles[2];
            statusCol.ConditionalStyles.Should().HaveCount(1);
            statusCol.ConditionalStyles[0].Style.BackgroundColorHex.Should().Be(ColorHelper.ToHex(ExcelColor.SoftRed));
            statusCol.ConditionalStyles[0].Style.FontColorHex.Should().Be(ColorHelper.ToHex(ExcelColor.DarkRed));

            var amountCol = profile.ColumnStyles[3];
            amountCol.ConditionalStyles.Should().HaveCount(2);
            amountCol.ConditionalStyles[0].Style.IsBold.Should().BeTrue();
            amountCol.ConditionalStyles[1].Style.BackgroundColorHex.Should().Be(ColorHelper.ToHex(ExcelColor.SoftGreen));
        }

        [Fact]
        public void ColumnStyle_MergeWith_Should_Overlay_Overrides_While_Preserving_Base_Properties()
        {
            var baseStyle = new ColumnStyle
            {
                NumberFormat = "$#,##0.00",
                IsBold = false,
                Width = 20,
                BackgroundColorHex = "#0000FF",
                FontColorHex = "#FFFFFF"
            };

            var conditionalOverride = new ColumnStyle()
                .WithBackgroundColor(ExcelColor.SoftRed)
                .SetBold(true);

            var merged = baseStyle.MergeWith(conditionalOverride);

            merged.NumberFormat.Should().Be("$#,##0.00"); // Preserved
            merged.Width.Should().Be(20);                 // Preserved
            merged.FontColorHex.Should().Be("#FFFFFF");   // Preserved
            merged.IsBold.Should().BeTrue();              // Overridden
            merged.BackgroundColorHex.Should().Be(ColorHelper.ToHex(ExcelColor.SoftRed)); // Overridden
        }

        [Fact]
        public void ColumnStyle_MergeWith_Null_Should_Return_Self()
        {
            var baseStyle = new ColumnStyle { NumberFormat = "General" };
            var merged = baseStyle.MergeWith(null!);
            merged.Should().BeSameAs(baseStyle);
        }

        [Fact]
        public void ColumnStyle_FluentSetters_Should_Update_Properties_Properly()
        {
            var style = new ColumnStyle()
                .WithBackgroundColor(ExcelColor.SoftGreen)
                .WithFontColor(ExcelColor.DarkGreen)
                .WithFormat("0.0%")
                .WithWidth(15.5)
                .SetBold(true);

            style.BackgroundColorHex.Should().Be(ColorHelper.ToHex(ExcelColor.SoftGreen));
            style.FontColorHex.Should().Be(ColorHelper.ToHex(ExcelColor.DarkGreen));
            style.NumberFormat.Should().Be("0.0%");
            style.Width.Should().Be(15.5);
            style.IsBold.Should().BeTrue();

            // System.Drawing.Color setters
            style.WithBackgroundColor(Color.Red);
            style.BackgroundColor.Should().Be(Color.FromArgb(255, 255, 0, 0));

            style.WithFontColor(Color.White);
            style.FontColor.Should().Be(Color.FromArgb(255, 255, 255, 255));

            // String hex setters
            style.WithBackgroundColor("#123456");
            style.BackgroundColorHex.Should().Be("#123456");

            style.WithFontColor("#654321");
            style.FontColorHex.Should().Be("#654321");

            // WithNumberFormat alias
            style.WithNumberFormat("yyyy-mm-dd");
            style.NumberFormat.Should().Be("yyyy-mm-dd");
        }

        [Fact]
        public void OpenXmlStyleManager_GetResolver_Fallback_For_Unmapped_Column()
        {
            var profile = new TestProfile();
            profile.EnsureBuilt();
            var manager = OpenXmlStyleManager.Create(profile);

            var fallback = manager.GetResolver(9999);
            fallback.Should().NotBeNull();
            fallback.ColumnIndex.Should().Be(9999);
            fallback.BaseStyleIndex.Should().Be(0);
        }

        [Fact]
        public void ColumnStyleResolver_Should_Follow_Precedence_Order()
        {
            var resolver = new ColumnStyleResolver
            {
                ColumnIndex = 1,
                BaseStyleIndex = 10,
                RowRules = new (IConditionalStyleRule Rule, uint StyleIndex)[]
                {
                    (new ConditionalStyleRule<TestModel>(x => x.IsUrgent, new ColumnStyle()), 20)
                },
                ColumnRules = new (IConditionalStyleRule Rule, uint StyleIndex)[]
                {
                    (new ConditionalStyleRule<TestModel>(x => x.Amount < 0, new ColumnStyle()), 30),
                    (new ConditionalStyleRule<TestModel>(x => x.Amount > 1000, new ColumnStyle()), 40)
                }
            };

            // 1. Column rule matches (Amount < 0) -> takes precedence over row rule even if IsUrgent is true
            var item1 = new TestModel { Amount = -50, IsUrgent = true };
            resolver.ResolveStyle(item1).Should().Be(30);

            // 2. Row rule matches (IsUrgent is true, but Amount is normal)
            var item2 = new TestModel { Amount = 100, IsUrgent = true };
            resolver.ResolveStyle(item2).Should().Be(20);

            // 3. Second column rule matches (Amount > 1000)
            var item3 = new TestModel { Amount = 5000, IsUrgent = false };
            resolver.ResolveStyle(item3).Should().Be(40);

            // 4. Neither rule matches -> returns BaseStyleIndex
            var item4 = new TestModel { Amount = 50, IsUrgent = false };
            resolver.ResolveStyle(item4).Should().Be(10);

            // 5. Null item -> returns BaseStyleIndex
            resolver.ResolveStyle(null).Should().Be(10);

            // 6. Non-matching object type -> returns BaseStyleIndex
            resolver.ResolveStyle("not a TestModel").Should().Be(10);
        }

        [Fact]
        public void Validation_Exceptions_Should_Be_Thrown_On_Null_Arguments()
        {
            var profile = new TestProfile();

            // Null predicate on WithConditionalRowStyle
            Action actRowNullPred = () => profile.WithRowStylePublic(null!, s => s.SetBold());
            actRowNullPred.Should().Throw<ArgumentNullException>();

            // Null action on WithConditionalRowStyle
            Action actRowNullAct = () => profile.WithRowStylePublic(x => x.IsUrgent, null!);
            actRowNullAct.Should().Throw<ArgumentNullException>();

            // ConditionalStyleRule null checks
            Action actRuleNullCond = () => new ConditionalStyleRule<TestModel>(null!, new ColumnStyle());
            actRuleNullCond.Should().Throw<ArgumentNullException>();

            Action actRuleNullStyle = () => new ConditionalStyleRule<TestModel>(x => x.IsUrgent, null!);
            actRuleNullStyle.Should().Throw<ArgumentNullException>();
        }

        private class HelperProfile : ExcelProfile<TestModel>
        {
            public void ExposeConditionalStyle(Action<ColumnBuilder<TestModel, decimal>> test)
            {
                var builder = Map(x => x.Amount);
                test(builder);
            }
        }

        [Fact]
        public void ColumnBuilder_WithConditionalStyle_Null_Checks()
        {
            var helper = new HelperProfile();

            helper.ExposeConditionalStyle(builder =>
            {
                Action actValNullCond = () => builder.WithConditionalStyle((Func<decimal, bool>)null!, s => s.SetBold());
                actValNullCond.Should().Throw<ArgumentNullException>();

                Action actValNullAct = () => builder.WithConditionalStyle(v => v < 0, (Action<ColumnStyle>)null!);
                actValNullAct.Should().Throw<ArgumentNullException>();

                Action actModelNullCond = () => builder.WithConditionalStyle((Func<TestModel, bool>)null!, s => s.SetBold());
                actModelNullCond.Should().Throw<ArgumentNullException>();

                Action actModelNullAct = () => builder.WithConditionalStyle(m => m.Amount < 0, (Action<ColumnStyle>)null!);
                actModelNullAct.Should().Throw<ArgumentNullException>();
            });
        }
    }

    internal static class TestProfileExtensions
    {
        public static void WithRowStylePublic<T>(this ExcelProfile<T> profile, Func<T, bool> condition, Action<ColumnStyle> configure) where T : class
        {
            // Call protected WithConditionalRowStyle via reflection or a test-subclass
            var method = typeof(ExcelProfile<T>).GetMethod("WithConditionalRowStyle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            try
            {
                method!.Invoke(profile, new object[] { condition, configure });
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        }
    }
}
