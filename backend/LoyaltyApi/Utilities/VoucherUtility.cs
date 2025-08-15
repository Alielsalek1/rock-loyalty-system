using System.Security.Cryptography;
using System.Text;

namespace LoyaltyApi.Utilities
{
    public class VoucherUtility
    {
        public int CalculateVoucherValue(int points, double ratio) => Convert.ToInt32(points * (ratio / 100));

        public string ShortenVoucherCode(string voucherCode)
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(voucherCode));

            StringBuilder hashString = new();
            foreach (byte b in hashBytes)
            {
                hashString.Append(b.ToString("x2"));
            }

            return hashString.ToString()[..5].ToUpper();
        }
    }
}