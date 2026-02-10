using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Enum
{
    public enum PluginStage
    {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40,
        PostCommit = 50
    }
    /// <summary>
    /// 插件执行模式
    /// </summary>
    public enum PluginExecutionMode
    {
        /// <summary>
        /// 同步
        /// </summary>
        /// </summary>
        Sync,
        /// <summary>
        /// 异步
        /// </summary>
        Async
    }
    public enum PluginExecutionStatus
    {
        Queued = 0,
        Running = 1,
        Success = 2,
        Failed = 3
    }
}
