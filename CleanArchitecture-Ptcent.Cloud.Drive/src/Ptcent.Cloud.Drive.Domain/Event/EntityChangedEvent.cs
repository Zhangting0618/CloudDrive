using Ptcent.Cloud.Drive.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Event
{
    public record EntityChangedEvent(
        string EntityName,
        object EntityId,
        EntityChangeType ChangeType,
        IReadOnlyDictionary<string, (object OldValue, object NewValue)> Changes
    );

}
