using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.Common
{
    /// <summary>
    /// 当前系统时间
    /// </summary>
    public class LocalClockDto : ISystemClock
    {
        /// <summary>
        /// 当前系统时间
        /// </summary>
        public DateTimeOffset UtcNow => DateTime.Now;
    }
}
