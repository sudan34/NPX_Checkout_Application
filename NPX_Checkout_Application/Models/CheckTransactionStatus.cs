using System.ComponentModel.DataAnnotations;

namespace NPX_Checkout_Application.Models
{
    public class CheckTransactionStatus
    {
        public string? MerchantId { get; set; }
        public string? MerchantName { get; set; }
        public string? MerchantTxnId { get; set; }
        public string? Signature { get; set; }
    }
    public class CheckTransactionStatusResponse
    {
        public string? code { get; set; }
        public string? message { get; set; }
        public List<string>? errors { get; set; }
        public TransactionData? data { get; set; }
    }

    public class TransactionData
    {
        public string? GatewayReferenceNo { get; set; }
        public string? Amount { get; set; }
        public string? ServiceCharge { get; set; }
        public string? TransactionRemarks { get; set; }
        public string? TransactionRemarks2 { get; set; }
        public string? TransactionRemarks3 { get; set; }
        public string? ProcessId { get; set; }
        public string? TransactionDate { get; set; }
        public string? MerchantTxnId { get; set; }
        public string? CbsMessage { get; set; }
        public string? Status { get; set; }
        public string? Institution { get; set; }
        public string? Instrument { get; set; }
        public string? PaymentCurrency { get; set; }
        public string? ExchangeRate { get; set; }
    }
}
