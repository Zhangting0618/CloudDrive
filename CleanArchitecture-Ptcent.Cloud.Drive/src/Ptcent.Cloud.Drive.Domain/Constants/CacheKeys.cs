namespace Ptcent.Cloud.Drive.Domain.Constants
{
    /// <summary>
    /// 缓存键常量
    /// </summary>
    public static class CacheKeys
    {
        public const string UserSession = "user:session:{0}";
        public const string FileMetadata = "file:metadata:{0}";
        public const string FileToken = "file:token:{0}";
        public const string UploadChunk = "upload:chunk:{0}:{1}";
        public const string UserQuota = "user:quota:{0}";
    }

    /// <summary>
    /// 缓存过期时间常量
    /// </summary>
    public static class CacheExpiration
    {
        public static readonly TimeSpan UserSession = TimeSpan.FromDays(30);
        public static readonly TimeSpan FileMetadata = TimeSpan.FromHours(1);
        public static readonly TimeSpan FileToken = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan UploadChunk = TimeSpan.FromHours(24);
        public static readonly TimeSpan UserQuota = TimeSpan.FromMinutes(5);
    }
}
