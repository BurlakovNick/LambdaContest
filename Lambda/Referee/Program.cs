using System;
using System.Linq;
using Core;

namespace Referee
{
    class Program
    {
        static void Main(string[] args)
        {
            var type = typeof(IPunter);
            var punterTypes = AppDomain.CurrentDomain.GetAssemblies()
                                       .SelectMany(s => s.GetTypes())
                                       .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                                       .ToArray();

            var firstName = args[0];
            var secondName = args[1];
            var first = PunterFactory.Create(firstName);
            var second = PunterFactory.Create(secondName);
        }
    }
}