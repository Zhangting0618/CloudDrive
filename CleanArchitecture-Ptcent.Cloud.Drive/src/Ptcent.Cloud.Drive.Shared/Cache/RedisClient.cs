using Microsoft.Extensions.Caching.StackExchangeRedis;
using Ptcent.Cloud.Drive.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Infrastructure.Cache
{
    /// <summary>
    /// 缓存
    /// </summary>
    public static class RedisClient
    {
        private static object CacheLocker = new(); //缓存锁对象
        private static ICache _cache = null; //缓存接口
        //private static string IsUserAliRedis = "F";
        private static object _objLock = new(); //缓存锁对象

        static RedisClient()
        {
            Load();
        }

        /// <summary>
        /// 加载缓存
        /// </summary>
        private static void Load()
        {
            try
            {
                lock (_objLock)
                {
                    if (_cache == null)
                    {
                        //if (IsUserAliRedis == "T")
                        {
                            _cache = new RedisCache();
                            if (_cache == null)
                            {
                                LogUtil.Info("redis链接异常");
                                throw new Exception("redis  链接异常");
                            }
                        }
                        //else
                        //{
                        //    //windows
                        //    //_cache = new RedisExc();
                        //    //if (_cache == null)
                        //    //{
                        //    //    Log.Info("redis _cache 链接异常999999");
                        //    //    throw new Exception("redis _cache 链接异常");
                        //    //}
                        //}
                    }
                }

            }
            catch (Exception ex)
            {
                //Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ICache GetCache()
        {
            return _cache;
        }


        /// <summary>
        /// 缓存过期时间
        /// </summary>
        public static int TimeOut
        {
            get { return _cache.TimeOut; }
            set
            {
                lock (CacheLocker)
                {
                    _cache.TimeOut = value;
                }
            }
        }

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public static object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            if (_cache == null)
            {
                Load();
            }
            return RetryUtil.DoRetry<object>(() => _cache.Get(key));
            //  return _cache.Get(key);
        }

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public static T Get<T>(string key)
        {
            if (_cache == null)
            {
                Load();
            }
            return RetryUtil.DoRetry<T>(() => _cache.Get<T>(key));
            // return _cache.Get<T>(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetToBuff<T>(string key)
        {
            if (_cache == null)
            {
                Load();
            }
            return RetryUtil.DoRetry<T>(() => _cache.GetToBuff<T>(key));
            //return _cache.GetToBuff<T>(key);
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        public static void Insert(string key, object data)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null)
                return;
            if (_cache == null)
            {
                Load();
            }
            RetryUtil.DoRetry(() => _cache.Insert(key, data));
            //_cache.Insert(key, data);
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        public static void Insert<T>(string key, T data)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null)
                return;
            if (_cache == null)
            {
                Load();
            }
            RetryUtil.DoRetry(() => _cache.Insert<T>(key, data));
            // _cache.Insert<T>(key, data);
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间(分钟)</param>
        public static void Insert(string key, object data, TimeSpan cacheTime)
        {
            if (!string.IsNullOrWhiteSpace(key) && data != null)
            {
                if (_cache == null)
                {
                    Load();
                }
                RetryUtil.DoRetry(() => _cache.Insert(key, data, cacheTime));
                // _cache.Insert(key, data, cacheTime);
            }
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中，并指定过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="data">缓存值</param>
        /// <param name="cacheTime">缓存过期时间(分钟)</param>
        public static void Insert<T>(string key, T data, TimeSpan cacheTime)
        {
            if (!string.IsNullOrWhiteSpace(key) && data != null)
            {
                if (_cache == null)
                {
                    Load();
                }
                RetryUtil.DoRetry(() => _cache.Insert<T>(key, data, cacheTime));
                //_cache.Insert<T>(key, data, cacheTime);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        public static void InsertToBuff<T>(string key, T data, TimeSpan cacheTime)
        {
            if (!string.IsNullOrWhiteSpace(key) && data != null)
            {
                if (_cache == null)
                {
                    Load();
                }
                RetryUtil.DoRetry(() => _cache.InsertToBuff<T>(key, data, cacheTime));
                //_cache.InsertToBuff<T>(key, data, cacheTime);
            }
        }

        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        public static void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;
            if (_cache == null)
            {
                Load();
            }
            RetryUtil.DoRetry(() => _cache.Remove(key));
            //cache.Remove(key);
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        public static bool Exists(string key)
        {
            if (_cache == null)
            {
                Load();
            }
            return RetryUtil.DoRetry(() => _cache.Exists(key));
            //return _cache.Exists(key);
        }

        /// <summary>
        /// 自增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long Increment(string key, long value)
        {
            if (_cache == null)
            {
                Load();
            }
            return RetryUtil.DoRetry(() => _cache.StringIncrement(key, value));
        }
        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static bool SetExpire(string key, TimeSpan ts)
        {
            if (_cache == null)
            {
                Load();
            }
            return RetryUtil.DoRetry<bool>(() => _cache.SetExpire(key, ts));
            //return Retry.DoRetry<bool>(()=>  _cache.SetExpire(key,ts));
        }

        public static bool SetAdd(string key, string value)
        {
            if (_cache == null)
            {
                Load();
            }
            return RetryUtil.DoRetry<bool>(() => _cache.SetAdd(key, value));
        }

        public static long SetLength(string key)
        {
            if (_cache == null)
            {
                Load();
            }
            return RetryUtil.DoRetry<long>(() => _cache.SetLength(key));
        }
    }
}
