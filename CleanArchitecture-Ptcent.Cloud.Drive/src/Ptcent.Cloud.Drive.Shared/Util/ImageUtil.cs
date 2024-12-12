using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Drawing;

namespace Ptcent.Cloud.Drive.Shared.Util
{
    /// <summary>
    /// 图片工具类
    /// </summary>
    public static class ImageUtil
    {
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="stream">图片文件保存的字节数组</param>
        /// <param name="width">缩略图长度</param>
        /// <param name="height">缩略图宽度</param>
        /// <param name="path">保存路径</param>
        public static byte[] CreateThumbnail(Stream stream, int width, int height, string path)
        {
            using (SixLabors.ImageSharp.Image imageFrom = SixLabors.ImageSharp.Image.Load(stream))
            {
                if (imageFrom.Width <= width && imageFrom.Height <= height)
                {
                    // 如果源图的大小没有超过缩略图指定的大小，则直接把源图复制到目标文件
                    using (var memoryStream = new MemoryStream())
                    {
                        // 保存图像到内存流中
                        imageFrom.Save(memoryStream, new JpegEncoder());
                        //将内存流转换为字节数组
                        return memoryStream.ToArray();
                    }
                }
                else
                {
                    // 源图宽度及高度
                    int imageFromWidth = imageFrom.Width;
                    int imageFromHeight = imageFrom.Height;
                    float scale = height / (float)imageFromHeight;
                    if ((width / (float)imageFromWidth) < scale)
                        scale = width / (float)imageFromWidth;
                    width = (int)(imageFromWidth * scale);
                    height = (int)(imageFromHeight * scale);
                    imageFrom.Mutate(x => x.Resize(width, height));
                    using (var memoryStream = new MemoryStream())
                    {
                        imageFrom.Save(memoryStream, new JpegEncoder());
                        //将内存流转换为字节数组
                        return memoryStream.ToArray();
                    }
                }
            }
        }
        /// <summary>
        /// 图片转base64字符串
        /// </summary>
        /// <param name="stream">图片流</param>

        public static string ImgToBase64String(Stream stream)
        {
            string strbaser64 = string.Empty;
            using (Bitmap bmp = new Bitmap(stream))
            {
                using (var memoryStream = new MemoryStream())
                {
                    bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] arr = new byte[memoryStream.Length];
                    memoryStream.Position = 0;
                    memoryStream.Read(arr, 0, (int)memoryStream.Length);
                    strbaser64 = Convert.ToBase64String(arr);
                }
            }
            return strbaser64;
        }
    }
}
