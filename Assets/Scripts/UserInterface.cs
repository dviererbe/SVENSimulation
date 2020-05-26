using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    [SerializeField]
    private GameObject _textContainer;

    [SerializeField]
    private TextMeshProUGUI _toggleVorlesung;

    private float _minTempStart;
    private float _maxTempStart;

    // Start is called before the first frame update
    void Start()
    {
        _minTempStart = 25.0f;
        _maxTempStart = 40.0f;

        if (OptionsManager.Vorlesung)
        {
            _toggleVorlesung.text = "Vorlesung aktiv";
        }
        else
        {
            _toggleVorlesung.text = "Vorlesung pause";
        }
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Laden der Temperaturen

        float temp = _maxTempStart;
        float div = (_maxTempStart - _minTempStart) / (_textContainer.transform.childCount - 1);


        foreach (Transform child in _textContainer.transform)
        {
            child.gameObject.GetComponent<TextMeshProUGUI>().text = temp.ToString("0.0");
            temp -= div;
        }
    }

    public void onVorlesung_Toggle ()
    {
        if(OptionsManager.Vorlesung)
        {
            _toggleVorlesung.text = "Vorlesung pause";
            OptionsManager.Vorlesung = false;
        }
        else
        {
            _toggleVorlesung.text = "Vorlesung aktiv";
            OptionsManager.Vorlesung = true;
        }
    }
}
