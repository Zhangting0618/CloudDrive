using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Domain.Entities;
using System.Linq.Expressions;

namespace Ptcent.Cloud.Drive.Infrastructure.Persistence
{
    /// <summary>
    /// 应用数据库上下文
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 移除 Entity 后缀的表名映射
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.ClrType.Name.Replace("Entity", string.Empty);
                entityType.SetTableName(tableName);
            }

            // 全局查询过滤器 - 软删除
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isDelProperty = entityType.FindProperty("IsDel");
                if (isDelProperty != null)
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var body = Expression.Equal(
                        Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(int) }, parameter, Expression.Constant("IsDel")),
                        Expression.Constant(0));
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
                }
            }
        }

        public virtual DbSet<UserEntity> Users { get; set; } = null!;
        public virtual DbSet<FileEntity> Files { get; set; } = null!;
    }
}
