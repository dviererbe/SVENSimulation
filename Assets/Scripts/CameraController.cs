using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MovementSpeed = 0.01f;
    public float CameraZoomSpeed = 4f;
    public bool inMenu = false;

    // Start is called before the first frame update
    void Start()
    {
        Options option = new Options();
        MovementSpeed = option.getMovement();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inMenu)
        {
            Vector2 translation = new Vector2(
                x: Input.GetAxis("Horizontal"),
                y: Input.GetAxis("Vertical"));

            float mouseInput = Input.GetAxis("Mouse ScrollWheel");

            Camera.main.fieldOfView -= mouseInput * CameraZoomSpeed;

            transform.Translate(translation * MovementSpeed * Time.deltaTime);
        }
    }
}
