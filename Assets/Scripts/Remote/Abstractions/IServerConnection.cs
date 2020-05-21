using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Remote.Abstractions
{
    public interface IServerConnection
    {
        string GetData(string device, string attribute);

        void SetData(string device, string attribut, string value);
    }
}
