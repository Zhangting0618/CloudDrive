namespace Ptcent.Cloud.Drive.Application.Options
{
    /// <summary>
    /// 回收站自动清理配置
    /// </summary>
    public class RecycleBinCleanupOptions
    {
        public bool Enabled { get; set; } = true;
        public int RetentionDays { get; set; } = 30;
        public int IntervalHours { get; set; } = 24;
    }
}
