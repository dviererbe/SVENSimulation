using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField]
    private GameObject _image;

    // Start is called before the first frame update
    void Start()
    {
        string imagePath = Application.dataPath + "/Textures/Wall_Models/Wall_NorthEastSouthVest.png";

        //Bildladen
        byte[] image = File.ReadAllBytes(imagePath.ToString());

        //Bild setzen
        Sprite sprite = _image.GetComponent<SpriteRenderer>().sprite;

        sprite.texture.LoadImage(image);

        _image.GetComponent<SpriteRenderer>().sprite = sprite;

    }
}
