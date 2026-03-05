namespace Ptcent.Cloud.Drive.Application.Services
{
    /// <summary>
    /// JWT 服务接口
    /// </summary>
    public interface IJwtService
    {
        string GenerateToken(string userId, string userName, string phone, string email);
        string? ValidateToken(string token);
    }
}
