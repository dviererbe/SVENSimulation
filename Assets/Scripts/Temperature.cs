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
    public float temperature = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _thermalManager = GameObject.Find("DependencyManager").GetComponent<DependencyManager>().ThermalManager;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
