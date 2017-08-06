using System.Collections.Generic;

namespace Core.Components
{
    public class DesireComponent
    {
        public HashSet<Component> Components { get; set; }
        public Component Root { get; set; }
    }
}