using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Exceler.Configuration;
using Exceler.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exceler.Core.OpenXml
{
    /// <summary>
    /// Compiles and manages OpenXML <see cref="Stylesheet"/> definitions derived from an <see cref="ExcelProfile{TModel}"/>.
    /// Enforces strict ECMA-376 XML schema element ordering (numFmts → fonts → fills → borders → cellStyleXfs → cellXfs)
    /// and provides $O(1)$ style index lookups for fast cell serialization.
    /// </summary>
    internal class OpenXmlStyleManager
    {
        private readonly Dictionary<int, uint> _columnStyleIndices = new();
        private readonly Dictionary<int, string?> _columnNumberFormats = new();
        private readonly Dictionary<int, ColumnStyleResolver> _resolvers = new();

        /// <summary>
        /// Gets the zero-based style index assigned to header cells (defaults to 1, Calibri 11pt Bold).
        /// </summary>
        public uint HeaderStyleIndex { get; private set; } = 1;

        /// <summary>
        /// Gets the compiled ECMA-376 OpenXML <see cref="Stylesheet"/> instance.
        /// </summary>
        public Stylesheet Stylesheet { get; private set; } = null!;

        /// <summary>
        /// Factory method that creates and compiles an <see cref="OpenXmlStyleManager"/> from the specified profile.
        /// </summary>
        /// <typeparam name="TModel">The row model type.</typeparam>
        /// <param name="profile">The Excel profile containing column styles and layout rules.</param>
        /// <returns>A fully initialized and compiled <see cref="OpenXmlStyleManager"/>.</returns>
        public static OpenXmlStyleManager Create<TModel>(ExcelProfile<TModel> profile) where TModel : class
        {
            var manager = new OpenXmlStyleManager();
            manager.Compile(profile);
            return manager;
        }

        /// <summary>
        /// Retrieves the pre-compiled style index for the specified 1-based column index.
        /// </summary>
        /// <param name="columnIndex">The 1-based column index.</param>
        /// <returns>The uint style index into the workbook stylesheet; returns 0 if no custom style is defined.</returns>
        public uint GetColumnStyleIndex(int columnIndex)
        {
            return _columnStyleIndices.TryGetValue(columnIndex, out var index) ? index : 0;
        }

        /// <summary>
        /// Retrieves the compiled <see cref="ColumnStyleResolver"/> for the specified 1-based column index.
        /// </summary>
        /// <param name="columnIndex">The 1-based column index.</param>
        /// <returns>A configured resolver capable of dynamic conditional style resolution.</returns>
        public ColumnStyleResolver GetResolver(int columnIndex)
        {
            if (_resolvers.TryGetValue(columnIndex, out var resolver))
            {
                return resolver;
            }

            return new ColumnStyleResolver
            {
                ColumnIndex = columnIndex,
                BaseStyleIndex = GetColumnStyleIndex(columnIndex)
            };
        }

        /// <summary>
        /// Retrieves the resolved number format string for the specified 1-based column index, if any.
        /// </summary>
        /// <param name="columnIndex">The 1-based column index.</param>
        /// <returns>The number format pattern string, or <c>null</c> if none is configured.</returns>
        public string? GetColumnNumberFormat(int columnIndex)
        {
            return _columnNumberFormats.TryGetValue(columnIndex, out var format) ? format : null;
        }

        #region Private Compilation Pipeline

        /// <summary>
        /// Executes the stylesheet compilation pipeline.
        /// </summary>
        /// <typeparam name="TModel">The row model type.</typeparam>
        /// <param name="profile">The Excel profile to compile.</param>
        private void Compile<TModel>(ExcelProfile<TModel> profile) where TModel : class
        {
            var fonts = InitializeDefaultFonts();
            var fills = InitializeDefaultFills();
            var borders = InitializeDefaultBorders();
            var (cellStyleFormats, cellFormats) = InitializeDefaultCellFormats();
            var numberingFormats = new List<NumberingFormat>();
            uint nextCustomNumFmtId = 164;

            HeaderStyleIndex = 1;

            var allColumnIndices = new HashSet<int>(profile.CompiledGetters.Keys);
            foreach (var col in profile.ColumnStyles.Keys)
            {
                allColumnIndices.Add(col);
            }

            foreach (int columnIndex in allColumnIndices)
            {
                profile.ColumnStyles.TryGetValue(columnIndex, out var style);
                style ??= new ColumnStyle();

                // 1. Base number format
                string? numFormatStr = ResolveColumnNumberFormat(style);
                _columnNumberFormats[columnIndex] = numFormatStr;

                // 2. Base cell format
                uint baseStyleIndex = CompileCellFormat(style, numberingFormats, fonts, fills, cellFormats, ref nextCustomNumFmtId);
                _columnStyleIndices[columnIndex] = baseStyleIndex;

                // 3. Compile Column-level Conditional Styles
                var colRules = new List<(IConditionalStyleRule Rule, uint StyleIndex)>();
                foreach (var rule in style.ConditionalStyles)
                {
                    var merged = style.MergeWith(rule.Style);
                    uint ruleStyleIndex = CompileCellFormat(merged, numberingFormats, fonts, fills, cellFormats, ref nextCustomNumFmtId);
                    colRules.Add((rule, ruleStyleIndex));
                }

                // 4. Compile Row-level Conditional Styles for this column
                var rowRules = new List<(IConditionalStyleRule Rule, uint StyleIndex)>();
                foreach (var rowRule in profile.RowConditionalStyles)
                {
                    var merged = style.MergeWith(rowRule.Style);
                    uint rowRuleStyleIndex = CompileCellFormat(merged, numberingFormats, fonts, fills, cellFormats, ref nextCustomNumFmtId);
                    rowRules.Add((rowRule, rowRuleStyleIndex));
                }

                _resolvers[columnIndex] = new ColumnStyleResolver
                {
                    ColumnIndex = columnIndex,
                    BaseStyleIndex = baseStyleIndex,
                    ColumnRules = colRules.ToArray(),
                    RowRules = rowRules.ToArray()
                };
            }

            Stylesheet = AssembleStylesheet(numberingFormats, fonts, fills, borders, cellStyleFormats, cellFormats);
        }

        /// <summary>
        /// Compiles a <see cref="ColumnStyle"/> into an ECMA-376 <see cref="CellFormat"/> entry in the stylesheet.
        /// </summary>
        private static uint CompileCellFormat(
            ColumnStyle style,
            List<NumberingFormat> numberingFormats,
            List<Font> fonts,
            List<Fill> fills,
            List<CellFormat> cellFormats,
            ref uint nextCustomNumFmtId)
        {
            string? numFormatStr = ResolveColumnNumberFormat(style);
            uint numFmtId = 0;
            if (!string.IsNullOrEmpty(numFormatStr))
            {
                numFmtId = ResolveNumberFormatId(numFormatStr, numberingFormats, ref nextCustomNumFmtId);
            }

            uint fontId = BuildCustomFont(style, fonts);
            uint fillId = BuildCustomFill(style, fills);

            if (numFmtId != 0 || fontId != 0 || fillId != 0)
            {
                var cellFormat = new CellFormat
                {
                    NumberFormatId = numFmtId,
                    FontId = fontId,
                    FillId = fillId,
                    BorderId = 0,
                    FormatId = 0,
                    ApplyNumberFormat = numFmtId != 0 ? true : (bool?)null,
                    ApplyFont = fontId != 0 ? true : (bool?)null,
                    ApplyFill = fillId != 0 ? true : (bool?)null
                };

                uint cellFormatId = (uint)cellFormats.Count;
                cellFormats.Add(cellFormat);
                return cellFormatId;
            }

            return 0;
        }

        /// <summary>
        /// Creates the required base fonts: Font 0 (Default Calibri 11pt) and Font 1 (Header Bold Calibri 11pt).
        /// </summary>
        private static List<Font> InitializeDefaultFonts()
        {
            return new List<Font>
            {
                // Font 0: Default font (Calibri 11pt)
                new Font(
                    new FontSize { Val = 11D },
                    new FontName { Val = "Calibri" }
                ),
                // Font 1: Default Header font (Calibri 11pt, Bold)
                new Font(
                    new Bold(),
                    new FontSize { Val = 11D },
                    new FontName { Val = "Calibri" }
                )
            };
        }

        /// <summary>
        /// Creates the mandatory ECMA-376 default fills: Fill 0 (None) and Fill 1 (Gray125).
        /// </summary>
        private static List<Fill> InitializeDefaultFills()
        {
            return new List<Fill>
            {
                new Fill(new PatternFill { PatternType = PatternValues.None }),
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 })
            };
        }

        /// <summary>
        /// Creates the required base border (Border 0: empty default border).
        /// </summary>
        private static List<Border> InitializeDefaultBorders()
        {
            return new List<Border>
            {
                new Border(
                    new LeftBorder(),
                    new RightBorder(),
                    new TopBorder(),
                    new BottomBorder(),
                    new DiagonalBorder()
                )
            };
        }

        /// <summary>
        /// Creates the initial cell style formats and cell formats (Format 0: Default, Format 1: Header Bold).
        /// </summary>
        private static (CellStyleFormats CellStyleFormats, List<CellFormat> CellFormats) InitializeDefaultCellFormats()
        {
            var cellStyleFormats = new CellStyleFormats(
                new CellFormat { NumberFormatId = 0, FontId = 0, FillId = 0, BorderId = 0 }
            );

            var cellFormats = new List<CellFormat>
            {
                // Format 0: Default
                new CellFormat { NumberFormatId = 0, FontId = 0, FillId = 0, BorderId = 0, FormatId = 0 },
                // Format 1: Header (Bold)
                new CellFormat { NumberFormatId = 0, FontId = 1, FillId = 0, BorderId = 0, FormatId = 0, ApplyFont = true }
            };

            return (cellStyleFormats, cellFormats);
        }

        /// <summary>
        /// Resolves the explicit or inferred number format string for a column style.
        /// </summary>
        private static string? ResolveColumnNumberFormat(ColumnStyle style)
        {
            string? numFormatStr = style.NumberFormat;
            if (string.IsNullOrEmpty(numFormatStr))
            {
                if (style.PropertyType == typeof(DateOnly) || style.PropertyType == typeof(DateOnly?))
                {
                    return "yyyy-mm-dd";
                }
                if (style.PropertyType == typeof(TimeOnly) || style.PropertyType == typeof(TimeOnly?))
                {
                    return "hh:mm:ss";
                }
            }
            return numFormatStr;
        }

        /// <summary>
        /// Resolves and appends a custom font definition if bold or custom text color is configured.
        /// </summary>
        /// <param name="style">The column style configuration.</param>
        /// <param name="fonts">The master fonts list to append to.</param>
        /// <returns>The font index to assign, or 0 if default.</returns>
        private static uint BuildCustomFont(ColumnStyle style, List<Font> fonts)
        {
            bool hasCustomFont = style.IsBold || !string.IsNullOrEmpty(style.FontColorHex);
            if (!hasCustomFont)
            {
                return 0;
            }

            var font = new Font(
                new FontSize { Val = 11D },
                new FontName { Val = "Calibri" }
            );

            if (style.IsBold)
            {
                font.Append(new Bold());
            }

            if (!string.IsNullOrEmpty(style.FontColorHex))
            {
                var color = ColorHelper.FromHex(style.FontColorHex);
                string argb = $"{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
                font.Append(new DocumentFormat.OpenXml.Spreadsheet.Color { Rgb = argb });
            }

            uint fontId = (uint)fonts.Count;
            fonts.Add(font);
            return fontId;
        }

        /// <summary>
        /// Resolves and appends a custom fill definition if background color is configured.
        /// </summary>
        /// <param name="style">The column style configuration.</param>
        /// <param name="fills">The master fills list to append to.</param>
        /// <returns>The fill index to assign, or 0 if default.</returns>
        private static uint BuildCustomFill(ColumnStyle style, List<Fill> fills)
        {
            if (string.IsNullOrEmpty(style.BackgroundColorHex))
            {
                return 0;
            }

            var color = ColorHelper.FromHex(style.BackgroundColorHex);
            string argb = $"{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

            var fill = new Fill(
                new PatternFill(
                    new ForegroundColor { Rgb = argb },
                    new BackgroundColor { Indexed = 64 }
                )
                {
                    PatternType = PatternValues.Solid
                }
            );

            uint fillId = (uint)fills.Count;
            fills.Add(fill);
            return fillId;
        }

        private static readonly Dictionary<string, uint> StandardNumberFormats = new(StringComparer.Ordinal)
        {
            ["General"] = 0,
            ["0"] = 1,
            ["0.00"] = 2,
            ["#,##0"] = 3,
            ["#,##0.00"] = 4,
            ["0%"] = 9,
            ["0.00%"] = 10,
            ["0.00E+00"] = 11,
            ["mm-dd-yy"] = 14,
            ["d-mmm-yy"] = 15,
            ["d-mmm"] = 16,
            ["mmm-yy"] = 17,
            ["h:mm AM/PM"] = 18,
            ["h:mm:ss AM/PM"] = 19,
            ["h:mm"] = 20,
            ["h:mm:ss"] = 21,
            ["m/d/yy h:mm"] = 22,
            ["@"] = 49
        };

        /// <summary>
        /// Resolves the standard or custom OpenXML NumberFormat ID for a format code string.
        /// Standard built-in formats (0-49) reuse predefined IDs. Custom formats are assigned unique IDs $\ge 164$.
        /// </summary>
        /// <param name="format">The number format code pattern.</param>
        /// <param name="numberingFormats">The collection of custom numbering formats.</param>
        /// <param name="nextCustomId">The running generator counter for custom format IDs.</param>
        /// <returns>The resolved OpenXML NumberFormatId.</returns>
        private static uint ResolveNumberFormatId(string format, List<NumberingFormat> numberingFormats, ref uint nextCustomId)
        {
            if (StandardNumberFormats.TryGetValue(format, out uint standardId))
            {
                return standardId;
            }

            // Check if this custom format has already been registered
            var existing = numberingFormats.FirstOrDefault(n => n.FormatCode?.Value == format);
            if (existing != null)
            {
                return existing.NumberFormatId?.Value ?? 0;
            }

            uint customId = nextCustomId++;
            numberingFormats.Add(new NumberingFormat
            {
                NumberFormatId = customId,
                FormatCode = format
            });

            return customId;
        }

        /// <summary>
        /// Assembles all style elements into a complete, valid ECMA-376 <see cref="Stylesheet"/>.
        /// Preserves the mandatory order: numFmts → fonts → fills → borders → cellStyleXfs → cellXfs.
        /// </summary>
        private static Stylesheet AssembleStylesheet(
            List<NumberingFormat> numberingFormats,
            List<Font> fonts,
            List<Fill> fills,
            List<Border> borders,
            CellStyleFormats cellStyleFormats,
            List<CellFormat> cellFormats)
        {
            var stylesheet = new Stylesheet();

            if (numberingFormats.Any())
            {
                var numFmts = new NumberingFormats { Count = (uint)numberingFormats.Count };
                foreach (var nf in numberingFormats)
                {
                    numFmts.Append(nf);
                }
                stylesheet.Append(numFmts);
            }

            var fontsElement = new Fonts { Count = (uint)fonts.Count };
            foreach (var f in fonts)
            {
                fontsElement.Append(f);
            }
            stylesheet.Append(fontsElement);

            var fillsElement = new Fills { Count = (uint)fills.Count };
            foreach (var fill in fills)
            {
                fillsElement.Append(fill);
            }
            stylesheet.Append(fillsElement);

            var bordersElement = new Borders { Count = (uint)borders.Count };
            foreach (var b in borders)
            {
                bordersElement.Append(b);
            }
            stylesheet.Append(bordersElement);

            stylesheet.Append(cellStyleFormats);

            var cellFormatsElement = new CellFormats { Count = (uint)cellFormats.Count };
            foreach (var cf in cellFormats)
            {
                cellFormatsElement.Append(cf);
            }
            stylesheet.Append(cellFormatsElement);

            return stylesheet;
        }

        #endregion
    }
}
