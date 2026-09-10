namespace TestExcelerWebApi.Inputs
{
    public enum OrderStatus
    {
        Pending,
        Approved,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public enum PaymentMethod
    {
        CreditCard,
        BankTransfer,
        CashOnDelivery,
        OnlineWallet,
        Crypto
    }

    public enum PriorityLevel
    {
        Low,
        Standard,
        High,
        Urgent
    }
}
