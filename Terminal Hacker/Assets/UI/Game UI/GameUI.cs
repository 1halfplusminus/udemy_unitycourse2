using System.Collections;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

public class GameUI : MonoBehaviour
{
    const string infoText = @"Entrer le mot de passe, indice: {{value}}.

Vous pouvez taper le mot menu a tout moment

";

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
        OnPasswordHint();
        OnBadGuess();
        OnGoodGuess();
    }
    void OnPasswordHint()
    {
        var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(NewPasswordHintEvent));
        var hints = query.ToComponentDataArray<NewPasswordHintEvent>(Allocator.Persistent);
        for (int i = 0; i < hints.Length; i++)
        {
            var hint = hints[i];
            info.text = infoText.Replace("{{value}}", hint.Value.ToString());
        }
        hints.Dispose();
    }
    void OnBadGuess()
    {
        var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(BadGuess));
        if (query.CalculateEntityCount() > 0)
        {
            WriteLine();
            WriteInput("Mot de passe incorrect.");
            WriteLine();
            input.ActivateInputField();
        }
    }
    void OnGoodGuess()
    {
        var query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(GoodGuess));
        if (query.CalculateEntityCount() > 0)
        {
            Debug.Log("Vous avez gagné");
            WriteLine();
            WriteInput("Mot de correct.");
            StartCoroutine(GoBack());
        }
    }
    void OnDeselect(string value)
    {
        input.ActivateInputField();
    }
    void OnSubmit(string value)
    {
        var lastLine = value.Split('\n').Last();
        EmptyInput();
        WriteInput(lastLine);
        if (value == "menu")
        {
            goBack.Invoke();
        }
        GuessPassword(lastLine);
    }
    void GuessPassword(string password)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = em.CreateEntity(typeof(PasswordGuess));
        em.AddComponentData(entity, new PasswordGuess() { Value = password });
    }
    IEnumerator GoBack()
    {
        yield return new WaitForSeconds(0.5f);

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