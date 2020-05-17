using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class Temperature : MonoBehaviour
{
    private IThermalManager _thermalManager;

    public Vector2 pos;

    // Start is called before the first frame update
    void Start()
    {
        _thermalManager = GameObject.Find("DependencyManager").GetComponent<DependencyManager>().ThermalManager;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<SpriteRenderer>().color = new Color(_thermalManager.GetTemperature(pos, TemperatureUnit.Kelvin) / 1024, 0, 0);
    }
}
