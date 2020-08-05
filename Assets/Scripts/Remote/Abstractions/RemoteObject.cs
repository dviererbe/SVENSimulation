
namespace Assets.Scripts.Remote.Abstractions
{
    public abstract class RemoteObject
    {
        protected RemoteObject(IServerConnection remoteConnection, string deviceName)
        {
            GetDeviceName = SetDeviceName = deviceName;
            RemoteConnection = remoteConnection;
        }

        protected RemoteObject(IServerConnection remoteConnection, string getdeviceName, string setDeviceName)
        {
            GetDeviceName = getdeviceName;
            SetDeviceName = setDeviceName;
            RemoteConnection = remoteConnection;
        }

        public string GetDeviceName { get; }
        public string SetDeviceName { get; }

        protected IServerConnection RemoteConnection { get; }

        public string GetAttribute(string attribute) => RemoteConnection.ExecuteCommand(GetDeviceName, attribute, null, CommandList.Get);

        public void SetAttribute(string attribute, string value) => RemoteConnection.ExecuteCommand(SetDeviceName, attribute, value, CommandList.Get);
    }
}
