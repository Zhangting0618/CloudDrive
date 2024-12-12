using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Infrastructure.Cache;
using Ptcent.Cloud.Drive.Infrastructure.Context;
using Ptcent.Cloud.Drive.Infrastructure.IRespository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
