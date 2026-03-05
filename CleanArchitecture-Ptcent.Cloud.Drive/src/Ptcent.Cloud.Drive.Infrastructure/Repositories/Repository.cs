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
    }
}
