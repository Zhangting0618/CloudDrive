using Newtonsoft.Json;
using Ptcent.Cloud.Drive.Shared.Util;
using Ptcent.Cloud.Drive.Shared.Extensions;
using StackExchange.Redis;
using System.Collections;
using ProtoBuf;

namespace Ptcent.Cloud.Drive.Infrastructure.Cache
{
    /// <summary>
    /// Redis
    /// </summary>
    public class RedisCache : ICache
    {
        private readonly static string RedisHost = ConfigUtil.GetValue("Redis:RedisHost");//"192.168.0.92";
        private readonly static int RedisPort = ConfigUtil.GetValue("Redis:RedisPort").ToInt();
        private readonly static string RedisPassword = ConfigUtil.GetValue("Redis:RedisPassword");
        private readonly static ConfigurationOptions ConfigurationOptions = ConfigurationOptions.Parse(RedisHost + ":" + RedisPort + ",password=" + RedisPassword + ",connectTimeout=2000");
        private readonly static object Locker = new();
        private static ConnectionMultiplexer RedisConn;
        private readonly IDatabase DB;

        private readonly JsonSerializerSettings jsonConfig = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public RedisCache()
        {
            RedisConn = GetRedisConn();
            DB = RedisConn.GetDatabase();

        }

        /// <summary>
        /// 连接超时设置
        /// </summary>
        public int TimeOut { get; set; } = 5;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            try
            {
                return Get<object>(key);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Get333----" + key, ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            try
            {
                var value = default(T);
                if (DB.KeyExists(key))
                {
                    //var begin = DateTime.Now;
                    var cacheValue = DB.StringGet(key);
                    //var endCache = DateTime.Now;
                    if (!cacheValue.IsNull)
                    {
                        var cacheObject = JsonConvert.DeserializeObject<T>(cacheValue, jsonConfig);
                        //if (!cacheObject.ForceOutofDate)
                        //    DB.KeyExpire(key, new TimeSpan(0, 0, cacheObject.ExpireTime));
                        value = cacheObject;
                    }
                }
                return value;
            }
            catch (Exception ex)
            {
                LogUtil.Error("ErrorRedisGet1ee1----" + key, ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetToBuff<T>(string key)
        {
            try
            {
                var value = default(T);
                if (DB.KeyExists(key))
                {
                    var cacheValue = DB.StringGet(key);
                    if (!cacheValue.IsNull)
                        value = GetByte<T>(cacheValue);
                }
                return value;
            }
            catch (Exception ex)
            {
                LogUtil.Error("ErrorRedisGetToBuffT11---" + key, ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Insert(string key, object data)
        {
            try
            {
                var jsonData = GetJsonData(data, TimeOut, false);
                DB.StringSet(key, jsonData);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Insert---" + key, ex.ToString());

                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        public void Insert<T>(string key, T data, TimeSpan cacheTime)
        {
            try
            {
                var jsonData = GetJsonData(data, TimeOut, true);
                DB.StringSet(key, jsonData, cacheTime);
            }
            catch (Exception ex)
            {
                LogUtil.Error("ErrorRedisInsert---" + key, ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        public void InsertToBuff<T>(string key, T data, TimeSpan cacheTime)
        {
            try
            {
                var b = GetBuff(data);
                DB.StringSet(key, b, cacheTime);
            }
            catch (Exception ex)
            {
                LogUtil.Error("ErrorRedisInsertToBuff---" + key, ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        public void Insert(string key, object data, TimeSpan cacheTime)
        {
            try
            {
                var jsonData = GetJsonData(data, TimeOut, true);
                DB.StringSet(key, jsonData, cacheTime);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Insert1122--" + key, ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Insert<T>(string key, T data)
        {
            try
            {
                var jsonData = GetJsonData(data, TimeOut, false);
                DB.StringSet(key, jsonData);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Insert11--" + key, ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            try
            {
                if (DB.KeyExists(key))
                    DB.KeyDelete(key, CommandFlags.HighPriority);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Remove--" + key, ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        public bool Exists(string key)
        {
            try
            {
                return DB.KeyExists(key);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Exists--" + key, ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 自增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long StringIncrement(string key, long value)
        {
            try
            {
                return DB.StringIncrement(key, value);
            }
            catch (Exception ex)
            {
                LogUtil.Error("Exists--" + key, ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public bool SetExpire(string key, TimeSpan ts)
        {
            try
            {
                return DB.KeyExpire(key, ts);
            }
            catch (Exception ex)
            {
                LogUtil.Error("SetExpire--" + key, ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ConnectionMultiplexer GetRedisConn()
        {
            if (RedisConn == null)
                lock (Locker)
                {
                    if (RedisConn == null || !RedisConn.IsConnected)
                    {
                        if (RedisConn != null)
                            RedisConn.Close();
                        ConfigurationOptions.SyncTimeout = 10000;
                        ConfigurationOptions.ConnectRetry = 3;
                        RedisConn = ConnectionMultiplexer.Connect(ConfigurationOptions);
                    }
                }
            return RedisConn;
        }

        /// <summary>
        ///     获取多个Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">Redis Key集合</param>
        /// <returns></returns>
        public T GetBathBuff<T>(List<string> keys) where T : IList, new()
        {
            //  CacheClientRedis.GetToBuff<List<NewMissEntity>>(key)
            var t = new T();

            try
            {
                var values = DB.StringGet(ConvertRedisKeys(keys));
                if (values == null || !values.Any())
                    return t;
                //if (!cacheValue.IsNull)
                //{
                //    //var cacheObject = JsonConvert.DeserializeObject<T>(cacheValue, jsonConfig);
                //    //if (!cacheObject.ForceOutofDate)
                //    //    DB.KeyExpire(key, new TimeSpan(0, 0, cacheObject.ExpireTime));
                //    // byte[] b = GetBuff<RedisValue>(cacheValue);

                //    value = GetByte<T>(cacheValue); //cacheObject;
                //}

                foreach (var value in values)
                {
                    var vals = GetByte<T>(value);
                    //t.Add(value);
                    foreach (var val in vals)
                        t.Add(val);
                }
                return t;

                // return values.Where(n=>!n.IsNull).Select(n =>GetByte<T>(n)).ToList();
            }
            catch (Exception ex)
            {
                LogUtil.Error("GetBathBuff", ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T[] GetBathBuffEntity<T>(List<string> keys)
        {
            try
            {
                var values = DB.StringGet(ConvertRedisKeys(keys));
                var t = new T[values.Length];
                if (!values.Any())
                    return default(T[]);
                // var val = GetByte<T>(values);
                //if (!cacheValue.IsNull)
                //{
                //    //var cacheObject = JsonConvert.DeserializeObject<T>(cacheValue, jsonConfig);
                //    //if (!cacheObject.ForceOutofDate)
                //    //    DB.KeyExpire(key, new TimeSpan(0, 0, cacheObject.ExpireTime));
                //    // byte[] b = GetBuff<RedisValue>(cacheValue);

                //    value = GetByte<T>(cacheValue); //cacheObject;
                //}
                var i = 0;
                foreach (var value in values)
                {
                    if (value.IsNull)
                        continue;
                    var val = GetByte<T>(value);
                    t[i] = val;
                    i++;
                }

                return t;

                // return values.Where(n=>!n.IsNull).Select(n =>GetByte<T>(n)).ToList();
            }
            catch (Exception ex)
            {
                LogUtil.Error("GetBathBuff", ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        private RedisKey[] ConvertRedisKeys(List<string> redisKeys)
        {
            try
            {
                return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
            }
            catch (Exception ex)
            {
                LogUtil.Error("ConvertRedisKeys--", ex.Message);
                throw ex;
            }
        }

        //public RedisValue[] StringGet(List<string> listKey)
        //{
        //    List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
        //    return Do(DB => DB.StringGet(ConvertRedisKeys(newKeys)));
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        private static byte[] GetBuff<T>(T value)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, value);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error("ErrorRedisGetBuffB", ex.ToString());
                throw ex;
                //Log.Error("CRedisManager GetBuff", ex.Message);
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resultBytes"></param>
        /// <returns></returns>
        private T GetByte<T>(byte[] resultBytes)
        {
            T objValue;
            try
            {
                using (var ms = new MemoryStream(resultBytes))
                {
                    objValue = Serializer.Deserialize<T>(ms);
                }
                return objValue;
            }
            catch (Exception ex)
            {
                LogUtil.Error("ErrorRedisGetByte", ex.ToString());
                throw ex;
                //Log.Error("GetByte", ex.Message);
                return default(T);
            }
        }
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keysStr"></param>
        /// <param name="valuesStr"></param>
        public void InsertToGroup<T>(string[] keysStr, T[] valuesStr)
        {
            try
            {
                var count = keysStr.Length;
                var keyValuePair = new KeyValuePair<RedisKey, RedisValue>[count];
                for (var i = 0; i < count; i++)
                {
                    var b = GetBuff(valuesStr[i]);
                    keyValuePair[i] = new KeyValuePair<RedisKey, RedisValue>(keysStr[i], b);
                }
                DB.StringSet(keyValuePair);
            }
            catch (Exception ex)
            {
                LogUtil.Error("EXCRedisManager setData", ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 同list 不会出现重复的元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetAdd(string key, string value)
        {
            return DB.SetAdd(key, value);
        }
        /// <summary>
        /// 得到sortset元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SetLength(string key)
        {
            return DB.SetLength(key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public long SetAdd<T>(string key, List<T> values)
        {
            RedisValue[] redisValues = new RedisValue[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                redisValues[0] = GetBuff(values[i]);
            }
            return DB.SetAdd(key, redisValues);
        }

        private string GetJsonData(object data, int cacheTime, bool forceOutOfDate)
        {
            try
            {
                var cacheObject = new CacheObject<object>
                {
                    Value = data,
                    ExpireTime = cacheTime,
                    ForceOutofDate = forceOutOfDate
                };
                return JsonConvert.SerializeObject(cacheObject, jsonConfig); //序列化对象
            }
            catch (Exception ex)
            {
                LogUtil.Error("GetJsonData--", ex.Message);
                throw ex;
            }
        }

        private string GetJsonData<T>(T data, int cacheTime, bool forceOutOfDate)
        {
            try
            {
                var cacheObject = data;
                // new CacheObject<T>() { Value = data, ExpireTime = cacheTime, ForceOutofDate = forceOutOfDate };
                return JsonConvert.SerializeObject(cacheObject, jsonConfig); //序列化对象
            }
            catch (Exception ex)
            {
                LogUtil.Error("GetJsonData--", ex.Message);
                throw ex;
            }
        }



        private class CacheObject<T>
        {
            public int ExpireTime { get; set; }
            public bool ForceOutofDate { get; set; }
            public T Value { get; set; }
        }
    }
}
