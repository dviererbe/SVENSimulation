using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Simulation
{
    partial class RoomThermalManagerBuilder
    {
        private class RoomThermalManager : IThermalManager
        {
            private readonly struct ThermalPixel
            {
                public readonly Temperature Temperature;

                public ThermalPixel(Temperature temperature)
                {
                    Temperature = temperature;
                }
            }

            private Vector3 _upperRightCorner;

            private float _thermalPixelSize;

            private Temperature _outsideTemperature;

            private ThermalPixel[,] _thermalPixels;

            private float _remainingTime = 0f;

            public RoomThermalManager(
                Vector3 roomSize,
                Vector3 roomPosition,
                float thermalPixelSize,
                Temperature initialTemperature)
            {
                RoomSize = roomSize;
                RoomPosition = roomPosition;
                _thermalPixelSize = thermalPixelSize;
                _outsideTemperature = Temperature.FromCelsius(10f);

                DrawThermalPixels(initialTemperature);
            }

            public Vector3 RoomSize { get; }

            public Vector3 RoomPosition { get; }

            public Temperature OutsideTemperature
            {
                get => _outsideTemperature;
                set
                {
                    if (value != _outsideTemperature)
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            public float ThermalPixelSize
            {
                get => _thermalPixelSize;
                set
                {
                    if (Mathf.Abs(_thermalPixelSize - value) > 0.01f)
                    {
                        RedrawThermalPixels(value);
                    }
                }
            }

            /// <summary>
            /// called on the frame when a script is enabled just before any of the Update methods are called the first time
            /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html">MonoBehaviour.Start</see>
            /// </summary>
            public void Start()
            {
                //Just for Testing Purpose...
                //TODO: REMOVE THIS IN PRODUCTION
                _thermalPixels[0, 0] = new ThermalPixel(Temperature.FromCelsius(80));
            }

            /// <summary>
            /// called every frame, if the MonoBehaviour is enabled
            /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
            /// </summary>
            public void Update()
            {
                float passedTime = _remainingTime + Time.deltaTime;
                int thermalTicks = Convert.ToInt32(passedTime);

                _remainingTime = passedTime - thermalTicks;

                for (int i = 0; i < thermalTicks; ++i)
                {
                    ThermalTick();
                }
            }

            private void ThermalTick()
            {
                return;

                lock (_thermalPixels)
                {
                    Vector2Int discreteRoomSize = new Vector2Int(
                        x: _thermalPixels.GetLength(0),
                        y: _thermalPixels.GetLength(1));

                    if (discreteRoomSize.x == 0 || discreteRoomSize.y == 0)
                        return;

                    if (discreteRoomSize.x == 1 && discreteRoomSize.y == 1)
                        return;

                    ThermalPixel[,] thermalPixels = new ThermalPixel[discreteRoomSize.x, discreteRoomSize.y];

                    /*

                    thermalPixels[0, 0]




                    //left
                    if (discreteRoomSize.x > 0)
                    {
                        if (discreteRoomSize.y > 0)
                    }

                    //right line
                    if (discreteRoomSize.x > 1)
                    {

                    }


                    for (int x = 0; x < _thermalPixels.GetLength(0); ++x)
                    {
                        for (int y = 0; y < _thermalPixels.GetLength(0); ++y)
                        {

                        }
                    }

                    _thermalPixels = thermalPixels;
                    */
                }
            }

            /// <summary>
            /// Gets the temperature at a certain point in space.
            /// </summary>
            /// <param name="position">
            /// Position (relative to the game world) where the temperature should be probed.
            /// </param>
            /// <returns>
            /// The temperature at the specified <paramref name="position"/> as <see cref="Temperature"/>.
            /// </returns>
            public Temperature GetTemperature(Vector3 position)
            {
                if (position.x < RoomPosition.x || 
                    position.y < RoomPosition.y || 
                    position.x >= _upperRightCorner.x || 
                    position.y >= _upperRightCorner.y)
                {
                    return _outsideTemperature;
                }

                return Temperature.FromCelsius(Random.Range(15, 25));
            }

            private void DrawThermalPixels(Temperature initialTemperature)
            {
                if (_thermalPixelSize <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(_thermalPixelSize), _thermalPixelSize, "Thermal-Pixel Size can't be negative.");
                }

                if (RoomSize.x <= 0 || RoomSize.y <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(RoomSize), RoomSize, "Size of room can't be negative.");
                }

                //The size of the room in amounts of thermal pixels.
                Vector2Int discreteRoomSize = new Vector2Int(
                    x: Mathf.RoundToInt(RoomSize.x / ThermalPixelSize), 
                    y: Mathf.RoundToInt(RoomSize.y / ThermalPixelSize));

                ThermalPixel[,] thermalPixels = new ThermalPixel[discreteRoomSize.x, discreteRoomSize.y];

                for (int x = 0; x < discreteRoomSize.x; ++x)
                {
                    for (int y = 0; y < discreteRoomSize.y; ++y)
                    {
                        thermalPixels[x,y] = new ThermalPixel(initialTemperature.ToKelvin());
                    }
                }

                _thermalPixels = thermalPixels;
                _upperRightCorner = new Vector3(
                    x: RoomPosition.x + discreteRoomSize.x * ThermalPixelSize,
                    y: RoomPosition.y + discreteRoomSize.y * ThermalPixelSize,
                    z: RoomPosition.z);
            }

            private void RedrawThermalPixels(float thermalPixelSize)
            {
                lock (_thermalPixels)
                {
                    if (thermalPixelSize <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(thermalPixelSize), thermalPixelSize, "Thermal-Pixel Size can't be negative.");
                    }

                    //The size of the room in amounts of thermal pixels.
                    Vector2Int discreteRoomSize = new Vector2Int(
                        x: Mathf.RoundToInt(RoomSize.x / thermalPixelSize),
                        y: Mathf.RoundToInt(RoomSize.y / thermalPixelSize));

                    ThermalPixel[,] thermalPixels = new ThermalPixel[discreteRoomSize.x, discreteRoomSize.y];

                    for (int x = 0; x < discreteRoomSize.x; ++x)
                    {
                        for (int y = 0; y < discreteRoomSize.y; ++y)
                        {
                            Vector3 thermalPixelPosition = new Vector3(
                                x: (x + 0.5f) * thermalPixelSize,
                                y: (y + 0.5f) * thermalPixelSize);

                            thermalPixels[x, y] = new ThermalPixel(GetTemperature(thermalPixelPosition).ToKelvin());
                        }
                    }

                    _thermalPixels = thermalPixels;
                    _upperRightCorner = new Vector3(
                        x: RoomPosition.x + discreteRoomSize.x * thermalPixelSize,
                        y: RoomPosition.y + discreteRoomSize.y * thermalPixelSize,
                        z: RoomPosition.z);
                    _thermalPixelSize = thermalPixelSize;
                }
            }
        }
    }
}