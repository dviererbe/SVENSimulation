using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option_Menu_Controller : MonoBehaviour
{
    [SerializeField]
    private GameObject _Main_Menu;

    [SerializeField]
    private GameObject _Option_Menu;

    [SerializeField]
    private Slider _Slider;

    private Options option;

    public void Start()
    {
        option = new Options();
    }

    public void OnSliderMove()
    {
        option.setMovement(_Slider.value);
    }

    public void OnBack_Click ()
    {
        _Option_Menu.SetActive(false);
        _Main_Menu.SetActive(true);
    }
}