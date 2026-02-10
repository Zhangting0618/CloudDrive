using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Attributes
{
    /// <summary>
    /// 插件优先级
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PluginPriorityAttribute : Attribute
    {
        public int Priority { get; }

        public PluginPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }

}
