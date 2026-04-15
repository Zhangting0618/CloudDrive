namespace Ptcent.Cloud.Drive.Application.Dto.ReponseModels
{
    /// <summary>
    /// 存储统计 DTO
    /// </summary>
    public class StorageStatsDto
    {
        public long UserUsedBytes { get; set; }
        public long UserRecycleBytes { get; set; }
        public int UserFileCount { get; set; }
        public int UserFolderCount { get; set; }
        public long SystemUsedBytes { get; set; }
        public long SystemRecycleBytes { get; set; }
        public int SystemFileCount { get; set; }
        public int SystemFolderCount { get; set; }
    }
}
