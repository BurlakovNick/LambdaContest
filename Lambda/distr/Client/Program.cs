using System;
using Core;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var punterName = args[0];

            var punter = PunterFactory.Create(punterName);
            var log = new ConsoleLog();
            var client = new OnlineClient(punter, log);
            client.Start();
            Console.ReadLine();
        }
    }
}