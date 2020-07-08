using Assets.Scripts.Remote.Abstractions;
using System;

namespace Assets.Scripts.Remote
{
    public class LSFInfoSchnittstelle : RemoteObject
    {

        private string[] _valueNames = new string[]
        {
            "isPause",
            "isVorlesung",
            "nachstePause",
            "Attributes:" //Absicherung für nextBreak. (FHEM null!!)
        };

        private static char[] _seperators = new char[]
        {
                ' ',
                '\n',
                '\r'
        };

        public LSFInfoSchnittstelle(IServerConnection remoteConnection, string deviceName)
            : base(remoteConnection, deviceName)
        {
        }

        
        public void GetStates(out bool lecture, out bool isBreak, out DateTime? nextBreak)
        {
            string data = RemoteConnection.ExecuteCommand(DeviceName, null, null, CommandList.List);

            lecture = false;
            isBreak = false;
            nextBreak = null;

            //Daten zerlegen und leer String entfernen.
            string[] dataSplit = data.Split(_seperators, StringSplitOptions.RemoveEmptyEntries);

            for(int i = 0; i < dataSplit.Length; i++)
            {
                if (_valueNames[0].Equals(dataSplit[i]))
                {
                    if(dataSplit[i+1].Equals("0"))
                    {
                        isBreak = false;
                    }
                    else
                    {
                        isBreak = true;
                    }
                }
                else if (_valueNames[1].Equals(dataSplit[i]))
                {

                    if (dataSplit[i + 1].Equals("0"))
                    {
                        lecture = false;
                    }
                    else
                    {
                        lecture = true;
                    }
                }
                else if (_valueNames[2].Equals(dataSplit[i]))
                {

                    //letztes Attribut ausgelesen Suche beenden
                    if(dataSplit[i+1].Equals(_valueNames[3]))
                    {
                        nextBreak = null;
                        break;
                    }
                    nextBreak = DateTime.Parse(dataSplit[i + 1]);
                    break;
                }
            }
        }

    }
}