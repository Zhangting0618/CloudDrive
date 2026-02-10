using Ptcent.Cloud.Drive.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Interfaces.Persistence
{
    public interface IEntityPlugin
    {
        Task ExecuteAsync(EntityChangedNotification notification, CancellationToken ct);
    }
}
