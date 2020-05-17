using UnityEngine;

namespace Assets.Scripts.Simulation.Abstractions
{
    public interface IThermalObject
    {
        /// <summary>
        /// Gets if the object is static and can not change its position.
        /// <see langword="true" /> if the thermal object is static and can not change its position; otherwise <see langword="false"/>.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Gets the Position of the thermal object.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Gets how large the thermal object is.
        /// </summary>
        Vector3 Size { get; }

        /// <summary>
        /// Gets the mass of the thermal object.
        /// </summary>
        /// <remarks>
        ///<see langword="float"/>
        /// </remarks>
        float Mass { get; }

        /// <summary>
        /// Gets the thermal material of the thermal object.
        /// </summary>
        /// <remarks>
        /// Used to calculate the temperature and the heat transfer from and to the the thermal object.
        /// </remarks>
        ThermalMaterial ThermalMaterial { get; }

        /// <summary>
        /// Gets the temperature of the thermal object.
        /// </summary>
        float Temperature { get; set; }
    }
}