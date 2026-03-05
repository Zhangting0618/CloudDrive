using System.Linq.Expressions;

namespace Ptcent.Cloud.Drive.Application.Interfaces
{
    /// <summary>
    /// 通用仓储接口
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<bool> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> AddBatchAsync(List<T> entities, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> UpdateBatchAsync(Expression<Func<T, bool>> where, Action<T> updateAction, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<int> DeleteBatchAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        Task<IQueryable<T>> QueryAsync(CancellationToken cancellationToken = default);
        Task<IQueryable<T>> QueryAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);
        Task<(List<T> Items, int Total)> GetPagingAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> where, Dictionary<string, bool>? orderBys = null, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? where = null, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);
    }
}
