using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

public class GameUI : MonoBehaviour
{
    const string infoText = @"Entrer le mot de passe, indice: {{value}}.
Vous pouvez taper le mot menu a tout moment";

    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Text log;

    [SerializeField] private TMP_Text info;
    public class OnGoBack : UnityEvent
    {

    }
    public OnGoBack goBack = new OnGoBack();

    private EntityQuery currentPasswordQuery;

    private EntityQuery stateQuery;

    // Start is called before the first frame update
    void Start()
    {
        currentPasswordQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(CurrentPassword));
        input.ActivateInputField();
        input.onDeselect.AddListener(OnDeselect);
        input.onSubmit.AddListener(OnSubmit);

    }
    void Update()
    {
        if (currentPasswordQuery.CalculateEntityCount() > 0)
        {
            var currentPassword = currentPasswordQuery.GetSingletonEntity();
            if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<NewPasswordHintEvent>(currentPassword))
            {
                var hint = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<NewPasswordHintEvent>(currentPassword);
                info.text = infoText.Replace("{{value}}", hint.Value.ToString());
                World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<NewPasswordHintEvent>(currentPassword);
            }
        }
    }
    void OnDeselect(string value)
    {
        input.ActivateInputField();
    }
    string CurrentPassword()
    {
        if (currentPasswordQuery.CalculateEntityCount() > 0)
        {
            var entity = currentPasswordQuery.GetSingleton<CurrentPassword>();
            return World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<GamePassword>(entity.password).Value.ToString();
        }
        return "";
    }
    void MarkCurrentPasswordAsCracked()
    {
        var entity = currentPasswordQuery.GetSingleton<CurrentPassword>();
        World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity.password, new CrackedPassword() { });
        World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<CurrentPassword>(currentPasswordQuery.GetSingletonEntity());
    }

    void OnSubmit(string value)
    {
        var currentPassword = CurrentPassword();
        var lastLine = value.Split('\n').Last();
        EmptyInput();
        WriteInput(lastLine);
        Debug.Log(currentPassword);
        if (value == "menu")
        {
            goBack.Invoke();
        }
        if (lastLine == currentPassword)
        {
            Debug.Log("Vous avez gagné");
            MarkCurrentPasswordAsCracked();
            WriteLine();
            WriteInput("Mot de correct.");
            StartCoroutine(GoBack());
        }
        else
        {
            WriteLine();
            WriteInput("Mot de passe incorrect.");
            WriteLine();
            input.ActivateInputField();
        }
    }
    IEnumerator GoBack()
    {
        yield return new WaitForSeconds(2);
        goBack.Invoke();
    }
    void EmptyInput()
    {
        input.text = "";
    }
    void WriteLine()
    {
        ClearIfFull();
        log.text += "\n";
    }
    void WriteInput(string value)
    {
        ClearIfFull();
        log.text += value.Replace("\n", "") + "\n";
    }
    void ClearIfFull()
    {
        if (log.textInfo.lineCount >= 18)
        {
            log.text = "";
        }
    }
}