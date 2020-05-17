﻿using System;
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
            IServerConnection connection = ServerConnectionFactory.CreateServerConnection(Username, Password, ServerAddress,
                    RequiresAuthentication);

            try
            {
                connection.SetData("dummy_Unity", "23");
                Debug.Log ("Setting Data succeeded.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Setting Data failed.");
                Debug.LogError(exception);
            }

            try
            {
                Debug.Log(connection.GetData("anyViews", "icon%20on"));
                Debug.Log("Getting Data succeeded.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Getting Data failed.");
                Debug.LogError(exception);
            }
        }
    }
}