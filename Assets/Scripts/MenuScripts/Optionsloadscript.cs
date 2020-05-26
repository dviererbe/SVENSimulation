using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Optionsloadscript : MonoBehaviour
{
    [SerializeField]
    private Slider _movementSpeed;

    [SerializeField]
    private Slider _zoomSpeed;

    // Start is called before the first frame update
    void Start()
    {
        _zoomSpeed.value = OptionsManager.CameraZoomSpeed;
        _movementSpeed.value = OptionsManager.MovementSpeed;
    }
}
