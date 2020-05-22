using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_Menu_Controller : MonoBehaviour
{

    [SerializeField]
    private GameObject _Simulation_Menu;

    [SerializeField]
    private GameObject _Main_Menu;

    [SerializeField]
    private GameObject _Option_Menu;

    public void OnStart_Click()
    {
        _Main_Menu.SetActive(false);
        _Simulation_Menu.SetActive(true);
    }

    public void OnQuit_Click()
    {
        Application.Quit();
    }

    public void OnOption_Click()
    {
        _Main_Menu.SetActive(false);
        _Option_Menu.SetActive(true);
    }
}
