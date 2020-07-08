
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

        public string GetAttribute(string attribute) => RemoteConnection.ExecuteCommand(DeviceName, attribute, null, CommandList.Get);

        public void SetAttribute(string attribute, string value) => RemoteConnection.ExecuteCommand(DeviceName, attribute, value, CommandList.Get);
    }
}
