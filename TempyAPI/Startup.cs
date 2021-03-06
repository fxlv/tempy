﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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
            services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            var cosmosDbAuthSettings = new TempyDbAuthCredentials();
            Configuration.Bind("CosmosDbAuthSettings", cosmosDbAuthSettings);
            services.AddSingleton(cosmosDbAuthSettings);
            //services.AddMvc(option => option.EnableEndpointRouting = false);
            // add a policy to allow CORS requests from any origins, 
            // this will be used for remote Javascript to fetch TempyAPI data
            services.AddCors(
                o => o.AddPolicy("AllowAnyOriginPolicy", builder =>
                    builder.AllowAnyOrigin())
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStaticFiles();
                Log.Debug("Using development configuration.");
            }
            else
            {
                // TODO: Handle exceptions by displaying nice error pages for PROD
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
                Log.Debug("Using production configuration.");
            }

            app.UseMvc();
        }
    }
}