using System;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public sealed class Edge
    {
        public Edge(Vertex target, Vertex start,  float cost)
        {
            if (cost < 0)
                throw new ArgumentOutOfRangeException(nameof(cost), cost, "Negative edge costs are not allowed.");

            Target = target;
            Cost = cost;
            _directionVector = start.Position - target.Position;

            if(start.Position.x - target.Position.x != 0)
            {
                _anstieg = (start.Position.y - target.Position.y) / (start.Position.x - target.Position.x);
                _offsetYAxes = target.Position.y - target.Position.x * _anstieg;
            }
            else
            {
                //darstellen vor Linearen Funktionen der Form z.B. x = 1;
                _anstieg = start.Position.x;
                _offsetYAxes = float.PositiveInfinity;
            }

        }

        public Vertex Target { get; }

        public float Cost { get; }

        private Vector2 _directionVector;

        private float _anstieg;

        private float _offsetYAxes;

        public bool CutEdge(Vector2 startPoint, Vector2 endPoint)
        {
            //Vorbereitung Position
            Vector2 directionVectorScondeEdge = endPoint - startPoint;


            float t1; // aus x = p0 + t * a (x p0 und a sind Vektoren)
            float t2;

            float anstiegScondeEdge;

            float offsetYAxesScondeEdge;

            if (endPoint.x - startPoint.x != 0)
            {
                anstiegScondeEdge = (endPoint.y - startPoint.y) / (endPoint.x - startPoint.x);
                offsetYAxesScondeEdge = startPoint.y - startPoint.x * anstiegScondeEdge;
            }
            else
            {
                //darstellen vor Linearen Funktionen der Form z.B. x = 1;
                anstiegScondeEdge = startPoint.x;
                offsetYAxesScondeEdge = float.PositiveInfinity;
            }


            Vector2 intersection = new Vector2();

            //IF Intersection bestimmen
            //kontrolle auf paralelität
            if ( Math.Abs(anstiegScondeEdge) == Math.Abs(_anstieg))
            {
                if(offsetYAxesScondeEdge == float.PositiveInfinity && _offsetYAxes == float.PositiveInfinity)
                {
                    if(_anstieg != anstiegScondeEdge)
                    {
                        return false;
                    }
                }
                if(startPoint.y != startPoint.x * _anstieg + _offsetYAxes)
                {
                    return false;
                }
                return ControlCutOfParrallelCuts(startPoint, endPoint);
            }
            else if(offsetYAxesScondeEdge != float.PositiveInfinity && _offsetYAxes == float.PositiveInfinity)
            {
                intersection.x = _anstieg;
                intersection.y = intersection.x * anstiegScondeEdge + offsetYAxesScondeEdge;
            }
            else if(offsetYAxesScondeEdge == float.PositiveInfinity && _offsetYAxes != float.PositiveInfinity)
            {
                intersection.x = anstiegScondeEdge;
                intersection.y = intersection.x * _anstieg + _offsetYAxes;
            }
            else if (offsetYAxesScondeEdge == float.PositiveInfinity && _offsetYAxes == float.PositiveInfinity)
            {
                if (_anstieg != anstiegScondeEdge)
                {
                    return false;
                }
                return ControlCutOfParrallelCuts(startPoint, endPoint);
            }
            else
            {
                /* Der Punkt S1 ist der Schnittpunkt der Geraden f(x1) und f(x2), daraus folgt:
                 * m1 * x + n1 = m2 * x + n2 |-(m2 * x) - n1
                 * m1 * x - m2 * x = n2 - n1
                 * x * (m1 - m2) = n2 - n1  |/(m1 -m2)
                 * x = (n2 - n1)/(m1 - m2)
                 */
                intersection.x = (offsetYAxesScondeEdge - _offsetYAxes) / (_anstieg - anstiegScondeEdge);

                intersection.y = intersection.x * _anstieg + _offsetYAxes;
            }//end if Intersectionberechnung

            if (_directionVector.x != 0)
            {
                t1 = (intersection.x - Target.Position.x) / _directionVector.x;
            }
            else
            {
                if (_directionVector.y == 0)
                {
                    throw new Exception("Direction Vektor is null Vektor!!");
                }
                t1 = (intersection.y - Target.Position.y) / _directionVector.y;
            }

            if (directionVectorScondeEdge.x != 0)
            {
                t2 = (intersection.x - startPoint.x) / directionVectorScondeEdge.x;
            }
            else
            {
                if (directionVectorScondeEdge.y == 0)
                {
                    throw new Exception("Direction Vektor is null Vektor!!");
                }
                t2 = (intersection.y - startPoint.y) / directionVectorScondeEdge.y;
            }

            if (t1 > 1 || t1 < 0 || t2 > 1 || t2 < 0)
            {
                return false;
            }
            return true;
        }

        private bool ControlCutOfParrallelCuts(Vector2 startPoint, Vector2 endPoint)
        {
            float t1; // aus x = p0 + t * a (x p0 und a sind Vektoren)
            float t2;

            if (_directionVector.x != 0)
            {
                t1 = (startPoint.x - Target.Position.x) / _directionVector.x;
                t2 = (endPoint.x - Target.Position.x) / _directionVector.x;
            }
            else
            {
                if (_directionVector.y == 0)
                {
                    throw new Exception("Direction Vektor is null Vektor!!");
}
                t1 = (startPoint.y - Target.Position.y) / _directionVector.y;
                t2 = (endPoint.y - Target.Position.y) / _directionVector.y;
            }

            if ((t1 < 0 && t2 < 0) || (t2 > 1 && t1 > 1))
            {
                return false;
            }
            return true;
        }
    }
}
