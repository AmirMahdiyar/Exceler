using System.Collections.Generic;
using Exceler.Configuration;
using TestExcelerWebApi.Converter;
using TestExcelerWebApi.Dtos;
using TestExcelerWebApi.Inputs;

namespace TestExcelerWebApi.Profile
{
    /// <summary>
    /// Excel mapping profile for importing raw FinancialOrderInput from Excel worksheets.
    /// Maps 19 columns, configures DelimitedTagsConverter for complex collection parsing,
    /// and enables string trimming and template header validation.
    /// Matched with GET /api/financial-orders/export-inputs and GET /api/financial-orders/export-template.
    /// </summary>
    public class FinancialOrderImportProfile : ExcelProfile<FinancialOrderInput>
    {
        public FinancialOrderImportProfile()
        {
            WithTrimStringValues(true);
            WithValidateTemplateOnRead(true);

            Map(x => x.TrackingCode).ToColumn(1).WithHeader("Tracking Code");
            Map(x => x.CustomerCode).ToColumn(2).WithHeader("Customer Code");
            Map(x => x.CustomerName).ToColumn(3).WithHeader("Customer Name");
            Map(x => x.CompanyName).ToColumn(4).WithHeader("Company Name");
            Map(x => x.OrderDate).ToColumn(5).WithHeader("Order Date");
            Map(x => x.DeliveryDate).ToColumn(6).WithHeader("Delivery Date");
            Map(x => x.PreferredDeliveryTime).ToColumn(7).WithHeader("Delivery Time");
            Map(x => x.CreatedAtUtc).ToColumn(8).WithHeader("Created (UTC)");
            Map(x => x.Status).ToColumn(9).WithHeader("Status");
            Map(x => x.Payment).ToColumn(10).WithHeader("Payment Method");
            Map(x => x.Priority).ToColumn(11).WithHeader("Priority");
            Map(x => x.Quantity).ToColumn(12).WithHeader("Quantity");
            Map(x => x.UnitPrice).ToColumn(13).WithHeader("Unit Price");
            Map(x => x.DiscountRate).ToColumn(14).WithHeader("Discount Rate");
            Map(x => x.TaxRate).ToColumn(15).WithHeader("Tax Rate");
            Map(x => x.IsExpressDelivery).ToColumn(16).WithHeader("Express");
            Map(x => x.IsInsured).ToColumn(17).WithHeader("Insured");
            Map(x => x.Tags).ToColumn(18).WithHeader("Tags").WithConverter(new DelimitedTagsConverter());
            Map(x => x.Notes).ToColumn(19).WithHeader("Notes");
        }
    }

    /// <summary>
    /// High-performance, fully-styled profile for FinancialOrderDto.
    /// Supports BOTH exporting (Write) and importing (Read) back the full 27-column report.
    /// Showcases:
    /// - Disabling AutoFit for ultra-fast large export generation (WithAutoFitColumns(false))
    /// - Explicit column widths (WithWidth)
    /// - Palette colors (ExcelColor.Navy, ExcelColor.SoftBlue, ExcelColor.SoftGreen, etc.)
    /// - Currency, percentage, and date/time number formats (WithNumberFormat)
    /// - Complex collection conversion (DelimitedTagsConverter)
    /// Matched with GET /api/financial-orders/export-excel and POST /api/financial-orders/import-excel.
    /// </summary>
    public class FinancialOrderExportProfile : ExcelProfile<FinancialOrderDto>
    {
        public FinancialOrderExportProfile()
        {
            // Disable expensive AutoFit for high-performance production exports
            WithAutoFitColumns(false);
            WithTrimStringValues(true);
            WithValidateTemplateOnRead(true);

            Map(x => x.TrackingCode)
                .ToColumn(1)
                .WithHeader("Tracking Code")
                .WithWidth(38)
                .IsBold(true)
                .WithFontColor(ExcelColor.Navy);

            Map(x => x.CustomerCode)
                .ToColumn(2)
                .WithHeader("Customer Code")
                .WithWidth(16)
                .IsBold(true);

            Map(x => x.CustomerName)
                .ToColumn(3)
                .WithHeader("Customer Name")
                .WithWidth(22);

            Map(x => x.CompanyName)
                .ToColumn(4)
                .WithHeader("Company Name")
                .WithWidth(24);

            Map(x => x.OrderDate)
                .ToColumn(5)
                .WithHeader("Order Date")
                .WithWidth(14)
                .WithNumberFormat("yyyy-mm-dd");

            Map(x => x.DeliveryDate)
                .ToColumn(6)
                .WithHeader("Delivery Date")
                .WithWidth(14)
                .WithNumberFormat("yyyy-mm-dd");

            Map(x => x.PreferredDeliveryTime)
                .ToColumn(7)
                .WithHeader("Delivery Time")
                .WithWidth(14)
                .WithNumberFormat("hh:mm");

            Map(x => x.CreatedAtUtc)
                .ToColumn(8)
                .WithHeader("Created (UTC)")
                .WithWidth(20)
                .WithNumberFormat("yyyy-mm-dd hh:mm:ss");

            Map(x => x.Status)
                .ToColumn(9)
                .WithHeader("Status")
                .WithWidth(15)
                .WithBackgroundColor(ExcelColor.SoftBlue);

            Map(x => x.Payment)
                .ToColumn(10)
                .WithHeader("Payment Method")
                .WithWidth(18);

            Map(x => x.Priority)
                .ToColumn(11)
                .WithHeader("Priority")
                .WithWidth(14)
                .WithBackgroundColor(ExcelColor.SoftYellow);

            Map(x => x.Quantity)
                .ToColumn(12)
                .WithHeader("Quantity")
                .WithWidth(12)
                .WithNumberFormat("#,##0");

            Map(x => x.UnitPrice)
                .ToColumn(13)
                .WithHeader("Unit Price")
                .WithWidth(14)
                .WithNumberFormat("$#,##0.00");

            Map(x => x.DiscountRate)
                .ToColumn(14)
                .WithHeader("Discount Rate")
                .WithWidth(14)
                .WithNumberFormat("0.0%");

            Map(x => x.Subtotal)
                .ToColumn(15)
                .WithHeader("Subtotal")
                .WithWidth(15)
                .WithNumberFormat("$#,##0.00");

            Map(x => x.DiscountAmount)
                .ToColumn(16)
                .WithHeader("Discount ($)")
                .WithWidth(14)
                .WithNumberFormat("$#,##0.00");

            Map(x => x.TaxRate)
                .ToColumn(17)
                .WithHeader("Tax Rate")
                .WithWidth(12)
                .WithNumberFormat("0.0%");

            Map(x => x.TaxAmount)
                .ToColumn(18)
                .WithHeader("Tax ($)")
                .WithWidth(14)
                .WithNumberFormat("$#,##0.00");

            Map(x => x.ShippingFee)
                .ToColumn(19)
                .WithHeader("Shipping Fee")
                .WithWidth(14)
                .WithNumberFormat("$#,##0.00");

            Map(x => x.InsuranceFee)
                .ToColumn(20)
                .WithHeader("Insurance Fee")
                .WithWidth(14)
                .WithNumberFormat("$#,##0.00");

            Map(x => x.TotalAmount)
                .ToColumn(21)
                .WithHeader("Total Amount")
                .WithWidth(16)
                .IsBold(true)
                .WithNumberFormat("$#,##0.00")
                .WithBackgroundColor(ExcelColor.SoftGreen);

            Map(x => x.IsExpressDelivery)
                .ToColumn(22)
                .WithHeader("Express")
                .WithWidth(12);

            Map(x => x.IsInsured)
                .ToColumn(23)
                .WithHeader("Insured")
                .WithWidth(12);

            Map(x => x.Tags)
                .ToColumn(24)
                .WithHeader("Tags")
                .WithWidth(26)
                .WithConverter(new DelimitedTagsConverter());

            Map(x => x.SlaStatus)
                .ToColumn(25)
                .WithHeader("SLA Status")
                .WithWidth(18)
                .IsBold(true);

            Map(x => x.ProcessedAtUtc)
                .ToColumn(26)
                .WithHeader("Processed (UTC)")
                .WithWidth(20)
                .WithNumberFormat("yyyy-mm-dd hh:mm:ss");

            Map(x => x.Notes)
                .ToColumn(27)
                .WithHeader("Notes")
                .WithWidth(28);
        }
    }

    /// <summary>
    /// Profile for C# 9+ immutable positional record (OrderSummaryRecord).
    /// Supports BOTH exporting (Write) and importing (Read) back the Persian RTL report.
    /// Showcases WithRightToLeft(true) for Persian/Arabic worksheets and custom formatting.
    /// Matched with GET /api/financial-orders/export-records and POST /api/financial-orders/import-records.
    /// </summary>
    public class OrderSummaryRecordProfile : ExcelProfile<OrderSummaryRecord>
    {
        public OrderSummaryRecordProfile()
        {
            // Demonstrates native Right-To-Left (RTL) worksheet configuration
            WithRightToLeft(true);
            WithAutoFitColumns(true);
            WithTrimStringValues(true);
            WithValidateTemplateOnRead(true);

            Map(x => x.TrackingCode)
                .ToColumn(1)
                .WithHeader("کد رهگیری (GUID)")
                .IsBold(true)
                .WithFontColor(ExcelColor.DarkBlue);

            Map(x => x.CustomerCode)
                .ToColumn(2)
                .WithHeader("کد مشتری")
                .IsBold(true);

            Map(x => x.CustomerName)
                .ToColumn(3)
                .WithHeader("نام مشتری");

            Map(x => x.OrderDate)
                .ToColumn(4)
                .WithHeader("تاریخ ثبت سفارش")
                .WithNumberFormat("yyyy-mm-dd");

            Map(x => x.PreferredDeliveryTime)
                .ToColumn(5)
                .WithHeader("ساعت تحویل")
                .WithNumberFormat("hh:mm");

            Map(x => x.Quantity)
                .ToColumn(6)
                .WithHeader("تعداد اقلام")
                .WithNumberFormat("#,##0");

            Map(x => x.TotalAmount)
                .ToColumn(7)
                .WithHeader("مبلغ کل نهایی")
                .IsBold(true)
                .WithNumberFormat("$#,##0.00")
                .WithBackgroundColor(ExcelColor.SoftGreen);

            Map(x => x.Status)
                .ToColumn(8)
                .WithHeader("وضعیت سفارش")
                .WithBackgroundColor(ExcelColor.SoftYellow);

            Map(x => x.Priority)
                .ToColumn(9)
                .WithHeader("اولویت ارسال");

            Map(x => x.IsExpressDelivery)
                .ToColumn(10)
                .WithHeader("ارسال اکسپرس");
        }
    }
}
