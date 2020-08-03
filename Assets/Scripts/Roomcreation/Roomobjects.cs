using UnityEngine;

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
            CLOSET,
            TABLET
        };

        private string _type;
        private float _positionX;
        private float _positionY;
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

        public float PositionX
        {
            get { return _positionX; }
            set { _positionX = value; }
        }

        public float PositionY
        {
            get { return _positionY; }
            set { _positionY = value; }
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
                return Rotation * Mathf.PI / 180;
            }
        }
    }
}
