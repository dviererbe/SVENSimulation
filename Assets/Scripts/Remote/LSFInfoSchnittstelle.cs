using Assets.Scripts.Remote.Abstractions;
using System;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

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

        public LSFInfoSchnittstelle(IServerConnection remoteConnection, string deviceName)
            : base(remoteConnection, deviceName)
        {
        }

        
        public void getStates(out bool lecture, out bool isBreak, out DateTime? nextBreak)
        {
            string data = RemoteConnection.GetReadingList(DeviceName);

            lecture = false;
            isBreak = false;
            nextBreak = null;

            char[] seperators = new char[]
            {
                ' ',
                '\n',
                '\r'
            };

            //Daten zerlegen und leer String entfernen.
            string[] datasplit = data.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            for(int i = 0; i < datasplit.Length; i++)
            {
                if (_valueNames[0].Equals(datasplit[i]))
                {
                    if(datasplit[i+1].Equals("0"))
                    {
                        isBreak = false;
                    }
                    else
                    {
                        isBreak = true;
                    }
                }
                else if (_valueNames[1].Equals(datasplit[i]))
                {

                    if (datasplit[i + 1].Equals("0"))
                    {
                        lecture = false;
                    }
                    else
                    {
                        lecture = true;
                    }
                }
                else if (_valueNames[2].Equals(datasplit[i]))
                {

                    //letztes Attribut ausgelesen Suche beenden
                    if(datasplit[i+1].Equals(_valueNames[3]))
                    {
                        nextBreak = null;
                        break;
                    }
                    nextBreak = DateTime.Parse(datasplit[i + 1]);
                    break;
                }
            }
        }

    }
}