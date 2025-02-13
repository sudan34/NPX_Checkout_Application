using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NPX_Checkout_Application.Utilities
{
    public static class SignatureUtility
    {
        public static string GeneratePlainText(string jsonString)
        {
            var jsonObject = JObject.Parse(jsonString);

            // Sort properties by name and exclude the "signature" field
            var concatenatedValues = new StringBuilder();
            foreach (var property in jsonObject.Properties()
                                               .OrderBy(p => p.Name)
                                               .Where(p => !p.Name.Equals("signature", StringComparison.OrdinalIgnoreCase)))
            {
                concatenatedValues.Append(property.Value.ToString());
            }

            return concatenatedValues.ToString();
        }

        public static string SignatureGeneration(string plainText, string secretKey)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
