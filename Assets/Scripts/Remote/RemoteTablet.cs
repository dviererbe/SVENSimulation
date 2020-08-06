using Assets.Scripts.Remote.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    class RemoteTablet 
    {
        private RemoteTargetTemperature _remoteTargetTemperature;
        private RemoteWindow _remoteWindow;

        public RemoteTablet(RemoteTargetTemperature remoteTargetTemperature, RemoteWindow remoteWindow)
        {
            _remoteTargetTemperature = remoteTargetTemperature;
            _remoteWindow = remoteWindow;
        }

        public float getTargetTemperature()
        {
            return _remoteTargetTemperature.GetState();
        }

        public void setTargetTemperatur(float value)
        {
            _remoteTargetTemperature.SetState(value);
        }

        public bool getWindowPosition()
        {
            return _remoteWindow.GetState();
        }

        public void setWindowPosition(bool value)
        {
            _remoteWindow.SetState(value);
        }

        public RemoteWindow getRemoteWindow()
        {
            return _remoteWindow;
        }

    }
}
