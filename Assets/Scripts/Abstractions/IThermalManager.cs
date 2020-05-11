using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Abstractions
{
    public interface IThermalManager
    {
        float ThermalPixelSize { get; set; }

        void AddSimulatedThermalArea(Vector3 position, Vector3 size, float initialTemperature = 22f);

        void RegisterThermalObject(IThermalObject thermalObject);

        /// <summary>
        /// Gets the temperature at a certain point in space.
        /// </summary>
        /// <param name="position">
        ///
        /// </param>
        /// <returns>
        ///
        /// </returns>
        float GetTemperature(Vector3 position);

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
    }
}
