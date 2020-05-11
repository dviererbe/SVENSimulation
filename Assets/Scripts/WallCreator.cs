using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCreator : MonoBehaviour
{
    public GameObject[,][,] wallArr;
    public GameObject wallObject;
   

    // Start is called before the first frame update
    void Start()
    {
        

        float tileSizeX = transform.lossyScale.x;
        float tileSizeY = transform.localScale.y;

        wallArr = new GameObject[WallDetails.height, WallDetails.width][,];
        for (int i = 0; i < WallDetails.height; i++) {
            for (int j = 0; j < WallDetails.width; j++) {
                if ((i == 0 || i == WallDetails.height - 1) || (j == 0 || j == WallDetails.width - 1))
                {
                    wallArr[i, j] = new GameObject[WallDetails.wallsPerGrid, WallDetails.wallsPerGrid];
                    for (int m = 0; m < WallDetails.wallsPerGrid; m++)
                    {
                        for (int n = 0; n < WallDetails.wallsPerGrid; n++)
                        {
                            // wallSize * wallsPerGrid is approximately equal to wallThickness. But in case wallsPerGrid = 3, sometimes small gaps occur between each wallsPerGrid*wallsPerGrid block.
                            // thus, that's the easiest option to fix that problem
                            //wallArr[i, j][m, n] = Instantiate(wallObject, new Vector3((wallSize * wallsPerGrid) * i + m * wallSize, (wallSize * wallsPerGrid) * j + n * wallSize, 0), wallObject.transform.rotation);
                            wallArr[i, j][m, n] = Instantiate(wallObject, new Vector3(WallDetails.wallThickness * i + m * WallDetails.wallSize, WallDetails.wallThickness * j + n * WallDetails.wallSize, 0), wallObject.transform.rotation);
                            
                        }
                    }
                }
            }
        }
            
    }

    // Update is called once per frame
    void Update()
    {

    }
}
