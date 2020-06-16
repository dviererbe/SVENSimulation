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

    [SerializeField]
    private GameObject _regulators;

    [SerializeField]
    private Button _minmizeRegulators_Button;

    [SerializeField]
    private Button _maximizeRegulators_Button;

    private float _minTempStart;
    private float _maxTempStart;

    // Start is called before the first frame update
    void Start()
    {
        _minTempStart = 25.0f;
        _maxTempStart = 40.0f;

        if (OptionsManager.Lecture)
        {
            _toggleVorlesung.text = "Lecture active";
        }
        else
        {
            _toggleVorlesung.text = "Lecture paused";
        }
    }

    // Update is called once per frame
    void Update()
    {
        _maxTempStart = OptionsManager.MaxTemperatur;
        _minTempStart = OptionsManager.MinTemperatur;

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
        if(OptionsManager.Lecture)
        {
            _toggleVorlesung.text = "Lecture paused";
            OptionsManager.Lecture = false;
        }
        else
        {
            _toggleVorlesung.text = "Lecture active";
            OptionsManager.Lecture = true;
        }
    }

    public void minimzeRegulators_OnClick()
    {
        _minmizeRegulators_Button.interactable = false;
        _maximizeRegulators_Button.interactable = true;
        _regulators.SetActive(false);
    }

    public void maximzeRegulators_OnClick()
    {
        _minmizeRegulators_Button.interactable = true;
        _maximizeRegulators_Button.interactable = false;
        _regulators.SetActive(true);
    }

    public void MaxUserSpeed_Slider(Slider slider)
    {
        slider.GetComponentInChildren<TextMeshProUGUI>().text = slider.value.ToString();
    }

    public void MinUserSpeed_Slider(Slider slider)
    {
        slider.GetComponentInChildren<TextMeshProUGUI>().text = slider.value.ToString();
    }

    public void UpperMaxOKUserTemperatur(TMP_InputField input)
    {

    }
    public void UpperMinOKUserTemperatur(TMP_InputField input)
    {

    }

    public void LowerMaxOKUserTemperatur(TMP_InputField input)
    {

    }

    public void LowerMinOKUserTemperatur(TMP_InputField input)
    {

    }


    public void ProbabilityOfUserStandingUpInPause_Slider(Slider slider)
    {
        slider.GetComponentInChildren<TextMeshProUGUI>().text = slider.value.ToString() + "%";
    }
}
