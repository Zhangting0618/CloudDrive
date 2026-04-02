using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace Ptcent.Cloud.Drive.Infrastructure.Persistence.Behaviors
{
    /// <summary>
    /// 事务管理管道行为
    /// 自动处理 MediatR 请求的事务提交和回滚
    /// </summary>
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

        // 存储需要独立提交的实体类型
        private static readonly ConcurrentDictionary<string, bool> _independentEntities = new();
        // 存储已标记独立提交的方法/请求
        private static readonly ConcurrentDictionary<string, bool> _independentMethods = new();

        public TransactionBehavior(
            AppDbContext dbContext,
            ILogger<TransactionBehavior<TRequest, TResponse>> logger)
        {
            _dbContext = dbContext;
            _logger = logger;

            // 初始化时扫描所有标记了 IndependentSubmitEntityAttribute 的实体
            ScanIndependentEntities();
        }

        /// <summary>
        /// 扫描独立提交实体
        /// </summary>
        private static void ScanIndependentEntities()
        {
            var assembly = typeof(TransactionBehavior<TRequest, TResponse>).Assembly;
            var types = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<IndependentSubmitEntityAttribute>() != null);

            foreach (var type in types)
            {
                _independentEntities.TryAdd(type.Name, true);
            }
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // 检查请求是否标记了独立提交
            if (IsIndependentRequest(request))
            {
                _logger.LogDebug("请求 {RequestType} 标记为独立提交，跳过事务管理", typeof(TRequest).Name);
                return await next();
            }

            // 检查是否标记了 TransactionalAttribute
            var transactionalAttr = GetTransactionalAttribute(request);
            if (transactionalAttr == null || !transactionalAttr.Enabled)
            {
                // 没有标记事务特性，直接执行
                return await next();
            }

            // 检查是否标记了 RequireNewTransaction
            if (HasRequireNewTransaction(request))
            {
                _logger.LogDebug("请求 {RequestType} 要求新事务，创建嵌套事务", typeof(TRequest).Name);
                return await ExecuteWithTransaction(request, next, transactionalAttr, cancellationToken);
            }

            // 检查是否已有活动事务
            if (_dbContext.Database.CurrentTransaction != null)
            {
                _logger.LogDebug("已存在活动事务，加入现有事务 {RequestType}", typeof(TRequest).Name);
                return await next();
            }

            // 创建新事务
            return await ExecuteWithTransaction(request, next, transactionalAttr, cancellationToken);
        }

        /// <summary>
        /// 执行带事务的请求
        /// </summary>
        private async Task<TResponse> ExecuteWithTransaction(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            TransactionalAttribute attr,
            CancellationToken cancellationToken)
        {
            IDbContextTransaction? transaction = null;

            try
            {
                _logger.LogDebug("开始事务处理请求 {RequestType}", typeof(TRequest).Name);

                // 创建事务
                var isolationLevel = ConvertIsolationLevel(attr.IsolationLevel);
                transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

                // 执行请求处理
                var response = await next();

                // 提交事务
                await transaction.CommitAsync(cancellationToken);
                _logger.LogDebug("事务提交成功 {RequestType}", typeof(TRequest).Name);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "事务执行失败，回滚事务 {RequestType}: {Message}", typeof(TRequest).Name, ex.Message);

                // 回滚事务
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                // 重新抛出异常，让上层处理
                throw;
            }
            finally
            {
                // 释放事务资源
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// 检查请求是否标记为独立提交
        /// </summary>
        private bool IsIndependentRequest(TRequest request)
        {
            var requestType = typeof(TRequest);
            var cacheKey = requestType.FullName ?? requestType.Name;

            if (_independentMethods.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            // 检查方法特性
            var methodInfo = requestType.GetMethod("Handle") ?? requestType.GetTypeInfo().GetMethod("Handle");
            if (methodInfo != null)
            {
                var independentAttr = methodInfo.GetCustomAttribute<IndependentSubmitAttribute>();
                if (independentAttr != null)
                {
                    _independentMethods.TryAdd(cacheKey, true);
                    return true;
                }
            }

            // 检查类特性
            var classAttr = requestType.GetCustomAttribute<IndependentSubmitAttribute>();
            if (classAttr != null)
            {
                _independentMethods.TryAdd(cacheKey, true);
                return true;
            }

            _independentMethods.TryAdd(cacheKey, false);
            return false;
        }

        /// <summary>
        /// 获取事务特性
        /// </summary>
        private TransactionalAttribute? GetTransactionalAttribute(TRequest request)
        {
            var requestType = typeof(TRequest);

            // 先检查类特性
            var classAttr = requestType.GetCustomAttribute<TransactionalAttribute>();
            if (classAttr != null)
            {
                return classAttr;
            }

            return null;
        }

        /// <summary>
        /// 检查是否要求新事务
        /// </summary>
        private bool HasRequireNewTransaction(TRequest request)
        {
            var requestType = typeof(TRequest);

            // 检查类特性
            var classAttr = requestType.GetCustomAttribute<RequireNewTransactionAttribute>();
            if (classAttr != null)
            {
                return true;
            }

            // 检查方法特性
            var methodInfo = requestType.GetMethod("Handle");
            if (methodInfo != null)
            {
                var methodAttr = methodInfo.GetCustomAttribute<RequireNewTransactionAttribute>();
                if (methodAttr != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 转换隔离级别
        /// </summary>
        private System.Data.IsolationLevel ConvertIsolationLevel(Application.Attributes.IsolationLevel level)
        {
            return level switch
            {
                Application.Attributes.IsolationLevel.Unspecified => System.Data.IsolationLevel.Unspecified,
                Application.Attributes.IsolationLevel.ReadUncommitted => System.Data.IsolationLevel.ReadUncommitted,
                Application.Attributes.IsolationLevel.ReadCommitted => System.Data.IsolationLevel.ReadCommitted,
                Application.Attributes.IsolationLevel.RepeatableRead => System.Data.IsolationLevel.RepeatableRead,
                Application.Attributes.IsolationLevel.Serializable => System.Data.IsolationLevel.Serializable,
                Application.Attributes.IsolationLevel.Snapshot => System.Data.IsolationLevel.Snapshot,
                _ => System.Data.IsolationLevel.ReadCommitted
            };
        }
    }
}
