using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Infrastructure.Context;

namespace Ptcent.Cloud.Drive.Infrastructure.Respository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class, new()
    {
        private readonly DbSet<T> dbSet;
        private readonly EFDbContext db;
        public BaseRepository(EFDbContext db)
        {
            this.db = db;// new EFDbContext();
            dbSet = db.Set<T>();
        }

        public async virtual Task<bool> Add(T model)
        {
            db.Set<T>().Add(model);
            return await db.SaveChangesAsync() > 0;
        }

        public async virtual Task<int> AddBatch(List<T> list)
        {
            db.Set<T>().AddRange(list);
            return await db.SaveChangesAsync();
        }

        public async virtual Task<bool> Update(T model, params Expression<Func<T, object>>[] updatedProperties)
        {
            var dbSet = db.Set<T>();
            dbSet.Attach(model);

            if (updatedProperties.Length > 0)
            {
                foreach (var prop in updatedProperties)
                {
                    db.Entry(model).Property(prop).IsModified = true;
                }
            }
            else
            {
                db.Entry(model).State = EntityState.Modified;
            }

            return await db.SaveChangesAsync() > 0;
        }


        //public async virtual Task<int> UpdateBatch(Expression<Func<T, bool>> whereLambda, Expression<Func<T, T>> expression)
        public async virtual Task<int> UpdateBatch(Expression<Func<T, bool>> whereLambda, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression)
        {
            //Expression<Func<SetPropertyCalls<TSource>, SetPropertyCalls<TSource>>> setPropertyCalls
            //context.Persons
            //.Where(p => p.PersonId <= 1_000)
            //.ExecuteUpdate(p =>
            //     p.SetProperty(x => x.LastName, x => "Updated" + x.LastName)
            //      .SetProperty(x => x.FirstName, x => "Updated" + x.FirstName));

            return await db.Set<T>().Where(whereLambda).ExecuteUpdateAsync(expression);
            //return await db.Set<T>().Where(whereLambda).BatchUpdateAsync(expression);
        }

        public async virtual Task<bool> Delete(T model)
        {
            db.Set<T>().Attach(model);
            db.Set<T>().Remove(model);
            return await db.SaveChangesAsync() > 0;
        }

        public async virtual Task<int> DeleteBatch(Expression<Func<T, bool>> whereLambda)
        {
            //context.Persons
            //  .Where(p => p.PersonId <= 500)
            //  .ExecuteDelete()
            return await db.Set<T>().Where(whereLambda).ExecuteDeleteAsync();
            //return await db.Set<T>().Where(whereLambda).BatchDeleteAsync();
        }

        public async virtual Task<T> GetById(params object[] keyValues)
        {
            return await db.Set<T>().FindAsync(keyValues);
        }

        public async virtual Task<IQueryable<T>> GetList()
        {
            return db.Set<T>();
        }

        public async virtual Task<IQueryable<T>> GetList(Expression<Func<T, bool>> whereLambda)
        {
            return db.Set<T>().Where(whereLambda);
        }

        public virtual IQueryable<T> GetPaging(int pageIndex, int pageSize, out int total, Expression<Func<T, bool>> whereLambda, Dictionary<string, bool> orderBys)
        {
            var queryable = db.Set<T>().Where(whereLambda);
            var parameter = Expression.Parameter(typeof(T));
            if (orderBys != null && orderBys.Count > 0)
            {
                List<KeyValuePair<string, bool>> keyValues = orderBys.ToList();
                for (int i = 0; i < keyValues.Count; i++)
                {
                    var property = typeof(T).GetProperty(keyValues[i].Key);
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    var orderByExp = Expression.Lambda(propertyAccess, parameter);
                    string orderName = keyValues[i].Value ? "OrderByDescending" : "OrderBy";
                    if (i > 0)
                    {
                        orderName = keyValues[i].Value ? "ThenByDescending" : "ThenBy";
                    }
                    MethodCallExpression resultExp = Expression.Call(typeof(Queryable), orderName, new Type[] { typeof(T), property.PropertyType }, queryable.Expression, Expression.Quote(orderByExp));
                    queryable = queryable.Provider.CreateQuery<T>(resultExp);
                }
            }
            total = queryable.Count();
            queryable = queryable.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
            return queryable;
        }

        public async virtual Task<bool> UpdateAsync(List<T> entitys, bool isSaveChange)
        {

            if (entitys == null || entitys.Count == 0)
            {
                return false;
            }
            entitys.ForEach(c =>
            {
                dbSet.Attach(c);
                db.Entry<T>(c).State = EntityState.Modified;
            });
            if (isSaveChange)
            {
                return await db.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async virtual Task<bool> DeleteAsync(List<T> entitys, bool isSaveChange)
        {
            entitys.ForEach(entity =>
            {
                dbSet.Attach(entity);
                dbSet.Remove(entity);
            });
            return isSaveChange ? await db.SaveChangesAsync() > 0 : false;
        }
        public int Count(Expression<Func<T, bool>> @where)
        {
            return dbSet.AsNoTracking().Count(@where);
        }
        /// <summary>
        /// 返回第一条记录
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> @where)
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(@where);
        }
        /// <summary>
        /// 去重查询
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <returns></returns>
        public async Task<List<T>> Distinct(Expression<Func<T, bool>> @where)
        {
            return await db.Set<T>().AsNoTracking().Where(@where).Distinct().ToListAsync();
        }
        /// <summary>
        /// 是否满足条件
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <returns></returns>
        public async Task<bool> Any(Expression<Func<T, bool>> @where)
        {
            return await dbSet.AsNoTracking().AnyAsync(@where);
        }
        /// <summary>
        /// 返回总条数 - 通过条件过滤
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>> @where)
        {
            return await dbSet.AsNoTracking().Where(@where).CountAsync();
        }
        /// <summary>
        /// 查询不跟踪实体状态 更新插入不能用 只适用查询
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public async Task<IQueryable<T>> WhereAsync(Expression<Func<T, bool>> where)
        {
            return dbSet.AsNoTracking().Where(where);
        }
        /// <summary>
        /// 查询 不跟踪实体状态 不带条件
        /// </summary>
        /// <returns></returns>
        public async Task<IQueryable<T>> WhereAsync()
        {
            return dbSet.AsNoTracking();
            // return db.Set<T>().AsNoTracking();
        }
    }
}
