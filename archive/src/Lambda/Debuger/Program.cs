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

            var moveMessage = JsonConvert.DeserializeObject<MoveMessage>(File.ReadAllText("..\\..\\..\\state.txt"));

            var scorer = new Scorer(new DistanceCalculator(), new GraphVisitor());
            var punter = new FriendshipPunter(scorer);
            var gameState = InitGameState(moveMessage.state, moveMessage.move.moves, scorer, punter);

            punter.Claim(new GameState { CurrentPunter = new Punter { Id = punterId }, Map = gameState.Map });


            //var puntersCount = JsonConvert.DeserializeObject<int>(File.ReadAllText("..\\..\\..\\players_count.txt"));
            //var mapContract = JsonConvert.DeserializeObject<MapContract>(File.ReadAllText("..\\..\\..\\map_command.txt"));
            //var map = Converter.Convert(mapContract);
            //
            //var moveStrings = File.ReadAllLines("..\\..\\..\\move_commands.txt");
            //var moves = moveStrings.Select(JsonConvert.DeserializeObject<MoveCommand>).ToArray();
            //
            //punter.Init(map, puntersCount, new Punter {Id = punterId});
            //
            //var currentMoves = new List<MoveCommand>();
            //foreach (var move in moves)
            //{
            //    currentMoves.Add(move);
            //    if (move.pass?.punter == punterId ||
            //        move.claim?.punter == punterId)
            //    {
            //        punter.Claim(new GameState {CurrentPunter = new Punter {Id = punterId}, Map = map});
            //    }
            //
            //    map = Converter.Convert(mapContract, currentMoves);
            //}
        }

        private static GameState InitGameState(GameStateMessage state, List<MoveCommand> moves, IScorer scorer, IPunter punter)
        {
            state.Moves.AddRange(moves);

            var map = Converter.Convert(state.MapContract, state.Moves);
            scorer.State = state.ScorerState;
            punter.State = state.PunterState;

            var gameState = new GameState { Map = map, CurrentPunter = new Punter { Id = state.MyPunter } };
            return gameState;
        }
    }
}
