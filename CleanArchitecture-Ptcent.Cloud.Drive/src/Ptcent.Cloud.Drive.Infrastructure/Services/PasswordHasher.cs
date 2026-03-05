using Ptcent.Cloud.Drive.Application.Services;
using System.Security.Cryptography;
using System.Text;

namespace Ptcent.Cloud.Drive.Infrastructure.Services
{
    /// <summary>
    /// 密码哈希服务实现
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        public (string HashedPassword, string Salt) HashPassword(string password, string? salt = null)
        {
            // 如果没有提供盐值，生成一个新的
            if (string.IsNullOrEmpty(salt))
            {
                salt = GenerateSalt();
            }

            // 使用 SHA256 + 盐值进行哈希
            var hash = ComputeHash(password, salt);

            return (hash, salt);
        }

        public bool VerifyPassword(string password, string salt, string hashedPassword)
        {
            var computedHash = ComputeHash(password, salt);
            return computedHash == hashedPassword;
        }

        private static string ComputeHash(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        private static string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
    }
}
