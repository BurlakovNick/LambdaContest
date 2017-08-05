
using System;
using System.Threading;
using Core;

namespace OfflinePlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            var offlinePlayer = new OfflinePlayer(new RandomPunter());
            offlinePlayer.Play();
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
