using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddOcelot(builder.Configuration)
    .AddConsul();

builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

app.UseSwaggerForOcelotUI();
await app.UseOcelot();

app.MapGet("/", () => "Hello World!").WithOpenApi();
app.Run("http://0.0.0.0:7001");
