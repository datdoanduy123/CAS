using System;
using System.Security.Cryptography;
using System.Text;

namespace Application.Helpers
{
    public static class SecurityHelper
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000; // Độ phức tạp PBKDF2

        /// <summary>
        /// Sinh Salt ngẫu nhiên
        /// </summary>
        public static string GenerateSalt()
        {
            var saltBytes = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hash mật khẩu bằng thuật toán PBKDF2
        /// </summary>
        public static string HashPassword(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var key = algorithm.GetBytes(KeySize);
                return Convert.ToBase64String(key);
            }
        }

        /// <summary>
        /// Xác thực mật khẩu PBKDF2
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            var newHash = HashPassword(password, salt);
            return newHash == hashedPassword;
        }

        /// <summary>
        /// Sinh Secret ngẫu nhiên bảo mật (Dùng cho AppSecret)
        /// </summary>
        public static string GenerateRandomSecret(int length = 32)
        {
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, Math.Min(length, 32));
        }

        /// <summary>
        /// Hash chuỗi Secret (SHA256)
        /// </summary>
        public static string HashSecret(string secret)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(secret));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
