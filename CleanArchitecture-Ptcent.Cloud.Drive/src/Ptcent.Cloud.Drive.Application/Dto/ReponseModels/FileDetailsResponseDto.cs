using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.ReponseModels
{
    /// <summary>
    /// 文件详情Dto
    /// </summary>
    public class FileDetailsResponseDto
    {
        /// <summary>
        /// 图片Base64编码
        /// </summary>
        public string? ImageBase64String { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string? LeafName { get; set; }
        /// <summary>
        /// 扩展名
        /// </summary>
        public string? Extension { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedDate { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedDate { get; set; }
        /// <summary>
        ///  文件大小
        /// </summary>
        public string? FileStr { get; set; }
        /// <summary>
        /// 所属目录
        /// </summary>
        public string? LocationDirectory { get; set; }
    }
}
