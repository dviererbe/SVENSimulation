using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public abstract class Vertex : IEquatable<Vertex>
    {

        protected Vertex(Vector2 position)
        {
            Position = position;
        }

        public Vector2 Position { get; }

        public int ID { get; }

        public abstract IReadOnlyList<Edge> Edges { get; }

        public bool Equals(Vertex vertex)
        {
            if(Position == vertex.Position)
            {
                return true;
            }
            return false;
        }
    }
}
