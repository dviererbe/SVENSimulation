using System;
using System.Collections.Generic;
using System.Xml;
using Assets.Scripts;
using Assets.Scripts.Pathfinding;
using Assets.Scripts.Remote;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class UserController : MonoBehaviour, IThermalObject
{
    public enum UserRole
    {
        Student,
        Lecturer
    }

    public enum UserState
    {
        Unknown,
        LeavingRoom,
        GoToSeat,
        Listening,
        Lecturing,
        Idle,
        Moving,
        OpeningWindow,
        ClosingWindow,
        TurningUpHeater,
        TurningDownHeater,
    }

    public event Action<UserController> Destroyed;

    private bool _initialized = false;

    private Vertex _lastVertex;
    private Vertex _nextVertex;
    private Path _currentPath = null;

    private Vertex _seat;
    private (Vertex Vertex, RemoteTablet RemoteTablet) _tablet;

    [SerializeField]
    private TextMesh _userStateTextMesh;

    [SerializeField]
    private float _probabilityOfChangingDirectionOnUpdate = 0.005f;

    private float _normalizedUserSpeed = 0f;
    
    private float _normalizedMaxOkTemperature = 0f;

    private float _normalizedMinOkTemperature = 0f;

    private Rigidbody2D _rigidbody;

    /// <summary>
    /// Gets the movement speed of the user.
    /// </summary>
    public float UserSpeed =>
        Mathf.Lerp(OptionsManager.MinUserSpeed, OptionsManager.MaxUserSpeed, _normalizedUserSpeed);

    /// <summary>
    /// Gets the lowest temperature that is okay for the user. The user will freeze for temperatures below this temperature.
    /// </summary>
    public Temperature MinOkTemperature => Temperature.FromCelsius(Mathf.Lerp(OptionsManager.LowerMinOkUserTemperature,
        OptionsManager.UpperMinOkUserTemperature, _normalizedMinOkTemperature));

    /// <summary>
    /// Gets the highest temperature that is okay for the user. The user will sweat for temperatures above this temperature.
    /// </summary>
    public Temperature MaxOkTemperature => Temperature.FromCelsius(Mathf.Lerp(OptionsManager.LowerMaxOkUserTemperature,
        OptionsManager.UpperMaxOkUserTemperature, _normalizedMaxOkTemperature));

    /// <summary>
    /// Gets if the user is freezing. This indicates that the temperature is too low for the user to be comfortable.
    /// </summary>
    public bool IsFreezing { get; private set; } = false;

    /// <summary>
    /// Gets if the user is sweating. This indicates that the temperature is too high for the user to be comfortable.
    /// </summary>
    public bool IsSweating { get; private set; } = false;

    private IRoomThermalManager RoomThermalManager { get; set; }

    public UserGroupController UserGroupController { get; private set; }

    /// <summary>
    /// Gets the role of the user.
    /// </summary>
    public UserRole Role { get; private set; } = UserRole.Student;

    /// <summary>
    /// Gets the current state of the user.
    /// </summary>
    public UserState State { get; private set; } = UserState.Unknown;

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

    public void Initialize(UserGroupController userGroupController, UserRole role, Vertex userSeat)
    {
        if (_initialized)
            throw new InvalidOperationException("User was already initialized!");

        _initialized = true;

        UserGroupController = userGroupController;
        Role = role;

        _seat = userSeat;
    }

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

    void Start()
    {
        _normalizedMinOkTemperature = Random.value;
        _normalizedMaxOkTemperature = Random.value;

        _rigidbody = GetComponent<Rigidbody2D>();

        Vertex doorVertex = GetRandomDoorVertex();
        _lastVertex = _nextVertex = doorVertex;

        _tablet = RoomThermalManager.Room.RoomGraph.Tablets[0];
    }

    void Update()
    {
        Temperature? temperature = RoomThermalManager?.GetTemperature(_rigidbody.position).ToCelsius();

        UpdateTemperatureFeeling(temperature);

        LectureState lectureState = RoomThermalManager.Room.LectureState;

        switch (lectureState)
        {
            case LectureState.Lecture:
                ExecuteLectureStateBehaviour();
                break;
            case LectureState.Pause:
                ExecutePauseStateBehaviour();
                break;
            case LectureState.None:
                ExecuteNothingStateBehaviour();
                break;
            default:
                throw new NotImplementedException();
        }

        UpdateUserStateCaption(temperature);
    }

    private void ExecuteLectureStateBehaviour()
    {
        if (State == UserState.Unknown ||
            State == UserState.Idle || 
            State == UserState.Moving)
        {
            GoToSeat();
        }
        else if (State == UserState.GoToSeat)
        {
            if (!FollowPath())
            {
                if (Role == UserRole.Lecturer)
                    State = UserState.Lecturing;
                else if (Role == UserRole.Student)
                    State = UserState.Listening;
            }
        }
        else if (State == UserState.Lecturing)
        {
            if (_currentPath == null)
                SetTarget(GetRandomVertex());

            FollowPath();
        }
    }

    private void ExecutePauseStateBehaviour()
    {
        if (State == UserState.Unknown ||
            State == UserState.Lecturing)
        {
            GoToSeat();
        }
        else if (State == UserState.GoToSeat)
        {
            if (!FollowPath())
            {
                if (Role == UserRole.Lecturer)
                    State = UserState.Idle;
                else if (Role == UserRole.Student)
                    State = UserState.Moving;
            }
        }
        else if (State == UserState.Listening)
        {
            State = UserState.Moving;
        }
        else if (State == UserState.Moving)
        {
            if (_currentPath == null)
                SetTarget(GetRandomVertex());

            FollowPath();
        }
    }

    private void ExecuteNothingStateBehaviour()
    {
        LeaveRoom();

        if (!FollowPath())
        {
            GameObject.Destroy(this);

            if (Role == UserRole.Student)
            {
                UserGroupController.UnoccupySeat(_seat);
            }

            Destroyed?.Invoke(this);
        }
    }

    public void LeaveRoom()
    {
        if (State != UserState.LeavingRoom)
        {
            UserGroupController.CancelGoToTabletRequest();
            SetTarget(GetRandomDoorVertex());
            State = UserState.LeavingRoom;
        }
    }

    public void GoToSeat()
    {
        if (State != UserState.GoToSeat)
        {
            SetTarget(_seat);
            State = UserState.GoToSeat;
        }
    }

    private void SetTarget(Vertex vertex)
    {
        if (Graph.GetPathTo(_lastVertex, vertex, out Path path))
        {
            _currentPath = path;
        }
        else
        {
            _currentPath = null;
        }
    }

    private bool FollowPath()
    {
        if (_currentPath == null)
            return false;

        float routeLength = UserSpeed * Time.deltaTime;

        Vector2 currentPosition = transform.position;

        while (routeLength > 0)
        {
            if (!_currentPath.TryGetNextVertex(out _nextVertex))
            {
                break;
            }

            float distanceToNextVertex = currentPosition.GetDistanceTo(_nextVertex.Position);

            routeLength -= distanceToNextVertex;
            currentPosition = _nextVertex.Position;
            _lastVertex = _nextVertex;
        }

        _rigidbody.MovePosition(currentPosition);

        if (_currentPath.HasNextVertex)
        {
            _currentPath = null;
            return false;
        }

        return true;
    }

    private void UpdateTemperatureFeeling(Temperature? temperature)
    {
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
    }

    private void UpdateUserStateCaption(Temperature? temperature)
    {
        string sweatingText = IsSweating ? "[Sweating]" : string.Empty;
        string freezingText = IsFreezing ? "[Freezing]" : string.Empty;

        _userStateTextMesh.text = $"{State} {sweatingText}{freezingText} ({temperature?.ToString() ?? "No Data"})";
    }

    private Vertex GetRandomDoorVertex()
    {
        IReadOnlyList<Vertex> doors = RoomThermalManager.Room.RoomGraph.Doors;

        return doors[Random.Range(0, doors.Count)];
    }

    private Vertex GetRandomVertex()
    {
        IReadOnlyList<Vertex> vertices = RoomThermalManager.Room.RoomGraph.Vertices;

        return vertices[Random.Range(0, vertices.Count)];
    }
}
