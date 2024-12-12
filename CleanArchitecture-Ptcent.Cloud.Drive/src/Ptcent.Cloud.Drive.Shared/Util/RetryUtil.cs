using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Shared.Util
{
    /// <summary>
    /// 重试
    /// </summary>
    public static class RetryUtil
    {
        //public static void Do<T>(Func<T> func, int maxAttemptCount = 3)
        //{
        //return
        //}

        public static T DoRetry<T>(Func<T> func, int maxAttemptCount = 1)
        {
            var exceptions = new List<Exception>();
            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    //if (attempted > 0)
                    //{
                    //    Thread.Sleep(retryInterval);
                    //}
                    return func();
                }
                catch (Exception ex)
                {
                    exceptions.Add(new Exception(ex.Message + ex.StackTrace));
                    Thread.Sleep(10);
                }
            }
            throw new AggregateException(exceptions);
        }
        public static void DoRetry(Action action, int maxAttemptCount = 1)
        {
            var exceptions = new List<Exception>();
            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    //if (attempted > 0)
                    //{
                    //    Thread.Sleep(retryInterval);
                    //}
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(10);
                }
            }
            throw new AggregateException(exceptions);
        }


    }
}
