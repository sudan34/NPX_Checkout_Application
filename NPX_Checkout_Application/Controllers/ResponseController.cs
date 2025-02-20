using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPX_Checkout_Application.Models;
using NPX_Checkout_Application.Utilities;
using System.Net.Http.Headers;
using System.Text;

namespace NPX_Checkout_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponseController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly AppSettings _settings;
        private readonly MerchantData _data;

        public ResponseController(HttpClient client, AppSettings settings, MerchantData data)
        {
            _client = client;
            _settings = settings;
            _data = data;
        }

        // In-memory collection to track processed transactions
        private static readonly HashSet<string> ProcessedTransactions = new HashSet<string>();

        [HttpGet("Notify")]
        public async Task<IActionResult> Notification(string MerchantTxnId, string GatewayTxnId)
        {
            var merchantId = _data.MerchantId;
            var merchantName = _data.MerchantName;
            var apiPassword = _data.ApiPassword;
            var secretKey = _data.SecretKey;
            var baseUrl = _settings.BaseUrl;

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

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
            var content = new StringContent(payloadData, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(baseUrl + "CheckTransactionStatus", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var resModel = JsonConvert.DeserializeObject<CheckTransactionStatusResponse>(responseData);

                if (resModel?.code == "0")
                {
                    // Check if the transaction has already been processed
                    if (ProcessedTransactions.Contains(MerchantTxnId))
                    {
                        return Ok("Already Received");
                    }
                    else
                    {
                        // Add the transaction to the processed list
                        ProcessedTransactions.Add(MerchantTxnId);
                        return Ok("Received");
                    }
                }
            }

            return Ok("Failed");
        }

        
    }
}
