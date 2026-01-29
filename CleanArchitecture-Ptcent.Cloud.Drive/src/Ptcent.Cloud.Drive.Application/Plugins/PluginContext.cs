using Ptcent.Cloud.Drive.Domain.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Plugins
{
    public sealed class PluginContext
    {
        public Guid CorrelationId { get; } = Guid.NewGuid();

        public int Depth { get; private set; } = 1;

        public void IncreaseDepth()
        {
            Depth++;
        }
    }


}
