using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    internal class Util
    {
        static List<(string sender, string message, string time)> chatLog = new List<(string, string, string)>();

        public static void PrintDisplayMessage(string sender, string message, string time)
        {
            chatLog.Add((sender, message, time));
            Console.Clear();
            DisplayChat();
        }

        public static void DisplayChat()
        {
            foreach (var (sender, message, time) in chatLog)
            {
                if (sender == "나")
                {
                    //Console.SetCursorPosition(Console.WindowWidth - message.Length - 10, Console.CursorTop);
                    Console.ForegroundColor = ConsoleColor.Green;
                    PrintRightAligned($"[나] {message}", 10);
                    PrintRightAligned($"      {time}", 10);
           
                }
                else
                {
                    //Console.WriteLine($"[{sender}] {message}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    PrintRightAligned($"[{sender}] {message}", 0);
                    PrintRightAligned($"      {time}", 0);
                }
                //Console.SetCursorPosition(Console.WindowWidth - 20, Console.CursorTop);
                //Console.WriteLine($"   ({time})");
                //Console.WriteLine(new string('-', Console.WindowWidth));
                //Console.WriteLine();
                Console.ResetColor();
            }
        }
        static void PrintRightAligned(string text, int padding)
        {
            Console.SetCursorPosition(Console.WindowWidth - text.Length - padding, Console.CursorTop);
            Console.WriteLine(text);
        }
    }
}
