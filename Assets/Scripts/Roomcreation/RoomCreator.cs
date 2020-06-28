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
using Assets.Scripts.Roomcreation;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class RoomCreator : MonoBehaviour, IRoom
{
    [SerializeField]
    private int _roomWidth;

    [SerializeField] 
    private int _roomHeight;

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
    private GameObject _doorPrefab;

    [SerializeField]
    private GameObject _furniture;

    [SerializeField]
    private GameObject _tablePrefab;

    [SerializeField]
    private GameObject _chairPrefab;

    [SerializeField]
    private GameObject _heaterPrefab;

    [SerializeField]
    private GameObject _closetPrefab;

    private (GameObject gameObject, bool isWall)[,] _roomObjects;

    private IRoomThermalManager _roomThermalManager;

    #region Properties

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public GameObject AirPrefab => _airPrefab;

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public GameObject WallPrefab => _wallPrefab;

    /// <summary>
    /// Gets the dimensional extent of the <see cref="IRoom"/> in meter (without the wall).
    /// </summary>
    /// <remarks>
    /// This value is not allowed to change.
    /// It's calculated by this formula: (WallThickness + ThermalPixelSize) / 2 + i * AirPrefab.transform.lossyScale.x
    /// Unfortunately, the variable i is unknown at this point. But 'i' represents: _roomObjects.GetLength(0) - 1,
    /// _roomObjects.GetLength(0) is calculated via: roomThermalPixelCount.x + 2 * wallThermalPixelCount
    /// 
    /// 
    /// roomThermalPixelCount:
    /// we get the roomThermalPixelCount with: _roomWidth / _roomThermalManager.ThermalPixelSize (_roomHeight respectively)
    /// We can't use the ThermalPixelSize from _roomThermalManager, but we can use the OptionsManager one (thermalManagerBuilder.ThermalPixelSize = OptionsManager.ThermalPixelSize)
    /// 
    /// wallThermalPixelCount:
    /// is calculated with the formula: _wallThickness / _roomThermalManager.ThermalPixelSize
    /// 
    /// Putting it all together, we get the formula below
    /// </remarks>
    public Vector3 RoomSize => new Vector3(
        x: (WallThickness + OptionsManager.ThermalPixelSize) / 2 + (((_roomWidth / OptionsManager.ThermalPixelSize) + 2 * (_wallThickness / OptionsManager.ThermalPixelSize)) - 1) * OptionsManager.ThermalPixelSize, 
        y: (WallThickness + OptionsManager.ThermalPixelSize) / 2 + (((_roomHeight / OptionsManager.ThermalPixelSize) + 2 * (_wallThickness / OptionsManager.ThermalPixelSize)) - 1) * OptionsManager.ThermalPixelSize); //TODO: replace by constant;

    /// <summary>
    /// Gets the global position of the <see cref="IRoom"/>.
    /// </summary>
    /// <remarks>
    /// This value is not allowed to change.
    /// </remarks>
    public Vector3 RoomPosition => new Vector3(WallThickness / 2, WallThickness / 2, 0); //TODO: replace by constant;

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
        get => _roomThermalManager?.ThermalPixelSize ?? OptionsManager.ThermalPixelSize;
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {

        RoomReader roomreader = new RoomReader(Application.dataPath + "/Roomdefinition/Room_0.xml");
        RoomObjects[] roomObjects = roomreader.ReadRoom();

        #region Load Options

        _roomHeight = Mathf.RoundToInt(OptionsManager.RoomHeight);
        _roomWidth = Mathf.RoundToInt(OptionsManager.RoomWidth);
        _wallThickness = OptionsManager.WallThickness;

        #endregion

        IServerConnection serverConnection = GameObject.Find("SimulatedServer").GetComponent<SimulatedServer>().SimulatedRemoteConnection;

        #region ThermalManager

        GameObject userGroupControllerObject = Instantiate(_userGroupControllerPrefab);
        UserGroupController userGroupController = userGroupControllerObject.GetComponent<UserGroupController>();

        RoomThermalManagerBuilder thermalManagerBuilder = new RoomThermalManagerBuilder();
        thermalManagerBuilder.Room = this;
        thermalManagerBuilder.ThermalPixelSize = new Temperature(OptionsManager.InitialRoomTemperature, TemperatureUnit.Celsius);
        thermalManagerBuilder.OutsideTemperature = OutsideTemperatureSource.Instance;
        thermalManagerBuilder.InitialRoomTemperature = new NoisyTemperatureSource(
                baseTemperatureValue: Temperature.FromCelsius(OptionsManager.InitialRoomTemperature), 
                noiseOffset: Temperature.FromKelvin(2.5f)); //TODO: Read from options
        thermalManagerBuilder.ThermalPixelSize = OptionsManager.ThermalPixelSize;
        userGroupController.CreateUsers(thermalManagerBuilder);
        //thermalManagerBuilder.AddThermalObject(windowController);

        //Calculate Room Size
        int wallThermalPixelCount = Mathf.RoundToInt(_wallThickness / thermalManagerBuilder.ThermalPixelSize);

        Vector2Int roomThermalPixelCount = new Vector2Int(
            x: Mathf.RoundToInt(_roomWidth / thermalManagerBuilder.ThermalPixelSize),
            y: Mathf.RoundToInt(_roomHeight / thermalManagerBuilder.ThermalPixelSize));

        #endregion

        #region Room Creator

        WallPrefab.transform.localScale = new Vector3(
            x: thermalManagerBuilder.ThermalPixelSize,
            y: thermalManagerBuilder.ThermalPixelSize,
            z: thermalManagerBuilder.ThermalPixelSize);

        AirPrefab.transform.localScale = new Vector3(
            x: thermalManagerBuilder.ThermalPixelSize,
            y: thermalManagerBuilder.ThermalPixelSize,
            z: thermalManagerBuilder.ThermalPixelSize);

        _roomObjects = new (GameObject, bool)[
                    roomThermalPixelCount.x + 2 * wallThermalPixelCount,
                    roomThermalPixelCount.y + 2 * wallThermalPixelCount];

        for (int i = 0; i < _roomObjects.GetLength(0); i++)
        {
            for (int j = 0; j < _roomObjects.GetLength(1); j++)
            {
                if (i < wallThermalPixelCount || j < wallThermalPixelCount || 
                    j > _roomObjects.GetLength(1) - wallThermalPixelCount - 1 || i > _roomObjects.GetLength(0) - wallThermalPixelCount - 1)
                {
                    _roomObjects[i, j] = (InstantiateWallObject(i, j), true);
                }
                else
                {
                    GameObject airObject = Instantiate(
                            AirPrefab, //the GameObject that will be instantiated
                            position: new Vector3(
                                x: (WallThickness + ThermalPixelSize) / 2 + i * AirPrefab.transform.lossyScale.x, //WallThickness - (WallThickness - 1) * 0.5f = w/2 + 0.5
                                y: (WallThickness + ThermalPixelSize) / 2 + j * AirPrefab.transform.lossyScale.y),
                            rotation: AirPrefab.transform.rotation);


                airObject.name = "Air_" + i + "." + j;
                airObject.transform.parent = gameObject.transform;
                airObject.GetComponent<TemperatureController>().Position = new Vector2Int(i, j);

                    _roomObjects[i, j] = (airObject, false);
                }
            }
        }

        #endregion

        #region RoomObjects Creator

        foreach(RoomObjects obj in roomObjects)
        {
            GameObject roomObject = null;

            if(obj.Element == RoomObjects.RoomElement.CHAIR)
            {
                roomObject = Instantiate(
                                _chairPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + obj.PosY) + 0.5f,
                                    y: (WallThickness + obj.PosX) + 0.5f),
                                rotation: _chairPrefab.transform.rotation);
                //roomObject.GetComponent<>().setSprite(obj.Type);
            }
            else if(obj.Element == RoomObjects.RoomElement.TABLE)
            {
                roomObject = Instantiate(
                                _tablePrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + obj.PosY) + 0.5f,
                                    y: (WallThickness + obj.PosX) + 0.5f),
                                rotation: _tablePrefab.transform.rotation);
                roomObject.GetComponent<TableController>().setSprite(obj.Type);
            }else  if (obj.Element == RoomObjects.RoomElement.HEATER)
            {
                roomObject = Instantiate(
                                _heaterPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + obj.PosY) + 0.5f,
                                    y: (WallThickness + obj.PosX) + 0.5f),
                                rotation: _tablePrefab.transform.rotation);
                //roomObject.GetComponent<TableController>().setSprite(obj.Type);
            }else if (obj.Element == RoomObjects.RoomElement.CLOSET)
            {
                roomObject = Instantiate(
                                _closetPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + obj.PosY) + 0.5f,
                                    y: (WallThickness + obj.PosX) + 0.5f),
                                rotation: _tablePrefab.transform.rotation);
                //roomObject.GetComponent<TableController>().setSprite(obj.Type);
            }

            //TODO: Roomobjects Door und Window

            if (roomObject != null)
            {
                roomObject.transform.Rotate(0, 0, obj.Rotation);
                roomObject.transform.parent = _furniture.transform;
                Vector3 objectsize = roomObject.transform.localScale;
                objectsize.x *= obj.Sizewidth;
                objectsize.y *= obj.Sizeheight;
                roomObject.transform.localScale = objectsize;

            }

        }
        #endregion

        //Build and start Thermal Manager
        _roomThermalManager = thermalManagerBuilder.Build();
        _roomThermalManager.Start();

        #region Thermometer

        GameObject thermometerObject = Instantiate(_thermometerPrefab);
        thermometerObject.transform.parent = gameObject.transform;
        ThermometerController thermometerController = thermometerObject.GetComponent<ThermometerController>();
        thermometerController.RoomThermalManager = _roomThermalManager;
        thermometerController.RemoteThermometer = new RemoteThermometer(serverConnection, "thermometer");
        thermometerController.Position = new Vector3(1, 1);

        #endregion
    }

    private GameObject InstantiateWallObject(int i, int j)
    {
        GameObject wallObject = Instantiate(
                                WallPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + ThermalPixelSize) / 2 + i * _wallPrefab.transform.lossyScale.x,
                                    y: (WallThickness + ThermalPixelSize) / 2 + j * _wallPrefab.transform.lossyScale.x),
                                rotation: WallPrefab.transform.rotation);

        wallObject.transform.parent = gameObject.transform;

        wallObject.name = "Wall_" + i + ":" + j;
        SetWallSprite(i, j, wallObject);

        return wallObject;
    }

    private void SetWallSprite(int i, int j, GameObject wallObject)
    {
        bool[] neightbours = new bool[4];

        if (i == 0)
        {
            neightbours[1] = _roomObjects[i + 1, j].isWall;
            neightbours[3] = false;
        }
        else if (i == _roomObjects.GetLength(0) - 1)
        {
            neightbours[1] = false;
            neightbours[3] = _roomObjects[i - 1, j].isWall;
        }
        else
        {
            neightbours[1] = _roomObjects[i + 1, j].isWall;
            neightbours[3] = _roomObjects[i - 1, j].isWall;
        }

        if (j == 0)
        {
            neightbours[0] = _roomObjects[i, j + 1].isWall;
            neightbours[2] = false;
        }
        else if (j == _roomObjects.GetLength(1) - 1)
        {
            neightbours[0] = false;
            neightbours[2] = _roomObjects[i, j - 1].isWall;
        }
        else
        {
            neightbours[0] = _roomObjects[i, j + 1].isWall;
            neightbours[2] = _roomObjects[i, j - 1].isWall;
        }

        wallObject.GetComponent<WallController>().SetWalls(neightbours);
    }

    // Update is called once per frame
    void Update()
    {
        #region ThermalManager

        _roomThermalManager.Update();

        #endregion

        #region Air Creator

        SetAirTemperatureColors();
        SetWallTemperatureColors();
        
        #endregion
    }

    private void SetWallTemperatureColors()
    {

    }

    /// <summary>
    /// Sets the color of all air game-objects relative to the temperature of a air game-object
    /// to the lowest and highest temperature of all air game-objects.
    /// </summary>
    private void SetAirTemperatureColors()
    {
        float lowestTemperature;
        float highestTemperature;
        float temperatureStep;

        SetTemperatures();
        lowestTemperature = _roomThermalManager.CurrentTemperatureStatistics.Minimum;
        highestTemperature = _roomThermalManager.CurrentTemperatureStatistics.Maximum;

        if (OptionsManager.DynamicTemperatureScaling)
        { 
            OptionsManager.MinTemperatur = lowestTemperature;
            OptionsManager.MaxTemperatur = highestTemperature;
        }
        else
        {
            lowestTemperature = OptionsManager.MinTemperatur;
            highestTemperature = OptionsManager.MaxTemperatur;
        }

        //Length -1, cuz otherwise we'd get values between 0 and 16
        float temperatureDifference = highestTemperature - lowestTemperature;
        if (temperatureDifference != 0)
        {
            temperatureStep = temperatureDifference / (AirColors.ColorArray.GetLength(0) - 1);

            for (int x = 0; x < _roomObjects.GetLength(0); x++)
            {
                for (int y = 0; y < _roomObjects.GetLength(1); y++)
                {
                    float localColorDiff = _roomObjects[x, y].gameObject.GetComponent<TemperatureController>().Temperature - lowestTemperature;

                    _roomObjects[x, y].gameObject.GetComponent<TemperatureController>().SetColor(InterpolateColor(localColorDiff, temperatureStep));
                }
            }
        }
    }

    private Color32 InterpolateColor(float localColorDiff, float temperatureStep)
    {
        // Just for static scaling possible
        if (localColorDiff < 0f)
        {
            return AirColors.ColorArray[0];
        }

        float colorSection = localColorDiff / temperatureStep;
        int currentColorIndex = (int)Math.Round(colorSection, 0);
        float minimumTemperatureValueForNextColorindex = (currentColorIndex + 1) * temperatureStep;

        if (-1 < currentColorIndex && currentColorIndex < AirColors.ColorArray.GetLength(0) - 1)
        {
            byte r = (byte)Mathf.Lerp(AirColors.ColorArray[currentColorIndex].r, AirColors.ColorArray[currentColorIndex + 1].r, minimumTemperatureValueForNextColorindex - localColorDiff);
            byte g = (byte)Mathf.Lerp(AirColors.ColorArray[currentColorIndex].g, AirColors.ColorArray[currentColorIndex + 1].g, minimumTemperatureValueForNextColorindex - localColorDiff);
            byte b = (byte)Mathf.Lerp(AirColors.ColorArray[currentColorIndex].b, AirColors.ColorArray[currentColorIndex + 1].b, minimumTemperatureValueForNextColorindex - localColorDiff);
            return new Color32(r, g, b, 255);
        }
        else
        {
            return AirColors.ColorArray[AirColors.ColorArray.GetLength(0) - 1];
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
    private void SetTemperatures()
    {

        for (int x = 0; x < _roomObjects.GetLength(0); x++)
        {
            for (int y = 0; y < _roomObjects.GetLength(1); y++)
            {
                float temperature = _roomThermalManager.GetTemperature(_roomObjects[x,y].gameObject.transform.position).ToCelsius().Value;

                _roomObjects[x, y].gameObject.GetComponent<TemperatureController>().Temperature = temperature;
            }
        }
    }
}
