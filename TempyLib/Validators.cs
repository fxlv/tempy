using System.IO;

namespace TempyLib
{
    public static class Validators
    {

        public static string ReadFile(string fileName)
        {
            string content = File.ReadAllText(fileName);
            // trim whitespace from both ends
            content = content.Trim();
            return content;
        }    
        
        
        public static bool IsValidJsonFile(string fileName)
        {
            var content = ReadFile(fileName);
            if (content.StartsWith("{"))
            {
                if (content.EndsWith("}"))
                {
                    return true;
                }
            }

            return false;

        }
    }
}