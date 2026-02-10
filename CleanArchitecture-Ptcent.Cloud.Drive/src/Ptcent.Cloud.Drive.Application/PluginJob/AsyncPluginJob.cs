using Ptcent.Cloud.Drive.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.PluginJob
{
    public sealed class AsyncPluginJob
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// 插件完整类型名
        /// </summary>
        public string PluginType { get; init; } = default!;

        /// <summary>
        /// 执行幂等Key
        /// </summary>
        public string ExecutionKey { get; init; } = default!;

        /// <summary>
        /// 实体变更通知（可直接用，也可以后续改为 JSON）
        /// </summary>
        public EntityChangedNotification Notification { get; init; } = default!;

        public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    }

}
