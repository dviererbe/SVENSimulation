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

            public MutableVertex(Vector2 position) : base(position)
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

        public static ExpandableVertex FindLowestCost(List<ExpandableVertex> vertexList)
        {
            float lowestCost = vertexList[0].Cost;
            int indexOfLowestCost = 0;

            for (int i = 1; i < vertexList.Count; i++)
            {
                if (vertexList[i].Cost < lowestCost)
                {
                    indexOfLowestCost = i;
                    lowestCost = vertexList[i].Cost;
                }
            }

            ExpandableVertex bestVertex = vertexList[indexOfLowestCost];
            vertexList.RemoveAt(indexOfLowestCost);
            return bestVertex;
        }

        /// <summary>
        /// The method runs as long as there's no end found or as long as there're vertices to expand.
        /// For any new found vertex, an object of the "ExpandableVertex" is instantiated, where the current path gets saved. That way, there can't be
        /// any conflicts with "jumping in the graph"
        /// The method won't append a vertex that has been expanded already
        /// And to increase performance, whe method "InsertBetterVertex" was written
        /// </summary>
        /// <param name="start"></param> Start Vertex
        /// <param name="end"></param> End Vertex
        /// <param name="p"></param> The way we pass the result
        /// <returns></returns>
        public static bool GetPathTo(Vertex start, Vertex end, out Path p)
        {
            p = new Path();
            // Whether the user was so funny to give the same start and end coordinate
            if (start.Equals(end))
            {
                p.AddVertex(start);
                return true;
            }
            List<ExpandableVertex> toExpandVertices = new List<ExpandableVertex>();
            List<Vertex> listOfAlreadyExpandedVertices = new List<Vertex>();

            bool endFound = false;
            toExpandVertices.Add(new ExpandableVertex(start, 0, p));

            //As long as we haven't found an end and there're still possibilities
            while (!endFound && toExpandVertices.Count > 0)
            {
                ExpandableVertex vertexWrapper = FindLowestCost(toExpandVertices);
                p = vertexWrapper.CurrPath;
                p.AddVertex(vertexWrapper.Vertex);

                // look for all connections
                foreach (Edge edge in vertexWrapper.Vertex.Edges)
                {
                    if (edge.Target.Equals(end))
                    {
                        p.AddVertex(end);
                        endFound = true;
                        break;
                    }
                    else if (!listOfAlreadyExpandedVertices.Contains(edge.Target))
                    {
                        InsertBetterVertex(toExpandVertices, new ExpandableVertex(edge.Target, vertexWrapper.Cost + edge.Cost, p));
                    }
                    //look if the end was found

                }
                listOfAlreadyExpandedVertices.Add(vertexWrapper.Vertex);
            }
            return endFound;
        }

        /// <summary>
        /// for instance: We have the graph:
        ///     a 2---2 b
        ///     5\  c  /3
        /// Where a finds a new ExpandableVertex "c" with the cost of 5 and "b" with the cost of 2
        /// As b also finds the ExpandableVertex "c", but with lower cost than "a" does, the ExpandableVertex "a" found get's kicked out
        /// </summary>
        /// <param name="list"></param>
        /// <param name="v"></param>
        private static void InsertBetterVertex(List<ExpandableVertex> list, ExpandableVertex v)
        {
            bool found = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Vertex == v.Vertex)
                {
                    found = true;
                    if (v.Cost < list[i].Cost)
                    {
                        list[i] = v;
                    }
                    break;
                }
            }
            if (!found)
            {
                list.Add(v);
            }
        }
    }
}
