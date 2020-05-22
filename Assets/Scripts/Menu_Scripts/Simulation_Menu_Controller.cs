using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Assets.Scripts.InputValidation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    //WET says hello
    public class Simulation_Menu_Controller: MonoBehaviour
    {
        [SerializeField]
        private Color _normalColor = Color.white;

        [SerializeField]
        private Color _errorColor = Color.red;

        [SerializeField]
        private float _roomWidth;

        [SerializeField]
        private float _roomHeight;

        [SerializeField]
        private float _wallThickness;

        [SerializeField]
        private float _outsideTemperature;

        [SerializeField]
        private float _initialRoomTemperature;

        [SerializeField]
        private float _thermalPixelSize;

        [SerializeField]
        private string _username;

        [SerializeField]
        private string _password;

        [SerializeField]
        private string _serverAddress;

        [SerializeField]
        private bool _requiresAuthentication;

        [SerializeField]
        private int _userCount;

        [SerializeField]
        private InputField _roomWidthInputField;

        [SerializeField]
        private InputField _roomHeightInputField;

        [SerializeField]
        private InputField _wallThicknessInputField;

        [SerializeField]
        private InputField _outsideTemperatureInputField;

        [SerializeField]
        private InputField _initialRoomTemperatureInputField;

        [SerializeField]
        private InputField _thermalPixelSizeInputField;

        [SerializeField]
        private InputField _usernameInputField;

        [SerializeField]
        private InputField _passwordInputField;

        [SerializeField]
        private InputField _serverAddressInputField;

        [SerializeField]
        private Toggle _requiresAuthenticationToggle;

        [SerializeField]
        private InputField _userCountInputField;

        [SerializeField]
        private Button _startSimulationButton;

        [SerializeField]
        private GameObject _Simulation_Menu;

        [SerializeField]
        private GameObject _Main_Menu;

        private static float ParseFloat(string input)
        {
            return float.TryParse(input, out float value) ? value : float.NaN;
        }

        private static int? ParseInt(string input)
        {
            return int.TryParse(input, out int value) ? new int?(value) : null;
        }

        private static bool ValidateSize(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > 0f;
        }

        private static bool ValidateTemperature(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > Temperature.AbsoluteZero;
        }

        private InputFieldInputChecker<float> CreateScaleInputChecker(InputField inputField, float initialValue)
        {
            return new InputFieldInputChecker<float>(
                inputField: inputField,
                initialValue: initialValue,
                normalColor: _normalColor,
                errorColor: _errorColor,
                inputParser: ParseFloat,
                validator: ValidateSize);
        }

        private InputFieldInputChecker<float> CreateTemperatureInputChecker(InputField inputField, float initialValue)
        {
            return new InputFieldInputChecker<float>(
                inputField: inputField,
                initialValue: initialValue,
                normalColor: _normalColor,
                errorColor: _errorColor,
                inputParser: ParseFloat,
                validator: ValidateTemperature);
        }

        private InputFieldInputChecker<string> CreateAnyStringInputChecker(InputField inputField, string initialValue)
        {
            return new InputFieldInputChecker<string>(
                inputField: inputField,
                initialValue: initialValue,
                normalColor: _normalColor,
                errorColor: _errorColor,
                inputParser: value => value,
                validator: value => true);
        }

        private InputFieldInputChecker<int?> CreatePositiveIntInputChecker(InputField inputField, int initialValue)
        {
            return new InputFieldInputChecker<int?>(
                inputField: inputField,
                initialValue: initialValue,
                normalColor: _normalColor,
                errorColor: _errorColor,
                inputParser: value => int.TryParse(value, out int intValue) ? new int?(intValue) : null,
                validator: value => value.HasValue && value.Value > 0);
        }

        private Dictionary<string, InputChecker> _inputChecker = null;
        void Awake()
        {
            _inputChecker = new Dictionary<string, InputChecker>()
            {
                { nameof(_roomWidth), CreateScaleInputChecker(_roomWidthInputField, _roomWidth) },
                { nameof(_roomHeight), CreateScaleInputChecker(_roomHeightInputField, _roomHeight) },
                { nameof(_wallThickness), CreateScaleInputChecker(_wallThicknessInputField, _wallThickness) },
                { nameof(_outsideTemperature), CreateTemperatureInputChecker(_outsideTemperatureInputField, _outsideTemperature) },
                { nameof(_initialRoomTemperature), CreateTemperatureInputChecker(_initialRoomTemperatureInputField, _initialRoomTemperature) },
                { nameof(_thermalPixelSize), CreateScaleInputChecker(_thermalPixelSizeInputField, _thermalPixelSize) },
                { nameof(_username), CreateAnyStringInputChecker(_usernameInputField, _username) },
                { nameof(_password), CreateAnyStringInputChecker(_passwordInputField, _password) },
                { nameof(_serverAddress), CreateAnyStringInputChecker(_serverAddressInputField, _serverAddress) },
                { nameof(_userCount), CreatePositiveIntInputChecker(_userCountInputField, _userCount) },
            };

            _requiresAuthenticationToggle.isOn = _requiresAuthentication;
        }

        private void saveOptions()
        {
            //speichern der Optionen
            Options options = new Options();

            {
                float width = float.Parse(_roomWidthInputField.GetComponent<InputField>().text);
                float heigth = float.Parse(_roomHeightInputField.GetComponent<InputField>().text);
                float wall = float.Parse(_wallThicknessInputField.GetComponent<InputField>().text);

                options.setGeometry(width, heigth, wall);
            }

            {
                float outside = float.Parse(_outsideTemperatureInputField.GetComponent<InputField>().text);
                float inside = float.Parse(_initialRoomTemperatureInputField.GetComponent<InputField>().text);
                float size = float.Parse(_thermalPixelSizeInputField.GetComponent<InputField>().text);

                options.setGeometry(outside, inside, size);
            }

            {
                string username = _usernameInputField.GetComponent<InputField>().text;
                string password = _passwordInputField.GetComponent<InputField>().text;
                string serveradresse = _serverAddressInputField.GetComponent<InputField>().text;
                bool needed = _requiresAuthenticationToggle.GetComponent<Toggle>().isOn;

                options.setFHEM(username, password, serveradresse, needed);
            }

            {
                int user = int.Parse(_userCountInputField.GetComponent<InputField>().text);

                options.setOther(user);
            }
        }

        public void OnClick_StartSimulation()
        {
            saveOptions();

            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }

        public void OnBack_Click()
        {
            saveOptions();

            _Simulation_Menu.SetActive(false);
            _Main_Menu.SetActive(true);
        }
    }
}