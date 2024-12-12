using QRCoder;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Ptcent.Cloud.Drive.Shared.Util
{
    /// <summary>
    /// 二维码工具类
    /// </summary>
    public static class QRCodeUtil
    {
        //创建一个新的QRCodeGenerator实例
        public static byte[] GenerateQrCodeBase64(string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //创建一个二维码 “123123”为显示内容
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M, true);
            //放入生成的二维码
            QRCode qrCode = new QRCode(qrCodeData);
            //获取到二维码图形
            Bitmap qrCodeImage = qrCode.GetGraphic(15, Color.Black, Color.White, false);
            Bitmap newBM = new Bitmap(550 + 60, 550 + (35 * 2) + 60);
            //新画布
            Graphics newGP = Graphics.FromImage(newBM);
            //清除所有背景色并指定背景颜色
            newGP.Clear(Color.White);
            // 插值算法的质量
            newGP.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //将旧图画入新图中
            newGP.DrawImage(qrCodeImage, new Rectangle(30, 30, 550, 550), new Rectangle(0, 0, qrCodeImage.Width, qrCodeImage.Height), GraphicsUnit.Pixel);
            //newBM.Save(Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid()}.jpg"));
            using (var memoryStream = new MemoryStream())
            {
                // 保存图像到内存流中
                newBM.Save(memoryStream, ImageFormat.Png);
                //将内存流转换为字节数组
                return memoryStream.ToArray();
            }
        }
    }
}
