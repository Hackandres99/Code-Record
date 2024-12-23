using System.Security.Cryptography;
using System.Text;

namespace Code_Record.Server.Extensions.Controllers
{
    public static class EncryptData
    {
        public static string EncryptPass(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var has = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return has;
            }
        }
    }
}
