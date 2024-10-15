using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MMLib.ServiceDiscovery.Consul;

/// <summary>
///
/// </summary>
public class SwaggerService : ISwaggerService
{
    /// <summary>
    ///
    /// </summary>
    private readonly SwaggerGeneratorOptions _swaggerGeneratorOptions;

    /// <summary>
    ///
    /// </summary>
    /// <param name="swaggerGeneratorOptions"></param>
    public SwaggerService(IOptions<SwaggerGeneratorOptions> swaggerGeneratorOptions)
    {
        _swaggerGeneratorOptions = swaggerGeneratorOptions.Value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public List<string> GetSwaggerInfo()
    {
        return _swaggerGeneratorOptions.SwaggerDocs.Keys.ToList();
    }
}
