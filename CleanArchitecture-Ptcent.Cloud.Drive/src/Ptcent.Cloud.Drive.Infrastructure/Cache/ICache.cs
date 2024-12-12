using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Infrastructure.Cache
{
    public interface ICache
    {
        /// <summary>
        /// 缓存过期时间
        /// </summary>
        int TimeOut { set; get; }
        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        object Get(string key);
        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        T Get<T>(string key);
        T GetToBuff<T>(string key);
        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        void Remove(string key);
        /// <summary>
        /// 清空所有缓存对象
        /// </summary>
        //void Clear();
        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        void Insert(string key, object data);
        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        void Insert<T>(string key, T data);
        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间(秒钟)</param>
        void Insert(string key, object data, TimeSpan cacheTime);

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间(秒钟)</param>
        void Insert<T>(string key, T data, TimeSpan cacheTime);
        void InsertToBuff<T>(string key, T data, TimeSpan cacheTime);
        /// <summary>
        /// 判断key是否存在
        /// </summary>
        bool Exists(string key);
        long StringIncrement(string key, long value);

        bool SetExpire(string key, TimeSpan ts);

        /// <summary>
        /// set list 元素不会重复
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetAdd(string key, string value);

        long SetLength(string key);
        long SetAdd<T>(string key, List<T> values);
    }
}
