namespace Ptcent.Cloud.Drive.Shared.Extensions
{
    /// <summary>
    /// 数值扩展方法
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// 字符串转整数
        /// </summary>
        public static int ToInt(this string value)
        {
            if (int.TryParse(value, out var result))
                return result;
            return 0;
        }

        /// <summary>
        /// 字符串转长整数
        /// </summary>
        public static long ToLong(this string value)
        {
            if (long.TryParse(value, out var result))
                return result;
            return 0;
        }
    }
}
