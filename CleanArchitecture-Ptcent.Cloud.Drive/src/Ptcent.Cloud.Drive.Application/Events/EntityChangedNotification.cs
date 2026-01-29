using MediatR;
using Ptcent.Cloud.Drive.Application.Plugins;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Domain.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Events
{
    public class EntityChangedNotification : INotification
    {
        public EntityChangedEvent Event { get; }
        public PluginStage Stage { get; }
        public PluginContext Context { get; }

        public EntityChangedNotification(
            EntityChangedEvent @event,
            PluginStage stage,
            PluginContext context)
        {
            Event = @event;
            Stage = stage;
            Context = context;
        }
    }

}
