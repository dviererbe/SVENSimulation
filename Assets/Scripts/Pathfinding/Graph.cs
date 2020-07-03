using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
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

        private List<Vertex> _vertices;

        public Graph()
        {
            _vertices = new List<Vertex>();
        }

        public IReadOnlyList<Vertex> Vertices => _vertices;

        public Vertex AddVertex(Vector2 position)
        {
            MutableVertex vertex = new MutableVertex(position);

            _vertices.Add(vertex);

            return vertex;
        }

        public void AddEdgeBetweenVertices(Vertex endpoint1, Vertex endpoint2, float cost)
        {
            if (endpoint1.Equals(endpoint2))
                throw new NotSupportedException("Self-loops are not supported.");

            if (endpoint1.Edges.Select(edge => edge.Target).Contains(endpoint2))
                return;

            if (endpoint1 is MutableVertex mutableEndpoint1 &&
                endpoint2 is MutableVertex mutableEndpoint2)
            {
                mutableEndpoint1.AddEdge(new Edge(mutableEndpoint2, cost));
                mutableEndpoint2.AddEdge(new Edge(mutableEndpoint1, cost));
            }
            else
            {
                throw new NotSupportedException("Vertex Type is not supported.");
            }
        }

        public Vertex GetNearestVertex(Vector2 position)
        {
            throw new NotImplementedException();
        }

        public static WrapperClass FindLowestCost(List<WrapperClass> vertexList)
        {
            float lowestCost = vertexList[0].Cost;
            int indexOfLowestCost = 0;
            for(int i = 1; i < vertexList.Count; i++)
            {
                if (vertexList[i].Cost < lowestCost)
                {
                    indexOfLowestCost = i;
                    lowestCost = vertexList[i].Cost;
                }
            }

            WrapperClass bestVertex = vertexList[indexOfLowestCost];
            vertexList.RemoveAt(indexOfLowestCost);
            return bestVertex;
        }

        public static bool TryToConfirm(Path p)
        {
            Vertex vertex;
            p.TryHasNext(out vertex);
            bool foundConnectedEdge = true;
            while (foundConnectedEdge && p.HasNext())
            {
                foundConnectedEdge = false;
                Vertex nextVertex;
                p.TryHasNext(out nextVertex);
                foreach(Edge g1 in vertex.Edges)
                {
                    if(g1.Target == nextVertex)
                    {
                        foundConnectedEdge = true;
                        break;
                    }
                }
            }

            return foundConnectedEdge;
        }

        public static Path GetPathTo(Vertex start, Vertex end)
        {
            List<WrapperClass> vertexList = new List<WrapperClass>();
            Path path = new Path();
            bool endFound = false;
            vertexList.Add(new WrapperClass(start, 0));
            path.AddVertex(start);
            Vertex lastVertex = null;
            while (!endFound && vertexList.Count > 0) 
            {
                WrapperClass vertexWrapper = FindLowestCost(vertexList);

                //confirm there's a connection between the lastVertex and the vertexWrapper.Vertex
                //if not, delete the lastVertex from Path
                bool connectionCheck = false;
                foreach(Edge g in lastVertex.Edges)
                {
                    if (connectionCheck = (g.Target == vertexWrapper.Vertex))
                        break;
                }
                if (!connectionCheck)
                {
                    path.RemoveLastVertex();
                }

                path.AddVertex(vertexWrapper.Vertex);
                
                // look for all connections
                foreach(Edge g in vertexWrapper.Vertex.Edges) 
                {
                    vertexList.Add(new WrapperClass(g.Target, vertexWrapper.Cost + g.Cost));
                    //look if the end was found
                    if (g.Target == end)
                    {
                        endFound = true;
                        break;
                    }
                }
                //save the last vertex
                lastVertex = vertexWrapper.Vertex;
            }

            return null;

        }

        

    }
}
