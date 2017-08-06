using System.Collections.Generic;
using System.Linq;

namespace Core.Components
{
    public class DesireComponent
    {
        public HashSet<Component> Components { get; set; }
        public Component Root { get; set; }

        public DesireComponent()
        {
            Components = new HashSet<Component>();
        }

        public void UpdateComponents(List<Component> stateComponents)
        {
            var dictionary = stateComponents.ToDictionary(x => x.Id);
            if (Root != null)
            {
                Root = dictionary[Root.Id];
            }
            Components = new HashSet<Component>(Components.Select(x => dictionary[x.Id]));
        }
    }
}