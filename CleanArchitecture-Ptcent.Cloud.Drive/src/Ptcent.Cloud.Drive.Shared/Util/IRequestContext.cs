using Microsoft.AspNetCore.Http;

namespace Ptcent.Cloud.Drive.Web
{
    public interface IRequestContext
    {
        HttpContext? Current { get; }
    }
}
