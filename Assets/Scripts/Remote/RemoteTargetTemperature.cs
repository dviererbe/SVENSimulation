using Assets.Scripts.Remote.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    class RemoteTargetTemperature : RemoteObject
    {
        public RemoteTargetTemperature(IServerConnection remoteConnection, string deviceName)
            : base(remoteConnection, deviceName)
        {
        }

        public RemoteTargetTemperature(IServerConnection remoteConnection, string getDeviceName, string setDeviceName)
            :base(remoteConnection, getDeviceName, setDeviceName)
        {
        }

        /// <summary>
        /// Loads the state of the heater
        /// </summary>
        /// <returns>
        /// float as % of opening of the heater
        /// </returns>
        public float GetState()
        {
            string serverResult = GetAttribute("sollTemp");

            if (float.TryParse(serverResult, out float value))
            {
                if (!float.IsNaN(value) && !float.IsInfinity(value))
                {
                    return value;
                }
            }

            throw new Exception("Retrieved invalid value from remote connection.");
        }

        public void SetState(float value)
        {
            SetAttribute("sollTemp", value.ToString());
        }
    }
}
