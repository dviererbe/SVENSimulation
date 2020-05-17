using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float MovementSpeed = 0.01f;
    public float CameraZoomSpeed = 4f;

    private Rigidbody2D _camera;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 translation = new Vector2(
            x: Input.GetAxis("Horizontal"),
            y: Input.GetAxis("Vertical"));

        float mouseInput = Input.GetAxis("Mouse ScrollWheel");

        Camera.main.fieldOfView -= mouseInput * CameraZoomSpeed;

        _camera.transform.Translate(translation.normalized * MovementSpeed);
        
    }
}
