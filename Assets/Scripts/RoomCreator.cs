using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.ObjectController;
using Assets.Scripts.Remote;
using Assets.Scripts.Remote.Abstractions;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class RoomCreator : MonoBehaviour, IRoom
{
    [SerializeField]
    private int _roomWidth = 30;

    [SerializeField] 
    private int _roomHeight = 30;

    [SerializeField]
    private float _wallThickness = 1f;

    [SerializeField]
    private GameObject _airPrefab;

    [SerializeField]
    private GameObject _wallPrefab;

    [SerializeField]
    private GameObject _thermometerPrefab;

    [SerializeField]
    private GameObject _userGroupControllerPrefab;

    [SerializeField]
    private GameObject _windowPrefab;

    [SerializeField]
    private GameObject _heaterPrefab;

    private GameObject[,] _airObjects;

    private GameObject[,] _wallObjects;

    private IRoomThermalManager _roomThermalManager;

    #region Properties

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public GameObject AirPrefab
    {
        get => _airPrefab;
        set => _airPrefab = value;
    }

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public GameObject WallPrefab
    {
        get => _wallPrefab;
        set => throw new NotImplementedException(); //TODO: Implemet me!
    }

    /// <summary>
    /// Gets the dimensional extent of the <see cref="IRoom"/> in meter (without the wall).
    /// </summary>
    /// <remarks>
    /// This value is not allowed to change.
    /// </remarks>
    public Vector3 RoomSize => new Vector3(_roomWidth - 1, _roomHeight - 1); //TODO: replace by constant;

    /// <summary>
    /// Gets the global position of the <see cref="IRoom"/>.
    /// </summary>
    /// <remarks>
    /// This value is not allowed to change.
    /// </remarks>
    public Vector3 RoomPosition => new Vector3(1, 1, 0); //TODO: replace by constant;

    /// <summary>
    /// Gets the thickness of the walls of the <see cref="IRoom"/>.
    /// </summary>
    /// <remarks>
    /// This value is not allowed to change.
    /// </remarks>
    public float WallThickness => _wallThickness;

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public float ThermalPixelSize
    {
        get => _roomThermalManager.ThermalPixelSize;
        set => _roomThermalManager.ThermalPixelSize = value;
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region Load Options
        
        _roomHeight = Mathf.RoundToInt(OptionsManager.RoomHeight);
        _roomWidth = Mathf.RoundToInt(OptionsManager.RoomWidth);
        _wallThickness = OptionsManager.WallThickness;

        #endregion

        IServerConnection serverConnection = GameObject.Find("SimulatedServer").GetComponent<SimulatedServer>().SimulatedRemoteConnection;

        #region ThermalManager

        GameObject userGroupControllerObject = Instantiate(_userGroupControllerPrefab);
        UserGroupController userGroupController = userGroupControllerObject.GetComponent<UserGroupController>();

        GameObject windowObject = Instantiate(_windowPrefab);
        windowObject.transform.position = new Vector3(5, 0);
        WindowController windowController = windowObject.GetComponent<WindowController>();
        windowController.RemoteWindow = new RemoteWindow(serverConnection, "window");


        RoomThermalManagerBuilder thermalManagerBuilder = new RoomThermalManagerBuilder();
        thermalManagerBuilder.Room = this;
        thermalManagerBuilder.ThermalPixelSize = new Temperature(OptionsManager.InitialRoomTemperature, TemperatureUnit.Celsius);
        thermalManagerBuilder.OutsideTemperature = OutsideTemperatureSource.Instance;
        thermalManagerBuilder.InitialRoomTemperature = new ConstantTemperatureSource(Temperature.FromCelsius(OptionsManager.InitialRoomTemperature));
        thermalManagerBuilder.ThermalPixelSize = OptionsManager.ThermalPixelSize;
        userGroupController.CreateUsers(thermalManagerBuilder);
        thermalManagerBuilder.AddThermalObject(windowController);


        _roomThermalManager = thermalManagerBuilder.Build();
        _roomThermalManager.Start();

        userGroupController.AddRoomThermalManagerToUsers(_roomThermalManager);
        windowController.RoomThermalManager = _roomThermalManager;

        #endregion

        

        

        GameObject thermometerObject = Instantiate(_thermometerPrefab);
        thermometerObject.transform.parent = gameObject.transform;
        ThermometerController thermometerController = thermometerObject.GetComponent<ThermometerController>();
        thermometerController.RoomThermalManager = _roomThermalManager;
        thermometerController.RemoteThermometer = new RemoteThermometer(serverConnection, "thermometer");
        thermometerController.Position = new Vector3(1, 1);
        
        #region Air Creator

        AirPrefab.transform.localScale = new Vector3(
            x: WallThickness / _roomThermalManager.ThermalPixelSize,
            y: WallThickness / _roomThermalManager.ThermalPixelSize,
            z: WallThickness / _roomThermalManager.ThermalPixelSize);

        _airObjects = new GameObject[
            (_roomHeight - 2) * Convert.ToInt32(_roomThermalManager.ThermalPixelSize),
            (_roomHeight - 2) * Convert.ToInt32(_roomThermalManager.ThermalPixelSize)];

        for (int i = 0; i < _airObjects.GetLength(0); i++)
        {
            for (int j = 0; j < _airObjects.GetLength(1); j++)
            {

                GameObject airObject = Instantiate(
                        AirPrefab, //the GameObject that will be instantiated
                        position: new Vector3(
                            x: 0.66666f * _wallThickness + i * AirPrefab.transform.localScale.x,
                            y: 0.66666f * _wallThickness + j * AirPrefab.transform.localScale.y),
                        rotation: AirPrefab.transform.rotation) ;

                airObject.transform.parent = gameObject.transform;
                airObject.GetComponent<AirTemperatureController>().Position = new Vector2Int(i, j);

                _airObjects[i,j] = airObject;
            }
        }

        #endregion

        #region Wall Creator

        float tileSizeX = transform.lossyScale.x;
        float tileSizeY = transform.localScale.y;

        _wallObjects = new GameObject[_roomHeight, _roomWidth];


        for (int i = 0; i < _roomHeight; i++)
        {
            for (int j = 0; j < _roomWidth; j++)
            {
                if ((i == 0 || i == _roomHeight - 1) ||
                    (j == 0 || j == _roomWidth - 1))
                {
                    GameObject wallObject = Instantiate(
                                WallPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: WallThickness * i,
                                    y: WallThickness * j),
                                rotation: WallPrefab.transform.rotation);

                    wallObject.transform.parent = gameObject.transform;

                    wallObject.name = "Wall_" + i + j;

                    Vector3 tempscale = wallObject.transform.localScale;

                    tempscale.x *= WallThickness;
                    tempscale.y *= WallThickness;

                    wallObject.transform.localScale = tempscale;

                    _wallObjects[i, j] = wallObject;

                }
            }
        }

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        #region ThermalManager

        _roomThermalManager.Update();

        #endregion

        #region Air Creator

        SetTemperatureColors();

        #endregion
    }

    /// <summary>
    /// Sets the color of all air game-objects relative to the temperature of a air game-object
    /// to the lowest and highest temperature of all air game-objects.
    /// </summary>
    private void SetTemperatureColors()
    {
        SetTemperaturesAndGetHighestAndLowest(out float highestTemperature, out float lowestTemperature, out float temperatureStep);

        float colorSection;

        //OptionsManager.MinTemperatur = lowestTemperature;
        //OptionsManager.MaxTemperatur = highestTemperature;

        for (int x = 0; x < _airObjects.GetLength(0); x++)
        {
            for (int y = 0; y < _airObjects.GetLength(1); y++)
            {
                colorSection = _airObjects[x, y].GetComponent<AirTemperatureController>().Temperature - lowestTemperature;
                colorSection = colorSection / temperatureStep;
                // With "(TileTemp - lowestTemp) / temperatureStep", we create a transformation from [0, (highestTemp-lowestTemp)] -> [0, AmounOfColorsWeHave-1]
                _airObjects[x, y].GetComponent<AirTemperatureController>().SetColor(ref AirColors.ColorArray[(int)Math.Round(colorSection, 0)]);
            }
        }
    }

    /// <summary>
    /// Sets the temperature of the air game-objects and returns the highest and lowest temperature assigned to an air game-object.
    /// </summary>
    /// <remarks>
    /// Getting and setting was was combined to avoid another n^2-iterations.
    /// </remarks>
    /// <param name="highestTemperature">
    /// When this method returns, contains single-precision floating-point number equivalent to
    /// the numeric value of the highest temperature displayed in the currently rendered frame.
    /// </param>
    /// <param name="lowestTemperature">
    /// When this method returns, contains single-precision floating-point number equivalent to
    /// the numeric value of the lowest temperature displayed in the currently rendered frame.
    /// </param>
    /// /// <param name="temperatureStep">
    /// When this method returns, contains single-precision floating-point number equivalent to
    /// the numeric value of the lowest temperature displayed in the currently rendered frame.
    /// temperatureStep is highestTemperature-lowestTemperature / amounts of colors we use
    /// that's an easy possibility in order to specify the color
    /// </param>
    private void SetTemperaturesAndGetHighestAndLowest(out float highestTemperature, out float lowestTemperature, out float temperatureStep)
    {
        highestTemperature = float.MinValue;
        lowestTemperature = float.MaxValue;

        for (int x = 0; x < _airObjects.GetLength(0); x++)
        {
            for (int y = 0; y < _airObjects.GetLength(1); y++)
            {
                float temperature = _roomThermalManager.GetTemperature(new Vector3(x, y)).ToCelsius().Value;

                _airObjects[x, y].GetComponent<AirTemperatureController>().Temperature = temperature;

                if (highestTemperature < temperature)
                {
                    highestTemperature = temperature;
                }
                else if (temperature < lowestTemperature)
                {
                    lowestTemperature = temperature;
                }
            }
        }

        //Length -1, cuz otherwise we'd get values between 0 and 16
        temperatureStep = (highestTemperature - lowestTemperature) / (AirColors.ColorArray.GetLength(0) - 1);
    }
}
