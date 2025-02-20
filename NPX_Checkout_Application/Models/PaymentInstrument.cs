namespace NPX_Checkout_Application.Models
{
    public class PaymentInstrument
    {
        public string InstitutionName { get; set; }
        public string InstrumentName { get; set; }
        public string InstrumentCode { get; set; }
        public string LogoUrl { get; set; }
        public string BankUrl { get; set; }
        public string BankType { get; set; }
    }
}
