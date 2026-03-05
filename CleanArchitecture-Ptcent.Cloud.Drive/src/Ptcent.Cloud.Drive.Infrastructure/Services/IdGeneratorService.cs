using Ptcent.Cloud.Drive.Application.Services;
using Yitter.IdGenerator;

namespace Ptcent.Cloud.Drive.Infrastructure.Services
{
    /// <summary>
    /// 分布式 ID 生成器实现
    /// </summary>
    public class IdGeneratorService : IIdGeneratorService
    {
        public IdGeneratorService()
        {
            // 初始化雪花算法
            var options = new IdGeneratorOptions(1)
            {
                WorkerIdBitLength = 10,
                SeqBitLength = 6
            };
            YitIdHelper.SetIdGenerator(options);
        }

        public long NewId()
        {
            return YitIdHelper.NextId();
        }

        public string NewString()
        {
            return YitIdHelper.NextId().ToString();
        }
    }
}
