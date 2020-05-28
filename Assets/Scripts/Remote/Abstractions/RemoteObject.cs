using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.MemoryProfiler;

namespace Assets.Scripts.Remote.Abstractions
{
    public abstract class RemoteObject
    {
        protected RemoteObject(IServerConnection remoteConnection, string deviceName)
        {
            DeviceName = deviceName;
            RemoteConnection = remoteConnection;
        }

        public string DeviceName { get; }

        protected IServerConnection RemoteConnection { get; }

        public string GetAttribute(string attribute) => RemoteConnection.GetData(DeviceName, attribute);

        public void SetAttribute(string attribute, string value) => RemoteConnection.SetData(DeviceName, attribute, value);
    }
}
