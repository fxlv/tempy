using System;
using TempyConfiguration;
using TempyLogger;

namespace TempyWorker
{
    public class Program
    {
        private static void Main(string[] args)
        {
            
            Configuration tConfiguration = new Configuration();
            Logger.Initilize(tConfiguration);
            
            Console.Title = "Tempy Worker";

            var w = new Worker();
            w.Run();
        }
    }
}