using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.ReponseModels
{
    public class ItemResponseDto
    {
        /// <summary>
        /// 文件 Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 扩展名
        /// </summary>
        public string? Extension { get; set; }

        /// <summary>
        /// 是否是文件夹 1-是 0-否
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// 文件大小字符串
        /// </summary>
        public string FileSizeStr { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 预览 URL
        /// </summary>
        public string? PreviewUrl { get; set; }
    }
}
