using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public sealed class Graph
    {
        private sealed class MutableVertex : Vertex
        {
            private List<Edge> _edges;

            public MutableVertex(UnityEngine.Vector2 position) : base(position)
            {
                _edges = new List<Edge>();
            }

            public override IReadOnlyList<Edge> Edges => _edges;

            public void AddEdge(Edge edge)
            {
                _edges.Add(edge);
            }
        }

        private const float CellLength = 1f;
        private const int HashTableLength = 512;
        private List<Vertex>[] _vertexHashTable;

        public Graph()
        {
            _vertexHashTable = new List<Vertex>[HashTableLength];

            for (int i = 0; i < _vertexHashTable.Length; ++i)
            {
                _vertexHashTable[i] = new List<Vertex>();
            }
        }

        public Vertex AddVertex(Vector2 position)
        {
            MutableVertex vertex = new MutableVertex(position);

            _vertexHashTable[CalculateHashIndex(position)].Add(vertex);
            
            return vertex;
        }

        private static uint CalculateHashIndex(Vector2 position)
        {
            const long Prime1 = 73856093L;
            const long Prime2 = 19349663L;
            
            long factor1 = (long)Mathf.Floor(position.x / CellLength) + 2147483648L;
            long factor2 = (long)Mathf.Floor(position.y / CellLength) + 2147483648L;

            long hashIndex = (factor1 ^ factor2) % HashTableLength;

            return (uint)hashIndex;
        }

        public void AddEdgeBetweenVertices(Vertex endpoint1, Vertex endpoint2)
        {
            if (endpoint1.Equals(endpoint2))
                throw new NotSupportedException("Self-loops are not supported.");

            if (endpoint1.Edges.Select(edge => edge.Target).Contains(endpoint2))
                return;

            if (endpoint1 is MutableVertex mutableEndpoint1 &&
                endpoint2 is MutableVertex mutableEndpoint2)
            {
                float cost = (endpoint1.Position - endpoint2.Position).sqrMagnitude;

                mutableEndpoint1.AddEdge(new Edge(mutableEndpoint2, cost));
                mutableEndpoint2.AddEdge(new Edge(mutableEndpoint1, cost));
            }
            else
            {
                throw new NotSupportedException("Vertex Type is not supported.");
            }
        }

        public Vertex GetNearestVertex(Vector2 position, int maxDistance = 20)
        {
            Vertex nearestVertex = null;

            for (int distance = 0; nearestVertex == null && distance <= maxDistance; ++distance)
            {
                foreach (Vector2 offset in GetOffsets(distance))
                {
                    foreach (var vertex in _vertexHashTable[CalculateHashIndex(position + offset)])
                    {
                        if (nearestVertex == null ||
                            position.GetDistanceTo(vertex.Position) < position.GetDistanceTo(nearestVertex.Position))
                        {
                            nearestVertex = vertex;
                        }
                    }
                }
            }

            return nearestVertex;

            IEnumerable<Vector2> GetOffsets(int distance)
            {
                if (distance == 0)
                {
                    yield return new Vector2(0, 0);
                }
                else
                {
                    Vector2 offset = new Vector2(distance * CellLength, distance * CellLength);

                    for (int i = 0; i < distance; ++i)
                    {
                        offset += new Vector2(1, 0);

                        yield return offset;
                    }

                    for (int i = 0; i < distance; ++i)
                    {
                        offset += new Vector2(0, -1);

                        yield return offset;
                    }

                    for (int i = 0; i < distance; ++i)
                    {
                        offset += new Vector2(-1, 0);

                        yield return offset;
                    }

                    for (int i = 0; i < distance; ++i)
                    {
                        offset += new Vector2(0, 1);

                        yield return offset;
                    }
                }
            }
        }

        public Path GetPathTo(Vertex start, Vertex end)
        {
            throw new NotImplementedException();
        }

        

    }
}
