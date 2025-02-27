using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPX_Checkout_Application.Models;
using NPX_Checkout_Application.Utilities;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime;
using System.Text;

namespace NPX_Checkout_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly MerchantData _merchantData;
        private readonly AppSettings _appSettings;
        private readonly ILogger<HomeController> _logger;

        public HomeController(HttpClient httpClient, MerchantData merchantData, AppSettings appSettings, ILogger<HomeController> logger)
        {
            _httpClient = httpClient;
            _merchantData = merchantData;
            _appSettings = appSettings;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        // Step 1: Fetch Payment Instruments
        public async Task<List<PaymentInstrument>> GetPaymentInstrumentsAsync()
        {
            var baseURL = _appSettings.BaseUrl;
            string merchantId = _merchantData.MerchantId;
            string merchantName = _merchantData.MerchantName!;
            string secretKey = _merchantData.SecretKey!;
            string apiPassword = _merchantData.ApiPassword!;

            JObject data = new JObject();
            data["MerchantId"] = merchantId;
            data["MerchantName"] = merchantName;

            string jsonData = JsonConvert.SerializeObject(data);
            string plainText = SignatureUtility.GeneratePlainText(jsonData);
            string signature = SignatureUtility.SignatureGeneration(plainText, secretKey);
            data["Signature"] = signature;
            string payloadData = JsonConvert.SerializeObject(data);

            string authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{merchantName}:{apiPassword}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);
            StringContent content = new StringContent(payloadData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(baseURL + "GetPaymentInstrumentDetails", content);
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                dynamic resModel = JsonConvert.DeserializeObject(responseData)!;

                if (resModel!.code == "0")
                {
                    var instruments = JsonConvert.DeserializeObject<List<PaymentInstrument>>(resModel.data.ToString());
                    return instruments;
                }
            }
            return new List<PaymentInstrument>();
        }

        // Step 2: Display Payment Instruments in a View
        public async Task<IActionResult> SelectInstrument(PaymentFormModel model)
        {
            var instruments = await GetPaymentInstrumentsAsync();
            ViewBag.Amount = model.Amount;
            ViewBag.TransactionRemarks = model.TransactionRemarks;

            return View(instruments);
        }

        // Step 3: Handle the Selection of the Payment Instrument
        [HttpPost]
        public IActionResult ProcessSelectedInstrument(string instrumentCode, PaymentFormModel model)
        {
            if (string.IsNullOrEmpty(instrumentCode))
            {
                // Handle the case where no instrument is selected
                ModelState.AddModelError("instrumentCode", "Please select a payment instrument.");
                return RedirectToAction("SelectInstrument", model);
            }

            model.InstrumentCode = instrumentCode;
            return RedirectToAction("GetProcessId", model);
        }

        // Step 4: Modified GetProcessId Method
        public async Task<IActionResult> GetProcessId(PaymentFormModel model)
        {
            try
            {
                model.MerchantTxnId = "NPX-0-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                var baseURL = _appSettings.BaseUrl;
                string merchantId = _merchantData.MerchantId;
                string merchantName = _merchantData.MerchantName!;
                string secretKey = _merchantData.SecretKey!;
                string apiPassword = _merchantData.ApiPassword!;

                JObject data = new JObject();
                data["MerchantId"] = merchantId;
                data["MerchantName"] = merchantName;
                data["Amount"] = model.Amount;
                data["MerchantTxnId"] = model.MerchantTxnId;
                data["TransactionRemarks"] = model.TransactionRemarks;

                string jsonData = JsonConvert.SerializeObject(data);

                string plainText = SignatureUtility.GeneratePlainText(jsonData);
                string signature = SignatureUtility.SignatureGeneration(plainText, secretKey);
                data["Signature"] = signature;
                string payloadData = JsonConvert.SerializeObject(data);

                string authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{merchantName}:{apiPassword}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);
                StringContent content = new StringContent(payloadData, Encoding.UTF8, "application/json");

                // Make a POST request to the API
                HttpResponseMessage response = await _httpClient.PostAsync(baseURL + "GetProcessId", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    dynamic resModel = JsonConvert.DeserializeObject(responseData)!;
                    _logger.LogInformation($"GetProcesId Respone: {resModel}");

                    if (resModel!.code == "0" && !string.IsNullOrEmpty(resModel.data.ProcessId.ToString()))
                    {
                        var redirectionModel = new RedirectionModel
                        {
                            MerchantId = merchantId,
                            MerchantName = merchantName,
                            Amount = model.Amount,
                            MerchantTxnId = data["MerchantTxnId"]!.ToString(),
                            TransactionRemarks = model.TransactionRemarks,
                            InstrumentCode = model.InstrumentCode, // Pass InstrumentCode to the redirection model
                            ProcessId = resModel.data.ProcessId.ToString(),
                        };

                        return RedirectToAction("PaymentIndex", redirectionModel);
                    }
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return RedirectToAction("Index");
        }
        public ActionResult PaymentIndex(RedirectionModel model)
        {
            return View(model);
        }

        [HttpGet("Receipt")]
        public async Task<IActionResult> Receipt(string MerchantTxnId, string GatewayTxnId)
        {
            var merchantId = _merchantData.MerchantId;
            var merchantName = _merchantData.MerchantName;
            var apiPassword = _merchantData.ApiPassword;
            var secretKey = _merchantData.SecretKey;
            var baseUrl = _appSettings.BaseUrl;

            var checkTransactionStatus = new CheckTransactionStatus
            {
                MerchantId = merchantId,
                MerchantName = merchantName,
                MerchantTxnId = MerchantTxnId
            };

            var plainText = SignatureUtility.GeneratePlainText(JsonConvert.SerializeObject(checkTransactionStatus));
            checkTransactionStatus.Signature = SignatureUtility.SignatureGeneration(plainText, secretKey!);

            var payloadData = JsonConvert.SerializeObject(checkTransactionStatus);
            var authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{merchantName}:{apiPassword}"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
            var content = new StringContent(payloadData, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(baseUrl + "CheckTransactionStatus", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var resModel = JsonConvert.DeserializeObject<CheckTransactionStatusResponse>(responseData);

                _logger.LogInformation($"CheckTransactionStatus Respone: {response.StatusCode}, Data: {JsonConvert.DeserializeObject(responseData)}");
                if (resModel?.code == "0" && resModel.data != null)
                {
                    // Pass the transaction data to the view
                    return View("Receipt", resModel.data);
                }
                else
                {
                    // Handle API error or invalid response
                    return View("Error", new ErrorViewModel { Message = "Failed to retrieve transaction details." });
                }
            }

            // Handle API call failure
            return View("Error", new ErrorViewModel { Message = "API call failed." });
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
