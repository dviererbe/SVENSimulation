using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class AirCreator : MonoBehaviour
{
    public GameObject[,] airArr;
    public GameObject airObj;
    Queue<WrappingThingy> temperatureQueue;
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


        /*
        Debug.Log(airArr[0, 0].GetComponent<Temperature>()._Temperature);
        Debug.Log(airArr[1, 1].GetComponent<Temperature>()._Temperature);
        airArr[1, 1].GetComponent<Temperature>()._Temperature += 40;
        Debug.Log(airArr[1, 1].GetComponent<Temperature>()._Temperature);
        */

        temperatureQueue = new Queue<WrappingThingy>();

        IThermalManager thermalManager = GameObject.Find("DependencyManager").GetComponent<DependencyManager>().ThermalManager;
        thermalManager.ThermalPixelSize = 1;
        thermalManager.AddSimulatedThermalArea(new Vector3(1, 1, 0), new Vector3(WallDetails.width - 1, WallDetails.height - 1));

    }

    // Update is called once per frame
    void Update()
    {
        //passedTime += Time.deltaTime;
        //if (passedTime > waitTimer){
        //    passedTime -= waitTimer;
        //    CalcTemp();
        //}
    }
    /*
    void CalcTemp() 
    {
        float mult = 0.1f;
        temperatureQueue.Enqueue(new WrappingThingy(airArr[0, 0].GetComponent<Temperature>(), 0, 0));
        while (temperatureQueue.Count > 0)
        {
            WrappingThingy mainWrappingTile = temperatureQueue.Dequeue();
            if (mainWrappingTile.i + 1 == WallDetails.height || mainWrappingTile.j + 1 == WallDetails.width)
                continue;
            
            if (mainWrappingTile.i > mainWrappingTile.j)
            {
                //Bottom thingy
                Temperature bottomTile = airArr[mainWrappingTile.i + 1, mainWrappingTile.j].GetComponent<Temperature>();
                float diff = mainWrappingTile.temp._Temperature - bottomTile._Temperature;
                mainWrappingTile.temp._Temperature += -(diff * mult)/3;
                bottomTile._Temperature += (diff * mult)/3;
                temperatureQueue.Enqueue(new WrappingThingy(bottomTile, mainWrappingTile.i + 1, mainWrappingTile.j));
            }
            else if (mainWrappingTile.i < mainWrappingTile.j)
            {
                //Right thingy
                Temperature rightTile = airArr[mainWrappingTile.i, mainWrappingTile.j + 1].GetComponent<Temperature>();
                float diff = mainWrappingTile.temp._Temperature - rightTile._Temperature;
                mainWrappingTile.temp._Temperature += -(diff * mult)/3;
                rightTile._Temperature += (diff * mult)/3;
                temperatureQueue.Enqueue(new WrappingThingy(rightTile, mainWrappingTile.i, mainWrappingTile.j + 1));
            }
            else
            {
                // We have a corner piece! :)
                Temperature bottomTile = airArr[mainWrappingTile.i + 1, mainWrappingTile.j].GetComponent<Temperature>();
                Temperature rightTile = airArr[mainWrappingTile.i, mainWrappingTile.j + 1].GetComponent<Temperature>();
                Temperature cornerTile = airArr[mainWrappingTile.i + 1, mainWrappingTile.j + 1].GetComponent<Temperature>();

                float avgTemp = bottomTile._Temperature + rightTile._Temperature + cornerTile._Temperature;
                avgTemp /= 3;
                float diff = mainWrappingTile.temp._Temperature - avgTemp;
                bottomTile._Temperature += (diff * mult) / 3;
                rightTile._Temperature += (diff * mult) / 3;
                cornerTile._Temperature += (diff * mult) / 3;
                Debug.Log(diff+ "    " + (diff*mult));
                // Ich weiß, dass die Rechnung Garbo ist :D
                mainWrappingTile.temp._Temperature += -(diff * mult);
                temperatureQueue.Enqueue(new WrappingThingy(bottomTile, mainWrappingTile.i + 1, mainWrappingTile.j));
                temperatureQueue.Enqueue(new WrappingThingy(rightTile, mainWrappingTile.i, mainWrappingTile.j + 1));
                temperatureQueue.Enqueue(new WrappingThingy(cornerTile, mainWrappingTile.i + 1, mainWrappingTile.j + 1));
            }
        }
    }
    */
}
