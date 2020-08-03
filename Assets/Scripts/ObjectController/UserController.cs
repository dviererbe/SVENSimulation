using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UserController : MonoBehaviour, IThermalObject
{
    public enum UserState
    {
        Unknown,
        Idle,
        Moving
    }

    [SerializeField]
    private float _userRadius = 2f;

    [SerializeField]
    private TextMesh _userStateTextMesh;

    private Rigidbody2D _rigidbody;

    private Vector2 _bottomLeftCorner;
    private Vector2 _upperRightCorner;

    [SerializeField]
    private float _probabilityOfChangingDirectionOnUpdate = 0.005f;

    [SerializeField]
    private float _playerSpeed = 0.5f;
    
    private Vector2 _direction;

    private float _normalizedMaxOkTemperature;

    private float _normalizedMinOkTemperature;

    private UserGroupController _userGroupController = null;

    public UserController()
    {
        _normalizedMinOkTemperature = Random.value;
        _normalizedMaxOkTemperature = Random.value;
    }

    public Temperature MinOkTemperature => Temperature.FromCelsius(Mathf.Lerp(OptionsManager.LowerMinOkUserTemperature,
        OptionsManager.UpperMinOkUserTemperature, _normalizedMinOkTemperature));

    public Temperature MaxOkTemperature => Temperature.FromCelsius(Mathf.Lerp(OptionsManager.LowerMaxOkUserTemperature,
        OptionsManager.UpperMaxOkUserTemperature, _normalizedMaxOkTemperature));

    public bool IsFreezing { get; private set; } = false;

    public bool IsSweating { get; private set; } = false;

    private IRoomThermalManager RoomThermalManager { get; set; }

    public UserGroupController UserGroupController
    {
        get => _userGroupController;
        set
        {
            if (_userGroupController == null)
                _userGroupController = value;
            else
                throw new NotSupportedException("Can't reassign user group controller.");
        }
    }

    public UserState State { get; private set; } = UserState.Unknown;

    private static Vector2 GetRandomDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private bool IsPointInRoom(Vector2 point)
    {
        return point.x >= _bottomLeftCorner.x &&
               point.y >= _bottomLeftCorner.y &&
               point.x <= _upperRightCorner.x &&
               point.y <= _upperRightCorner.y;
    }

    private void MovePlayerToMiddleOfRoom()
    {
        _rigidbody.position = new Vector2(
            x: RoomThermalManager.Room.RoomPosition.x + RoomThermalManager.Room.RoomSize.x / 2f,
            y: RoomThermalManager.Room.RoomPosition.y + RoomThermalManager.Room.RoomSize.y / 2f);
    }

    private Vector2 CalculateNextPoint()
    {
        return _rigidbody.position + _playerSpeed * Time.deltaTime * _direction;
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _bottomLeftCorner = RoomThermalManager.Room.RoomPosition + new Vector3(_userRadius, _userRadius);
        _upperRightCorner = RoomThermalManager.Room.RoomPosition + RoomThermalManager.Room.RoomSize - new Vector3(_userRadius, _userRadius); ;
        _direction = GetRandomDirection();

        MovePlayerToMiddleOfRoom();
    }

    void Update()
    {
        Temperature? temperature = RoomThermalManager?.GetTemperature(_rigidbody.position).ToCelsius();

        if (UserGroupController.GroupState == UserGroupController.UserGroupState.Lecture)
        {
            State = UserState.Idle;
        }
        else if (UserGroupController.GroupState == UserGroupController.UserGroupState.Pause)
        {
            State = UserState.Moving;

            float random = Random.value;

            if (random <= _probabilityOfChangingDirectionOnUpdate)
            {
                _direction = GetRandomDirection();
            }

            bool valueUnchanged = true;

            do
            {
                Vector2 newPosition = CalculateNextPoint();

                if (IsPointInRoom(newPosition))
                {
                    _rigidbody.position = newPosition;
                    valueUnchanged = false;
                }
                else
                {
                    _direction = GetRandomDirection();
                }
            }
            while (valueUnchanged);
        }
        else
        {
            State = UserState.Unknown;
        }

        if (temperature.HasValue)
        {
            if (temperature > MaxOkTemperature)
            {
                IsSweating = true;
                IsFreezing = false;
            }
            else if (temperature < MinOkTemperature)
            {
                IsSweating = false;
                IsFreezing = true;
            }
            else
            {
                IsSweating = false;
                IsFreezing = false;
            }

        }
        else
        {
            IsFreezing = false;
            IsSweating = false;
        }

        string sweatingText = IsSweating ? "[Sweating]" : string.Empty;
        string freezingText = IsFreezing ? "[Freezing]" : string.Empty;

        _userStateTextMesh.text = $"{State} {sweatingText}{freezingText} ({temperature?.ToString() ?? "No Data"})";
    }

    /// <summary>
    /// Gets if the <see cref="IThermalObject"/> can not change its position.
    /// <see langword="true" /> can not change its position; otherwise <see langword="false"/>.
    /// </summary>
    public bool CanNotChangePosition => false;

    /// <summary>
    /// Gets the absolute (global) Position of the <see cref="IThermalObject"/> in m.
    /// </summary>
    public Vector3 Position => _rigidbody.position;

    /// <summary>
    /// Gets how large the <see cref="IThermalObject"/> is in m (meter).
    /// </summary>
    public Vector3 Size => new Vector3(1f, 1f);

    /// <summary>
    /// Gets the area of the surface of the <see cref="IThermalObject"/> in m² (square meter).
    /// </summary>
    public float ThermalSurfaceArea { get; private set; } = 2f;

    /// <summary>
    /// Gets the <see cref="ThermalMaterial"/> of the <see cref="IThermalObject"/>.
    /// </summary>
    /// <remarks>
    /// Used to calculate the temperature and the heat transfer from and to the the <see cref="IThermalObject"/>.
    /// </remarks>
    public ThermalMaterial ThermalMaterial => ThermalMaterial.Human;

    /// <summary>
    /// Gets the temperature of the <see cref="IThermalObject"/>.
    /// </summary>
    public Temperature Temperature => Temperature.FromCelsius(32f);

    /// <summary>
    /// A <see cref="IRoomThermalManager"/> signals the <see cref="IThermalObject"/> that the thermal simulation was started.
    /// </summary>
    /// <param name="roomThermalManager">
    /// The <see cref="IRoomThermalManager"/> that starts the thermal simulation with this <see cref="IThermalObject"/>. 
    /// </param>
    public void ThermalStart(IRoomThermalManager roomThermalManager)
    {
        RoomThermalManager = roomThermalManager;
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
        //((Skin Surface Area) / (Body Height)) * (Height of Thermal Pixel)
        //((       1m³       ) /     1.8m     ) * (Height of Thermal Pixel) 
        ThermalSurfaceArea = (2f / 1.8f) * roomThermalManager.ThermalPixelSize;
    }
}
