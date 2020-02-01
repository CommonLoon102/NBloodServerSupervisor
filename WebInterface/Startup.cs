using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebInterface.Services;

namespace WebInterface
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StartSupervisor();
        }

        private void StartSupervisor()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            string supervisor = "Supervisor";
            if (isWindows)
                supervisor += ".exe";
            string supervisorPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), supervisor);
            if (!File.Exists(supervisorPath))
                throw new Exception($"Couldn't find {supervisor} at {supervisorPath}.");
            Process.Start(supervisorPath);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddControllersWithViews();
            services.Add(new ServiceDescriptor(typeof(IStateService), typeof(StateService), ServiceLifetime.Singleton));
            services.Add(new ServiceDescriptor(typeof(IPrivateServerService), typeof(PrivateServerService), ServiceLifetime.Transient));
            services.Add(new ServiceDescriptor(typeof(IRateLimiterService), typeof(RateLimiterService), ServiceLifetime.Singleton));
            services.Add(new ServiceDescriptor(typeof(ICustomMapService), typeof(CustomMapService), ServiceLifetime.Transient));

            services.Configure<FormOptions>(options =>
            {
                // Set the limit to 1 MB
                options.MultipartBodyLengthLimit = Constants.FileSizeLimit;
            });

            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions
                        .AddPageApplicationModelConvention("/nblood/private", model =>
                        {
                            model.Filters.Add(
                                new RequestFormLimitsAttribute()
                                {
                                    // Set the limit to 1 MB
                                    MultipartBodyLengthLimit = Constants.FileSizeLimit
                                });
                            model.Filters.Add(
                                new RequestSizeLimitAttribute(Constants.FileSizeLimit));
                        });
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
