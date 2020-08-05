using Assets.Scripts.Remote.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    public class RemoteTablet : RemoteObject
    {
        public RemoteTablet(IServerConnection remoteConnection, string deviceName)
            : base(remoteConnection, deviceName)
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
            string serverResult = GetAttribute("state");

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
            SetAttribute("state", value.ToString());
        }
    }
}
