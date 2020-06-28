﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Remote.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Remote
{
    public class SimulatedServer : MonoBehaviour
    {
        [SerializeField]
        private bool _windowState = false;

        [SerializeField]
        private float _heaterState = 0f;

        [SerializeField]
        private float _thermometerState = 22f;

        public IServerConnection SimulatedRemoteConnection { get; private set; }

        void Start()
        {
            SimulatedRemoteConnection = new SimulatedConnection(this);
        }

        public class SimulatedConnection : IServerConnection
        {
            private readonly SimulatedServer _simulatedServer;

            public SimulatedConnection(SimulatedServer simulatedServer)
            {
                _simulatedServer = simulatedServer;
            }

            public string GetData(string device, string attribute)
            {
                if (attribute.Equals("state", StringComparison.OrdinalIgnoreCase))
                {
                    if (device.Equals("window", StringComparison.OrdinalIgnoreCase))
                    {
                        return _simulatedServer._windowState.ToString();
                    }
                    else if (device.Equals("heater", StringComparison.OrdinalIgnoreCase))
                    {
                        return _simulatedServer._heaterState.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (device.Equals("thermometer", StringComparison.OrdinalIgnoreCase))
                    {
                        return _simulatedServer._thermometerState.ToString(CultureInfo.InvariantCulture);
                    }
                }

                throw new NotImplementedException();
            }

            public string GetReadingList(string device)
            {
                throw new NotImplementedException();
            }

            public void SetData(string device, string attribute, string value)
            {
                if (attribute.Equals("state", StringComparison.OrdinalIgnoreCase))
                {
                    if (device.Equals("window", StringComparison.OrdinalIgnoreCase))
                    {
                        _simulatedServer._windowState = bool.Parse(value);
                        return;
                    }
                    else if (device.Equals("heater", StringComparison.OrdinalIgnoreCase))
                    {
                        _simulatedServer._heaterState = float.Parse(value);
                        return;
                    }
                    else if (device.Equals("thermometer", StringComparison.OrdinalIgnoreCase))
                    {
                        _simulatedServer._thermometerState = float.Parse(value);
                        return;
                    }
                }

                throw new NotImplementedException();
            }
        }
    }
}
