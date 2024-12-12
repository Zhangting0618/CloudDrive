using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Shared.Util
{
    public static class HttpContextUtil
    {
        public static IServiceCollection serviceCollection;

        public static HttpContext Current
        {
            get
            {
                object factory = serviceCollection.BuildServiceProvider().GetService(typeof(IHttpContextAccessor));
               HttpContext context = ((HttpContextAccessor)factory).HttpContext;
                return context;
            }
        }

    }
}
