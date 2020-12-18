using Microsoft.AspNetCore.Http;
using MMLib.SwaggerForOcelot.Configuration;

namespace MMLib.SwaggerForOcelot.Middleware
{
    /// <summary>
    /// Use this interceptor to control downstream of a swagger endpoint.
    /// When you do not downstream do not forget to write the response.
    /// ex:
    /// <code>
    /// httpContext.Response.StatusCode = 403;
    /// httpContext.Response.WriteAsync("you are not allowed to access this resource");
    /// </code>
    /// </summary>
    public interface ISwaggerDownstreamInterceptor
    {
        bool DoDownstreamSwaggerEndpoint(HttpContext httpContext, string version, SwaggerEndPointOptions endPoint);
    }
}
