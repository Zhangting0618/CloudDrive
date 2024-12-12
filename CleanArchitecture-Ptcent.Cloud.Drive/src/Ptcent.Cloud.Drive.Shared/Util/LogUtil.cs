using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Shared.Util
{
    public static class LogUtil
    {
        private readonly static ILog log = LogManager.GetLogger("Local");

        public static void Info(string message)
        {
            log.Info(message);
        }

        public static void Error(string message)
        {
            log.Error(message);
        }
        public static void Error(string msgFormat, params object[] args)
        {
            string msg = string.Format(msgFormat, args);
            Error(msg);
        }
    }
}
