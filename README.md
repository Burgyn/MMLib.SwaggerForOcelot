# MMLib.SwaggerForOcelot [![Build status](https://ci.appveyor.com/api/projects/status/qw082a4fh004p11u?svg=true)](https://ci.appveyor.com/project/Burgyn/mmlib-swaggerforocelot)

**SwaggerForOcelot** combines two amazing projects **[Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)** and **[Ocelot](https://github.com/ThreeMammals/Ocelot)**. Allows you to view and use swagger documentation for downstream services directly through the Ocelot project.

Direct via `http://ocelotprojecturl:port/swagger` provides documentation for downstream services configured in `ocelot.json`. Additionally, the addresses are modified to match the `UpstreamPathTemplate` from the configuration.

![SwaggerForOcelot](https://github.com/Burgyn/MMLib.SwaggerForOcelot/blob/master/demo/image.png?raw=true)

# Get Started
1. Configure SwaggerGen in your downstream services.
   > Follow the [SwashbuckleAspNetCore documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#getting-started).
2. Install Nuget package into yout ASP.NET Core Ocelot project.
   > dotnet add package MMLib.SwaggerForOcelot
3. Configure SwaggerForOcelot in `ocelot.json`.
```Json
 {
  "ReRoutes": [
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
    >`SwaggerEndPoint` is configuration for downstream service swagger generator endpoint.
    Property `Key` is used to pair with the ReRoute configuration. `Name` is displayed in the combobox. `Url` is downstream service swagger generator endpoint.
4. In the `ConfigureServices` method of `Startup.cs`, register the SwaggerForOcelot generator.
   ```CSharp
   services.AddSwaggerForOcelot(Configuration);
   ```
5. In `Configure` method, insert the `SwaggerForOcelot` middleware to expose interactive documentation.
   ```CSharp
          app.UseSwaggerForOcelotUI(Configuration, opt => {
                opt.EndPointBasePath = "/swagger/docs";
            })
   ```
6. Show your microservices interactive documentation.
   >`http://ocelotserviceurl/swagger`

# Virtual directory
If you have a `downstream service` hosted in the virtual directory, you probably have a `DownstreamPathTemplate` starting with the name of this virtual directory `/virtualdirectory/api/{everything}`. In order to properly replace the paths, it is necessary to set the property route `"Virtualdirectory":"/virtualdirectory"`.

Example:
```
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

## ToDo:
- [x] Design.
- [x] Create demo project.
- [x] Add swagger endpoints into SwagerUI combobox.
- [x] Create extension for adding middleware into pipeline.
- [x] Create `SwaggerForOcelotMiddleware` and replace downstream path.
- [x] Add capability for confiuragtion SwaggerUI and configure EndpointBasePath.
- [x] Unit tests
- [ ] Complex Ocelot configuration.
  - [x] Add `Petstore` swagger Example.
  - [x] Group more reroutes with same `SwaggerKey`.
  - [x] Filter `UpstreamHttpMethod`.
  - [ ] Query parameters.
  - [ ] Aggregates.
- [x] Documentations
- [x] Continuous delivery
- [ ] Custom Index.html.
