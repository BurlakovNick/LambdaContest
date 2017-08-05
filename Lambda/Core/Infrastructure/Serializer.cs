using System;
using Newtonsoft.Json;

namespace Core.Infrastructure
{
    public class Serializer
    {
        public T Deserialize<T>(string message)
        {
            var delimeter = message.IndexOf(":", StringComparison.Ordinal);
            if (delimeter == -1)
            {
                throw new Exception($"No delimeter in message: {message}");
            }

            var size = int.Parse(message.Substring(0, delimeter));
            var body = message.Substring(delimeter + 1);
            return JsonConvert.DeserializeObject<T>(body);
        }

        public string Serialize<T>(T message)
        {
            var body = JsonConvert.SerializeObject(message);
            return $"{body.Length}:{body}";
        }
    }
}