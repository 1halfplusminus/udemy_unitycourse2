using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    public class OnGoBack : UnityEvent
    {

    }
    public OnGoBack goBack = new OnGoBack();

    private EntityQuery currentPasswordQuery;
    // Start is called before the first frame update
    void Start()
    {
        currentPasswordQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(CurrentPassword));
        input.ActivateInputField();
        input.onDeselect.AddListener(OnDeselect);
        input.onSubmit.AddListener(OnSubmit);
    }

    void OnDeselect(string value)
    {
        input.ActivateInputField();
    }
    string CurrentPassword()
    {
        var entity = currentPasswordQuery.GetSingleton<CurrentPassword>();
        return World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<GamePassword>(entity.password).value.ToString();
    }
    void MarkCurrentPasswordAsCracked()
    {
        var entity = currentPasswordQuery.GetSingleton<CurrentPassword>();
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity.password, new CrackedPassword() { });
        World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<CurrentPassword>(currentPasswordQuery.GetSingletonEntity());
    }
    void OnSubmit(string value)
    {
        var lastLine = value.Split('\n').Last();
        var currentPassword = CurrentPassword();
        Debug.Log(currentPassword);
        Debug.Log(lastLine);
        if (value == "menu")
        {
            goBack.Invoke();
        }
        if (lastLine == currentPassword)
        {
            Debug.Log("Vous avez gagné");
            MarkCurrentPasswordAsCracked();
            goBack.Invoke();
        }
    }
}
