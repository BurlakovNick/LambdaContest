using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Referee
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args);
        }

        private static void Run(string[] punters)
        {
            Log("#################################################");
            Log(string.Join(" VS ", punters));
            Log("#################################################");
            var maps = GetAllMaps().Where(x => x == "sample").ToArray();
            var i = 1;
            foreach (var map in maps)
            {
                try
                {
                    Log($"Round {i}/{maps.Length}");
                    Log($"Map: {map}");

                    var serverTask = Task.Run(() => RunServer(map, punters.Length));

                    var clientsTasks = new List<Task>();
                    foreach (var punterName in punters)
                    {
                        var thePunterName = punterName;
                        clientsTasks.Add(Task.Run(() => RunClient(thePunterName)));
                    }

                    Task.WaitAll(clientsTasks.Concat(new[] { serverTask }).ToArray());

                    Log("Scores:");
                    var scores = GetScores();
                    foreach (var score in scores)
                        Log($"{score.Item1.Name}:\t{score.Item2.Score}");

                    LogResults(map, punters, scores);

                    Log("#################################################");
                    i++;
                    Thread.Sleep(2000);
                }
                catch (Exception e)
                {
                    Log("Error: " + e.Message);
                }
            }
        }

        private static void LogResults(string map,
                                       string[] punters,
                                       Tuple<Who, ScoreContract>[] scores)
        {
            if (!Directory.Exists("Results"))
                Directory.CreateDirectory("Results");
            var fileName = Path.Combine("Results", string.Join("_VS_", punters) + ".txt");
            var sb = new StringBuilder();
            sb.AppendLine($"Map: {map}");
            foreach (var score in scores)
                sb.AppendLine($"{score.Item1.Name}:\t{score.Item2.Score}");
            sb.AppendLine("#############################");
            File.AppendAllText(fileName, sb.ToString());

            var folder = Path.Combine("Results", string.Join("_VS_", punters), map);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.Copy("moves.txt", Path.Combine(folder, "moves.txt"));
            if (File.Exists("passes.txt"))
                File.Copy("passes.txt", Path.Combine(folder, "passes.txt"));
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
                         .OrderByDescending(x => x.Item2.Score)
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