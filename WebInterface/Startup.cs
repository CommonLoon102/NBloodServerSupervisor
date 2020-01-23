using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            string nbloodPath = Configuration.GetValue<string>("NBloodPath");
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            string supervisor = "Supervisor";
            if (isWindows)
                supervisor += ".exe";
            if (!File.Exists(nbloodPath))
                throw new Exception($"Couldn't find nblood_server at {nbloodPath}");
            if (!File.Exists(supervisor))
                throw new Exception($"Couldn't find {supervisor} in {Directory.GetCurrentDirectory()}");
            Process.Start(supervisor, nbloodPath);
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
