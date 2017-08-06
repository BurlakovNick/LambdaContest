using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using Core.Contracts;
using Core.Contracts.Converters;
using Core.Objects;
using Newtonsoft.Json;

namespace Debuger
{
    class Program
    {
        static void Main(string[] args)
        {
            var punterId = int.Parse(args[0]);
            var punter = PunterFactory.Create(args[1]);

            var puntersCount = JsonConvert.DeserializeObject<int>(File.ReadAllText("..\\..\\..\\players_count.txt"));
            var mapContract = JsonConvert.DeserializeObject<MapContract>(File.ReadAllText("..\\..\\..\\map_command.txt"));
            var map = Converter.Convert(mapContract);

            var moveStrings = File.ReadAllLines("..\\..\\..\\move_commands.txt");
            var moves = moveStrings.Select(JsonConvert.DeserializeObject<MoveCommand>).ToArray();

            punter.Init(map, puntersCount, new Punter {Id = punterId});

            var currentMoves = new List<MoveCommand>();
            foreach (var move in moves)
            {
                currentMoves.Add(move);
                if (move.pass?.punter == punterId ||
                    move.claim?.punter == punterId)
                {
                    punter.Claim(new GameState {CurrentPunter = new Punter {Id = punterId}, Map = map});
                }

                map = Converter.Convert(mapContract, currentMoves);
            }
        }
    }
}
