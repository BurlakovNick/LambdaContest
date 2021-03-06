﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;

namespace Distributor
{
    class Program
    {
        private static SemaphoreSlim deploySemaphore;
        private static readonly object lockObject = new object();
        private static readonly Random random = new Random();

        static void Main(string[] args)
        {
            if (Directory.Exists("Results"))
                Directory.Delete("Results", true);

            deploySemaphore = new SemaphoreSlim(72, 72);
            var agents = GetAllAgents().Except(new[] { "elba-6" }).ToArray();
            Log($"{agents.Length} agents");

            var allPunters = GetAllPunters();
            Log($"{allPunters.Length} punters");

            var queue = GetBattlesQueue(allPunters);
            Log($"{queue.Count} battles");

            var distrFolder = CreateDistributive();

            var agentTasks = new List<Task>();
            foreach (var agent in agents)
            {
                var task = Task.Run(() => RunAgent(agent, queue, distrFolder));
                agentTasks.Add(task);
            }

            Task.WaitAll(agentTasks.ToArray());
        }

        private static void RunAgent(string agent,
                                     ConcurrentQueue<string[]> queue,
                                     string distrFolder)
        {
            ProcessHelpers.KillProcess("Referee.exe", agent);
            ProcessHelpers.KillProcess("Server.exe", agent);
            ProcessHelpers.KillProcess("Client.exe", agent);

            DeployAgent(agent, distrFolder);

            while (!queue.IsEmpty)
            {
                string[] battle;
                while (!queue.TryDequeue(out battle))
                    Thread.Sleep(10);

                try
                {
                    ProcessHelpers.KillProcess("Referee.exe", agent);
                    ProcessHelpers.KillProcess("Server.exe", agent);
                    ProcessHelpers.KillProcess("Client.exe", agent);

                    RunBattle(agent, battle);
                }
                catch (Exception e)
                {
                    Log($"Error: {e.Message}");
                    queue.Enqueue(battle);
                }

                Log($"{queue.Count} left!");
            }
        }

        private static void RunBattle(string agent,
                                      string[] battle)
        {
            const string dir = @"C:\LambdaContest\distr\Referee\bin\Debug\";
            var exe = Path.Combine(dir, "Referee.exe");
            var parameters = string.Join(" ", battle);
            ProcessHelpers.ExecuteCommand($"PsExec64 \\\\{agent} -w {dir} {exe} {parameters}", ProcessHelpers.FailIfError, Environment.CurrentDirectory);
            var resultsFolder = Path.Combine(GetAgentDistrFolder(agent), @"Referee\bin\Debug\Results");
            RoboCopy(resultsFolder, "Results");
        }

        private static void DeployAgent(string agent,
                                        string distrFolder)
        {
            deploySemaphore.Wait();
            Log($"Deploy agent {agent}");
            var agentDistrFolder = GetAgentDistrFolder(agent);
            RoboCopy(distrFolder, agentDistrFolder);
            Log($"Done deploy agent {agent}");
            deploySemaphore.Release();
        }

        private static string CreateDistributive()
        {
            const string distrFolder = "distr";

            if (!Directory.Exists(distrFolder))
                Directory.CreateDirectory(distrFolder);

            RoboCopy(GetClientFolder(), Path.Combine(distrFolder, @"Client\bin\Debug"));
            RoboCopy(GetServerFolder(), Path.Combine(distrFolder, @"Server\bin\Debug"));
            RoboCopy(GetRefereeFolder(), Path.Combine(distrFolder, @"Referee\bin\Debug"));

            return distrFolder;
        }

        private static ConcurrentQueue<string[]> GetBattlesQueue(string[] allPunters)
        {
            var battles = GetAllBattles(allPunters);
            var queue = new ConcurrentQueue<string[]>();
            foreach (var battle in battles)
                queue.Enqueue(battle);
            return queue;
        }

        private static IEnumerable<string[]> GetAllBattles(string[] allPunters)
        {
            foreach (var x in allPunters)
            foreach (var y in allPunters)
                yield return new[] { x, y };

            for (var i = 0; i < 10; i++)
                yield return GetRandomBattle(allPunters, 16).ToArray();

            foreach (var x in allPunters)
            foreach (var y in allPunters)
            foreach (var z in allPunters)
                yield return new[] { x, y, z };

            for (var i = 0; i < 10; i++)
                yield return GetRandomBattle(allPunters, 10).ToArray();
        }

        private static IEnumerable<string> GetRandomBattle(string[] allPunters, int count)
        {
            for (var i = 0; i < count; i++)
                yield return allPunters[random.Next(0, allPunters.Length - 1)];
        }

        private static string[] GetAllPunters()
        {
            return new[]
                   {
                       typeof(FriendshipPunter),
                       typeof(MaxFriendshipPunter),
                       typeof(BargeHauler3),
                       typeof(BargeHauler4),
                       typeof(GreedyEdgeChooserPunterWithStupidZergRush),
                       typeof(BargeHauler5),
                   }
                .Select(x => x.Name)
                .ToArray();
        }

        private static string[] GetAllAgents()
        {
            return Enumerable.Range(1, 72).Select(x => $"elba-{x}").ToArray();
        }

        private static string GetClientFolder() => Path.Combine(GetSolutionFolder(), @"Client\bin\Debug\");

        private static string GetServerFolder() => Path.Combine(GetSolutionFolder(), @"Server\bin\Debug\");

        private static string GetRefereeFolder() => Path.Combine(GetSolutionFolder(), @"Referee\bin\Debug\");

        private static string GetSolutionFolder() => Path.Combine(Environment.CurrentDirectory, @"..\..\..\");

        private static string GetAgentDistrFolder(string agent)
        {
            return $@"\\{agent}\C$\LambdaContest\distr";
        }

        private static void Log(string text)
        {
            try
            {
                Console.Out.WriteLine(text);
                lock (lockObject)
                    File.AppendAllLines("log.txt", new[] { $"{DateTime.Now.ToShortTimeString()} {text}" });
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(text + " " + e.Message);
                throw;
            }
        }

        public static string RoboCopy(string sourceDir,
                                      string destinationDir,
                                      IEnumerable<string> includedFilesFilter = null,
                                      IEnumerable<string> excludedFilesFilter = null,
                                      string additionalOptions = null)
        {
            var include = includedFilesFilter != null
                              ? string.Join(" ", includedFilesFilter.Select(x => $"\"{x}\""))
                              : "";
            var exclude = excludedFilesFilter != null
                              ? string.Format("/xd {0}", string.Join(" ", excludedFilesFilter.Select(x => $"\"{x}\"")))
                              : "";
            var parameters = string.Format(@"""{0}"" ""{1}"" /r:10 /w:2 /fft {2} /e /xo {3}{4}", PathHelpers.ExcludeTrailingSlash(sourceDir),
                                           PathHelpers.ExcludeTrailingSlash(destinationDir), include, exclude,
                                           additionalOptions);

            return ProcessHelpers.ExecuteCommand("robocopy.exe " + parameters, result => result.ExitCode >= 8, Environment.CurrentDirectory);
        }
    }
}