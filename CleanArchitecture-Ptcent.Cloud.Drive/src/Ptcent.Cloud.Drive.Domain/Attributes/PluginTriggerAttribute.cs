using Ptcent.Cloud.Drive.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Attributes
{
    /// <summary>
    /// 插件触发器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginTriggerAttribute : Attribute
    {
        /// <summary>
        /// Create / Update / Delete
        /// </summary>
        public EntityChangeType ChangeType { get; set; }

        /// <summary>
        /// Update 时关心的字段（空 = 任意字段）
        /// </summary>
        public string[]? Attributes { get; set; }
    }

}
