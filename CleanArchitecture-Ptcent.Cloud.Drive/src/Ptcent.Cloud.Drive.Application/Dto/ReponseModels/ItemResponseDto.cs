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
        /// 文件Id
        /// </summary>
        //public long Id { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
       public string? LeafName { get; set; }
        /// <summary>
        /// 扩展名
        /// </summary>
        public string? Extension { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public string FileSizeStr { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long? FileSize { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeletedDate { get; set; }
    }
}
