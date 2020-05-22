using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    class Options
    {
        private static float _roomWidth;

        private static float _roomHeight;

        private static float _wallThickness;

        private static float _outsideTemperature;

        private static float _initialRoomTemperature;

        private static float _thermalPixelSize;

        private static string _username;

        private static string _password;

        private static string _serverAddress;

        private static bool _requiresAuthentication;

        private static int _userCount;

        public void setGeometry(float roomWidth, float roomHeight, float wallThickness)
        {
            _roomHeight = roomHeight;
            _roomWidth = roomWidth;
            _wallThickness = wallThickness;
        }

        public void setGeometry(float[] roomdata)
        {
            _roomHeight = roomdata[0];
            _roomWidth = roomdata[1];
            _wallThickness = roomdata[2];
        }

        public void setThermodynamics(float outsideTemperature, float initialRoomTemperature, float thermalPixelSize)
        {
            _outsideTemperature = outsideTemperature;
            _initialRoomTemperature = initialRoomTemperature;
            _thermalPixelSize = thermalPixelSize;
        }

        public void setThermodynamics(float[] thermodata)
        {
            _outsideTemperature = thermodata[0];
            _initialRoomTemperature = thermodata[1];
            _thermalPixelSize = thermodata[2];
        }

        public void setFHEM(string username, string password, string serverAderess, bool requiresAuthentication)
        {
            _username = username;
            _password = password;
            _serverAddress = serverAderess;
            _requiresAuthentication = requiresAuthentication;
        }

        public void setFHEM(string[] fhemdata)
        {
            _username = fhemdata[0];
            _password = fhemdata[1];
            _serverAddress = fhemdata[2];
            _requiresAuthentication = bool.Parse(fhemdata[3]);
        }

        public void setOther(int userCount)
        {
            _userCount = userCount;
        }

        public float[] getGeometry()
        {
            return new float[]
            {
                _roomHeight,
                _roomWidth,
                _wallThickness
            };
        }

        public float[] getThermodynamics()
        {
            return new float[]
            {
                _outsideTemperature,
                _initialRoomTemperature,
                _thermalPixelSize
            };
        }

        public string[] getFHEM()
        {
            return new String[]
            {
                _username,
                _password,
                _serverAddress,
                _requiresAuthentication.ToString()
            };
        }

        public float getOther ()
        {
            return _userCount;
        }
    }
}
