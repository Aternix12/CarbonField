using System;
using System.Runtime.CompilerServices;

namespace CarbonField
{
    public static class ConsoleLogger
    {
        // Log method with optional color and caller location
        public static void Log(string message, ConsoleColor textColor = ConsoleColor.White, [CallerMemberName] string caller = null)
        {
            // Set timestamp color to white
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{DateTime.Now:HH:mm:ss} ");

            // Set user-defined text color
            Console.ForegroundColor = textColor;
            Console.WriteLine($"[{caller}]: {message}");

            // Reset the text color to its default value
            Console.ResetColor();
        }
    }
}