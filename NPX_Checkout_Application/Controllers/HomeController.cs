using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPX_Checkout_Application.Models;
using NPX_Checkout_Application.Utilities;
using System.Diagnostics;
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

                    if (resModel!.code == "0" && !string.IsNullOrEmpty(resModel.data.ProcessId.ToString()))
                    {
                        var redirectionModel = new RedirectionModel
                        {
                            MerchantId = merchantId,
                            MerchantName = merchantName,
                            Amount = model.Amount,
                            MerchantTxnId = data["MerchantTxnId"]!.ToString(),
                            TransactionRemarks = model.TransactionRemarks,
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
                
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
