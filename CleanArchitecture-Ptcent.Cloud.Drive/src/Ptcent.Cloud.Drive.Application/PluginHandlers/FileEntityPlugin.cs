using MediatR;
using Ptcent.Cloud.Drive.Application.Events;
using Ptcent.Cloud.Drive.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.PluginHandlers
{
    /// <summary>
    /// 插件处理 示例
    /// </summary>
    public class FileEntityPlugin : INotificationHandler<EntityChangedNotification>
    {
        public async Task Handle(EntityChangedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Stage != PluginStage.PostOperation)
                return;
            if (notification.Event.EntityName != "file")
                return;
        }
    }
}
