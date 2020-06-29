using System;
using UnityEditorInternal;
using UnityEngine;

namespace Assets.Scripts.Pathfinder
{
    class Route
    {
        private Vector2 _startPoint;

        private Vector2 _endPoint;

        private Vector2 _directionVector;
        

        public Route( Vector2 startpoint, Vector2 endpoint)
        {
            _startPoint = startpoint;
            _endPoint = endpoint;
            _directionVector = endpoint - startpoint;
        }

        public bool Cut(Vector2 startPoint, Vector2 endPoint)
        {
            Vector2 directionVectorSecoundRoute = endPoint - startPoint;

            float[,] linearMatrix = new float[2, 3];

            linearMatrix[0, 0] = _directionVector.x;
            linearMatrix[0, 1] = - directionVectorSecoundRoute.x;
            linearMatrix[0, 2] = startPoint.x - _startPoint.x;

            linearMatrix[1, 0] = _directionVector.y;
            linearMatrix[1, 1] = - directionVectorSecoundRoute.y;
            linearMatrix[1, 2] = startPoint.y - _startPoint.y;

            /*   x1 - y1 = z1
             *   x2 -y2 = z2
             */
            #region Gauß-Algorithmus

            float factorRow1 = linearMatrix[0, 0];
            float factorRow0 = linearMatrix[1, 0];

            for(int i = 0; i < linearMatrix.GetLength(1); i++)
            {
                linearMatrix[1, i] = (linearMatrix[0, i] * factorRow1) - (linearMatrix[1, i] * factorRow0);
            }

            //faktorebn der Gleichung y = x + nr (y, x, r sind Vektoren)
            float n1;
            float n2;

            if (linearMatrix[1, 1] != 0)
            {
                n1 = linearMatrix[1, 2] / linearMatrix[1, 1];
            }
            else
            {
                //Geraden liegen Paralle!!
                return false;
            }

            if(linearMatrix[0, 0] != 0)
            {
                n2 = (linearMatrix[0, 2] - linearMatrix[0, 1]) / linearMatrix[0, 0];
            }
            else
            {
                //Ausgangszustand wieerherstellen
                linearMatrix[0, 0] = _directionVector.y;
                linearMatrix[0, 1] = -directionVectorSecoundRoute.y;
                linearMatrix[0, 2] = startPoint.y - _startPoint.y;

                if(linearMatrix[0, 0] != 0)
                {
                    n2 = (linearMatrix[0, 2] - linearMatrix[0, 1]) / linearMatrix[0, 0];
                }
                else
                {
                    throw new Exception("Rout is null Route");
                }
            }

            if(n1 < 0 || n1 > 1 || n2 < 0 || n2 > 1)
            {
                return false;
            }

            #endregion


            return true;
        }

    }
}
