using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Pathfinding
{
    public abstract class Vertex : IEquatable<Vertex>
    {
        protected Vertex(UnityEngine.Vector2 position)
        {
            Position = position;
        }

        public UnityEngine.Vector2 Position { get; }

        public abstract IReadOnlyList<Edge> Edges { get; }

        public bool Equals(Vertex vertex)
        {
            return Object.ReferenceEquals(this, vertex);
        }
    }
}
