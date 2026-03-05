using System.Linq.Expressions;

namespace Ptcent.Cloud.Drive.Application.Interfaces
{
    /// <summary>
    /// 通用仓储接口
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // 新增
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

        // 从 IBaseRepository 合并过来的方法
        Task<bool> Add(T model);
        Task<int> AddBatch(List<T> list);
        Task<bool> Update(T model, params Expression<Func<T, object>>[] updatedProperties);
        Task<bool> Delete(T model);
        Task<int> DeleteBatch(Expression<Func<T, bool>> whereLambda);
        Task<T> GetById(params object[] keyValues);
        Task<IQueryable<T>> GetList();
        Task<IQueryable<T>> GetList(Expression<Func<T, bool>> whereLambda);
        IQueryable<T> GetPaging(int pageIndex, int pageSize, out int total, Expression<Func<T, bool>> whereLambda, Dictionary<string, bool> orderBys);
        Task<bool> UpdateAsync(List<T> entitys, bool isSaveChange);
        Task<bool> DeleteAsync(List<T> entitys, bool isSaveChange);
        Task<List<T>> Distinct(Expression<Func<T, bool>> @where);
        Task<bool> Any(Expression<Func<T, bool>> @where);
        Task<IQueryable<T>> WhereAsync(Expression<Func<T, bool>> @where);
        Task<IQueryable<T>> WhereAsync();
    }
}
