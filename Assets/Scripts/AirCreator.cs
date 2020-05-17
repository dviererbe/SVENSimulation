using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Assets.Scripts;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class AirCreator : MonoBehaviour
{
    public GameObject[,] airArr;
    public GameObject airObj;
    private IThermalManager thermalManager;
    private float passedTime = 0;
    private float waitTimer = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        //airObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        //Material myMaterial = new Material(Shader.Find("Diffuse"));
        //myMaterial.color = new Color(255,255,255);
        //airObj.GetComponent<Renderer>().material = myMaterial;
        airObj.transform.localScale = new Vector3(WallDetails.wallThickness, WallDetails.wallThickness, WallDetails.wallThickness);
        //airObj.AddComponent<Temperature>();

        airArr = new GameObject[(WallDetails.height - 2) * WallDetails.wallsPerGrid, (WallDetails.width - 2) * WallDetails.wallsPerGrid];
        for (int i = 1; i < WallDetails.height - 1; i++)
        {
            for (int j = 1; j < WallDetails.width - 1; j++)
            {
                for (int m = 0; m < WallDetails.wallsPerGrid; m++)
                {
                    for (int n = 0; n < WallDetails.wallsPerGrid; n++)
                    {
                        airArr[(i - 1) * WallDetails.wallsPerGrid + m, (j - 1) * WallDetails.wallsPerGrid + n] = Instantiate(airObj, new Vector3(
                             i * WallDetails.wallThickness + m * WallDetails.wallSize,
                             j * WallDetails.wallThickness + n * WallDetails.wallSize, 0),
                             airObj.transform.rotation);
                        airArr[(i - 1) * WallDetails.wallsPerGrid + m, (j - 1) * WallDetails.wallsPerGrid + n].GetComponent<Temperature>().pos = new Vector2((i - 1) * WallDetails.wallsPerGrid + m, (j - 1) * WallDetails.wallsPerGrid + n);
                    }
                }
            }
        }

        thermalManager = GameObject.Find("DependencyManager").GetComponent<DependencyManager>().ThermalManager;
        thermalManager.ThermalPixelSize = 1;
        thermalManager.AddSimulatedThermalArea(new Vector3(1, 1, 0), new Vector3(WallDetails.width - 1, WallDetails.height - 1));

    }

    // Update is called once per frame
    void Update()
    {
        float highestTemp = SetTemperaturesAndGetHigehst();
        Color color;
        for(int i = 0; i < airArr.GetLength(0); i++)
        {
            for(int j = 0; j < airArr.GetLength(0); j++)
            {
                color = new Color(airArr[i, j].GetComponent<Temperature>().temperature / highestTemp, 0, 0);
                airArr[i, j].GetComponent<SpriteRenderer>().color = color;
            }
        }
    }
 
    float SetTemperaturesAndGetHigehst()
    {
        float highestTemp = float.MinValue;
        for(int i = 0; i < airArr.GetLength(0); i++)
        {
            for(int j = 0; j < airArr.GetLength(0); j++)
            {
                float temp = thermalManager.GetTemperature(new Vector3(i, j), TemperatureUnit.Celsius);
                airArr[i, j].GetComponent<Temperature>().temperature = temp;
                if (highestTemp < temp)
                    highestTemp = temp;

            }
        }
        return highestTemp;
    }

 }
