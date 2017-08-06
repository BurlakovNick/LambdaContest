﻿using System;
using Core;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var punterName = args[0];

            var punter = PunterFactory.Create(punterName);
            var client = new OnlineClient(punter);
            client.Start();
            Console.ReadLine();
        }
    }
}