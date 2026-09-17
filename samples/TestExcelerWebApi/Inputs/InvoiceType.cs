namespace TestExcelerWebApi.Inputs
{
    /// <summary>
    /// Represents invoice classification categories.
    /// Used with Exceler's <c>WithDropdownFromEnum&lt;InvoiceType&gt;()</c> for EPPlus data validation.
    /// </summary>
    public enum InvoiceType
    {
        Standard,
        Proforma,
        Commercial,
        CreditNote,
        DebitNote
    }
}
