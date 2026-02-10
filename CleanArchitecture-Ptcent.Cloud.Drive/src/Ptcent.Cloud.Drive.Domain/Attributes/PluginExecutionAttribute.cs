using Ptcent.Cloud.Drive.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Attributes
{
    /// <summary>
    /// 插件执行特性 用来标记插件的异步还是同步模式
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PluginExecutionAttribute : Attribute
    {
        public PluginExecutionMode Mode { get; }

        public PluginExecutionAttribute(PluginExecutionMode mode)
        {
            Mode = mode;
        }
    }

}
