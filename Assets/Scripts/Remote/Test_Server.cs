using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

namespace Assets.Scripts.Remote
{
    public class Test_Server : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            ServerConnection connection = GameObject.Find("DependencyManager").GetComponent<DependencyManager>().ServerConnection;
            Debug.Log(connection.setData("dummy_Unity", "23"));
            Debug.Log(connection.getData("anyViews", "icon%20on"));
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}