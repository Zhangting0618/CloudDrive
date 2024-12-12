using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Infrastructure.IRespository
{
    public interface IBaseRepository<T>
    {
        //IQueryable<SysConfigEntity> GetSysConfigList(Expression<Func<SysConfigEntity, bool>> whereLambda);
        Task<bool> Add(T model);
        Task<int> AddBatch(List<T> list);
        Task<bool> Update(T model);
        // Task<int> UpdateBatch(Expression<Func<T, bool>> whereLambda, Expression<Func<T, T>> expression);
        Task<int> UpdateBatch(Expression<Func<T, bool>> whereLambda, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> expression);

        Task<bool> Delete(T model);
        Task<int> DeleteBatch(Expression<Func<T, bool>> whereLambda);
        Task<T> GetById(params object[] keyValues);
        Task<IQueryable<T>> GetList();
        Task<IQueryable<T>> GetList(Expression<Func<T, bool>> whereLambda);
        IQueryable<T> GetPaging(int pageIndex, int pageSize, out int total, Expression<Func<T, bool>> whereLambda, Dictionary<string, bool> orderBys);
        Task<bool> UpdateAsync(List<T> entitys, bool isSaveChange);
        Task<bool> DeleteAsync(List<T> entitys, bool isSaveChange);
        /// <summary>
        /// 返回第一条记录
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <returns></returns>
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> @where);
        /// <summary>
        /// 去重查询
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <returns></returns>
        Task<List<T>> Distinct(Expression<Func<T, bool>> @where);
        /// <summary>
        /// 是否满足条件
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <returns></returns>
        Task<bool> Any(Expression<Func<T, bool>> @where);
        /// <summary>
        /// 返回总条数 - 通过条件过滤
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<T, bool>> @where);
        /// <summary>
        /// 条件查询 (异步)
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <returns></returns>
        Task<IQueryable<T>> WhereAsync(Expression<Func<T, bool>> @where);
        /// <summary>
        /// 查询 不跟踪实体状态 不带条件
        /// </summary>
        /// <returns></returns>
        Task<IQueryable<T>> WhereAsync();
    }
}
