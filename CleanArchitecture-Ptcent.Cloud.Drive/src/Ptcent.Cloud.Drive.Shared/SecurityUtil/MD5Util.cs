using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Shared.SecurityUtil
{
    public static class MD5Util
    {
        /// <summary>
        /// MD5盐加密
        /// </summary>
        /// <param name="input">密码</param>
        /// <param name="salt">盐</param>
        /// <returns></returns>
        public static string ComputeMD5(string input, string salt)
        {
            // 将输入字符串转换为字节数组
            byte[] inputBytes = Encoding.UTF8.GetBytes(input + salt);
            //创建MD对象
            using (MD5 md5 = MD5.Create())
            {
                // 计算哈希值
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // 将哈希值转换为字符串
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// 计算文件Hash值
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string CalculateFileHash(string filePath)
        {
            string hashValues = string.Empty;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Position = 0;
                    byte[] hashValue = mySHA256.ComputeHash(fileStream);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashValue)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    hashValues = sb.ToString();
                }
            }
            return hashValues;
        }

        public static string GetMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
