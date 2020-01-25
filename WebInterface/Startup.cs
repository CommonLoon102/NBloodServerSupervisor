using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                throw new Exception($"Couldn't find {supervisor} at {supervisorPath}");
            Process.Start(supervisor);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
