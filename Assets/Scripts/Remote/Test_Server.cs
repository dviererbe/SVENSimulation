using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Remote.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Remote
{
    public class Test_Server : MonoBehaviour
    {
        public string Username = "tsuksdl";
        public string Password = "A00Schottisch!";
        public string ServerAddress = "192.168.1.102:8083";
        public bool RequiresAuthentication = true;

        // Start is called before the first frame update
        void Start()
        {

            Debug.Log("Tests erfolgen");

            IServerConnection connection = ServerConnectionFactory.CreateServerConnection(Username, Password, ServerAddress,
                    RequiresAuthentication);

            Heizung heizung = new Heizung(connection, "Heizung");

            try
            {
                //Heizung sollt auf 0 sein (wenn richtig eingestellt)
                float start = heizung.get();
                Debug.Log("Heizung: " + start);

                heizung.set(30);
                float value = heizung.get();
                Debug.Log("Heizung: " + value);

                heizung.set("temperatur", start);
                value = heizung.get("temperatur");
                Debug.Log("Heizung: " + value);

                Debug.Log ("Setting Data succeeded.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Setting Data failed.");
                Debug.LogError(exception);
            }

            Options options = new Options();

            foreach(String fhem in options.getFHEM())
            {
                Debug.Log("Load: " + fhem);
            }

        }
    }
}