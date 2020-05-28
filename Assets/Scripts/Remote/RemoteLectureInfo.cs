using Assets.Scripts.Remote.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    public class RemoteLectureInfo : RemoteObject
    {
        public RemoteLectureInfo(IServerConnection remoteConnection, string deviceName)
            : base(remoteConnection, deviceName)
        {
        }

        public bool GetState()
        {
            string serverResult = GetAttribute("state");

            if (bool.TryParse(serverResult, out bool value))
            {
                return value;
            }

            throw new Exception("Retrieved invalid value from remote connection.");
        }

        public void SetState(bool value)
        {
            SetAttribute("state", value.ToString());
        }
    }
}
