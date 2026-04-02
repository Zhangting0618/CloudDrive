using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;
using System.Data;

namespace Ptcent.Cloud.Drive.Infrastructure.Persistence.Behaviors
{
    /// <summary>
    /// 自动事务管理管道行为
    /// 无需标记特性，自动为所有 Command 提供事务支持
    /// </summary>
    public class AutomaticTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AutomaticTransactionBehavior<TRequest, TResponse>> _logger;

        public AutomaticTransactionBehavior(
            AppDbContext dbContext,
            ILogger<AutomaticTransactionBehavior<TRequest, TResponse>> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // 如果已有活动事务，直接执行（支持嵌套）
            if (_dbContext.Database.CurrentTransaction != null)
            {
                return await next();
            }

            IDbContextTransaction? transaction = null;

            try
            {
                _logger.LogDebug("开始自动事务处理 {RequestType}", typeof(TRequest).Name);

                // 开始事务
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted,
                    cancellationToken);

                // 执行请求
                var response = await next();

                // 提交事务
                await transaction.CommitAsync(cancellationToken);
                _logger.LogDebug("事务提交成功 {RequestType}", typeof(TRequest).Name);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "事务执行失败，回滚 {RequestType}: {Message}", typeof(TRequest).Name, ex.Message);

                // 回滚事务
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                // 重新抛出异常
                throw;
            }
            finally
            {
                // 释放资源
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }
    }
}
