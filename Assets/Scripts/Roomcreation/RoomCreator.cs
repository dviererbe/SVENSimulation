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
    public Vector3 RoomSize => new Vector3(
        x: (WallThickness + OptionsManager.ThermalPixelSize) / 2 + ((_roomWidth - 2) * Convert.ToInt32(WallThickness) / Convert.ToInt32(OptionsManager.ThermalPixelSize)-1) * OptionsManager.ThermalPixelSize, 
        y: (WallThickness + OptionsManager.ThermalPixelSize) / 2 + ((_roomHeight - 2) * Convert.ToInt32(WallThickness) / Convert.ToInt32(OptionsManager.ThermalPixelSize)-1) * OptionsManager.ThermalPixelSize); //TODO: replace by constant;

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
        get => _roomThermalManager.ThermalPixelSize;
        set => _roomThermalManager.ThermalPixelSize = value;
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {

        RoomReader roomreader = new RoomReader(Application.dataPath + "/Roomdefinition/9.428.xml");
        RoomObjects.RoomElement[,] walls;
        float scaling = 0;
        RoomObjects[] roomObjects = roomreader.ReadRoom( out walls, out scaling);

        #region Load Options

        _roomHeight = Mathf.RoundToInt(OptionsManager.RoomHeight);
        _roomWidth = Mathf.RoundToInt(OptionsManager.RoomWidth);
        _wallThickness = OptionsManager.WallThickness;

        #endregion

        IServerConnection serverConnection = GameObject.Find("SimulatedServer").GetComponent<SimulatedServer>().SimulatedRemoteConnection;

        #region ThermalManager

        GameObject userGroupControllerObject = Instantiate(_userGroupControllerPrefab);
        UserGroupController userGroupController = userGroupControllerObject.GetComponent<UserGroupController>();

        //GameObject windowObject = Instantiate(_windowPrefab);
        //windowObject.transform.position = new Vector3(5, 0);
        //WindowController windowController = windowObject.GetComponent<WindowController>();
        //windowController.RemoteWindow = new RemoteWindow(serverConnection, "window");


        RoomThermalManagerBuilder thermalManagerBuilder = new RoomThermalManagerBuilder();
        thermalManagerBuilder.Room = this;
        thermalManagerBuilder.ThermalPixelSize = new Temperature(OptionsManager.InitialRoomTemperature, TemperatureUnit.Celsius);
        thermalManagerBuilder.OutsideTemperature = OutsideTemperatureSource.Instance;
        thermalManagerBuilder.InitialRoomTemperature = new ConstantTemperatureSource(Temperature.FromCelsius(OptionsManager.InitialRoomTemperature));
        thermalManagerBuilder.ThermalPixelSize = OptionsManager.ThermalPixelSize;
        userGroupController.CreateUsers(thermalManagerBuilder);
        //thermalManagerBuilder.AddThermalObject(windowController);


        _roomThermalManager = thermalManagerBuilder.Build();
        _roomThermalManager.Start();

        userGroupController.AddRoomThermalManagerToUsers(_roomThermalManager);
        //windowController.RoomThermalManager = _roomThermalManager;

        #endregion

        
        GameObject thermometerObject = Instantiate(_thermometerPrefab);
        thermometerObject.transform.parent = gameObject.transform;
        ThermometerController thermometerController = thermometerObject.GetComponent<ThermometerController>();
        thermometerController.RoomThermalManager = _roomThermalManager;
        thermometerController.RemoteThermometer = new RemoteThermometer(serverConnection, "thermometer");
        thermometerController.Position = new Vector3(1, 1);
        
        #region Air Creator

        AirPrefab.transform.localScale = new Vector3(
            x: _roomThermalManager.ThermalPixelSize,
            y: _roomThermalManager.ThermalPixelSize,
            z: _roomThermalManager.ThermalPixelSize);

        _airObjects = new GameObject[
            (_roomWidth - 2) * Convert.ToInt32(WallThickness) / Convert.ToInt32(ThermalPixelSize),
            (_roomHeight - 2) * Convert.ToInt32(WallThickness) / Convert.ToInt32(ThermalPixelSize)];

        for (int i = 0; i < _airObjects.GetLength(0); i++)
        {
            for (int j = 0; j < _airObjects.GetLength(1); j++)
            {
                GameObject airObject = Instantiate(
                        AirPrefab, //the GameObject that will be instantiated
                        position: new Vector3(
                            x: (WallThickness + ThermalPixelSize) / 2 + i * AirPrefab.transform.lossyScale.x, //WallThickness - (WallThickness - 1) * 0.5f = w/2 + 0.5
                            y: (WallThickness + ThermalPixelSize) / 2 + j * AirPrefab.transform.lossyScale.y),
                        rotation: AirPrefab.transform.rotation) ;


                airObject.name = "Air_" + i + "." + j;
                airObject.transform.parent = gameObject.transform;
                airObject.GetComponent<AirTemperatureController>().Position = new Vector2Int(i, j);

                _airObjects[i,j] = airObject;
            }
        }

        #endregion

        #region Wall Creator

        //_windowPrefab.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        _wallPrefab.transform.localScale = _doorPrefab.transform.localScale = new Vector3(WallThickness, WallThickness, WallThickness);

        _wallObjects = new GameObject[_roomWidth, _roomHeight];

        for (int i = 0; i < _roomWidth; i++)
        {
            for (int j = 0; j < _roomHeight; j++)
            {
                if (walls[i, j] == RoomObjects.RoomElement.WALL)
                {
                    _wallObjects[i, j] = InstantiateWallObject(i, j, walls);
                }
                else if(walls[i, j] == RoomObjects.RoomElement.WINDOW)
                {
                    GameObject windowObject = Instantiate(
                               _windowPrefab, //the GameObject that will be instantiated
                               position: new Vector3(
                                   x: WallThickness * i,
                                   y: WallThickness * j),
                               rotation: _windowPrefab.transform.rotation);

                    windowObject.transform.parent = gameObject.transform;

                    windowObject.name = "Window_" + i + ":" + j;

                    if(i != 0)
                    {
                        if(walls[i - 1, j] == RoomObjects.RoomElement.WALL || walls[i - 1, j] == RoomObjects.RoomElement.WINDOW)
                        {
                            windowObject.transform.Rotate(0, 0, 90);
                        }
                    }

                    WindowController windowController = windowObject.GetComponent<WindowController>();
                    windowController.RemoteWindow = new RemoteWindow(serverConnection, "window");
                    thermalManagerBuilder.AddThermalObject(windowController);
                    windowController.RoomThermalManager = _roomThermalManager;

                    _wallObjects[i, j] = windowObject;
                }
                else if(walls[i, j] == RoomObjects.RoomElement.DOOR)
                {
                    GameObject doorObject = Instantiate(
                                _doorPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: WallThickness * i,
                                    y: WallThickness * j),
                                rotation: _windowPrefab.transform.rotation);

                    doorObject.transform.parent = gameObject.transform;

                    doorObject.name = "Door_" + i + ":" + j;

                    if (j != 0)
                    {
                        if (walls[i, j - 1] == RoomObjects.RoomElement.WALL || walls[i, j - 1] == RoomObjects.RoomElement.DOOR)
                        {
                            doorObject.transform.Rotate(0, 0, 90);
                        }
                    }


                    _wallObjects[i, j] = doorObject;
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
                                   x: (WallThickness * obj.PosY) + 0.5f,
                                   y: (WallThickness * obj.PosX) + 0.5f),
                               rotation: _chairPrefab.transform.rotation);
                //roomObject.GetComponent<>().setSprite(obj.Type);
            }
            else if(obj.Element == RoomObjects.RoomElement.TABLE)
            {
                roomObject = Instantiate(
                               _tablePrefab, //the GameObject that will be instantiated
                               position: new Vector3(
                                   x: (WallThickness * obj.PosY) + 0.5f,
                                   y: (WallThickness * obj.PosX) + 0.5f),
                               rotation: _tablePrefab.transform.rotation);
                roomObject.GetComponent<TableController>().setSprite(obj.Type);
            }else  if (obj.Element == RoomObjects.RoomElement.HEATER)
            {
                roomObject = Instantiate(
                               _heaterPrefab, //the GameObject that will be instantiated
                               position: new Vector3(
                                   x: (WallThickness * obj.PosY) + 0.5f,
                                   y: (WallThickness * obj.PosX) + 0.5f),
                               rotation: _tablePrefab.transform.rotation);
                //roomObject.GetComponent<TableController>().setSprite(obj.Type);
            }else if (obj.Element == RoomObjects.RoomElement.CLOSET)
            {
                roomObject = Instantiate(
                               _closetPrefab, //the GameObject that will be instantiated
                               position: new Vector3(
                                   x: (WallThickness * obj.PosY) + 0.5f,
                                   y: (WallThickness * obj.PosX) + 0.5f),
                               rotation: _tablePrefab.transform.rotation);
                //roomObject.GetComponent<TableController>().setSprite(obj.Type);
            }
            if (roomObject != null)
            {
                Vector3 size = roomObject.transform.localScale;
                size.y *= obj.Sizeheight * scaling * WallThickness;
                size.x *= obj.Sizewidth * scaling * WallThickness;
                roomObject.transform.localScale = size;
                roomObject.transform.Rotate(0, 0, obj.Rotation);
                roomObject.transform.parent = _furniture.transform;
            }
            
        }
        #endregion
    }

    private GameObject InstantiateWallObject(int i, int j, RoomObjects.RoomElement[,] walls)
    {
        GameObject wallObject = Instantiate(
                                WallPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: WallThickness * i,
                                    y: WallThickness * j),
                                rotation: WallPrefab.transform.rotation);

        wallObject.transform.parent = gameObject.transform;

        wallObject.name = "Wall_" + i + ":" + j;

        SetWallSprite(walls, i, j, wallObject);


        return wallObject;
    }

    private void SetWallSprite(RoomObjects.RoomElement[,] walls, int i, int j, GameObject wallObject)
    {
        bool[] neightbours = new bool[4];

        if (i == 0)
        {
            neightbours[1] = walls[i + 1, j] == RoomObjects.RoomElement.WALL;
            neightbours[3] = false;
        }
        else if (i == _roomWidth - 1)
        {
            neightbours[1] = false;
            neightbours[3] = walls[i - 1, j] == RoomObjects.RoomElement.WALL;
        }
        else
        {
            neightbours[1] = walls[i + 1, j] == RoomObjects.RoomElement.WALL;
            neightbours[3] = walls[i - 1, j] == RoomObjects.RoomElement.WALL;
        }

        if (j == 0)
        {
            neightbours[0] = walls[i, j + 1] == RoomObjects.RoomElement.WALL;
            neightbours[2] = false;
        }
        else if (j == _roomHeight - 1)
        {
            neightbours[0] = false;
            neightbours[2] = walls[i, j - 1] == RoomObjects.RoomElement.WALL;
        }
        else
        {
            neightbours[0] = walls[i, j + 1] == RoomObjects.RoomElement.WALL;
            neightbours[2] = walls[i, j - 1] == RoomObjects.RoomElement.WALL;
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

        SetTemperatureColors();

        #endregion
    }

    /// <summary>
    /// Sets the color of all air game-objects relative to the temperature of a air game-object
    /// to the lowest and highest temperature of all air game-objects.
    /// </summary>
    private void SetTemperatureColors()
    {
        float lowestTemperature;
        float highestTemperature;
        float temperatureStep;

        float colorSection;

        SetTemperaturesAndGetHighestAndLowest(out highestTemperature, out lowestTemperature);

        if (OptionsManager.DynamicSkalar)
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
        temperatureStep = (highestTemperature - lowestTemperature) / (AirColors.ColorArray.GetLength(0) - 1);

        for (int x = 0; x < _airObjects.GetLength(0); x++)
        {
            for (int y = 0; y < _airObjects.GetLength(1); y++)
            {
                colorSection = _airObjects[x, y].GetComponent<AirTemperatureController>().Temperature - lowestTemperature;
                colorSection = colorSection / temperatureStep;
                int colorindex = (int)Math.Round(colorSection, 0);
                colorindex = colorindex < 0 ? 0 : colorindex;
                colorindex = colorindex > AirColors.ColorArray.GetLength(0) - 1 ? AirColors.ColorArray.GetLength(0) - 1 : colorindex;
                // With "(TileTemp - lowestTemp) / temperatureStep", we create a transformation from [0, (highestTemp-lowestTemp)] -> [0, AmounOfColorsWeHave-1]
                _airObjects[x, y].GetComponent<AirTemperatureController>().SetColor(ref AirColors.ColorArray[colorindex]);
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
    private void SetTemperaturesAndGetHighestAndLowest(out float highestTemperature, out float lowestTemperature)
    {
        highestTemperature = float.MinValue;
        lowestTemperature = float.MaxValue;

        for (int x = 0; x < _airObjects.GetLength(0); x++)
        {
            for (int y = 0; y < _airObjects.GetLength(1); y++)
            {
                float temperature = _roomThermalManager.GetTemperature(_airObjects[x,y].transform.position).ToCelsius().Value;

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
    }
}
