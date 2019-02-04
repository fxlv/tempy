using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TempyAPI
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            TempyDbAuthCredentials cosmosDbAuthSettings = new TempyDbAuthCredentials();
            Configuration.Bind("CosmosDbAuthSettings",cosmosDbAuthSettings);
            services.AddSingleton(cosmosDbAuthSettings);

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
                // TODO: Handle exceptions by displaying nice error pages for PROD
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
                
            }

            app.UseMvc();
        }
    }
}