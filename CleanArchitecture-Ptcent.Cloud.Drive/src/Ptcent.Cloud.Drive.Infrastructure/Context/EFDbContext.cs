using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Infrastructure.Context
{
    public partial class EFDbContext : DbContext
    {
        public EFDbContext(DbContextOptions<EFDbContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ////optionsBuilder.UseSqlServer(ConfigUtil.GetValue("PtcentYiDocUserWebApiConnection"));
            optionsBuilder.UseLazyLoadingProxies();
            if (ConfigUtil.GetValue("IsOpenPrintSQL") == "on")//开启控制台打印SQL
            {
                optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                }));
            }
            //optionsBuilder.AddInterceptors(new QueryWithNoLockDbCommandInterceptor());
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.ClrType.Name.Replace("Entity", ""));
            }
            base.OnModelCreating(modelBuilder);
            //IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> entityTypes = modelBuilder.Model.GetEntityTypes();
            //foreach (var entityType in entityTypes)
            //{
            //    InitGobalFilter(entityType, modelBuilder);
            //}
        }
        public virtual DbSet<UserEntity> UserEntities { get; set; } = null!;
        public virtual DbSet<FileEntity> FileEntities { get; set; } = null!;
        //全局查询过滤器

        //public static void InitGobalFilter(IMutableEntityType entityType, ModelBuilder modelBuilder)
        //{
        //    if (modelBuilder == null)
        //    {
        //        return;
        //    }
        //    IEnumerable<IMutableProperty> props = entityType.GetProperties();
        //    if (props.Any(x => x.Name == "IsDel"))
        //    {
        //        ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
        //        byte defaultValue = 0;
        //        BinaryExpression body = Expression.Equal(
        //            Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(byte) }, parameter, Expression.Constant("IsDel")),
        //       Expression.Constant(defaultValue));
        //        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
        //    }
        //}

    }
}
