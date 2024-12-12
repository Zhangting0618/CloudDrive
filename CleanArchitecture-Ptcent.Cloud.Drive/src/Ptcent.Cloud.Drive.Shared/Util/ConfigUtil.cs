using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Shared.Util
{
    public static class ConfigUtil
    {
        private static IConfiguration config;
        static ConfigUtil()
        {
            var builder = new ConfigurationBuilder();//创建config的builder
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("Configs/appsettings.json");//设置配置文件所在的路径加载配置文件信息
            config = builder.Build();
        }

        /// <summary>
        /// 根据key获取对应的配置值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            return config[key] ?? "";
        }

        /// <summary>
        /// 获取ConnectionStrings下默认的配置连接字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConnectionString(string key)
        {
            return config.GetConnectionString(key);
        }

    }
}
