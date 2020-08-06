using Assets.Scripts.Remote.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    public class RemoteWindow : RemoteObject
    {
        public RemoteWindow(IServerConnection remoteConnection, string deviceName)
            : base(remoteConnection, deviceName)
        {
        }

        public RemoteWindow(IServerConnection remoteConnection, string getDeviceName, string setDeviceName)
           : base(remoteConnection, getDeviceName, setDeviceName)
        {
        }

        public bool GetState()
        {
            string serverResult = GetAttribute("status");

            if (serverResult.Equals("geschlossen\n"))
            {
                return false;
            }
            else if(serverResult.Equals("offen\n"))
            {
                return true;
            }

            throw new Exception("Retrieved invalid value from remote connection.");
        }

        public void SetState(bool value)
        {
            SetAttribute("Open/Close", value.ToString());
        }
    }
}
