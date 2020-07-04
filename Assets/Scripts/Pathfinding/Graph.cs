//#define fixtabelegdes

using Assets.Scripts.Roomcreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            public void RemoveEdge(Vertex targe)
            {
                for(int i = 0; i < _edges.Count; i++)
                {
                    if (_edges[i].Target == targe)
                    {
                        _edges.RemoveAt(i);
                        break;
                    }
                }
            }

            public bool Contains(Vertex target)
            {
                foreach(Edge edge in Edges)
                {
                    if(edge.Target == target)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private List<Vertex> _vertices;

        private List<Edge> _tabelEdges;

        public Graph()
        {
            _vertices = new List<Vertex>();
            _tabelEdges = new List<Edge>();
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
                mutableEndpoint1.AddEdge(new Edge(mutableEndpoint2, mutableEndpoint1, cost));
                mutableEndpoint2.AddEdge(new Edge(mutableEndpoint1, mutableEndpoint2, cost));
            }
            else
            {
                throw new NotSupportedException("Vertex Type is not supported.");
            }
        }

        public void AddSqaureObject( RoomObjects obj)
        {
            #region Offset zu den Node ermitteln

            double cosinus = Math.Cos(obj.RotationRadians);
            double sinus = Math.Sin(obj.RotationRadians);

            //Minimalfehler bei der berechnug korrigieren
            sinus = Math.Round(sinus, 10);
            cosinus = Math.Round(cosinus, 10);

#if SinCosDebugging
            Debug.Log("Sin: " + sinus.ToString());
            Debug.Log("Cos: " + cosinus.ToString());
#endif

            //Nodes des Vierecks zwischenspeichern, für alle weitern Schritte
            //Ecke A
            MutableVertex[] squareVertexPositions = new MutableVertex[8];
            squareVertexPositions[0] = new MutableVertex(new Vector2(
                x: obj.PosX,
                y: obj.PosY));

            //Nächste Ecke B
            squareVertexPositions[1] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x + (float)cosinus * obj.Sizewidth,
                y: squareVertexPositions[0].Position.y + (float)sinus * obj.Sizewidth));


            //Nächste Ecke D
            squareVertexPositions[3] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x - (float)sinus * obj.Sizeheight,
                y: squareVertexPositions[0].Position.y + (float)cosinus * obj.Sizeheight));


            //Ecke C
            squareVertexPositions[2] = new MutableVertex(new Vector2(
                x: squareVertexPositions[3].Position.x + (float)cosinus * obj.Sizewidth,
                y: squareVertexPositions[3].Position.y + (float)sinus * obj.Sizewidth));


            //Mitte AB
            squareVertexPositions[4] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x + (float)cosinus * obj.Sizewidth / 2,
                y: squareVertexPositions[0].Position.y + (float)sinus * obj.Sizewidth / 2));


            //Mitte DA
            squareVertexPositions[7] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x - (float)sinus * obj.Sizeheight / 2,
                y: squareVertexPositions[0].Position.y + (float)cosinus * obj.Sizeheight / 2));

            //Mitte CD
            squareVertexPositions[6] = new MutableVertex(new Vector2(
                x: squareVertexPositions[3].Position.x + (float)cosinus * obj.Sizewidth / 2,
                y: squareVertexPositions[3].Position.y + (float)sinus * obj.Sizewidth / 2));


            //Mitte BC
            squareVertexPositions[5] = new MutableVertex(new Vector2(
                x: squareVertexPositions[1].Position.x - (float)sinus * obj.Sizeheight / 2,
                y: squareVertexPositions[1].Position.y + (float)cosinus * obj.Sizeheight / 2));

            //Kanten die nicht passiert werden dürfen anlegen
            _tabelEdges.Add(new Edge(squareVertexPositions[0], squareVertexPositions[2], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[3], squareVertexPositions[1], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[4], squareVertexPositions[6], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[5], squareVertexPositions[4], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[3], squareVertexPositions[4], 0));
            _tabelEdges.Add(new Edge(squareVertexPositions[7], squareVertexPositions[4], 0));

            //Diagonale des Rechtecks!!
            Vector2 offset = (squareVertexPositions[3].Position - squareVertexPositions[0].Position) + 
                (squareVertexPositions[1].Position - squareVertexPositions[0].Position);
            offset = -offset;   //Direction umdrehen
            offset.Normalize();

            #endregion

            float newWidth = obj.Sizewidth + (2 * OptionsManager.VertexObjectOffSet);
            float newHeigth = obj.Sizeheight + (2 * OptionsManager.VertexObjectOffSet);

            //Ecke A
            squareVertexPositions[0] = new MutableVertex(squareVertexPositions[0].Position + offset * ((float)Math.Sqrt(2) * OptionsManager.VertexObjectOffSet));

            //Ecke B
            squareVertexPositions[1] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x + (float)cosinus * newWidth,
                y: squareVertexPositions[0].Position.y + (float)sinus * newWidth));


            //Ecke D
            squareVertexPositions[3] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x - (float)sinus * newHeigth,
                y: squareVertexPositions[0].Position.y + (float)cosinus * newHeigth));
            

            //Ecke C
            squareVertexPositions[2] = new MutableVertex(new Vector2(
                x: squareVertexPositions[3].Position.x + (float)cosinus * newWidth,
                y: squareVertexPositions[3].Position.y + (float)sinus * newWidth));

            //Mitte AB
            squareVertexPositions[4] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x + (float)cosinus * newWidth / 2,
                y: squareVertexPositions[0].Position.y + (float)sinus * newWidth / 2));


            //Mitte DA
            squareVertexPositions[7] = new MutableVertex(new Vector2(
                x: squareVertexPositions[0].Position.x - (float)sinus * newHeigth / 2,
                y: squareVertexPositions[0].Position.y + (float)cosinus * newHeigth / 2));

            //Mitte CD
            squareVertexPositions[6] = new MutableVertex(new Vector2(
                x: squareVertexPositions[3].Position.x + (float)cosinus * newWidth / 2,
                y: squareVertexPositions[3].Position.y + (float)sinus * newWidth / 2));


            //Mitte BC
            squareVertexPositions[5] = new MutableVertex(new Vector2(
                x: squareVertexPositions[1].Position.x - (float)sinus * newHeigth / 2,
                y: squareVertexPositions[1].Position.y + (float)cosinus * newHeigth / 2));

            foreach (MutableVertex vertex in squareVertexPositions)
            {
                _vertices.Add(vertex);
            }
        }

        public void MeshGraph()
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                for (int j = i + 1; j < _vertices.Count; j++)
                {
                    MutableVertex vertex = (MutableVertex)_vertices[i];
                    if (!vertex.Contains(_vertices[j]))
                    {
                        bool possible = true;
                        foreach (Edge edge in _tabelEdges)
                        {
                            if (edge.CutEdge(_vertices[i].Position, _vertices[j].Position))
                            {
                                possible = false;
                                break;
                            }
                        }
                        if (possible)
                        {
                            AddEdgeBetweenVertices(_vertices[i], _vertices[j], ((Vector2)(_vertices[i].Position - _vertices[j].Position)).sqrMagnitude);
                        }
                    }
                }
            }
            //Nicht genutzet Vertex aufräumen und letzte Fragmente entfernen
            for (int i = 0; i < _vertices.Count; i++)
            {
                if (_vertices[i].Edges.Count < 5)
                {
                    CleanGraph((MutableVertex)_vertices[i]);
                }
            }
        }

        private void CleanGraph(MutableVertex vertex)
        {
            for(int i = 0; i < vertex.Edges.Count; i++)
            {
                MutableVertex nextVertex = (MutableVertex)vertex.Edges[i].Target;
                nextVertex.RemoveEdge(vertex);
                vertex.RemoveEdge(nextVertex);
                //Angrenzende Vertex Kontrollieren
                if(nextVertex.Edges.Count < 5)
                {
                    CleanGraph(nextVertex);
                }
            }
            _vertices.Remove(vertex);
        }

        public Vertex GetNearestVertex(Vector2 position)
        {
            throw new NotImplementedException();
        }

        public Path GetPathTo(Vertex start, Vertex end)
        {
            throw new NotImplementedException();
        }

        public void PrintGraph()
        {
            int[,] adjazensmatrix = new int[Vertices.Count, Vertices.Count]; 
            StringBuilder graphmatrix = new StringBuilder();
            StringBuilder vertexPositions = new StringBuilder();

            for(int j = 0; j < Vertices.Count; j++)
            {
                vertexPositions.Append(Vertices[j].Position.x.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + " " + Vertices[j].Position.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + "; ");
                foreach(Edge edge in Vertices[j].Edges)
                {
                    for(int i = 0; i < Vertices.Count; i++)
                    {
                        if(edge.Target.Equals(Vertices[i]))
                        {
                            adjazensmatrix[i, j] = 1;
                            adjazensmatrix[j, i] = 1;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < adjazensmatrix.GetLength(0); i++)
            {
                for(int j = 0; j < adjazensmatrix.GetLength(0); j++)
                {
                    graphmatrix.Append(adjazensmatrix[i, j] + " ");
                }
                graphmatrix.Append(";\r\n");
            }

            Debug.Log(graphmatrix.ToString());
            Debug.Log(vertexPositions.ToString());
        }

    }
}
