using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.VnPay
{
    public static class HmacSHA
    {
        public static string HmacSHA512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
