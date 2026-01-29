using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Domain.Entities;
using System.Linq.Expressions;

namespace Ptcent.Cloud.Drive.Infrastructure.Context
{
    public partial class EFDbContext : DbContext
    {
        public EFDbContext(DbContextOptions<EFDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 实体表名去掉 Entity 后缀
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.ClrType.Name.Replace("Entity", ""));
            }

            // 可选全局查询过滤器
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.FindProperty("IsDel") != null)
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var body = Expression.Equal(
                        Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(byte) }, parameter, Expression.Constant("IsDel")),
                        Expression.Constant((byte)0));
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        // DbSet 示例
        public virtual DbSet<UserEntity> UserEntities { get; set; } = null!;
        public virtual DbSet<FileEntity> FileEntities { get; set; } = null!;
    }
}

