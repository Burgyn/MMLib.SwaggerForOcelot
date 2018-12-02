# MMLib.SwaggerForOcelot

**SwaggerForOcelot** combines two amazing projects **[Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)** and **[Ocelot](https://github.com/ThreeMammals/Ocelot)**. Allows you to view and use swagger documentation for downstream services directly through the Ocelot project.

Direct via `http://ocelotprojecturl:port/swagger` provides documentation for downstream services configured in `ocelot.json`. Additionally, the addresses are modified to match the `UpstreamPathTemplate` from the configuration.

![SwaggerForOcelot](https://github.com/Burgyn/MMLib.SwaggerForOcelot/blob/master/demo/image.png?raw=true)

# Get Started
1. Configure SwaggerGen in your downstream services.
   > Follow the [SwashbuckleAspNetCore documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#getting-started).
2. Install Nuget package into yout ASP.NET Core Ocelot project.
   > dotnet add package MMLib.SwaggerForOcelot
3. Configure SwaggerForOcelot in `ocelot.json`
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
            "UpstreamPathTemplate": "/api/projects/{everything}",
            "UpstreamHttpMethod": [ "Get" ],
            "SwaggerKey": "projects"
          }
        ],
        "SwaggerEndPoints": [
          {
            "Key": "projects",
            "Name": "Projects API",
            "Url": "http://localhost:5200/swagger/v1/swagger.json"
          },
          {
            "Key": "contacts",
            "Name": "Contacts API",
            "Url": "http://localhost:5100/swagger/v1/swagger.json"
          }
        ]
    }
    ```

4. In the `ConfigureServices` method of `Startup.cs`, register the SwaggerForOcelot generator.
   ```CSharp
   services.AddSwaggerForOcelot(Configuration);
   ```
5. In `Configure` method, insert the `SwaggerForOcelot` middleware to expose interactive documentation.
   ```CSharp
   app.UseSwaggerForOcelotUI(Configuration)
   ```

## ToDo:
- [x] Design.
- [x] Create demo project.
- [x] Add swagger endpoints into SwagerUI combobox.
- [x] Create extension for adding middleware into pipeline.
- [x] Create `SwaggerForOcelotMiddleware` and replace downstream path.
- [x] Add capability for confiuragtion SwaggerUI and configure EndpointBasePath.
- [ ] Complex Ocelot configuration.
- [ ] Unit tests
- [ ] Documentations
- [ ] Custom Index.html.