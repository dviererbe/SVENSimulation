using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector2 translation = new Vector2(
            x: Input.GetAxis("Horizontal"),
            y: Input.GetAxis("Vertical"));

        float mouseInput = Input.GetAxis("Mouse ScrollWheel");

        Camera.main.fieldOfView -= mouseInput * OptionsManager.CameraZoomSpeed;

        transform.Translate(translation * OptionsManager.MovementSpeed * Time.deltaTime);
    }
}
