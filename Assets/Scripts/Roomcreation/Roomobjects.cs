using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Roomcreation
{
    public class RoomObjects
    {
        public enum RoomElement
        {
            CHAIR,
            TABLE,
            WINDOW,
            DOOR,
            HEATER,
            CLOSET
        };

        private string _type;
        private float _posX;
        private float _posY;
        private float _rotation;
        private RoomElement _roomElement;
        private float _sizeheight;
        private float _sizewidth;

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

        public float PosX
        {
            get { return _posX; }
            set { _posX = value; }
        }

        public float PosY
        {
            get { return _posY; }
            set { _posY = value; }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public float Sizeheight
        {
            get { return _sizeheight; }
            set { _sizeheight = value; }
        }
        public float Sizewidth
        {
            get { return _sizewidth; }
            set { _sizewidth = value; }
        }

        public double RotationRadians
        {
            get
            {
                return Rotation * Math.PI / 180;
            }
        }
    }
}
