using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class RoomCreator : MonoBehaviour
{
    [SerializeField]
    private int _roomWidth = 30;

    [SerializeField] 
    private int _roomHeight = 30;

    [SerializeField]
    private int _wallsPerGrid = 3;

    [SerializeField]
    private float _wallThickness = 1f;

    //This is no [SerializeField] because the value is calculated automatically and depends on WallThickness and WallsPerGrid.
    private float _wallSize = 0f;

    [SerializeField]
    private GameObject _airPrefab;

    [SerializeField]
    private GameObject _wallPrefab;

    [SerializeField]
    private GameObject _userPrefab;

    [SerializeField]
    private float _initialTemperature = 22f;

    private float _passedTime = 0;

    private float _waitTimer = 0.5f;

    private GameObject[,] _airObjects;

    private GameObject[,][,] _wallObjects;

    [SerializeField]
    private float _initialThermalPixelSize = 1f;

    private IRoomThermalManager _roomThermalManager;

    #region Properties

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public GameObject AirPrefab
    {
        get => _airPrefab;
        set => throw new NotImplementedException(); //TODO: Implemet me!
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
    /// TODO: Write me!
    /// </summary>
    public int RoomWidth
    {
        get => _roomWidth;
        set => throw new NotImplementedException(); //TODO: Implemet me!
    }

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public int RoomHeight
    {
        get => _roomHeight;
        set => throw new NotImplementedException(); //TODO: Implemet me!
    }

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public int WallsPerGrid
    {
        get => _wallsPerGrid;
        set => throw new NotImplementedException(); //TODO: Implemet me!
    }

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public float WallThickness
    {
        get => _wallThickness;
        set => throw new NotImplementedException(); //TODO: Implemet me!
    }

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public float WallSize
    {
        get => _wallSize;
    }

    /// <summary>
    /// TODO: Write me!
    /// </summary>
    public float ThermalPixelSize
    {
        get => _roomThermalManager.ThermalPixelSize;
        set => _roomThermalManager.ThermalPixelSize = value;
    }

    #endregion

    public RoomCreator()
    {
        _wallSize = WallThickness / WallsPerGrid;
    }

    // Start is called before the first frame update
    void Start()
    {
        #region ThermalManager

        RoomThermalManagerBuilder thermalManagerBuilder = new RoomThermalManagerBuilder();
        thermalManagerBuilder.InitialTemperature = new Temperature(_initialTemperature, TemperatureUnit.Celsius);
        thermalManagerBuilder.ThermalPixelSize = _initialThermalPixelSize;
        thermalManagerBuilder.Position = new Vector3(1, 1, 0);
        thermalManagerBuilder.Size = new Vector3(RoomWidth - 1, RoomHeight - 1);

        _roomThermalManager = thermalManagerBuilder.Build();
        _roomThermalManager.Start();

        #endregion

        GameObject userObject = Instantiate(_userPrefab);
        UserController userController = userObject.GetComponent<UserController>();
        userController.RoomThermalManager = _roomThermalManager;

        #region Air Creator

        AirPrefab.transform.localScale = new Vector3(
            x: WallThickness,
            y: WallThickness,
            z: WallThickness);

        _airObjects = new GameObject[
            (RoomHeight - 2) * WallsPerGrid,
            (RoomWidth - 2) * WallsPerGrid];

        for (int i = 1; i < RoomHeight - 1; i++)
        {
            for (int j = 1; j < RoomWidth - 1; j++)
            {
                for (int m = 0; m < WallsPerGrid; m++)
                {
                    for (int n = 0; n < WallsPerGrid; n++)
                    {
                        Vector2Int position = new Vector2Int(
                            x: (i - 1) * WallsPerGrid + m,
                            y: (j - 1) * WallsPerGrid + n);

                        GameObject airObject = Instantiate(
                                AirPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: i * WallThickness + m * WallSize,
                                    y: j * WallThickness + n * WallSize),
                                rotation: AirPrefab.transform.rotation);

                        airObject.transform.parent = gameObject.transform;
                        airObject.GetComponent<AirTemperatureController>().Position = position;
                        
                        _airObjects[position.x, position.y] = airObject;
                    }
                }
            }
        }

        #endregion

        #region Wall Creator

        float tileSizeX = transform.lossyScale.x;
        float tileSizeY = transform.localScale.y;

        _wallObjects = new GameObject[RoomHeight, RoomWidth][,];

        for (int i = 0; i < RoomHeight; i++)
        {
            for (int j = 0; j < RoomWidth; j++)
            {
                if ((i == 0 || i == RoomHeight - 1) ||
                    (j == 0 || j == RoomWidth - 1))
                {
                    _wallObjects[i, j] = new GameObject[WallsPerGrid, WallsPerGrid];

                    for (int m = 0; m < WallsPerGrid; m++)
                    {
                        for (int n = 0; n < WallsPerGrid; n++)
                        {
                            GameObject wallObject = Instantiate(
                                WallPrefab, //the GameObject that will be instantiated
                                position: new Vector3(
                                    x: WallThickness * i + m * WallSize,
                                    y: WallThickness * j + n * WallSize),
                                rotation: WallPrefab.transform.rotation);

                            wallObject.transform.parent = gameObject.transform;

                            _wallObjects[i, j][m, n] = wallObject;
                        }
                    }
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
        SetTemperaturesAndGetHighestAndLowest(out float highestTemperature, out float lowestTemperature);

        for (int x = 0; x < _airObjects.GetLength(x); x++)
        {
            for (int y = 0; y < _airObjects.GetLength(y); y++)
            {
                _airObjects[x, y].GetComponent<AirTemperatureController>().SetColor(highestTemperature, lowestTemperature);
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
    ///When this method returns, contains single-precision floating-point number equivalent to
    /// the numeric value of the lowest temperature displayed in the currently rendered frame.
    /// </param>
    private void SetTemperaturesAndGetHighestAndLowest(out float highestTemperature, out float lowestTemperature)
    {
        highestTemperature = float.MinValue;
        lowestTemperature = float.MaxValue;

        for (int x = 0; x < _airObjects.GetLength(x); x++)
        {
            for (int y = 0; y < _airObjects.GetLength(y); y++)
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
    }
}
