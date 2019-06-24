using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hexbotify.Middlewares;
using Hexbotify.Services;

namespace Hexbotify
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions()
                .Configure<NoOpsChallengeOptions>(Configuration)

                .AddHttpClient()

                .AddTransient<IHexbotifier, Hexbotifier>()
                .AddTransient<IApiRequestProvider, ApiRequestProvider>()
                .AddTransient<IApiClient, ApiClient>()
                .AddTransient<INoOpsApiClient, NoOpsApiClient>()
                .AddTransient<IWebImageClient, WebImageClient>()

                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<CorrelationIdHeaderMiddleware>();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
