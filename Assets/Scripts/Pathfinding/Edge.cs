using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public sealed class Edge
    {
        public Edge(Vertex target, float cost)
        {
            if (cost < 0)
                throw new ArgumentOutOfRangeException(nameof(cost), cost, "Negative edge costs are not allowed.");

            Target = target;
            Cost = cost;
        }

        public Vertex Target { get; }

        public float Cost { get; }
    }
}
