using System;
using System.Collections.Generic;
using ColossalFramework.IO;
using UnityEngine;

namespace ForestBrushRevisited.Persistence
{
    public class Precision
    {
        public class Data
        {
            public ushort x;
            public ushort z;
        }

        public static Dictionary<uint, Data> data = new Dictionary<uint, Data>();
    }
}