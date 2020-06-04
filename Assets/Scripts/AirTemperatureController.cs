using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class AirTemperatureController : MonoBehaviour
{
    private SpriteRenderer _airSprite;

    public Vector2 Position { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _airSprite = GetComponent<SpriteRenderer>();
    }

    public void SetColor(ref Color32 col)
    {
        _airSprite.color = col;
    }
}
