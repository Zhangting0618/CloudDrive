using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Attributes;

namespace Ptcent.Cloud.Drive.Infrastructure.Persistence
{
    /// <summary>
    /// 数据库事务实现（包装 EF Core 的 IDbContextTransaction）
    /// </summary>
    internal class DbTransaction : Application.Interfaces.IDbTransaction
    {
        private readonly IDbContextTransaction _innerTransaction;

        public DbTransaction(IDbContextTransaction innerTransaction)
        {
            _innerTransaction = innerTransaction;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _innerTransaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _innerTransaction.RollbackAsync(cancellationToken);
        }

        public void Dispose()
        {
            _innerTransaction.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return _innerTransaction.DisposeAsync();
        }
    }

    /// <summary>
    /// 工作单元实现
    /// 用于手动控制事务边界
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _currentTransaction;
        private bool _disposed;

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public bool HasActiveTransaction => _currentTransaction != null;

        /// <inheritdoc/>
        public async Task<Application.Interfaces.IDbTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("已存在活动事务，请先提交或回滚当前事务");
            }

            var sqlIsolationLevel = ConvertIsolationLevel(isolationLevel);
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(sqlIsolationLevel, cancellationToken);
            return new DbTransaction(_currentTransaction);
        }

        /// <summary>
        /// 转换隔离级别
        /// </summary>
        private static System.Data.IsolationLevel ConvertIsolationLevel(IsolationLevel level)
        {
            return level switch
            {
                IsolationLevel.Unspecified => System.Data.IsolationLevel.Unspecified,
                IsolationLevel.ReadUncommitted => System.Data.IsolationLevel.ReadUncommitted,
                IsolationLevel.ReadCommitted => System.Data.IsolationLevel.ReadCommitted,
                IsolationLevel.RepeatableRead => System.Data.IsolationLevel.RepeatableRead,
                IsolationLevel.Serializable => System.Data.IsolationLevel.Serializable,
                IsolationLevel.Snapshot => System.Data.IsolationLevel.Snapshot,
                _ => System.Data.IsolationLevel.ReadCommitted
            };
        }

        /// <inheritdoc/>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("没有活动的事务可以提交");
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <inheritdoc/>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("没有活动的事务可以回滚");
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 独立提交 - 立即保存并提交，不受外层事务影响
        /// 用于日志、审计等需要保证数据不丢失的场景
        /// </summary>
        public async Task<int> IndependentSaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                // 创建一个新的事务来提交独立数据
                using var independentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await _dbContext.SaveChangesAsync(cancellationToken);
                    await independentTransaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await independentTransaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else
            {
                // 没有事务，直接保存
                return await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _currentTransaction?.Dispose();
                    _dbContext?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
