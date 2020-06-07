using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Roomcreation
{
    class RoomObjects
    {
        public enum RoomElement
        {
            CHAIR,
            TABLE
        };

        private string _type;
        private int _posX;
        private int _posY;
        private float _rotation;
        private int _roomElement;

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public RoomElement Element
        {
            get { return _roomElement; }
            set { _roomElement = value; }
        }

        public int PosX
        {
            get { return _posX; }
            set { _posX = value; }
        }

        public int PosY
        {
            get { return _posY; }
            set { _posY = value; }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }
    }
}
