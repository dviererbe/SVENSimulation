using System;
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
    private float _wallThickness = 0.02f;

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

    private RoomGraph _roomGraph;

    private IRoomThermalManager _roomThermalManager;

    private LSFInfoSchnittstelle _lsfInfoSchnittstelle;

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

    public float ThermalPixelSize => _roomThermalManager?.ThermalPixelSize ?? OptionsManager.ThermalPixelSize;

    public RoomGraph RoomGraph => _roomGraph;

    public LectureState LectureState { get; } = LectureState.None;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        RoomReader roomreader = new RoomReader(OptionsManager.RoomFile);
        RoomObject[] roomObjects = roomreader.ReadRoom();

        #region PreperationsGraph

        _roomGraph = new RoomGraph();

        #endregion

        #region Load Options

        _roomHeight = Mathf.RoundToInt(OptionsManager.RoomHeight);
        _roomWidth = Mathf.RoundToInt(OptionsManager.RoomWidth);
        _wallThickness = OptionsManager.WallThickness;

        #endregion

        IServerConnection serverConnection = ServerConnectionFactory.CreateServerConnection(OptionsManager.Username, OptionsManager.Password, OptionsManager.ServerAddress, OptionsManager.RequiresAuthentication);
        _lsfInfoSchnittstelle = new LSFInfoSchnittstelle(serverConnection, "Der Name fehlt noch!!");

        #region ThermalManager

        GameObject userGroupControllerObject = Instantiate(_userGroupControllerPrefab);
        UserGroupController userGroupController = userGroupControllerObject.GetComponent<UserGroupController>();

        RoomThermalManagerBuilder thermalManagerBuilder = new RoomThermalManagerBuilder();
        thermalManagerBuilder.Room = this;
        thermalManagerBuilder.ThermalPixelSize = new Temperature(OptionsManager.InitialRoomTemperature, TemperatureUnit.Celsius);
        thermalManagerBuilder.OutsideTemperature = OutsideTemperatureSource.Instance;
        thermalManagerBuilder.InitialRoomTemperature = new NoisyTemperatureSource(
                baseTemperatureValue: Temperature.FromCelsius(OptionsManager.InitialRoomTemperature), 
                noiseOffset: Temperature.FromKelvin(0.5f));
        thermalManagerBuilder.ThermalPixelSize = OptionsManager.ThermalPixelSize;
        userGroupController.CreateUsers(thermalManagerBuilder);
        
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

        float aktWallVertexDistance = OptionsManager.WallVertexDistance;

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

        #region InsertWallVertex

        for(float i = wallThermalPixelCount + OptionsManager.WallVertexDistance; i < _roomObjects.GetLength(0) - wallThermalPixelCount - OptionsManager.WallVertexDistance;
            i += OptionsManager.WallVertexDistance)
        {
            _roomGraph.AddVertex(new Vector2(
                x: i,
                y: wallThermalPixelCount + OptionsManager.VertexObjectOffSet));
            _roomGraph.AddVertex(new Vector2(
                x: i,
                y: _roomObjects.GetLength(1) - wallThermalPixelCount - OptionsManager.VertexObjectOffSet));
        }

        // *2 becaus double Corners are a Problem
        for (float i = wallThermalPixelCount + OptionsManager.WallVertexDistance * 2; i < _roomObjects.GetLength(1) - wallThermalPixelCount - OptionsManager.WallVertexDistance * 2;
           i += OptionsManager.WallVertexDistance)
        {
            _roomGraph.AddVertex(new Vector2(
                x: wallThermalPixelCount + OptionsManager.VertexObjectOffSet,
                y: i));
            _roomGraph.AddVertex(new Vector2(
                x: _roomObjects.GetLength(0) - wallThermalPixelCount - OptionsManager.VertexObjectOffSet,
                y: i));
        }

        #endregion

        #region RoomObjects Creator

        foreach (RoomObject roomObject in roomObjects)
        {
            InstatiateRoomGameObject(roomObject, serverConnection, thermalManagerBuilder);
        }

        _roomGraph.MeshGraph();
        #endregion

        //Build and start Thermal Manager
        _roomThermalManager = thermalManagerBuilder.Build();
        _roomThermalManager.Start();
    }

    private void InstatiateRoomGameObject(RoomObject roomObject, IServerConnection serverConnection,
        RoomThermalManagerBuilder thermalManagerBuilder)
    {
        switch (roomObject.Element)
        {
            case RoomObject.RoomElement.CHAIR:
                InstantiateChairGameObject(roomObject);
                break;
            case RoomObject.RoomElement.TABLE:
                InstantiateTableGameObject(roomObject);
                break;
            case RoomObject.RoomElement.HEATER:
                InstantiateHeaterGameObject(roomObject, serverConnection);
                break;
            case RoomObject.RoomElement.CLOSET:
                InstantiateClosetGameObject(roomObject);
                break;
            case RoomObject.RoomElement.DOOR:
                InstantiateDoorGameObject(roomObject);
                break;
            case RoomObject.RoomElement.WINDOW:
                InstantiateWindowGameObject(roomObject, thermalManagerBuilder, serverConnection);
                break;
            case RoomObject.RoomElement.TABLET:
                InstatiateTabletGameObject(roomObject, serverConnection);
                break;
            case RoomObject.RoomElement.THERMOMETER:
                InstatiateThermometerGameObject(roomObject, serverConnection);
                break;
        }
    }

    private void InstatiateTabletGameObject(RoomObject roomObject, IServerConnection serverConnection)
    {
        RemoteTargetTemperature remoteTargetTemperature = new RemoteTargetTemperature(serverConnection, roomObject.FhemGetName, roomObject.FhemSetName);
        RemoteWindow remoteWindow = new RemoteWindow(serverConnection, "Sven_FensterManager_Sim", "Fensterkontakt_Sim");

        RemoteTablet remoteTablet = new RemoteTablet(remoteTargetTemperature, remoteWindow);
        
        _roomGraph.AddTablet(
            position: new Vector2(roomObject.PositionX, roomObject.PositionY),
            remoteTablet: remoteTablet);
    }

    private void InstatiateThermometerGameObject(RoomObject roomObject, IServerConnection serverConnection)
    {
        RemoteThermometer remoteThermometer = new RemoteThermometer(serverConnection, roomObject.FhemGetName, roomObject.FhemSetName);

        GameObject thermometerObject = Instantiate(_thermometerPrefab);
        thermometerObject.transform.parent = gameObject.transform;
        ThermometerController thermometerController = thermometerObject.GetComponent<ThermometerController>();
        thermometerController.RoomThermalManager = _roomThermalManager;
        thermometerController.RemoteThermometer = remoteThermometer;
        thermometerController.Position = new Vector3(1, 1);
    }

    private void InstantiateWindowGameObject(RoomObject roomObject, RoomThermalManagerBuilder thermalManagerBuilder,
        IServerConnection serverConnection)
    {
        GameObject window = Instantiate(
            _windowPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        thermalManagerBuilder.AddThermalObject(window.GetComponent<WindowController>());

        Vector2 windowVertexPosition = window.GetComponentInChildren<GameObject>().transform.position;
        
        _roomGraph.AddVertex(windowVertexPosition);
        RemoteWindow remoteWindow = new RemoteWindow(serverConnection, roomObject.FhemGetName, roomObject.FhemSetName);

        Transform(window, roomObject);
    }

    private void InstantiateDoorGameObject(RoomObject roomObject)
    {
        GameObject door = Instantiate(
            _doorPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        _roomGraph.AddDoor(door.transform.position);

        Transform(door, roomObject);
    }

    private void InstantiateClosetGameObject(RoomObject roomObject)
    {
        GameObject closet = Instantiate(
            _closetPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        Transform(closet, roomObject);
    }

    private void InstantiateHeaterGameObject(RoomObject roomObject, IServerConnection serverConnection)
    {
        GameObject heater = Instantiate(
            _heaterPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        RemoteHeater remoteHeater = new RemoteHeater(serverConnection, roomObject.FhemGetName, roomObject.FhemSetName);

        _roomGraph.AddVertex(heater.transform.position);

        Transform(heater, roomObject);
    }


    private void InstantiateChairGameObject(RoomObject roomObject)
    {
        GameObject chair = Instantiate(
            _chairPrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _chairPrefab.transform.rotation);

        Vector2 centerOfChair = CalculateCenterOfRoomObject(
            position: chair.transform.position,
            roomObject: roomObject);

        if (roomObject.Type.Equals("Dozent"))
        {
            _roomGraph.AddLecturerChair(centerOfChair);
        }
        else
        {
            _roomGraph.AddStudentsChair(centerOfChair);
        }

        Transform(chair, roomObject);
    }

    private void InstantiateTableGameObject(RoomObject roomObject)
    {
        GameObject table = Instantiate(
            _tablePrefab, //the GameObject that will be instantiated
            position: new Vector3(
                x: (WallThickness + roomObject.PositionX) + 0.5f,
                y: (WallThickness + roomObject.PositionY) + 0.5f),
            rotation: _tablePrefab.transform.rotation);

        table.GetComponent<TableController>().setSprite(roomObject.Type);

        _roomGraph.AddSqaureObject(roomObject);

        Transform(table, roomObject);
    }

    private void Transform(GameObject gameObject, RoomObject roomObject)
    {
        gameObject.transform.Rotate(0, 0, roomObject.Rotation);
        gameObject.transform.parent = _furniture.transform;
        
        Vector3 objectSize = gameObject.transform.localScale;
        objectSize.x *= roomObject.Sizewidth;
        objectSize.y *= roomObject.Sizeheight;
        
        gameObject.transform.localScale = objectSize;
    }

    private static Vector2 CalculateCenterOfRoomObject(Vector2 position, RoomObject roomObject)
    {
        float cosinus = Mathf.Cos((float)roomObject.RotationRadians);
        float sinus = Mathf.Sin((float)roomObject.RotationRadians);

        return new Vector2
        (
            x: position.x + (cosinus * roomObject.Sizewidth) - (sinus * roomObject.Sizeheight),
            y: position.y + (sinus * roomObject.Sizewidth) + (cosinus * roomObject.Sizeheight)
        );
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
