using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UserController : MonoBehaviour
{
    public enum UserState
    {
        Unknown,
        Ok,
        Freeze,
        Sweat
    }

    [SerializeField]
    private float _userRadius = 2f;

    [SerializeField]
    private TextMesh _userStateTextMesh;

    private IRoomThermalManager _roomThermalManager = null;

    private Rigidbody2D _rigidbody;

    public Temperature MinOkTemperature { get; set; }

    public Temperature MaxOkTemperature { get; set; }

    public IRoomThermalManager RoomThermalManager
    {
        get => _roomThermalManager;
        set
        {
            if (_roomThermalManager == null)
                _roomThermalManager = value;
            else
                throw new NotSupportedException("Can't reassign thermal manager.");
        }
    }

    public UserState State { get; private set; } = UserState.Unknown;

    //TODO: move variables to the top of the class
    //      this is just for developing

    private Vector2 _bottomLeftCorner;
    private Vector2 _upperRightCorner;

    [SerializeField]
    private float _probabilityOfChangingDirectionOnUpdate = 0.005f;
    private float _playerSpeed = 2f;
    private Vector2 _direction;


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
            x: RoomThermalManager.RoomPosition.x + RoomThermalManager.RoomSize.x / 2f,
            y: RoomThermalManager.RoomPosition.y + RoomThermalManager.RoomSize.y / 2f);
    }

    private Vector2 CalculateNextPoint()
    {
        return _rigidbody.position + _playerSpeed * Time.deltaTime * _direction;
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _bottomLeftCorner = RoomThermalManager.RoomPosition + new Vector3(_userRadius, _userRadius);
        _upperRightCorner = RoomThermalManager.RoomPosition + RoomThermalManager.RoomSize - new Vector3(_userRadius, _userRadius); ;
        _direction = GetRandomDirection();

        MovePlayerToMiddleOfRoom();
    }

    void Update()
    {
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

        _userStateTextMesh.text = $"{State} ({Mathf.Round(RoomThermalManager.GetTemperature(_rigidbody.position).ToCelsius())} °C)";
    }

    public void InitializeMinAndMaxOkTemperatureWithRandomValues()
    {

    }
}
