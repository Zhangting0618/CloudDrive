using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Shared.Util
{
    public static class FileExtensionUtil
    {
        /// <summary>
        /// 将文件字节大小 转换成对应的可读大小
        /// </summary>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public static string GetFileSize(long fileSize)
        {
            if (fileSize > Math.Pow(1024, 3))
            {
                return Math.Round(fileSize / Math.Pow(1024, 3), 1) + "G";
            }
            if (fileSize > Math.Pow(1024, 2))
            {
                return Math.Round(fileSize / Math.Pow(1024, 2), 1) + "M";
            }
            if (fileSize > 1024)
            {
                return Math.Round((double)fileSize / 1024, 1) + "KB";
            }
            if (fileSize == 0)
                return "0B";
            return fileSize + "B";
        }
    }
}
