using System;
using Core;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapName = args[0];
            var playersCount = int.Parse(args[1]);
            IServer server = new OnlineServer(new Scorer(new DistanceCalculator(), new GraphVisitor()),
                                              mapName);

            server.Start(playersCount);

            Console.ReadLine();
        }
    }
}