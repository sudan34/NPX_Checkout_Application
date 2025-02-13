namespace NPX_Checkout_Application.Models
{
    public class MerchantData
    {
        public string MerchantId { get; set; } = null!;
        public string? MerchantName { get; set; } = null!;
        public string? ApiUsername { get; set; } = null!;
        public string? SecretKey { get; set; } = null!;
        public string? ApiPassword { get; set; } = null!;
        public string? InstrumentCode { get; set; } = null!;
    }
}
