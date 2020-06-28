using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public class ThermalWindow : IThermalObject
    {
        private bool _isOpen = false;

        public ThermalWindow(Vector3 position, Vector3 size, bool isOpen = false)
        {
            _isOpen = isOpen;
            
            Position = position;
            Size = size;
            ThermalMaterial = isOpen ? ThermalMaterial.WindowOpen : ThermalMaterial.WindowClosed;
            ThermalSurfaceArea = size.x * size.y;
            Temperature = Temperature.FromCelsius(OptionsManager.InitialRoomTemperature).ToKelvin();
        }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (value != _isOpen)
                {
                    _isOpen = value;
                    ThermalMaterial = _isOpen ? ThermalMaterial.WindowOpen : ThermalMaterial.WindowClosed;
                }
            }
        }

        /// <summary>
        /// Gets if the <see cref="IThermalObject"/> can not change its position.
        /// <see langword="true" /> can not change its position; otherwise <see langword="false"/>.
        /// </summary>
        public bool CanNotChangePosition => true;

        /// <summary>
        /// Gets the absolute (global) Position of the <see cref="IThermalObject"/> in m.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Gets how large the <see cref="IThermalObject"/> is in m (meter).
        /// </summary>
        public Vector3 Size { get; }

        /// <summary>
        /// Gets the area of the surface of the <see cref="IThermalObject"/> in m² (square meter).
        /// </summary>
        public float ThermalSurfaceArea { get; }

        /// <summary>
        /// Gets the <see cref="IThermalObject.ThermalMaterial"/> of the <see cref="IThermalObject"/>.
        /// </summary>
        /// <remarks>
        /// Used to calculate the temperature and the heat transfer from and to the the <see cref="IThermalObject"/>.
        /// </remarks>
        public ThermalMaterial ThermalMaterial { get; private set; }

        /// <summary>
        /// Gets the temperature of the <see cref="IThermalObject"/>.
        /// </summary>
        public Temperature Temperature { get; private set; }

        /// <summary>
        /// A <see cref="IRoomThermalManager"/> signals the <see cref="IThermalObject"/> that the thermal simulation was started.
        /// </summary>
        /// <param name="roomThermalManager">
        /// The <see cref="IRoomThermalManager"/> that starts the thermal simulation with this <see cref="IThermalObject"/>. 
        /// </param>
        public void ThermalStart(IRoomThermalManager roomThermalManager)
        {
            //Do nothing
        }

        /// <summary>
        /// Is called from the <see cref="IThermalObject"/> once per thermal update.
        /// </summary>
        /// <param name="transferredHeat">
        /// The heat that was transferred to the <see cref="IThermalObject"/> during the thermal update in J (Joule).
        /// </param>
        public void ThermalUpdate(float transferredHeat)
        {
            if (transferredHeat == 0f)
                return;

            Temperature = Temperature.FromKelvin(
                Temperature.Value +
                transferredHeat /
                (ThermalSurfaceArea * OptionsManager.ThermalPixelSize * ThermalMaterial.Density *
                 ThermalMaterial.SpecificHeatCapacity));
        }
    }
}
