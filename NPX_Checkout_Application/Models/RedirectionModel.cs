namespace NPX_Checkout_Application.Models
{
    public class RedirectionModel
    {
        public string? MerchantId { get; set; }
        public string? MerchantUserId { get; set; }
        public string? MerchantName { get; set; }
        public string? Amount { get; set; }
        public string? MerchantTxnId { get; set; }
        public string? TransactionRemarks { get; set; }
        public string? PaymentCurrency { get; set; }
        public string? ProcessId { get; set; }
        public string? InstrumentCode { get; set; }
        public string? ResponseUrl { get; set; }
    }
}
