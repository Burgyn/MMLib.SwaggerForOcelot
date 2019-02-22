using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MMLib.SwaggerForOcelot;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace ApiGateway
{
    public class Startup
    {

        private readonly IHostingEnvironment _env;
        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            _env = env;
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot();
            services.AddSwaggerForOcelot(Configuration);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseSwaggerForOcelotUI(Configuration, opt => {
                opt.EndPointBasePath = "/swagger/docs";
            })
                .UseOcelot()
                .Wait();
        }
    }
}