using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pathfinder
{
    class RoomGraphListe : IRoomGraph
    {
        private const float OFFSETVALUE = 1f;
        private const float MINDISTANCE = 1f;

        private List<List<int>> _adjazenzListe;

        private List<Vector2> _nodePosition;

        private List<Route> _tabelEdges;

        public RoomGraphListe()
        {
            _adjazenzListe = new List<List<int>>();
            _nodePosition = new List<Vector2>();
            _tabelEdges = new List<Route>();
        }

        public bool AddNode(float posY, float posX)
        {
            _adjazenzListe.Add(new List<int>());
            _nodePosition.Add(new Vector2(
                x: posX,
                y: posY));
            return true;
        }

        public bool AddSqaureObject(float posY, float posX, float rotation, float height, float width)
        {
            #region Offset zu den Node ermitteln

            //Nodes des Vierecks zwischenspeichern, für alle weitern Schritte
            //Ecke A
            Vector2[] squareNodePositions = new Vector2[4];
            squareNodePositions[0] = new Vector2(
                x: posX,
                y: posY );

            //Nächste Ecke B
            squareNodePositions[1] = new Vector2(
                x: posX + (float)Math.Cos(rotation) * width,
                y: posY + (float)Math.Sin(rotation) * height);


            //Nächste Ecke D
            squareNodePositions[3] = new Vector2(
                x: posX + (float)Math.Cos(rotation  + 90) * width,
                y: posY + (float)Math.Sin(rotation + 90) * height);


            //Diagonale des Rechtecks!!
            Vector2 offset = (squareNodePositions[3] - squareNodePositions[0]) + (squareNodePositions[1] - squareNodePositions[0]);

            //Direction umdrehen
            offset = -offset;

            offset.Normalize();

            #endregion

            width += 2 * OFFSETVALUE;
            height += 2 * OFFSETVALUE;

            //Ecke A
            squareNodePositions[0] += offset * OFFSETVALUE;


            //Ecke B
            squareNodePositions[1] = new Vector2(
                x: posX + (float)Math.Cos(rotation) * width,
                y: posY + (float)Math.Sin(rotation) * height);


            //Ecke D
            squareNodePositions[3] = new Vector2(
                x: posX + (float)Math.Cos(rotation + 90) * width,
                y: posY + (float)Math.Sin(rotation + 90) * height);


            //Ecke C
            squareNodePositions[2] = new Vector2(
                x: squareNodePositions[3].x + (float)Math.Cos(rotation) * width,
                y: squareNodePositions[3].y + (float)Math.Sin(rotation) * height);

            #region Controll Nodes

            int[] nodePositionsInList = new int[4];
            int j = 0;

            foreach (Vector2 node in squareNodePositions)
            {
                bool isNew = true;
                for(int i = 0; i < _nodePosition.Count; i++)
                {
                    Vector2 differenz = node - _nodePosition[i];

                    if(differenz.sqrMagnitude < Math.Pow(MINDISTANCE, 2))
                    {
                        isNew = false;
                        nodePositionsInList[j] = i;
                        break;
                    }
                }
                if(isNew)
                {
                    nodePositionsInList[j] = _nodePosition.Count;
                    _nodePosition.Add(node);
                }
                j++;
            }

            #endregion

            //unpassiebare Kannten!!
            _tabelEdges.Add(new Route(_nodePosition[nodePositionsInList[0]], _nodePosition[nodePositionsInList[1]]));
            _tabelEdges.Add(new Route(_nodePosition[nodePositionsInList[1]], _nodePosition[nodePositionsInList[2]]));
            _tabelEdges.Add(new Route(_nodePosition[nodePositionsInList[2]], _nodePosition[nodePositionsInList[3]]));
            _tabelEdges.Add(new Route(_nodePosition[nodePositionsInList[3]], _nodePosition[nodePositionsInList[0]]));

            _adjazenzListe[nodePositionsInList[0]].Add(nodePositionsInList[1]);
            _adjazenzListe[nodePositionsInList[0]].Add(nodePositionsInList[3]);

            _adjazenzListe[nodePositionsInList[1]].Add(nodePositionsInList[2]);
            _adjazenzListe[nodePositionsInList[1]].Add(nodePositionsInList[0]);

            _adjazenzListe[nodePositionsInList[2]].Add(nodePositionsInList[1]);
            _adjazenzListe[nodePositionsInList[2]].Add(nodePositionsInList[3]);

            _adjazenzListe[nodePositionsInList[3]].Add(nodePositionsInList[2]);
            _adjazenzListe[nodePositionsInList[3]].Add(nodePositionsInList[0]);

            return true;
        }

        public void MeshList()
        {
            for(int i = 0; i < _nodePosition.Count; i++)
            {
                for(int j =  i + 1; j < _nodePosition.Count; j++)
                {
                    if(!_adjazenzListe[i].Contains(j))
                    {
                        bool possible = true;
                        foreach(Route route in _tabelEdges)
                        {
                            if(route.Cut(_nodePosition[i], _nodePosition[j]))
                            {
                                possible = false;
                                break;
                            }
                        }

                        if(possible)
                        {
                            _adjazenzListe[i].Add(j);
                            _adjazenzListe[j].Add(i);
                        }
                    }
                }
            }
        }
    }
}
