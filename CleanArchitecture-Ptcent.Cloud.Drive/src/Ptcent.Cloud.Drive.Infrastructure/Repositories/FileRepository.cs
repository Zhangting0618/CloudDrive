using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;

namespace Ptcent.Cloud.Drive.Infrastructure.Repositories
{
    /// <summary>
    /// 文件仓储实现
    /// </summary>
    public class FileRepository : Repository<FileEntity>, IFileRepository
    {
        public FileRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<FileEntity>> GetByParentFolderIdAsync(long parentFolderId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(f => f.ParentFolderId == parentFolderId && f.IsDel == 0)
                .OrderByDescending(f => f.IsFolder)
                .ThenBy(f => f.LeafName)
                .ToListAsync(cancellationToken);
        }

        public async Task<FileEntity?> GetByIdpathAsync(string idpath, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Idpath == idpath && f.IsDel == 0, cancellationToken);
        }

        public async Task<List<FileEntity>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(f => f.CreatedBy == userId && f.IsDel == 0)
                .ToListAsync(cancellationToken);
        }
    }
}
