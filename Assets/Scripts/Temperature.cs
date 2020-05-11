using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temperature : MonoBehaviour
{

    private float temperature = 22.0f;

    public float _Temperature {
        set { temperature = value; UpdateColor(); }
        get { return temperature; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateColor() 
    {
        GetComponent<SpriteRenderer>().color = Mathf.CorrelatedColorTemperatureToRGB(temperature + 273.15f + 1000);
    }
}
