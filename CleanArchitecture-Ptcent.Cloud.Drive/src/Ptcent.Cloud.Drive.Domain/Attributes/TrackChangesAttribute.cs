using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TrackChangesAttribute : Attribute
    {
        public string[] Properties { get; }

        public TrackChangesAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }

}
