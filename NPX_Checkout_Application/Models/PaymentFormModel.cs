using System.ComponentModel.DataAnnotations;

namespace NPX_Checkout_Application.Models
{
    public class PaymentFormModel
    {
        [Required(ErrorMessage = "Please enter the Amount.")]
        public string? Amount { get; set; }
        public string? TransactionRemarks { get; set; }
        public string MerchantTxnId { get; set; } = null!;
        public string? InstrumentCode { get; set; }
    }
}
