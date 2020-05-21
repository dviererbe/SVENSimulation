﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Simulation.Abstractions
{
    public interface IRoomThermalManager
    {
        Vector3 RoomSize { get; }

        Vector3 RoomPosition { get; }

        float ThermalPixelSize { get; set; }

        /// <summary>
        /// called on the frame when a script is enabled just before any of the Update methods are called the first time
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html">MonoBehaviour.Start</see>
        /// </summary>
        void Start();

        /// <summary>
        /// called every frame, if the MonoBehaviour is enabled
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
        /// </summary>
        void Update();

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
        Temperature GetTemperature(Vector3 position);
    }
}