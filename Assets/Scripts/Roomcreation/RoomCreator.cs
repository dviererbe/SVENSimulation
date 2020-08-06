using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Assets.Scripts;
using Assets.Scripts.ObjectController;
using Assets.Scripts.Pathfinding;
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

    private List<Tuple<GameObject, Vertex, RemoteHeater>> _heaterList = new List<Tuple<GameObject, Vertex, RemoteHeater>>();

    private List<Tuple<GameObject, Vertex>> _doorList = new List<Tuple<GameObject, Vertex>>();

    private List<Tuple<GameObject, Vertex, RemoteWindow>> _windowList = new List<Tuple<GameObject, Vertex, RemoteWindow>>();

    private List<GameObject> _tableList = new List<GameObject>();

    private List<Tuple<GameObject, Vertex>> _chairStudentList = new List<Tuple<GameObject, Vertex>>();

    private List<Tuple<GameObject, Vertex>> _chairLecturerList = new List<Tuple<GameObject, Vertex>>();

    private List<GameObject> _closetList = new List<GameObject>();

    private List<Tuple<Vector2, RemoteThermometer>> _remoteThermometers = new List<Tuple<Vector2, RemoteThermometer>>();

    private List<Tuple<Vertex, RemoteTablet>> _tabletList = new List<Tuple<Vertex, RemoteTablet>>();

    private Graph _roomGraph;

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

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public float ThermalPixelSize
    {
        get => _roomThermalManager?.ThermalPixelSize ?? OptionsManager.ThermalPixelSize;
    }

    public Graph RoomGraph => _roomGraph;

    public IReadOnlyList<Tuple<GameObject, Vertex>> ChairStudentPositions
    {
        get { return _chairStudentList; }
    }
    public IReadOnlyList<Tuple<GameObject, Vertex>> ChairLecturerPositions
    {
        get { return _chairLecturerList; }
    }

    public IReadOnlyList<Tuple<GameObject, Vertex, RemoteWindow>> WindowPositions
    {
        get { return _windowList; }
    }

    public IReadOnlyList<Tuple<GameObject, Vertex>> DoorPositions
    {
        get { return _doorList; }
    }

    public IReadOnlyList<Tuple<Vector2, RemoteThermometer>> RemoteThermometers
    {
        get { return _remoteThermometers; }
    }

    public LSFInfoSchnittstelle LSFInfoSchnittstelle
    {
        get { return _lsfInfoSchnittstelle; }
    }

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        RoomReader roomreader = new RoomReader(OptionsManager.RoomFile);
        RoomObjects[] roomObjects = roomreader.ReadRoom();

        #region PreperationsGraph

        _roomGraph = new Graph();

        #endregion

        #region Load Options

        _roomHeight = Mathf.RoundToInt(OptionsManager.RoomHeight);
        _roomWidth = Mathf.RoundToInt(OptionsManager.RoomWidth);
        _wallThickness = OptionsManager.WallThickness;

        #endregion

        IServerConnection serverConnection = ServerConnectionFactory.CreateServerConnection(OptionsManager.Username, OptionsManager.Password, OptionsManager.ServerAddress, OptionsManager.RequiresAuthentication);
        _lsfInfoSchnittstelle = new LSFInfoSchnittstelle(serverConnection, "LSF_Info_Sim");

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

        
        foreach (RoomObjects roomObjektNewRoomObjekt in roomObjects)
        {
            GameObject gameObjektNewRoomObject = null;

            if(roomObjektNewRoomObjekt.Element == RoomObjects.RoomElement.CHAIR)
            {
                gameObjektNewRoomObject = Instantiate(
                                _chairPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + roomObjektNewRoomObjekt.PositionX) + 0.5f,
                                    y: (WallThickness + roomObjektNewRoomObjekt.PositionY) + 0.5f),
                                rotation: _chairPrefab.transform.rotation);

                if (roomObjektNewRoomObjekt.Type.Equals("Dozent"))
                {
                    _chairLecturerList.Add(new Tuple<GameObject, Vertex>(gameObjektNewRoomObject,
                        _roomGraph.AddVertex(getCenterOfChair(gameObjektNewRoomObject.transform.position, roomObjektNewRoomObjekt))));
                }
                else
                {
                    _chairStudentList.Add(new Tuple<GameObject, Vertex>(gameObjektNewRoomObject,
                        _roomGraph.AddVertex(getCenterOfChair(gameObjektNewRoomObject.transform.position, roomObjektNewRoomObjekt))));
                }
            }
            else if(roomObjektNewRoomObjekt.Element == RoomObjects.RoomElement.TABLE)
            {
                gameObjektNewRoomObject = Instantiate(
                                _tablePrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + roomObjektNewRoomObjekt.PositionX) + 0.5f,
                                    y: (WallThickness + roomObjektNewRoomObjekt.PositionY) + 0.5f),
                                rotation: _tablePrefab.transform.rotation);
                gameObjektNewRoomObject.GetComponent<TableController>().setSprite(roomObjektNewRoomObjekt.Type);
                _tableList.Add(gameObjektNewRoomObject);
                _roomGraph.AddSqaureObject(roomObjektNewRoomObjekt);
            }else  if (roomObjektNewRoomObjekt.Element == RoomObjects.RoomElement.HEATER)
            {
                gameObjektNewRoomObject = Instantiate(
                                _heaterPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + roomObjektNewRoomObjekt.PositionX) + 0.5f,
                                    y: (WallThickness + roomObjektNewRoomObjekt.PositionY) + 0.5f),
                                rotation: _tablePrefab.transform.rotation);
                //roomObject.GetComponent<TableController>().setSprite(obj.Type);
                RemoteHeater remoteHeater = new RemoteHeater(serverConnection, roomObjektNewRoomObjekt.GetNameFHEM, roomObjektNewRoomObjekt.SetNameFHEM);
                _heaterList.Add(new Tuple<GameObject, Vertex, RemoteHeater>(gameObjektNewRoomObject, _roomGraph.AddVertex(gameObjektNewRoomObject.transform.position), remoteHeater));
                //thermalManagerBuilder.AddThermalObject(roomObject.GetComponent<HeaterController>());
                _roomGraph.AddVertex((Vector2)gameObjektNewRoomObject.transform.position);
            }
            else if (roomObjektNewRoomObjekt.Element == RoomObjects.RoomElement.CLOSET)
            {
                gameObjektNewRoomObject = Instantiate(
                                _closetPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + roomObjektNewRoomObjekt.PositionX) + 0.5f,
                                    y: (WallThickness + roomObjektNewRoomObjekt.PositionY) + 0.5f),
                                rotation: _tablePrefab.transform.rotation);
                //roomObject.GetComponent<TableController>().setSprite(obj.Type);
                _closetList.Add(gameObjektNewRoomObject);
            }
            else if(roomObjektNewRoomObjekt.Element == RoomObjects.RoomElement.DOOR)
            {
                gameObjektNewRoomObject = Instantiate(
                                _doorPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + roomObjektNewRoomObjekt.PositionX) + 0.5f,
                                    y: (WallThickness + roomObjektNewRoomObjekt.PositionY) + 0.5f),
                                rotation: _tablePrefab.transform.rotation);
                //roomObject.GetComponent<TableController>().setSprite(obj.Type);
                _doorList.Add(new Tuple<GameObject, Vertex>(gameObjektNewRoomObject, _roomGraph.AddVertex(gameObjektNewRoomObject.transform.position)));
            }
            else if (roomObjektNewRoomObjekt.Element == RoomObjects.RoomElement.WINDOW)
            {
                gameObjektNewRoomObject = Instantiate(
                                _windowPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: (WallThickness + roomObjektNewRoomObjekt.PositionX) + 0.5f,
                                    y: (WallThickness + roomObjektNewRoomObjekt.PositionY) + 0.5f),
                                rotation: _tablePrefab.transform.rotation);
                //roomObject.GetComponent<TableController>().setSprite(obj.Type);
                thermalManagerBuilder.AddThermalObject(gameObjektNewRoomObject.GetComponent<WindowController>());
                RemoteWindow remoteWindow = new RemoteWindow(serverConnection, roomObjektNewRoomObjekt.GetNameFHEM, roomObjektNewRoomObjekt.SetNameFHEM);
                gameObjektNewRoomObject.GetComponent<WindowController>().RemoteWindow = remoteWindow;
                _windowList.Add(new Tuple<GameObject, Vertex, RemoteWindow>(gameObjektNewRoomObject, 
                    _roomGraph.AddVertex(gameObjektNewRoomObject.transform.position), remoteWindow));
            }
            else if (roomObjektNewRoomObjekt.Element == RoomObjects.RoomElement.TABLET)
            {
                _tabletList.Add(new Tuple<Vertex, RemoteTablet>(_roomGraph.AddVertex(new Vector2(roomObjektNewRoomObjekt.PositionX, roomObjektNewRoomObjekt.PositionY)), 
                    new RemoteTablet(new RemoteTargetTemperature(serverConnection, roomObjektNewRoomObjekt.GetNameFHEM, roomObjektNewRoomObjekt.SetNameFHEM),
                    new RemoteWindow(serverConnection, "Sven_FensterManager_Sim", "Fensterkontakt_Sim"))));
            }
            else if (roomObjektNewRoomObjekt.Element == RoomObjects.RoomElement.THERMOMETER)
            {
                _remoteThermometers.Add(new Tuple<Vector2, RemoteThermometer>(new Vector2(roomObjektNewRoomObjekt.PositionX, roomObjektNewRoomObjekt.PositionY), 
                    new RemoteThermometer(serverConnection, roomObjektNewRoomObjekt.GetNameFHEM, roomObjektNewRoomObjekt.SetNameFHEM)));
            }

            if (gameObjektNewRoomObject != null)
            {
                gameObjektNewRoomObject.transform.Rotate(0, 0, roomObjektNewRoomObjekt.Rotation);
                gameObjektNewRoomObject.transform.parent = _furniture.transform;
                Vector3 objectsize = gameObjektNewRoomObject.transform.localScale;
                objectsize.x *= roomObjektNewRoomObjekt.Sizewidth;
                objectsize.y *= roomObjektNewRoomObjekt.Sizeheight;
                gameObjektNewRoomObject.transform.localScale = objectsize;

            }
        }

        _roomGraph.MeshGraph();
        _roomGraph.PrintGraph();
        #endregion

        #region Thermometer

        //Build and start Thermal Manager
        _roomThermalManager = thermalManagerBuilder.Build();

        GameObject thermometerObject = Instantiate(_thermometerPrefab);
        thermometerObject.transform.parent = gameObject.transform;
        ThermometerController thermometerController = thermometerObject.GetComponent<ThermometerController>();
        thermometerController.RoomThermalManager = _roomThermalManager;
        thermometerController.RemoteThermometer = _remoteThermometers[0].Item2;
        thermometerController.Position = new Vector3(1, 1);

        #endregion

        _roomThermalManager.Start();
    }

    private Vector2 getCenterOfChair(Vector2 position, RoomObjects roomObject)
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
