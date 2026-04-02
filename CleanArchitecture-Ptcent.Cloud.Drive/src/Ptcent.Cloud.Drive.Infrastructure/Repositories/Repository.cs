using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Ptcent.Cloud.Drive.Infrastructure.Repositories
{
    /// <summary>
    /// 通用仓储实现
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<bool> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

        public virtual async Task<int> AddBatchAsync(List<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

        public virtual async Task<int> UpdateBatchAsync(Expression<Func<T, bool>> where, Action<T> updateAction, CancellationToken cancellationToken = default)
        {
            var entities = await _dbSet.Where(where).ToListAsync(cancellationToken);
            foreach (var entity in entities)
            {
                updateAction(entity);
            }
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

        public virtual async Task<int> DeleteBatchAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            var entities = await _dbSet.Where(where).ToListAsync(cancellationToken);
            _dbSet.RemoveRange(entities);
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new[] { id }, cancellationToken);
        }

        public virtual Task<IQueryable<T>> QueryAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.AsNoTracking());
        }

        public virtual Task<IQueryable<T>> QueryAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.AsNoTracking().Where(where));
        }

        public virtual async Task<(List<T> Items, int Total)> GetPagingAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<T, bool>> where,
            Dictionary<string, bool>? orderBys = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().Where(where);

            if (orderBys != null && orderBys.Count > 0)
            {
                var parameter = Expression.Parameter(typeof(T));
                var currentQuery = query.Expression;

                for (int i = 0; i < orderBys.Count; i++)
                {
                    var kvp = orderBys.ElementAt(i);
                    var property = typeof(T).GetProperty(kvp.Key);
                    if (property == null) continue;

                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);
                    string methodName = i == 0
                        ? (kvp.Value ? "OrderByDescending" : "OrderBy")
                        : (kvp.Value ? "ThenByDescending" : "ThenBy");

                    currentQuery = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new[] { typeof(T), property.PropertyType },
                        currentQuery,
                        Expression.Quote(orderByExp));
                }

                query = query.Provider.CreateQuery<T>(currentQuery);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return (items, total);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().AnyAsync(where, cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? where = null, CancellationToken cancellationToken = default)
        {
            return where == null
                ? await _dbSet.AsNoTracking().CountAsync(cancellationToken)
                : await _dbSet.AsNoTracking().CountAsync(where, cancellationToken);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(where, cancellationToken);
        }

        // IRepository<T> 其他方法实现
        public virtual Task<bool> Add(T model)
        {
            _dbSet.Add(model);
            return Task.FromResult(_context.SaveChanges() > 0);
        }

        public virtual Task<int> AddBatch(List<T> list)
        {
            _dbSet.AddRange(list);
            return Task.FromResult(_context.SaveChanges());
        }

        public virtual Task<bool> Update(T model, params Expression<Func<T, object>>[] updatedProperties)
        {
            _dbSet.Attach(model);
            foreach (var property in updatedProperties)
            {
                _context.Entry(model).Property(property).IsModified = true;
            }
            return Task.FromResult(_context.SaveChanges() > 0);
        }

        public virtual Task<bool> Delete(T model)
        {
            _dbSet.Remove(model);
            return Task.FromResult(_context.SaveChanges() > 0);
        }

        public virtual Task<int> DeleteBatch(Expression<Func<T, bool>> whereLambda)
        {
            var entities = _dbSet.Where(whereLambda).ToList();
            _dbSet.RemoveRange(entities);
            return Task.FromResult(_context.SaveChanges());
        }

        public virtual Task<T> GetById(params object[] keyValues)
        {
            return Task.FromResult(_dbSet.Find(keyValues)!);
        }

        public virtual Task<IQueryable<T>> GetList()
        {
            return Task.FromResult(_dbSet.AsNoTracking().AsQueryable());
        }

        public virtual Task<IQueryable<T>> GetList(Expression<Func<T, bool>> whereLambda)
        {
            return Task.FromResult(_dbSet.AsNoTracking().Where(whereLambda).AsQueryable());
        }

        public virtual IQueryable<T> GetPaging(int pageIndex, int pageSize, out int total, Expression<Func<T, bool>> whereLambda, Dictionary<string, bool> orderBys)
        {
            var query = _dbSet.AsNoTracking().Where(whereLambda);
            total = query.Count();

            if (orderBys != null && orderBys.Count > 0)
            {
                var parameter = Expression.Parameter(typeof(T));
                var currentQuery = query.Expression;

                for (int i = 0; i < orderBys.Count; i++)
                {
                    var kvp = orderBys.ElementAt(i);
                    var property = typeof(T).GetProperty(kvp.Key);
                    if (property == null) continue;

                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);
                    string methodName = i == 0
                        ? (kvp.Value ? "OrderByDescending" : "OrderBy")
                        : (kvp.Value ? "ThenByDescending" : "ThenBy");

                    currentQuery = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new[] { typeof(T), property.PropertyType },
                        currentQuery,
                        Expression.Quote(orderByExp));
                }

                query = query.Provider.CreateQuery<T>(currentQuery);
            }

            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public virtual Task<bool> UpdateAsync(List<T> entitys, bool isSaveChange)
        {
            foreach (var entity in entitys)
            {
                _dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            }
            return isSaveChange
                ? Task.FromResult(_context.SaveChanges() > 0)
                : Task.FromResult(true);
        }

        public virtual Task<bool> DeleteAsync(List<T> entitys, bool isSaveChange)
        {
            _dbSet.RemoveRange(entitys);
            return isSaveChange
                ? Task.FromResult(_context.SaveChanges() > 0)
                : Task.FromResult(true);
        }

        public virtual Task<List<T>> Distinct(Expression<Func<T, bool>> @where)
        {
            return _dbSet.AsNoTracking().Where(@where).Distinct().ToListAsync();
        }

        public virtual Task<bool> Any(Expression<Func<T, bool>> @where)
        {
            return _dbSet.AsNoTracking().AnyAsync(@where);
        }

        public virtual Task<IQueryable<T>> WhereAsync(Expression<Func<T, bool>> @where)
        {
            return Task.FromResult(_dbSet.AsNoTracking().Where(@where).AsQueryable());
        }

        public virtual Task<IQueryable<T>> WhereAsync()
        {
            return Task.FromResult(_dbSet.AsNoTracking().AsQueryable());
        }

        /// <summary>
        /// 独立提交 - 新增实体并立即提交，不受外层事务影响
        /// 用于日志、审计等需要保证数据不丢失的场景
        /// </summary>
        public virtual async Task<bool> AddIndependentAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);

            // 如果当前有活动事务，使用独立提交方式
            if (_context.Database.CurrentTransaction != null)
            {
                // 创建一个新事务来提交这个操作
                using var independentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await _context.SaveChangesAsync(cancellationToken);
                    await independentTransaction.CommitAsync(cancellationToken);
                    return result > 0;
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
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
        }

        /// <summary>
        /// 独立提交 - 更新实体并立即提交，不受外层事务影响
        /// </summary>
        public virtual async Task<bool> UpdateIndependentAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            if (_context.Database.CurrentTransaction != null)
            {
                using var independentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await _context.SaveChangesAsync(cancellationToken);
                    await independentTransaction.CommitAsync(cancellationToken);
                    return result > 0;
                }
                catch
                {
                    await independentTransaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else
            {
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
        }

        /// <summary>
        /// 独立提交 - 删除实体并立即提交，不受外层事务影响
        /// </summary>
        public virtual async Task<bool> DeleteIndependentAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);

            if (_context.Database.CurrentTransaction != null)
            {
                using var independentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await _context.SaveChangesAsync(cancellationToken);
                    await independentTransaction.CommitAsync(cancellationToken);
                    return result > 0;
                }
                catch
                {
                    await independentTransaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else
            {
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
        }
    }
}
