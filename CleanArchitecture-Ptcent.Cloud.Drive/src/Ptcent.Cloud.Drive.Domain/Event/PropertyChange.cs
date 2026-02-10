using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Event
{
    public sealed class PropertyChange
    {
        public string Name { get; init; } = default!;
        public object? OldValue { get; init; }
        public object? NewValue { get; init; }
    }
}
