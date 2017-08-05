
using System;
using System.Threading;

namespace OfflinePlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            var offlinePlayer = new OfflinePlayer();
            offlinePlayer.Play();
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
    }
}
