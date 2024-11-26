using System;

namespace MiniChat.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "MiniChat服务终端";
            Serverbin.CreateConfigFile();
            Sqlbin.SQLServerString = Serverbin.GetDatabaseConnectionString();
            ServerShell serverShell = new ServerShell(Serverbin.GetServerIPEndPoint());
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Server>");
                Console.ResetColor();
                serverShell.ExecuteCommand(Console.ReadLine().Trim().ToLower());
            }
        }
    }
}