using UnityEngine;

namespace Assets.Scripts.Roomcreation
{
    public class RoomObject
    {
        public enum RoomElement
        {
            CHAIR,
            TABLE,
            WINDOW,
            DOOR,
            HEATER,
            CLOSET,
            TABLET,
            THERMOMETER
        };

        public string Type { get; set; }

        public RoomElement Element { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float Rotation { get; set; }

        public float Sizeheight { get; set; }

        public float Sizewidth { get; set; }

        public double RotationRadians => Rotation * Mathf.PI / 180;

        public string FhemGetName { get; set; }

        public string FhemSetName { get; set; }
    }
}
