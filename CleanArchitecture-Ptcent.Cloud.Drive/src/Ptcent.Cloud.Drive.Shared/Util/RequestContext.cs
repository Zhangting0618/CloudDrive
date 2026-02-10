using Microsoft.AspNetCore.Http;

namespace Ptcent.Cloud.Drive.Web
{
    public sealed class RequestContext : IRequestContext
    {
        private readonly IHttpContextAccessor _accessor;

        public RequestContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public HttpContext? Current => _accessor.HttpContext;
    }

}
