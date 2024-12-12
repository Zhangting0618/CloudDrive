using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Entities
{
    public partial class FileEntity
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string LeafName { get; set; }
        /// <summary>
        /// 扩展名
        /// </summary>
        public string? Extension { get; set; }
        /// <summary>
        /// 中文路径
        /// </summary>
        public string? Path { get; set; }
        /// <summary>
        /// 父级文件Id
        /// </summary>
        public long? ParentFolderId { get; set; }
        /// <summary>
        /// Idpath
        /// </summary>
        public string Idpath { get; set; }
        /// <summary>
        /// 是否是文件夹 1、是 0、否
        /// </summary>
        public int IsFolder { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public int? FileType { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public int? IsDel { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreatedBy { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>
        public long? UpdatedBy { get; set; }
        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeletedDate { get; set; }
        /// <summary>
        /// 删除人
        /// </summary>
        public long? DeletedBy { get; set; }
        /// <summary>
        /// 版本Id
        /// </summary>
        public long? VersionId { get; set; }
        /// <summary>
        /// 文件Hash
        /// </summary>
        public string? ItemHash { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long? FileSize { get; set; }
        /// <summary>
        /// 文件存储路径 源文件存放路径 上传之后 就不发生改变 如果编辑 则修改映射
        /// </summary>
        public string? PhysicalDirectory { get; set; }
        /// <summary>
        /// 文件 映射路径 文件编辑的时候 更新该字段的值
        /// </summary>
        public string? ItemFileMapUrl { get; set; }
    }
}
