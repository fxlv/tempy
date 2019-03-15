using System;

namespace TempyWorker
{
    public class Program
    {
        private static void Main(string[] args)
        {
            
            TempyConfiguration tConfiguration = new TempyConfiguration();
            TempyLogger.Initilize(tConfiguration);
            
            Console.Title = "Tempy Worker";

            var w = new Worker();
            w.Run();
        }
    }
}