using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Linq;




public class Bootstrap : MonoBehaviour
{
    /*   [System.Serializable]
      public class Words
      {
          public List<string> list;
      } */
    [SerializeField] private GameObject mainMenuPrefab;
    [SerializeField] private GameObject gameUIPrefab;
    /* [SerializeField] private List<Words> dictionary; */

    private MainMenu mainMenu;
    private GameUI gameUI;
    private Entity gameStateEntity;
    private EntityManager entityManager;
    private EntityQuery stateQuery;
    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        stateQuery = entityManager.CreateEntityQuery(typeof(HackerGameState));
        CreateGameState();
        ShowMainMenu();
        /*  CreateDictionaryEntity(); */
        /*    LoadWords(); */
    }
    /* void CreateDictionaryEntity()
    {

        for (int i = 0; i < dictionary.Count; i++)
        {
            var entity = entityManager.CreateEntity(typeof(GameDictionnary), typeof(Level));
            entityManager.AddComponentData(entity, new GameDictionnary());
            entityManager.AddComponentData(entity, new Level() { Value = i + 1 });
            entityManager.AddBuffer<DirectoryPasswordElement>(entity);
            entityManager.SetName(entity, "Dictionnary " + i + 1);
        }
    } */
    /*  void LoadWords()
     {
         var nativeDictionnaries = entityManager.CreateEntityQuery(typeof(GameDictionnary), typeof(Level))
         .ToEntityArray(Allocator.TempJob);
         var dictionaries = nativeDictionnaries
         .ToDictionary((l) => entityManager.GetComponentData<Level>(l).Value);
         var flattenList = dictionary.SelectMany((x, level) => x.list.Select((word) => new GamePassword() { Value = word, level = level + 1 })).ToList();
         var archetype = entityManager.CreateArchetype(typeof(GamePassword));
         var wordsArrays = new NativeArray<Entity>(flattenList.Count, Allocator.Temp);
         entityManager.CreateEntity(archetype, wordsArrays);
         for (int i = 0; i < wordsArrays.Length; i++)
         {
             entityManager.AddComponentData(wordsArrays[i], flattenList[i]);
             Entity dictionary;
             if (dictionaries.TryGetValue(flattenList[i].level, out dictionary))
             {
                 var buffer = entityManager.GetBuffer<DirectoryPasswordElement>(dictionary).Add(new DirectoryPasswordElement() { Value = wordsArrays[i] });
             }
         }
         wordsArrays.Dispose();
         nativeDictionnaries.Dispose();
     } */
    private void CreateGameState()
    {
        var archetype = entityManager.CreateArchetype(typeof(HackerGameDifficulty), typeof(HackerGameState));
        gameStateEntity = entityManager.CreateEntity(archetype);
        entityManager.SetName(gameStateEntity, "Game State");
        SetGameStateDefault();
    }
    private void SetGameStateDefault()
    {
        entityManager.SetComponentData(gameStateEntity, new HackerGameState() { screen = HackerGameState.Screen.MainMenu });
        entityManager.RemoveComponent<HackerGameDifficulty>(gameStateEntity);
    }
    private void ShowMainMenu()
    {
        var mainMenuGameObject = Instantiate(mainMenuPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        if (mainMenuGameObject.TryGetComponent(out mainMenu))
        {
            mainMenu.onValidSubmit.AddListener(OnValidDifficultySubmit);
        }
    }
    private void ShowGameMenu()
    {
        var gameUIGameObject = Instantiate(gameUIPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        if (gameUIGameObject.TryGetComponent(out gameUI))
        {
            gameUI.goBack.AddListener(ReturnToMainMenu);
        }
    }
    private void ReturnToMainMenu()
    {
        Destroy(gameUI.gameObject);
        SetGameStateDefault();
        ShowMainMenu();
    }
    private void StartGame(int difficulty)
    {
        var entity = stateQuery.GetSingletonEntity();
        entityManager.AddComponentData(entity, new HackerGameDifficulty() { level = difficulty });
        entityManager.AddComponentData(entity, new SelectRandomPassword() { level = difficulty });
        entityManager.SetComponentData(entity, new HackerGameState() { screen = HackerGameState.Screen.Password });
        ShowGameMenu();
    }
    void OnValidDifficultySubmit(int difficulty)
    {
        Destroy(mainMenu.gameObject);
        StartGame(difficulty);
    }
}
