using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core;
using lib.Ai;
using lib.Ai.StrategicFizzBuzz;
using lib.Interaction;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log("Client started " + args[0]);
                var punterName = args[0];
                var server = args.Length > 1 ? args[1] : "localhost";
                var port = args.Length > 2 ? args[2] : "7777";
                var isKonturRu = args.Length > 3;

                if (isKonturRu)
                {
                    var interaction = new OnlineInteraction(int.Parse(port));
                    interaction.Start();
                    interaction.RunGame((IAi) UberfullessnessAi.All.FirstOrDefault(
                                            x => x.Name ==
                                                 "FutureIsNowSetupStrategyoptions-FutureIsNowStrategyoptions-ExtendComponentStrategyoptions-SumGreedyStrategyUberAi") ??
                                        new ConnectClosestMinesAi());
                    return;
                }

                var punter = PunterFactory.Create(punterName);
                var client = new OnlineClient(punter);
                client.Start(server, port);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Log("Error:" + e);
                throw;
            }
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
            File.WriteAllText($"client{Process.GetCurrentProcess().Id}Log.txt", text);
        }
    }
}