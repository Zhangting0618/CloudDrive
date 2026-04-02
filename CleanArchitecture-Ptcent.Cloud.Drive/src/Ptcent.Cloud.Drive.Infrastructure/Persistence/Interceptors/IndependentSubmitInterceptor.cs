using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ptcent.Cloud.Drive.Application.Attributes;
using System.Collections.Concurrent;
using System.Reflection;

namespace Ptcent.Cloud.Drive.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// 独立提交拦截器
    /// 用于处理标记了 [IndependentSubmit] 的实体的立即提交
    /// </summary>
    public class IndependentSubmitInterceptor : SaveChangesInterceptor
    {
        // 存储需要独立提交的实体类型名称
        private static readonly ConcurrentDictionary<string, bool> _independentEntityTypes = new();

        // 标记当前上下文是否允许独立提交
        private static readonly AsyncLocal<bool> _allowIndependentSubmit = new();

        /// <summary>
        /// 设置是否允许独立提交
        /// </summary>
        public static void SetAllowIndependentSubmit(bool allow)
        {
            _allowIndependentSubmit.Value = allow;
        }

        /// <summary>
        /// 检查是否允许独立提交
        /// </summary>
        public static bool IsAllowIndependentSubmit => _allowIndependentSubmit.Value;

        /// <summary>
        /// 注册独立提交实体类型
        /// </summary>
        public static void RegisterIndependentEntityType<T>()
        {
            _independentEntityTypes.TryAdd(typeof(T).Name, true);
        }

        /// <summary>
        /// 注册独立提交实体类型
        /// </summary>
        public static void RegisterIndependentEntityType(Type type)
        {
            _independentEntityTypes.TryAdd(type.Name, true);
        }

        /// <summary>
        /// 检查实体是否为独立提交类型
        /// </summary>
        public static bool IsIndependentEntityType(string entityTypeName)
        {
            return _independentEntityTypes.ContainsKey(entityTypeName);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return result;

            // 如果不允许独立提交，直接返回
            if (!IsAllowIndependentSubmit)
            {
                return result;
            }

            // 检查是否有标记了 IndependentSubmitEntityAttribute 的实体被修改
            var hasIndependentEntity = false;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                {
                    var entityType = entry.Entity.GetType();

                    // 检查是否标记了特性
                    var attr = entityType.GetCustomAttribute<IndependentSubmitEntityAttribute>();
                    if (attr != null)
                    {
                        hasIndependentEntity = true;
                        break;
                    }

                    // 检查是否在注册列表中
                    if (IsIndependentEntityType(entityType.Name))
                    {
                        hasIndependentEntity = true;
                        break;
                    }
                }
            }

            // 如果有独立提交实体，立即保存
            if (hasIndependentEntity)
            {
                // 独立提交实体有自己的事务处理，这里只是标记
                // 实际的独立提交由 UnitOfWork 处理
            }

            return result;
        }
    }
}
