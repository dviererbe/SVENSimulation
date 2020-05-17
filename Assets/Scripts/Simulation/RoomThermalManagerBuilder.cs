using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public partial class RoomThermalManagerBuilder
    {
        private Vector3? _position;
        private Vector3? _size;
        private Temperature _initialTemperature = new Temperature(22f, TemperatureUnit.Celsius);

        public RoomThermalManagerBuilder()
        {

        }

        public Vector3 Position
        {
            get
            {
                if (_position.HasValue)
                    return _position.Value;

                throw new InvalidOperationException("No value was set previously.");
            }
            set => _position = value;
        }

        public Vector3 Size
        {
            get
            {
                if (_size.HasValue)
                    return _size.Value;

                throw new InvalidOperationException("No value was set previously.");
            }
            set => _size = value;
        }

        public float ThermalPixelSize { get; set; } = 1f;

        public Temperature InitialTemperature { get; set; } = new Temperature(22f, TemperatureUnit.Celsius);

        public RoomThermalManagerBuilder AddThermalObject(IThermalObject thermalObject)
        {
            throw new NotImplementedException();
        }

        public IThermalManager Build()
        {
            if (!_size.HasValue)
                throw new InvalidOperationException($"No value for the property {nameof(Size)} was set previously.");

            if (!_position.HasValue)
                throw new InvalidOperationException($"No value for the property {nameof(Position)} was set previously.");

            return new RoomThermalManager(_size.Value, _position.Value, ThermalPixelSize, InitialTemperature);
        }
    }
}
