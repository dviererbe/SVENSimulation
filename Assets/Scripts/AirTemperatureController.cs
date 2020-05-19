using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class AirTemperatureController : MonoBehaviour
{
    private float _temperature = 0f;
    private SpriteRenderer _spriteRenderer;

    public Vector2 Position { get; set; }

    public float Temperature
    {
        get => _temperature;
        set => _temperature = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetColor(float highestTemperature, float lowestTemperature)
    {
        float minMaxDelta = highestTemperature - lowestTemperature;

        //avoid division by zero error
        if (minMaxDelta == 0f)
        {
            _spriteRenderer.color = Color.black;
        }
        else
        {
            //Diff = HighestTemp - LowestTemp
            //Formula creates a transformation from [0, Diff] -> [0, 1.0f]. Thus, we have rough transitions between each air-pixel.
            _spriteRenderer.color = new Color((_temperature - lowestTemperature) / minMaxDelta, 0, 0);
        }
    }
}
