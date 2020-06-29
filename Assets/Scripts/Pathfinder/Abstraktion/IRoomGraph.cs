using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Pathfinder
{
    interface IRoomGraph
    {
        public bool AddNode(float posY, float posX);

        public bool AddSqaureObject(float posY, float posX, float rotation, float height, float width);

        public void MeshList();


    }
}
