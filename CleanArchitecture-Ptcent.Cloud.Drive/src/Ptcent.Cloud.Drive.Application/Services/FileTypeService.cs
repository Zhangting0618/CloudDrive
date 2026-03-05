using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Services
{
    /// <summary>
    /// 文件类型服务
    /// </summary>
    public static class FileTypeService
    {
        /// <summary>
        /// 根据文件扩展名判断文件类型
        /// </summary>
        /// <param name="extension">文件扩展名（包含点号，如 .jpg）</param>
        /// <returns>文件类型枚举</returns>
        public static FileTypeEnum JudgmentFileType(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return FileTypeEnum.Other;
            }

            var ext = extension.ToLower().TrimStart('.');

            // 图片类型
            if (ext is "jpg" or "jpeg" or "png" or "gif" or "bmp" or "webp" or "ico" or "svg")
            {
                return FileTypeEnum.Image;
            }

            // 文档类型
            if (ext is "doc" or "docx" or "pdf" or "xls" or "xlsx" or "ppt" or "pptx" or "txt" or "md" or "csv")
            {
                return FileTypeEnum.Docs;
            }

            // 视频类型
            if (ext is "mp4" or "avi" or "mkv" or "mov" or "wmv" or "flv" or "webm")
            {
                return FileTypeEnum.Video;
            }

            // 音频类型
            if (ext is "mp3" or "wav" or "flac" or "aac" or "ogg" or "wma")
            {
                return FileTypeEnum.Audio;
            }

            return FileTypeEnum.Other;
        }
    }
}
