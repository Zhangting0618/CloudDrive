using System.Text.RegularExpressions;

namespace Ptcent.Cloud.Drive.Shared.Extensions
{
    /// <summary>
    /// 通用扩展方法
    /// </summary>
    public static partial class CommonExtension
    {
        /// <summary>
        /// 判断是否为空
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 判断是否为邮箱格式
        /// </summary>
        public static bool IsEmail(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return EmailRegex().IsMatch(value);
        }

        /// <summary>
        /// 判断是否为手机号格式（中国大陆）
        /// </summary>
        public static bool IsMobile(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return MobileRegex().IsMatch(value);
        }

        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"^1[3-9]\d{9}$")]
        private static partial Regex MobileRegex();
    }
}
