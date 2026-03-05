namespace Ptcent.Cloud.Drive.Application.Services
{
    /// <summary>
    /// 密码哈希服务接口
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// 哈希密码
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <param name="salt">盐值</param>
        /// <returns>(哈希后的密码，盐值)</returns>
        (string HashedPassword, string Salt) HashPassword(string password, string? salt = null);

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="hashedPassword">存储的哈希密码</param>
        /// <returns>是否匹配</returns>
        bool VerifyPassword(string password, string salt, string hashedPassword);
    }
}
