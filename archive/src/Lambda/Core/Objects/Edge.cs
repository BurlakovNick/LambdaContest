using JetBrains.Annotations;

namespace Core.Objects
{
    public class Edge
    {
        [NotNull]
        public Node Source { get; set; }

        [NotNull]
        public Node Target { get; set; }

        [CanBeNull]
        public Punter Punter { get; set; }
    }
}