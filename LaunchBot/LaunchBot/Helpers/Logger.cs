using System;
using System.IO;

namespace LaunchBot.Helpers
{
    public static class Logger
    {
        private static string FileName = "Log.txt";

        public static void Log(string message)
        {
            File.AppendAllText(FileName, $"{Environment.NewLine}{DateTime.Now.ToString("G")}: {message}");
        }
    }
}
