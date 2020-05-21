using Assets.Scripts.Remote.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote
{
    class Fenster
    {
        private IServerConnection _connection;

        private String _device;

        public Fenster(IServerConnection connection, String device)
        {
            _connection = connection;
            _device = device;
        }

        /// <summary>
        /// Loads the state of the heater
        /// </summary>
        /// <returns>
        /// float as % of opening of the heater
        /// </returns>
        public bool get()
        {
            return get("state");
        }

        /// <summary>
        /// Loads the state of the heater
        /// </summary>
        /// <returns>
        /// float as % of opening of the heater
        /// </returns>
        public bool get(String attribut)
        {

            String server_result = _connection.GetData(_device, attribut);

            bool b_result = bool.Parse(server_result);

            return b_result;
        }

        public void set(bool value)
        {
            set("state", value);
        }

        public void set(String attribut, bool value)
        {
            _connection.SetData(_device, attribut, value.ToString());
        }
    }
}
