using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Remote;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;



namespace Assets.Scripts
{
    public class DependencyManager : MonoBehaviour
    {
        private ServerConnection _serverConnection = null;

        /// <summary>
        /// Gets the ThermalManager dependency.
        /// </summary>
        public IThermalManager ThermalManager { get; private set; }

        public ServerConnection ServerConnection
        {
            get
            {
                if (_serverConnection == null)
                    _serverConnection = new ServerConnection();

                return _serverConnection;
            }
        }

        /// <summary>
        /// called when the script instance is being loaded
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html">MonoBehaviour.Awake</see>
        /// </summary>
        private void Awake()
        {
            ThermalManager = new ThermalManager();
        }

        /// <summary>
        /// called on the frame when a script is enabled just before any of the Update methods are called the first time
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html">MonoBehaviour.Start</see>
        /// </summary>
        private void Start()
        {
            ThermalManager.Start();
        }

        /// <summary>
        /// called every frame, if the MonoBehaviour is enabled
        /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
        /// </summary>
        private void Update()
        {
            ThermalManager.Update();
        }
    }
}

