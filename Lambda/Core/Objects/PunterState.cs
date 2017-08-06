using Core.Components;

namespace Core.Objects
{
    public class PunterState
    {
        public int movesCount { get; set; }
        public int puntersCount { get; set; }
        public int lambdasCount { get; set; }
        public DesireComponent desire { get; set; }
        public ComponentManagerState ComponentManagerState { get; set; }
    }
}