using Assets.Scripts;
using Assets.Scripts.Abstractions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temperature : MonoBehaviour
{

    private IThermalManager thermalManager;
    public Vector2 pos;

    // Start is called before the first frame update
    void Start()
    {
        thermalManager = GameObject.Find("DependencyManager").GetComponent<DependencyManager>().ThermalManager;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<SpriteRenderer>().color = new Color((thermalManager.GetTemperature(pos)+ThermalManager.AbsoluteZero)/1024, 0, 0);
    }

}
