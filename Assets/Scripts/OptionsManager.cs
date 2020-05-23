using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Simulation.Abstractions;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class OptionsManager : MonoBehaviour
    {
        [SerializeField]
        private float _roomWidth = 30;

        [SerializeField]
        private float _roomHeight = 30;

        [SerializeField]
        private float _wallThickness = 1;

        [SerializeField]
        private float _outsideTemperature = 10;

        [SerializeField]
        private float _initialRoomTemperature = 20;

        [SerializeField]
        private float _thermalPixelSize = 3;

        [SerializeField]
        private string _username = string.Empty;

        [SerializeField]
        private string _password = string.Empty;

        [SerializeField]
        private string _serverAddress = string.Empty;

        [SerializeField]
        private bool _requiresAuthentication = false;

        [SerializeField]
        private int _userCount = 1;

        [SerializeField]
        private float _movementSpeed = 5;

        [SerializeField]
        private float _cameraZoomSpeed = 4f;

        private static OptionsManager _instance;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        /// <summary>
        /// Gets or sets the width of the room that should be created when the simulation starts.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float RoomWidth
        {
            get => _instance._roomWidth;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(RoomWidth), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._roomWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the room that should be created when the simulation starts.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float RoomHeight
        {
            get => _instance._roomHeight;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(RoomHeight), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._roomHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the thickness of the walls of the room that should be created when the simulation starts.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float WallThickness
        {
            get => _instance._wallThickness;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(WallThickness), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._wallThickness = value;
            }
        }

        /// <summary>
        /// Gets or sets the temperature outside the room in °C.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value below <see cref="Temperature.AbsoluteZero"/>.
        /// </exception>
        public static float OutsideTemperature
        {
            get => _instance._outsideTemperature;
            set
            {
                if (!IsValidTemperature(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(OutsideTemperature), value, "An attempt was made to set the value below absolute zero.");
                }

                _instance._outsideTemperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the temperature with which the thermal pixels of the room should be initialized.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value below <see cref="Temperature.AbsoluteZero"/>.
        /// </exception>
        public static float InitialRoomTemperature
        {
            get => _instance._initialRoomTemperature;
            set
            {
                if (!IsValidTemperature(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(InitialRoomTemperature), value, "An attempt was made to set the value below absolute zero.");
                    
                }

                _instance._initialRoomTemperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the thermal pixels.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value below <see cref="Temperature.AbsoluteZero"/>.
        /// </exception>
        public static float ThermalPixelSize
        {
            get => _instance._thermalPixelSize;
            set
            {
                if (!IsValidSize(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(ThermalPixelSize), value, "An attempt was made to set the value to zero or an negative value.");
                    
                }

                _instance._thermalPixelSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the username to login with to the FHEM server.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// When trying to set the value to <see langword="null"/>.
        /// </exception>
        public static string Username
        {
            get => _instance._username;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(Username), "An attempt was made to set the value to null.");
                }

                _instance._username = value;
            }
        }

        /// <summary>
        /// Gets or sets the password to login with to the FHEM server.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// When trying to set the value to <see langword="null"/>.
        /// </exception>
        public static string Password
        {
            get => _instance._password;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(Password), "An attempt was made to set the value to null.");
                }

                _instance._password = value;
            }
        }

        /// <summary>
        /// Gets or sets the address of the FHEM server.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// When trying to set the value to <see langword="null"/>.
        /// </exception>
        public static string ServerAddress
        {
            get => _instance._serverAddress;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(ServerAddress), "An attempt was made to set the value to null.");
                }

                _instance._serverAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets if the FHEM server requires the client to authenticate itself.
        /// </summary>
        public static bool RequiresAuthentication
        {
            get => _instance._requiresAuthentication;
            set => _instance._requiresAuthentication = value;
        }

        /// <summary>
        /// Gets or sets the amount of users that should be created when the simulation is started.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static int UserCount
        {
            get => _instance._userCount;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(UserCount), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._userCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the movement speed of the camera.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float MovementSpeed
        {
            get => _instance._movementSpeed;
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(MovementSpeed), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._movementSpeed = value;
            }
        }

        /// <summary>
        /// Gets or sets the zoom speed of the camera.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// When tried to set the value to zero or an negative value.
        /// </exception>
        public static float CameraZoomSpeed
        {
            get => _instance._cameraZoomSpeed;
            set
            {
                if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(CameraZoomSpeed), value, "An attempt was made to set the value to zero or an negative value.");
                }

                _instance._cameraZoomSpeed = value;
            }
        }

        public static float ParseFloat(string input)
        {
            return float.TryParse(input, out float value) ? value : float.NaN;
        }

        public static int? ParseInt(string input)
        {
            return int.TryParse(input, out int value) ? new int?(value) : null;
        }

        public static bool IsValidSize(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > 0f;
        }

        public static bool IsValidTemperature(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > Temperature.AbsoluteZero;
        }
    }
}
