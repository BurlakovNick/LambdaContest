using System;
using System.Text;
using Core.Infrastructure;

namespace OfflinePlayer
{
    public class Transport
    {
        private readonly Serializer serializer;

        public Transport()
        {
            serializer = new Serializer();
        }

        public void Send<T>(T message)
        {
            Log($"Lets send message");
            var bytes = serializer.Serialize(message);
            Log($"Sending message {bytes}");
            Console.Out.Write(bytes);
        }

        public T Recieve<T>()
        {
            Log($"Lets recieve message");

            var buffer = new char[1];
            var sb = new StringBuilder();
            while (true)
            {
                var read = Console.In.Read(buffer, 0, 1);
                if (buffer[0] == ':')
                {
                    break;
                }

                if (read <= 0 || !char.IsDigit(buffer[0]))
                    continue;

                sb.Append(buffer[0]);
            }

            Log($"gonna parse {sb} to int");
            var size = int.Parse(sb.ToString());
            Log($"Message size {size}");

            sb.Append(':');

            buffer = new char[size];
            Console.In.ReadBlock(buffer, 0, size);
            sb.Append(buffer);

            Log($"Recieve message {sb}");

            return serializer.Deserialize<T>(sb.ToString());
        }

        private static void Log(string message)
        {
#if DEBUG
            Console.Error.WriteLine(message);
#endif
        }
    }
}