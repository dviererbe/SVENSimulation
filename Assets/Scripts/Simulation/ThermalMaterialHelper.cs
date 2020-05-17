using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public static class ThermalMaterialHelper
    {
        //This is not thermodynamically correct. The specific heat capacity changes depending on temperature and pressure.
        public static float GetSpecificHeatCapacity(this ThermalMaterial thermalMaterial)
        {
            switch (thermalMaterial)
            {
                case ThermalMaterial.Air:
                    return 1.0177f; //dry air (0,76N2 + 0,23O2 + 0,01Ar) has a specific heat capacity of 1.0054  kJ / (kg * K)
                                    //air at 20°C with 100% humidity has a specific heat capacity of ≈ 1,030 kJ / (kg * K)
                case ThermalMaterial.Water:
                    return 4.184f;

                default:
                    throw new NotSupportedException("Thermal material is not supported.");
            }
        }
            
        public static float GetHeatTransferCoefficientToOtherThermalMaterial(
            this ThermalMaterial thermalMaterial1,
            ThermalMaterial thermalMaterial2)
        {
            throw new NotImplementedException();
        }
    }
}
