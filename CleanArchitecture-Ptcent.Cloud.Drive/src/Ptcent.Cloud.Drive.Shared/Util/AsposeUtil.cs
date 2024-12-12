using Aspose.Words.Saving;
using System.Drawing;
using System.Drawing.Imaging;

namespace Ptcent.Cloud.Drive.Shared.Util
{
    public static class AsposeUtil
    {
        /// <summary>
        /// Word生成缩略图
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Stream CreateWordtTumbnail(Stream stream)
        {
            //需要在启动程序中执行注册码，否则生成的Word文档会有水印。
            #region 密钥
            new Aspose.Words.License().SetLicense(new MemoryStream(Convert.FromBase64String(
            "PExpY2Vuc2U+CiAgPERhdGE+CiAgICA8TGljZW5zZWRUbz5TdXpob3UgQXVuYm94IFNvZnR3YXJlIENvLiwgTHRkLjwvTGl" +
            "jZW5zZWRUbz4KICAgIDxFbWFpbFRvPnNhbGVzQGF1bnRlYy5jb208L0VtYWlsVG8+CiAgICA8TGljZW5zZVR5cGU+RGV2ZW" +
            "xvcGVyIE9FTTwvTGljZW5zZVR5cGU+CiAgICA8TGljZW5zZU5vdGU+TGltaXRlZCB0byAxIGRldmVsb3BlciwgdW5saW1pd" +
            "GVkIHBoeXNpY2FsIGxvY2F0aW9uczwvTGljZW5zZU5vdGU+CiAgICA8T3JkZXJJRD4yMDA2MDIwMTI2MzM8L09yZGVySUQ+" +
            "CiAgICA8VXNlcklEPjEzNDk3NjAwNjwvVXNlcklEPgogICAgPE9FTT5UaGlzIGlzIGEgcmVkaXN0cmlidXRhYmxlIGxpY2V" +
            "uc2U8L09FTT4KICAgIDxQcm9kdWN0cz4KICAgICAgPFByb2R1Y3Q+QXNwb3NlLlRvdGFsIGZvciAuTkVUPC9Qcm9kdWN0Pg" +
            "ogICAgPC9Qcm9kdWN0cz4KICAgIDxFZGl0aW9uVHlwZT5FbnRlcnByaXNlPC9FZGl0aW9uVHlwZT4KICAgIDxTZXJpYWxOd" +
            "W1iZXI+OTM2ZTVmZDEtODY2Mi00YWJmLTk1YmQtYzhkYzBmNTNhZmE2PC9TZXJpYWxOdW1iZXI+CiAgICA8U3Vic2NyaXB0" +
            "aW9uRXhwaXJ5PjIwMjEwODI3PC9TdWJzY3JpcHRpb25FeHBpcnk+CiAgICA8TGljZW5zZVZlcnNpb24+My4wPC9MaWNlbnN" +
            "lVmVyc2lvbj4KICAgIDxMaWNlbnNlSW5zdHJ1Y3Rpb25zPmh0dHBzOi8vcHVyY2hhc2UuYXNwb3NlLmNvbS9wb2xpY2llcy" +
            "91c2UtbGljZW5zZTwvTGljZW5zZUluc3RydWN0aW9ucz4KICA8L0RhdGE+CiAgPFNpZ25hdHVyZT5wSkpjQndRdnYxV1NxZ" +
            "1kyOHFJYUFKSysvTFFVWWRrQ2x5THE2RUNLU0xDQ3dMNkEwMkJFTnh5L3JzQ1V3UExXbjV2bTl0TDRQRXE1aFAzY2s0WnhE" +
            "ejFiK1JIWTBuQkh1SEhBY01TL1BSeEJES0NGbWg1QVFZRTlrT0FxSzM5NVBSWmJRSGowOUNGTElVUzBMdnRmVkp5cUhjblJ" +
            "vU3dPQnVqT1oyeDc4WFE9PC9TaWduYXR1cmU+CjwvTGljZW5zZT4=")));
            #endregion
            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            Aspose.Words.Saving.ImageSaveOptions iso = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Jpeg);
            iso.PageSet = new PageSet(0); // 设置要渲染的页面索引
            iso.Resolution = 128;
            iso.PrettyFormat = true;
            iso.UseAntiAliasing = true;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                doc.Save(memoryStream, iso);
                Bitmap thumbnail = new Bitmap(memoryStream);
                memoryStream.Position = 0;
                thumbnail.Save(memoryStream, ImageFormat.Jpeg);
                return memoryStream;
            }
        }
    }
}
