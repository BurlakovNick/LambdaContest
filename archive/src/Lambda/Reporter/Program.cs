using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reporter
{
    class Program
    {
        private static readonly Regex regex = new Regex(@"Map: (?<map>.*)\r\n(.* (?<name>.*) .*:\t(?<score>\d+)\r\n)+", RegexOptions.Compiled);

        static void Main(string[] args)
        {
            var resultsFolder = Path.Combine(Environment.CurrentDirectory, @"..\..\..\Distributor\bin\Debug\Results");
            var battleFiles = Directory.GetFiles(resultsFolder, "*.txt", SearchOption.TopDirectoryOnly);
            var battleResults = battleFiles.Select(ParseBattle).ToArray();

            var resumes = new Dictionary<string, PunterResume>();
            foreach (var battleResult in battleResults)
            {
                var puntersCount = battleResult.PunterNames.Length;
                foreach (var round in battleResult.Rounds)
                {
                    var winner = round.Punters.OrderByDescending(x => x.Score).First();
                    if (!resumes.ContainsKey(winner.Name))
                        resumes.Add(winner.Name, new PunterResume
                                                 {
                                                     Name = winner.Name,
                                                     MapResumes = new Dictionary<string, Dictionary<int, int>>()
                                                 });

                    var punterMapResumes = resumes[winner.Name].MapResumes;
                    if (!punterMapResumes.ContainsKey(round.Map))
                        punterMapResumes.Add(round.Map, new Dictionary<int, int>());

                    if (!punterMapResumes[round.Map].ContainsKey(puntersCount))
                        punterMapResumes[round.Map].Add(puntersCount, 0);

                    punterMapResumes[round.Map][puntersCount]++;
                }
            }

            MaxWins(resumes);

            var battleSizes = resumes.SelectMany(x => x.Value.MapResumes.Values.SelectMany(y => y.Keys))
                                     .Distinct()
                                     .OrderBy(x => x)
                                     .ToArray();
            foreach (var battleSize in battleSizes)
                MaxWinsByPuntersCount(resumes, battleSize);

            var maps = resumes.SelectMany(x => x.Value.MapResumes.Keys).Distinct().ToArray();
            foreach (var map in maps)
                MaxWinsByMap(resumes, map);
        }

        private static void MaxWins(Dictionary<string, PunterResume> resumes)
        {
            Console.Out.WriteLine("#############");
            Console.Out.WriteLine("Max Wins Punter");
            Console.Out.WriteLine("#############");
            var maxWinsPunters = resumes.Select(x => new
                                                     {
                                                         x.Value.Name,
                                                         Wins = x.Value.MapResumes.Sum(y => y.Value.Sum(z => z.Value))
                                                     })
                                        .OrderByDescending(x => x.Wins)
                                        .ToArray();


            foreach (var maxWinsPunter in maxWinsPunters)
                Console.Out.WriteLine($"{maxWinsPunter.Name}: {maxWinsPunter.Wins}");
            Console.Out.WriteLine("#############");
            Console.Out.WriteLine("");
        }

        private static void MaxWinsByPuntersCount(Dictionary<string, PunterResume> resumes,
                                                  int puntersCount)
        {
            Console.Out.WriteLine("#############");
            Console.Out.WriteLine("Max Wins Punter By Punters Count " + puntersCount);
            Console.Out.WriteLine("#############");
            var maxWinsPunters = resumes.Select(x => new
                                                     {
                                                         x.Value.Name,
                                                         Wins = x.Value.MapResumes.Sum(y => y.Value.TryGetValue(puntersCount, out var r) ? r : 0)
                                                     })
                                        .OrderByDescending(x => x.Wins)
                                        .ToArray();


            foreach (var maxWinsPunter in maxWinsPunters)
                Console.Out.WriteLine($"{maxWinsPunter.Name}: {maxWinsPunter.Wins}");
            Console.Out.WriteLine("#############");
            Console.Out.WriteLine("");
        }

        private static void MaxWinsByMap(Dictionary<string, PunterResume> resumes,
                                         string map)
        {
            Console.Out.WriteLine("#############");
            Console.Out.WriteLine("Max Wins Punter By Map " + map);
            Console.Out.WriteLine("#############");
            var maxWinsPunters = resumes.Select(x => new
                                                     {
                                                         x.Value.Name,
                                                         Wins = x.Value.MapResumes.TryGetValue(map, out var r) ? r.Sum(z => z.Value) : 0
                                                     })
                                        .OrderByDescending(x => x.Wins)
                                        .ToArray();


            foreach (var maxWinsPunter in maxWinsPunters)
                Console.Out.WriteLine($"{maxWinsPunter.Name}: {maxWinsPunter.Wins}");
            Console.Out.WriteLine("#############");
            Console.Out.WriteLine("");
        }

        private static BattleResults ParseBattle(string fileName)
        {
            var fileContent = File.ReadAllText(fileName);
            var roundResults = new List<RoundResult>();
            foreach (Match match in regex.Matches(fileContent))
            {
                var map = match.Groups["map"].Value;
                var namesCount = match.Groups["name"].Captures.Count;
                var punters = new List<Punter>();
                for (var i = 0; i < namesCount; i++)
                {
                    var name = match.Groups["name"].Captures[i].Value;
                    var score = match.Groups["score"].Captures[i].Value;
                    punters.Add(new Punter { Name = name, Score = int.Parse(score) });
                }
                roundResults.Add(new RoundResult { Map = map, Punters = punters.ToArray() });
            }
            return new BattleResults
                   {
                       PunterNames = Path.GetFileNameWithoutExtension(fileName)
                                         .Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
                                         .Where(x => x != "VS")
                                         .ToArray(),
                       Rounds = roundResults.ToArray()
                   };
        }

        private class BattleResults
        {
            public string[] PunterNames { get; set; }
            public RoundResult[] Rounds { get; set; }
        }

        private class RoundResult
        {
            public string Map { get; set; }
            public Punter[] Punters { get; set; }
        }

        private class Punter
        {
            public string Name { get; set; }
            public int Score { get; set; }
        }

        private class PunterResume
        {
            public string Name { get; set; }
            public Dictionary<string, Dictionary<int, int>> MapResumes { get; set; }
        }
    }
}