using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Abstractions
{
    public interface IThermalObject
    {
        /// <summary>
        /// Gets if the object is static and can not change its position.
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
        float Mass { get; }

        /// <summary>
        /// Gets the temperature of the thermal object.
        /// </summary>
        float Temperature { get; }

        /// <summary>
        /// WIP (may be removed/replaced in future versions)
        ///
        /// Exchanged thermal energy with the thermal object.
        /// </summary>
        /// <param name="energy">
        /// Amount of thermal energy in watts. If positive the thermal object receives thermal energy and if negative it looses thermal energy.
        /// </param>
        void TransferHeat(float energy);
    }
}
