using System.Data;
using IsolationLevel = Ptcent.Cloud.Drive.Application.Attributes.IsolationLevel;

namespace Ptcent.Cloud.Drive.Application.Interfaces
{
    /// <summary>
    /// 数据库事务接口
    /// 用于保持 Application 层不依赖 EF Core
    /// </summary>
    public interface IDbTransaction : IDisposable
    {
        /// <summary>
        /// 提交事务
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 回滚事务
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 工作单元接口
    /// 用于手动控制事务边界
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="isolationLevel">隔离级别</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task<IDbTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 提交当前事务
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 回滚当前事务
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 保存更改
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 独立提交 - 立即提交，不受外层事务影响
        /// </summary>
        Task<int> IndependentSaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 是否有活动事务
        /// </summary>
        bool HasActiveTransaction { get; }
    }
}
