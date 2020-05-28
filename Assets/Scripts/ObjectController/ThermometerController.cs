using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Remote;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

namespace Assets.Scripts.ObjectController
{
    public class ThermometerController : MonoBehaviour
    {
        [SerializeField] 
        private TextMesh _userStateTextMesh;

        private float _lastTransmittedValue = 20f;

        public float TransmissionThreshold { get; set; } = 0.1f;

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public IRoomThermalManager RoomThermalManager { get; set; }

        public RemoteThermometer RemoteThermometer { get; set; }

        void Start()
        {
            try
            {
                _lastTransmittedValue = RemoteThermometer.GetState();
            }
            catch (Exception exception)
            {
                Debug.Log("Failed to get temperature from remote thermometer.");
                Debug.LogException(exception);
            }
        }

        void Update()
        {
            float temperature = RoomThermalManager.GetTemperature(Position).ToCelsius();

            if (Mathf.Abs(_lastTransmittedValue - temperature) > TransmissionThreshold)
            {
                try
                {
                    RemoteThermometer.SetState(temperature);
                    _lastTransmittedValue = temperature;
                }
                catch (Exception exception)
                {
                    Debug.Log("Failed to update remote thermometer.");
                    Debug.LogException(exception);
                }
            }

            _userStateTextMesh.text = RoomThermalManager.GetTemperature(Position).ToCelsius().ToString();
        }
    }

    


}
