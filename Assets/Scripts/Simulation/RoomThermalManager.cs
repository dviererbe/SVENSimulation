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
            private Vector3 _size;

            private Vector3 _position;

            private float _thermalPixelSize;
            

            public RoomThermalManager(
                Vector3 size,
                Vector3 position,
                float thermalPixelSize = 1f,
                float initialTemperature = 22f)
            {
                Size = size;
                Position = position;
                _thermalPixelSize = thermalPixelSize;
            }

            public Vector3 Size { get; }

            public Vector3 Position { get; }

            public float ThermalPixelSize
            {
                get => _thermalPixelSize;
                set
                {
                    if (value != _thermalPixelSize)
                        throw new NotImplementedException();
                }
            }

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
                
            }

            /// <summary>
            /// Gets the temperature at a certain point in space.
            /// </summary>
            /// <param name="position">
            ///
            /// </param>
            /// <param name="temperatureUnit">
            ///
            /// </param>
            /// <returns>
            ///
            /// </returns>
            public Temperature GetTemperature(Vector3 position, TemperatureUnit temperatureUnit = TemperatureUnit.Kelvin)
            {
                //TODO: Implement Thermal Logic
                return new Temperature(Random.value * 5f + 20f, TemperatureUnit.Celsius);
            }
        }
    }
}