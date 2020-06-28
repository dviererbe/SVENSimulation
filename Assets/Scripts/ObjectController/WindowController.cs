using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Remote;
using Assets.Scripts.Remote.Abstractions;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ObjectController
{
    public class WindowController : MonoBehaviour, IThermalObject
    {
        private bool _isOpen = false;

        [SerializeField]
        private SpriteRenderer _windowSpriteRenderer;

        [SerializeField]
        private Sprite _openWindowSpirte;

        [SerializeField]
        private Sprite _closedWindowSprite;

        private bool _started = false;

        private IRoomThermalManager RoomThermalManager { get; set; }
        public RemoteWindow RemoteWindow { get; set; }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (value != _isOpen)
                {
                    try
                    {
                        RemoteWindow?.SetState(value);
                    }
                    catch (Exception exception)
                    {
                        Debug.Log("Failed to set window state of remote window.");
                        Debug.LogException(exception);
                    }

                    if (_isOpen = value)
                    {
                        _windowSpriteRenderer.sprite = _openWindowSpirte;
                        ThermalMaterial = ThermalMaterial.WindowOpen;
                    }
                    else
                    {
                        _windowSpriteRenderer.sprite = _openWindowSpirte;
                        ThermalMaterial = ThermalMaterial.WindowClosed;
                    }
                }
            }
        }

        /// <summary>
        /// Gets if the <see cref="IThermalObject"/> can not change its position.
        /// <see langword="true" /> can not change its position; otherwise <see langword="false"/>.
        /// </summary>
        public bool CanNotChangePosition => true;

        /// <summary>
        /// Gets the absolute (global) Position of the <see cref="IThermalObject"/> in m.
        /// </summary>
        public Vector3 Position => transform.position; //Todo !!!

        /// <summary>
        /// Gets how large the <see cref="IThermalObject"/> is in m (meter).
        /// </summary>
        public Vector3 Size => transform.lossyScale; //Todo !!!

        /// <summary>
        /// Gets the area of the surface of the <see cref="IThermalObject"/> in m² (square meter).
        /// </summary>
        public float ThermalSurfaceArea => Size.x * Size.y;

        /// <summary>
        /// Gets the <see cref="IThermalObject.ThermalMaterial"/> of the <see cref="IThermalObject"/>.
        /// </summary>
        /// <remarks>
        /// Used to calculate the temperature and the heat transfer from and to the the <see cref="IThermalObject"/>.
        /// </remarks>
        public ThermalMaterial ThermalMaterial { get; private set; }

        /// <summary>
        /// Gets the temperature of the <see cref="IThermalObject"/>.
        /// </summary>
        public Temperature Temperature => Temperature.FromCelsius(OptionsManager.OutsideTemperature);

        /// <summary>
        /// A <see cref="IRoomThermalManager"/> signals the <see cref="IThermalObject"/> that the thermal simulation was started.
        /// </summary>
        /// <param name="roomThermalManager">
        /// The <see cref="IRoomThermalManager"/> that starts the thermal simulation with this <see cref="IThermalObject"/>. 
        /// </param>
        public void ThermalStart(IRoomThermalManager roomThermalManager)
        {
            RoomThermalManager = roomThermalManager;
            Start();
        }

        /// <summary>
        /// Is called from the <see cref="IThermalObject"/> once per thermal update.
        /// </summary>
        /// <param name="transferredHeat">
        /// The heat that was transferred to the <see cref="IThermalObject"/> during the thermal update in J (Joule).
        /// </param>
        /// <param name="roomThermalManager">
        /// The <see cref="IRoomThermalManager"/> that does the thermal update.
        /// </param>
        public void ThermalUpdate(float transferredHeat, IRoomThermalManager roomThermalManager)
        {
            //TODO
        }

        void Start()
        {
            if (_started)
            {
                return;
            }

            _started = true;

            ThermalMaterial = ThermalMaterial.WindowClosed;

            try
            {
                IsOpen = RemoteWindow.GetState();
            }
            catch (Exception exception)
            {
                Debug.Log("Failed to get state from remote window.");
                Debug.LogException(exception);
            }
        }
    }
}
