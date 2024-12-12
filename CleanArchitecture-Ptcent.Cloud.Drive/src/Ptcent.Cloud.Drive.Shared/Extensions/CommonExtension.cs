using Newtonsoft.Json;
using Ptcent.Cloud.Drive.Shared.Util;
using System.Text.RegularExpressions;
namespace Ptcent.Cloud.Drive.Shared.Extensions
{
    public static class CommonExtension
    {
        public static T ToEntity<T>(this string str)
        {
            JsonSerializerSettings setting = new()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            return JsonConvert.DeserializeObject<T>(str, setting);
        }
        /// <summary>
        /// 转换整形
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ToInt(this string str)
        {
            return Convert.ToInt32(str);
        }
        /// <summary>
        /// 转换长整型
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long ToLong(this string str)
        {
            return Convert.ToInt64(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 验证是否为手机号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsMobile(this string str)
        {
            var regstr = @"^(13[0-9]|14[0-9]|15[0-9]|16[0-9]|17[0-9]|18[0-9]|19[0-9])\d{8}$";
            var reg = new Regex(regstr);
            if (reg.IsMatch(str)) return true;
            return false;
        }
        /// <summary>
        /// 验证身份证
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsCard(this string str)
        {
            var regstr = @"(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)";
            var reg = new Regex(regstr);
            if (reg.IsMatch(str)) return true;
            return false;
        }

        public static bool IsEmail(this string str)
        {
            var regstr = @"([a-zA-Z]|[0-9])(\w|\-)+@[a-zA-Z0-9]+\.([a-zA-Z]{2,4})$";
            var reg = new Regex(regstr);
            if (reg.IsMatch(str)) return true;
            return false;
        }
        /// <summary>
        /// 验证是否为数字
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static bool IsNumber(this string str)
        {
            var regstr = @"^\d+$";
            var reg = new Regex(regstr);
            if (reg.IsMatch(str)) return true;
            return false;
        }

        /// <summary>
        /// 是否为日期型字符串
        /// </summary>
        /// <param name="strDate"></param>
        /// <returns></returns>
        public static bool IsDate(string strDate)
        {
            try
            {
                DateTime.Parse(strDate);
                return true;
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 集合是否有值 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNull<T>(this IEnumerable<T> list)
        {
            return list == null || !list.Any();
        }
        /// <summary>
        /// 判断文件类型
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static string JudgmentFileType(string fileType)
        {
            if (ConfigUtil.GetValue("FileType:Docs").Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(fileType))
            {
                return "Docs";
            }
            if (ConfigUtil.GetValue("FileType:Image").Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(fileType))
            {
                return "Image";
            }
            if (ConfigUtil.GetValue("FileType:Video").Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(fileType))
            {
                return "Video";
            }
            if (ConfigUtil.GetValue("FileType:Audio").Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(fileType))
            {
                return "Audio";
            }
            else
            {
                return "Other";
            }
        }
    }
}
