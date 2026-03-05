using System.ComponentModel.DataAnnotations;

namespace Ptcent.Cloud.Drive.Domain.Entities
{
    /// <summary>
    /// 文件实体
    /// </summary>
    public class FileEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string LeafName { get; set; } = string.Empty;

        /// <summary>
        /// 扩展名
        /// </summary>
        public string? Extension { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 父级文件夹 Id
        /// </summary>
        public long? ParentFolderId { get; set; }

        /// <summary>
        /// ID 路径
        /// </summary>
        public string Idpath { get; set; } = string.Empty;

        /// <summary>
        /// 是否是文件夹 1-是 0-否
        /// </summary>
        public int IsFolder { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public int? FileType { get; set; }

        /// <summary>
        /// 是否删除 0-否 1-是
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
        /// 版本 Id
        /// </summary>
        public long? VersionId { get; set; }

        /// <summary>
        /// 文件 Hash
        /// </summary>
        public string? ItemHash { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// 物理存储路径
        /// </summary>
        public string? PhysicalDirectory { get; set; }

        /// <summary>
        /// 文件映射 URL
        /// </summary>
        public string? ItemFileMapUrl { get; set; }
    }
}
