using UnityEngine;

namespace ForestBrushRevisited.Persistence
{
    public class XmlInputKey
    {
        public string Name { get; set; }
        public KeyCode Key { get; set; }
        public bool Control { get; set; }
        public bool Shift { get; set; }
        public bool Alt { get; set; }
    }
}
