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
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}