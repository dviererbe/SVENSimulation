using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Pathfinding
{
    public class Path
    {

        int listIndex = -1;
        List<Vertex> VertexPath = new List<Vertex>();

        public Path()
        {

        }

        public void AddVertex(Vertex v)
        {
            VertexPath.Add(v);
        }

        public bool HasNext()
        {
            return listIndex + 1 < VertexPath.Count;
        }

        public void RemoveLastVertex()
        {
            VertexPath.RemoveAt(VertexPath.Count - 1);
        }

        public bool TryHasNext(out Vertex nextVertex)
        {
            if ((++listIndex) < VertexPath.Count)
            {
                nextVertex = VertexPath[listIndex];
                return true;
            }
            nextVertex = null;
            return false;
        }

    }
}
