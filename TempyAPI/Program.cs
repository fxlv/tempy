using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace TempyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            
            // TODO: setup logging
            Console.Title = "Tempy API";
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}