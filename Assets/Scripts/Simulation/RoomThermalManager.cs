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
        private class RoomRoomThermalManager : IRoomThermalManager
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

            public RoomRoomThermalManager(
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
                    UpdateThermalValues();
                }
            }

            private ThermalPixel CalculateUpdatedThermalPixel(ThermalPixel[,] thermalPixels, int x, int y)
            {
                int count = 0;
                float q = 0f;

                int xLength = thermalPixels.GetLength(0);
                int yLength = thermalPixels.GetLength(1);

                if (x > 0 && xLength > 1)
                {
                    q += thermalPixels[x - 1, y].Temperature.ToKelvin();
                    ++count;
                }

                if (y > 0 && yLength > 1)
                {
                    q += thermalPixels[x, y - 1].Temperature.ToKelvin();
                    ++count;
                }

                if (x < (xLength - 1) && xLength > 1)
                {
                    q += thermalPixels[x + 1, y].Temperature.ToKelvin();
                    ++count;
                }

                if (y < (yLength - 1) && yLength > 1)
                {
                    q += thermalPixels[x, y + 1].Temperature.ToKelvin();
                    ++count;
                }

                q -= count * thermalPixels[x,y].Temperature.ToKelvin();

                return new ThermalPixel(new Temperature(
                        value: ((q * ThermalMaterial.Air.GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial.Air)) / 
                                (ThermalMaterial.Air.Density * _thermalPixelSize * ThermalMaterial.Air.SpecificHeatCapacity))
                               + thermalPixels[x, y].Temperature.ToKelvin(),
                        unit: TemperatureUnit.Kelvin));
            }

            private void UpdateThermalValues()
            {
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

                    for (int x = 0; x < discreteRoomSize.x; ++x)
                    {
                        for (int y = 0; y < discreteRoomSize.y; ++y)
                        {
                            thermalPixels[x, y] = CalculateUpdatedThermalPixel(_thermalPixels, x, y);
                        }
                    }

                    _thermalPixels = thermalPixels;


                    /*
                    //This implementation has a lot of code duplication, but this will run
                    //more efficient, because we don't have to check 



                    #region left bottom corner

                    if (isInXDirectionGreaterThan1 && isInYDirectionGreaterThan1)
                    {
                        _thermalPixels[0, 0] = CalculateUpdatedThermalPixel(_thermalPixels[0, 0], _thermalPixels[1, 0], _thermalPixels[0, 1]);
                    }
                    else if (isInXDirectionGreaterThan1)
                    {
                        _thermalPixels[0, 0] = CalculateUpdatedThermalPixel(_thermalPixels[0, 0], _thermalPixels[1, 0]);
                    }
                    else if (isInYDirectionGreaterThan1)
                    {
                        _thermalPixels[0, 0] = CalculateUpdatedThermalPixel(_thermalPixels[0, 0], _thermalPixels[0, 1]);
                    }

                    #endregion

                    #region left upper corner

                    if (isInXDirectionGreaterThan1 && isInYDirectionGreaterThan1)
                    {
                        _thermalPixels[0, discreteRoomSize.y - 1] = CalculateUpdatedThermalPixel(_thermalPixels[0, discreteRoomSize.y - 1], _thermalPixels[1, discreteRoomSize.y - 1], _thermalPixels[0, discreteRoomSize.y - 2]);
                    }
                    else if (isInXDirectionGreaterThan1)
                    {
                        _thermalPixels[0, discreteRoomSize.y - 1] = CalculateUpdatedThermalPixel(_thermalPixels[0, discreteRoomSize.y - 1], _thermalPixels[1, discreteRoomSize.y - 1]);
                    }
                    else if (isInYDirectionGreaterThan1)
                    {
                        _thermalPixels[0, discreteRoomSize.y - 1] = CalculateUpdatedThermalPixel(_thermalPixels[0, discreteRoomSize.y - 1], _thermalPixels[0, discreteRoomSize.y - 2]);
                    }

                    #endregion

                    #region right upper corner

                    if (isInXDirectionGreaterThan1 && isInYDirectionGreaterThan1)
                    {
                        _thermalPixels[discreteRoomSize.y - 1, discreteRoomSize.y - 1] = CalculateUpdatedThermalPixel(_thermalPixels[0, discreteRoomSize.y - 1], _thermalPixels[1, discreteRoomSize.y - 1], _thermalPixels[0, discreteRoomSize.y - 2]);
                    }
                    else if (isInXDirectionGreaterThan1)
                    {
                        _thermalPixels[0, discreteRoomSize.y - 1] = CalculateUpdatedThermalPixel(_thermalPixels[0, discreteRoomSize.y - 1], _thermalPixels[1, discreteRoomSize.y - 1]);
                    }
                    else if (isInYDirectionGreaterThan1)
                    {
                        _thermalPixels[0, discreteRoomSize.y - 1] = CalculateUpdatedThermalPixel(_thermalPixels[0, discreteRoomSize.y - 1], _thermalPixels[0, discreteRoomSize.y - 2]);
                    }

                    #endregion


                    thermalPixels[0, 0] = CalculateUpdatedThermalPixel(thermalPixels[0, 0], )

                    /*

                    




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
                    position.y < RoomPosition.y)
                {
                    return _outsideTemperature;
                }

                Vector2Int discreteRoomPosition = new Vector2Int(
                    x: Mathf.FloorToInt((position.x - RoomPosition.x) / ThermalPixelSize),
                    y: Mathf.FloorToInt((position.y - RoomPosition.y) / ThermalPixelSize));

                if (discreteRoomPosition.x >= _thermalPixels.GetLength(0) ||
                    discreteRoomPosition.y >= _thermalPixels.GetLength(1))
                {
                    return _outsideTemperature;
                }

                return _thermalPixels[discreteRoomPosition.x, discreteRoomPosition.y].Temperature;
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