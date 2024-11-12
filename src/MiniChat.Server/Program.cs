using System;

namespace MiniComm.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "MiniChat服务终端";
            
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Server>");
                Console.ResetColor();
                Console.ReadLine();
            }
        }
    }
}