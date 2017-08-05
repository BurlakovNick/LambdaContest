using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Referee
{
    class Program
    {
        static void Main(string[] args)
        {
            Log("#################################################");
            Log(string.Join(" VS ", args));
            Log("#################################################");
            var maps = GetAllMaps();
            var i = 1;
            foreach (var map in maps)
            {
                Log($"Round {i}/{maps.Length}");
                Log($"Map: {map}");

                Log("Runing server...");
                var serverTask = Task.Run(() => RunServer(map, args.Length));

                Log("Runing clients...");
                var clientsTasks = new List<Task>();
                foreach (var punterName in args)
                {
                    var thePunterName = punterName;
                    clientsTasks.Add(Task.Run(() => RunClient(thePunterName)));
                }

                Log("Wait round ends...");
                Task.WaitAll(clientsTasks.Concat(new[] { serverTask }).ToArray());

                Log("Round finished");
                Log("Scores:");
                var scores = GetScores();
                foreach (var score in scores.OrderByDescending(x => x.Item2.Score))
                    Log($"{score.Item1.Name}:\t{score.Item2.Score}");

                Log("#################################################");
                i++;
                Thread.Sleep(2000);
            }
        }

        private static Tuple<Who, ScoreContract>[] GetScores()
        {
            var whos = JsonConvert.DeserializeObject<Who[]>(File.ReadAllText("who.txt"));
            var scores = JsonConvert.DeserializeObject<ScoreContract[]>(File.ReadAllText("scores.txt"));
            return scores.Join(whos,
                               x => x.Punter,
                               x => x.Id,
                               (x,
                                y) => new Tuple<Who, ScoreContract>(y, x))
                         .ToArray();
        }

        private static void RunServer(string mapName,
                                      int playersCount)
        {
            var exeName = Path.Combine(GetServerFolder(), "Server.exe");
            Process.Start(exeName, $"{mapName} {playersCount}").WaitForExit();
        }

        private static void RunClient(string punterName)
        {
            var exeName = Path.Combine(GetClientFolder(), "Client.exe");
            Process.Start(exeName, punterName).WaitForExit();
        }

        private static string[] GetAllMaps()
        {
            return Directory.GetFiles("Maps").Select(Path.GetFileNameWithoutExtension).ToArray();
        }

        private static string GetClientFolder() => Path.Combine(GetSolutionFolder(), @"Client\bin\Debug\");
        private static string GetServerFolder() => Path.Combine(GetSolutionFolder(), @"Server\bin\Debug\");
        private static string GetSolutionFolder() => Path.Combine(Environment.CurrentDirectory, @"..\..\..\");

        private static void Log(string message)
        {
            Console.Out.WriteLine(message);
            File.AppendAllLines("log.txt", new[] { message });
        }

        private class ScoreContract
        {
            public int Punter { get; set; }
            public int Score { get; set; }
        }

        private class Who
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}