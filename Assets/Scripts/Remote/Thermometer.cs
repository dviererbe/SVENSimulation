using Assets.Scripts.Remote.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    class Thermometer
    {

        private IServerConnection _connection;

        private String _device;

        public Thermometer(IServerConnection connection, String device)
        {
            _connection = connection;
            _device = device;
        }

        public float get()
        {
            //lädt den Standarrt wert
            return get("state");
        }


        public float get(String attribut)
        {

            String server_result = _connection.GetData(_device, attribut);

            float f_result = float.Parse(server_result);

            if(float.IsNaN(f_result))
            {
                return float.NaN;
            }

            return f_result;
        }

        public void set(float value)
        {
            //setzt den standart Wert
            set("state", value);
        }

        public void set(String attribut, float value)
        {
            _connection.SetData(_device, attribut, value.ToString());
        }
    }
}
