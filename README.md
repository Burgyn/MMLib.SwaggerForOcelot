![alt logo](https://github.com/Burgyn/MMLib.SwaggerForOcelot/blob/master/.github/MMLibLogo.png?raw=true)

![Publish package](https://github.com/Burgyn/MMLib.SwaggerForOcelot/workflows/Publish%20package/badge.svg)

**SwaggerForOcelot** combines two amazing projects **[Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)** and **[Ocelot](https://github.com/ThreeMammals/Ocelot)**. Allows you to view and use swagger documentation for downstream services directly through the Ocelot project.

Direct via `http://ocelotprojecturl:port/swagger` provides documentation for downstream services configured in `ocelot.json`. Additionally, the addresses are modified to match the `UpstreamPathTemplate` from the configuration.

![SwaggerForOcelot](https://github.com/Burgyn/MMLib.SwaggerForOcelot/blob/master/demo/image.png?raw=true)

---
Did this library help you? Buy me a beer 😎🍺

<a href="https://www.buymeacoffee.com/0dQ7tNG" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Beer" height="41" width="174"></a>

## Get Started

1. Configure SwaggerGen in your downstream services.
   > Follow the [SwashbuckleAspNetCore documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#getting-started).
2. Install Nuget package into yout ASP.NET Core Ocelot project.
   > dotnet add package MMLib.SwaggerForOcelot
3. Configure SwaggerForOcelot in `ocelot.json`.

```Json
 {
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5100
        }
      ],
      "UpstreamPathTemplate": "/api/contacts/{everything}",
      "UpstreamHttpMethod": [ "Get" ],
      "SwaggerKey": "contacts"
    },
    {
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5200
        }
      ],
      "UpstreamPathTemplate": "/api/orders/{everything}",
      "UpstreamHttpMethod": [ "Get" ],
      "SwaggerKey": "orders"
    }
  ],
  "SwaggerEndPoints": [
    {
      "Key": "contacts",
      "Config": [
        {
          "Name": "Contacts API",
          "Version": "v1",
          "Url": "http://localhost:5100/swagger/v1/swagger.json"
        }
      ]
    },
    {
      "Key": "orders",
      "Config": [
        {
          "Name": "Orders API",
          "Version": "v0.9",
          "Url": "http://localhost:5200/swagger/v0.9/swagger.json"
        },
        {
          "Name": "Orders API",
          "Version": "v1",
          "Url": "http://localhost:5200/swagger/v1/swagger.json"
        },
        {
          "Name": "Orders API",
          "Version": "v2",
          "Url": "http://localhost:5200/swagger/v2/swagger.json"
        },
        {
          "Name": "Orders API",
          "Version": "v3",
          "Url": "http://localhost:5200/swagger/v3/swagger.json"
        }
      ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost"
  }
}
```

   > `SwaggerEndPoint` is configuration for downstream service swagger generator endpoint. Property `Key` is used to pair with the Route configuration. `Name` is displayed in the combobox. `Url` is downstream service swagger generator endpoint.

4. In the `ConfigureServices` method of `Startup.cs`, register the SwaggerForOcelot generator.

```CSharp
services.AddSwaggerForOcelot(Configuration);
```

5. In `Configure` method, insert the `SwaggerForOcelot` middleware to expose interactive documentation.

```CSharp
app.UseSwaggerForOcelotUI(opt => {
  opt.PathToSwaggerGenerator = "/swagger/docs";
})
```

   You can optionally include headers that your Ocelot Gateway will send when requesting a swagger endpoint. This can be especially useful if your downstream microservices require contents from a header to authenticate.

  ```CSharp
app.UseSwaggerForOcelotUI(opt => {
    opts.DownstreamSwaggerHeaders = new[]
    {
        new KeyValuePair<string, string>("Auth-Key", "AuthValue"),
    };
})
  ```

  After swagger for ocelot transforms the downstream swagger to the upstream swagger, you have the ability to alter the upstream swagger if you need to by setting the `ReConfigureUpstreamSwaggerJson` option or `ReConfigureUpstreamSwaggerJsonAsync` option for async methods.

  ```CSharp
public string AlterUpstreamSwaggerJson(HttpContext context, string swaggerJson)
{
    var swagger = JObject.Parse(swaggerJson);
    // ... alter upstream json
    return swagger.ToString(Formatting.Indented);
}

app.UseSwaggerForOcelotUI(opt => {
    opts.ReConfigureUpstreamSwaggerJson = AlterUpstreamSwaggerJson;
})
  ```
You can optionally customize the swagger server prior to calling the endpoints of the microservices as follows:
```CSharp
app.UseSwaggerForOcelotUI(opt => {
    opts.ReConfigureUpstreamSwaggerJson = AlterUpstreamSwaggerJson;
	opts.ServerOcelot = "/siteName/apigateway" ;
})
  ```

6. Show your microservices interactive documentation.

   > `http://ocelotserviceurl/swagger`

## Virtual directory

If you have a `downstream service` hosted in the virtual directory, you probably have a `DownstreamPathTemplate` starting with the name of this virtual directory `/virtualdirectory/api/{everything}`. In order to properly replace the paths, it is necessary to set the property route `"Virtualdirectory":"/virtualdirectory"`.

Example:

``` Json
 {
  "DownstreamPathTemplate": "/project/api/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
      {
      "Host": "localhost",
      "Port": 5100
      }
  ],
  "UpstreamPathTemplate": "/api/project/{everything}",
  "UpstreamHttpMethod": [ "Get" ],
  "SwaggerKey": "project",
  "VirtualDirectory":"/project"
}
```

## Service discovery

If you use [Ocelot Service Discovery Provider](https://ocelot.readthedocs.io/en/latest/features/servicediscovery.html) to find the host and port for the downstream service, then you can use the same service name for swagger configuration.

``` Json
"Routes": [
  {
    "DownstreamPathTemplate": "/api/{everything}",
    "ServiceName": "projects",
    "UpstreamPathTemplate": "/api/project/{everything}",
    "SwaggerKey": "projects",
  }
],
 "SwaggerEndPoints": [
    {
      "Key": "projects",
      "Config": [
        {
          "Name": "Projects API",
          "Version": "v1",
          "Service": {
            "Name": "projects",
            "Path": "/swagger/v1/swagger.json"
          }
        }
      ]
    }
  ],

  "GlobalConfiguration": {
    "ServiceDiscoveryProvider": {
      "Type": "AppConfiguration",
      "PollingInterval": 1000
    }
  }
```

## The Gateway documentation itself

There are several real scenarios when you need to have a controller directly in your gateway. For example: specific aggregation of results from multiple services / legacy part of your system / ...

If you need to, you can also add documentation.

1. Configure SwaggerGen in your Ocelot API Gateway services.
   > Follow the [SwashbuckleAspNetCore documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#getting-started).

`ConfigureServices`

```csharp
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });
});
```

`Configure`
```csharp
app.UseSwagger();
```

2. Add `SwaggerEndPoint` into ocelot json configuration.

```json
{
  "Key": "gateway",
  "TransformByOcelotConfig": false,
  "Config": [
    {
      "Name": "Gateway",
      "Version": "v1",
      "Url": "http://localhost:5000/swagger/v1/swagger.json"
    }
  ]
}
```

The key is to set it up property `TransformByOcelotConfig` to `false`, because in this case you do not need to transform the documentation according to the ocelot configuration.

![ocelot docs](./demo/ocelotdocs.png)

## Merging configuration files

Optionally you can use the Ocelot feature [Merging configuration files](https://ocelot.readthedocs.io/en/latest/features/configuration.html#merging-configuration-files) to load the apigateway configuration from multiple configuration files named as follows: `ocelot.exampleName.json`. To activate this feature you need to use the following extension:

```CSharp
WebHost.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration((hostingContext, config) =>
  {
      config.AddOcelotWithSwaggerSupport();
  })
  .UseStartup<Startup>();
```

Using this extension the swagger path settings must be in a file called: `ocelot.SwaggerEndPoints.json`. If instead you want to use another name for this file you could set the name as follows _(without the .json extension)_:

```CSharp
WebHost.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration((hostingContext, config) =>
  {
     config.AddOcelotWithSwaggerSupport(fileOfSwaggerEndPoints: "ocelot.swagger")
  })
  .UseStartup<Startup>();
```

Optionally you can put the configuration files in a folder, and for that you have to set the extension as follows:

```CSharp
WebHost.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration((hostingContext, config) =>
  {
    config.AddOcelotWithSwaggerSupport();
  })
  .UseStartup<Startup>(folder: "Configuration");
```

Optionally you can also add configuration files with the format `ocelot.exampleName.json` per environment, to use this functionality you must configure the extension as follows:

```CSharp
WebHost.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration((hostingContext, config) =>
  {
    config.AddOcelotWithSwaggerSupport(environment: hostingContext.HostingEnvironment);
  })
  .UseStartup<Startup>(folder: "Configuration");
```

## Limitation

- Now, this library support only `{everything}` as a wildcard in routing definition. #68
- This package unfortunately does not support parameter translating between upstream and downstream path template. #59

## Version 2.0.0

This version is breaking change. Because support Ocelot 16.0.0, which rename `ReRoutes` to `Routes`. See Ocelot [v16.0.0](https://github.com/ThreeMammals/Ocelot/releases/tag/16.0.0).
