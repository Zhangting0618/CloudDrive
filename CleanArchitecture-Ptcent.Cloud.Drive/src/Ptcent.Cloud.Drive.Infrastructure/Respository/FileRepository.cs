using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.Cache;
using Ptcent.Cloud.Drive.Infrastructure.Context;

namespace Ptcent.Cloud.Drive.Infrastructure.Respository
{
    public class FileRepository : BaseRepository<FileEntity>, IFileRepository
    {
        public FileRepository(EFDbContext db) : base(db)
        {
        }
        /// <summary>
        /// 获取单个文件缓存
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<FileEntity> GetFileCacheByItemId(long fileId)
        {
            var cachKey = CacheKey.Ptcent_Cloud_Drive_Item_SingleItem + fileId;
            var fileEntity = RedisClient.Get<FileEntity>(cachKey);
            if (fileEntity == null)
            {
                fileEntity = await GetById(fileId);
                if (fileEntity != null)
                {
                    RedisClient.Insert<FileEntity>(cachKey, fileEntity, new TimeSpan(30, 0, 0, 0));
                }
            }
            return fileEntity;
        }

        /// <summary>
        /// 保存文件实体
        /// </summary>
        /// <param name="fileEntity">实体</param>
        /// <param name="userId">用户Id</param>
        /// <param name="isSave">是否保存</param>
        /// <returns></returns>
        public async Task<FileEntity> SaveFileEntity(FileEntity fileEntity, long userId, bool isSave = false)
        {
            fileEntity.CreatedDate = DateTime.Now;
            fileEntity.UpdatedDate = DateTime.Now;
            fileEntity.UpdatedBy = userId;
            fileEntity.CreatedBy = userId;
            fileEntity.IsDel = 0;
            if (isSave)
            {
                await Add(fileEntity);
                return fileEntity;
            }
            else
            {
                return fileEntity;
            }
        }
        /// <summary>
        /// 获取文件列表
        /// </summary>
        /// <param name="idPath"></param>
        /// <param name="stats"></param>
        /// <returns></returns>
        public async Task<IQueryable<FileEntity>> GetFilesByPathPrefixAsync(
       string idPath,
       FileStatsType stats)
        {
            return  (await GetList(f =>EF.Functions.Like(f.Idpath, $"{idPath}%") && f.IsDel == (int)stats));
        }
    }
}
