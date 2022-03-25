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
        /// <summary>
        /// Do downstream swagger endopint
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="version"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        bool DoDownstreamSwaggerEndpoint(HttpContext httpContext, string version, SwaggerEndPointOptions endPoint);
    }
}
