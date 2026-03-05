namespace Ptcent.Cloud.Drive.Application.Services
{
    /// <summary>
    /// 分布式 ID 生成器接口
    /// </summary>
    public interface IIdGeneratorService
    {
        long NewId();
        string NewString();
    }
}
