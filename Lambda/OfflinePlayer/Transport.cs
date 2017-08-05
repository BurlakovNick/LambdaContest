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

            var buffer = new char[4096];
            var sb = new StringBuilder();
            var size = 0;
            while (true)
            {
                Console.In.Read(buffer, 0, 1);
                sb.Append(buffer[0]);

                if (buffer[0] == ':')
                {
                    break;
                }

                size = size * 10 + (buffer[0] - '0');
            }

            Log($"Message size {size}");

            for (var index = 0; index < size; index += buffer.Length)
            {
                var count = Math.Min(buffer.Length, size - index);
                Console.In.ReadBlock(buffer, index, count);
                sb.Append(buffer);
            }

            Log($"Recieve message {sb}");

            return serializer.Deserialize<T>(sb.ToString());
        }

        private static void Log(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}