using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    public class OnGoBack : UnityEvent
    {

    }
    public OnGoBack goBack = new OnGoBack();
    // Start is called before the first frame update
    void Start()
    {
        input.ActivateInputField();
        input.onDeselect.AddListener(OnDeselect);
        input.onSubmit.AddListener(OnSubmit);
    }

    void OnDeselect(string value)
    {
        input.ActivateInputField();
    }
    void OnSubmit(string value)
    {
        Debug.Log(value);
        if (value == "menu")
        {
            goBack.Invoke();
        }
    }
}
