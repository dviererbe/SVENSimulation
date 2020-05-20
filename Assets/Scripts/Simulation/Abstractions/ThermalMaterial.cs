using System;
using UnityEngine;

namespace Assets.Scripts.Simulation.Abstractions
{
    /// <remarks>
    /// WARNING: The functions are NOT thermodynamically correct implemented.
    /// Many thermodynamical depend on the temperature, pressure and other thermodynamical context.
    /// Because this is a dumb an simple thermodynamical simulation the corresponding inaccuracies will be ignored.
    /// </remarks>
    public abstract class ThermalMaterial
    { 
        public static ThermalMaterial Air = new AirThermalMaterial();
        public static ThermalMaterial Human = new HumanThermalMaterial();
        public static ThermalMaterial Wall = null;
        public static ThermalMaterial Window = null;

        private ThermalMaterial()
        {
        }

        /// <summary>
        /// Density of the <see cref="ThermalMaterial"/> in kg/m³.
        /// </summary>
        /// <remarks>
        /// Mass per unit of volume.
        /// </remarks>
        public abstract float Density { get; }

        /// <summary>
        /// Specific heat capacity of the <see cref="ThermalMaterial"/> in J/(K·kg).
        /// </summary>
        /// <remarks>
        /// Amount of energy that must be added, in the form of heat, to one unit of mass of the substance in order to cause an increase of one unit in its temperature.
        /// </remarks>
        public abstract float SpecificHeatCapacity { get; }

        /// <summary>
        /// Thermal conductivity of the <see cref="ThermalMaterial"/> in W/(m·K). 
        /// </summary>
        /// <remarks>
        /// Measure of its ability to conduct heat.
        /// </remarks>
        public abstract float ThermalConductivity { get; }

        /// <summary>
        /// Gets the heat transfer coefficient from this <see cref="ThermalMaterial"/> to another <see cref="ThermalMaterial"/>.
        /// </summary>
        /// <param name="otherThermalMaterial">
        /// The other <see cref="ThermalMaterial"/> that is involved in the heat transfer to get the heat transfer coefficient for.
        /// </param>
        /// <returns>
        /// The heat transfer coefficient from this <see cref="ThermalMaterial"/> to <paramref name="otherThermalMaterial"/> in W/(m²·K).
        /// </returns>
        public abstract float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial);

        private class AirThermalMaterial : ThermalMaterial
        {
            public override float Density => 1.225f; //at 15°C and 1013.25 hPa

            public override float SpecificHeatCapacity => 1.012f; //isobaric mass heat capacity at 20°C and 101,3kPa

            public override float ThermalConductivity => 0.025f; //at 20°C and 101,3kPa

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial)
            {
                if (otherThermalMaterial == Air)
                    return 8.1f;

                throw new NotImplementedException();
            }
        }

        private class HumanThermalMaterial : ThermalMaterial
        {
            public override float Density => 1000f; //at 20°C and 101,3kPa

            public override float SpecificHeatCapacity => 4.19f; //at 20°C

            public override float ThermalConductivity => 0.6f; //at 20°C and 101,3kPa

            public override float GetHeatTransferCoefficientToOtherThermalMaterial(ThermalMaterial otherThermalMaterial)
            {
                throw new NotImplementedException();
            }
        }
    }
}