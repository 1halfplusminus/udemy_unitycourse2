using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public class MainMenu : MonoBehaviour
{
    public class OnValidSubmit : UnityEvent<int>
    {

    }
    [SerializeField] private TMP_InputField input;
    private List<int> validNumber = new List<int>() { 1, 2, 3 };

    public OnValidSubmit onValidSubmit = new OnValidSubmit();

    bool valid;
    // Start is called before the first frame update
    void Start()
    {
        input.ActivateInputField();
        input.onDeselect.AddListener(OnDeselect);
        input.onSubmit.AddListener(OnSubmit);
    }

    void OnDeselect(string value)
    {
        if (!valid)
        {
            input.ActivateInputField();
        }

    }
    void OnSubmit(string value)
    {
        int parsed;
        if (Int32.TryParse(value, out parsed) && validNumber.Contains(parsed))
        {
            valid = true;
            input.text = "";
            onValidSubmit.Invoke(parsed);
        }
        else
        {
            input.ActivateInputField();
        }
    }
}