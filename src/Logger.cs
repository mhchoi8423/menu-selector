using System.IO;

namespace MenuSelector
{
    public class Logger
    {
        public static void Log(string message)
        {
            using (var file = new StreamWriter("MenuSelector.Log", true))
            {
                file.WriteLine(message);
            }
        }
    }
}
