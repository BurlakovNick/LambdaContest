
using System;
using System.Threading;
using Core;

namespace OfflinePlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            var scorer = new Scorer(new DistanceCalculator(), new GraphVisitor());
            var graphVisitor = new GraphVisitor();
            var punter = new BargeHauler4(scorer, graphVisitor);
            var offlinePlayer = new OfflinePlayer(punter, scorer);

            offlinePlayer.Play();
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }
}
