using System.Collections.Generic;
using Core.Objects;

namespace Core.Components
{
    public class ComponentManagerState
    {
        public Node[] mines { get; set; }
        public Map map { get; set; }
        public Punter punter { get; set; }
        public List<Component> components { get; set; }
    }
}