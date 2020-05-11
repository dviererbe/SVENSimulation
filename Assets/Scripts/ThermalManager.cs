using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Abstractions;
using UnityEngine;

namespace Assets.Scripts
{
    public class ThermalManager : IThermalManager
    {
        public const float AbsoluteZero = -273.15f;

        private float _remainingTime = 0f;

        private Vector2 _position = new Vector2(0, 0);
        private Vector2 _size = new Vector2(0, 0);

        private float[,] _thermalPixels = new float[0, 0];

        private List<IThermalObject> _thermalObjects;

        public ThermalManager()
        {
            _thermalObjects = new List<IThermalObject>();
        }

        public float ThermalPixelSize { get; set; }

        /// <summary>
        /// called on the frame when a script is enabled just before any of the Update methods are called the first time
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html">MonoBehaviour.Start</see>
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// called every frame, if the MonoBehaviour is enabled
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
        /// </summary>
        public void Update()
        {
            float passedTime = _remainingTime + Time.deltaTime;
            int thermalTicks = Convert.ToInt32(passedTime);

            _remainingTime = passedTime - Convert.ToSingle(thermalTicks);

            for (int i = 0; i < thermalTicks; ++i)
            {
                ThermalTick();
            }
        }

        private void ThermalTick()
        {
            for (int x = 0; x < _thermalPixels.GetLength(0); ++x)
            {
                for (int y = 0; y < _thermalPixels.GetLength(1); ++y)
                {
                    _thermalPixels[x, y] += 0.25f;
                }
            }
        }

        public void AddSimulatedThermalArea(Vector3 position, Vector3 size, float initialTemperature = 22f)
        {
            if (_size.x > 0f && _size.y > 0f)
            {
                throw new NotSupportedException("Currently does not support multiple simulated thermal areas.");
            }

            if (size.x <= 0f)
            {
                throw new ArgumentOutOfRangeException($"{nameof(size)}.{nameof(size.x)}", size.x, "Size in x-direction has to be larger than zero.");
            }

            if (size.y <= 0f)
            {
                throw new ArgumentOutOfRangeException($"{nameof(size)}.{nameof(size.y)}", size.y, "Size in y-direction has to be larger than zero.");
            }

            _size = size;
            _position = position;

            _thermalPixels = new float[Mathf.FloorToInt(size.x / ThermalPixelSize), Mathf.FloorToInt(size.y / ThermalPixelSize)];
            //Debug.Log("size: " + _thermalPixels.GetLength(0) + "   1: "+_thermalPixels.GetLength(1));

            for (int x = 0; x < _thermalPixels.GetLength(0); ++x)
            {
                for (int y = 0; y < _thermalPixels.GetLength(1); ++y)
                {
                    _thermalPixels[x, y] = initialTemperature;
                }
            }
        }

        public void RegisterThermalObject(IThermalObject thermalObject)
        {
            _thermalObjects.Add(thermalObject);
        }

        public float GetTemperature(Vector3 position)
        {
            if (position.x < _position.x ||
                position.x > (_position.x + _size.x - 1) ||
                position.y < _position.y ||
                position.y > (_position.y + _size.y - 1))
            {
                return AbsoluteZero;
            }
            else
            {
                Vector2Int relativePosition = new Vector2Int(
                    x: Mathf.FloorToInt((position.x - _position.x) / ThermalPixelSize),
                    y: Mathf.FloorToInt((position.y - _position.y) / ThermalPixelSize));
                //Debug.Log("Pos x: " + relativePosition.x + " y: " + relativePosition.y);
                return _thermalPixels[relativePosition.x, relativePosition.y];
            }
        }
    }
}
