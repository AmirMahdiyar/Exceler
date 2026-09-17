using Exceler.Configuration;
using OpenXmlSampleWebApi.Models;

namespace OpenXmlSampleWebApi.Profiles
{
    /// <summary>
    /// Exceler mapping profile configuring formatting, widths, and styling for high-volume order exports.
    /// </summary>
    public class OrderExportProfile : ExcelProfile<OrderExportModel>
    {
        public OrderExportProfile()
        {
            // Configure layout
            WithAutoFitColumns(false); // Explicit column widths are optimized for O(1) streaming!

            // Conditional Style Example 1: Row-level conditional styling
            // Highlights the entire row with soft yellow when an order has express delivery
            WithConditionalRowStyle(
                x => x.IsExpressDelivery,
                s => s.WithBackgroundColor(ExcelColor.SoftYellow)
            );

            Map(x => x.Id)
                .ToColumn(1)
                .WithHeader("Order ID")
                .WithWidth(12)
                .IsBold();

            Map(x => x.OrderNumber)
                .ToColumn(2)
                .WithHeader("Order Number")
                .WithWidth(18);

            Map(x => x.CustomerName)
                .ToColumn(3)
                .WithHeader("Customer Name")
                .WithWidth(25)
                .WithFontColor(ExcelColor.DarkBlue);

            Map(x => x.CustomerEmail)
                .ToColumn(4)
                .WithHeader("Email")
                .WithWidth(30);

            // Smart Dropdown Example 1: Comma-safe reference list
            // Notice: "Tehran, Iran" contains a comma. Exceler's dual-routing algorithm detects this
            // and automatically stores the choices in a hidden "_ValidationData" sheet, avoiding broken formulas!
            Map(x => x.Country)
                .ToColumn(5)
                .WithHeader("Country")
                .WithWidth(18)
                .WithDropdown(
                    "United States",
                    "Germany",
                    "United Kingdom",
                    "France",
                    "Tehran, Iran",
                    "Tokyo, Japan",
                    "Sydney, Australia"
                );

            // Conditional Style Example 2: Column-level conditional styling based on property value
            // High-value orders (> $1,500) are automatically rendered with bold text and soft green background,
            // while strictly preserving the currency NumberFormat!
            Map(x => x.TotalAmount)
                .ToColumn(6)
                .WithHeader("Total Amount")
                .WithWidth(16)
                .WithFormat("$#,##0.00")
                .WithConditionalStyle(
                    val => val > 1500m,
                    s => s.WithBackgroundColor(ExcelColor.SoftGreen).SetBold(true)
                );

            // Smart Dropdown Example 2: Strongly-typed Enum dropdown
            // Conditional Style Example 3: Cross-property/model conditional style
            // If the order was Cancelled or Refunded, it is highlighted in soft red with dark red text!
            Map(x => x.Status)
                .ToColumn(7)
                .WithHeader("Status")
                .WithWidth(14)
                .WithDropdownFromEnum<OrderStatus>()
                .WithConditionalStyle(
                    x => x.Status == "Cancelled" || x.Status == "Refunded",
                    s => s.WithBackgroundColor(ExcelColor.SoftRed).WithFontColor(ExcelColor.DarkRed).SetBold(true)
                );

            Map(x => x.OrderDate)
                .ToColumn(8)
                .WithHeader("Order Date")
                .WithWidth(14)
                .WithFormat("yyyy-mm-dd");

            Map(x => x.OrderTime)
                .ToColumn(9)
                .WithHeader("Order Time")
                .WithWidth(14)
                .WithFormat("hh:mm:ss");

            Map(x => x.IsExpressDelivery)
                .ToColumn(10)
                .WithHeader("Express?")
                .WithWidth(12);

            Map(x => x.ItemCount)
                .ToColumn(11)
                .WithHeader("Items")
                .WithWidth(10);

            Map(x => x.Notes)
                .ToColumn(12)
                .WithHeader("Notes")
                .WithWidth(35);
        }
    }
}
