namespace OpenXmlSampleWebApi.Models
{
    /// <summary>
    /// Represents the lifecycle status of an e-commerce order.
    /// Used with Exceler's <c>WithDropdownFromEnum&lt;OrderStatus&gt;()</c> for data-entry validation.
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Refunded
    }
}
