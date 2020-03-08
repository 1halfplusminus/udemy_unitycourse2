using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPrefab;
    [SerializeField] private GameObject gameUIPrefab;
    private MainMenu mainMenu;
    private GameUI gameUI;
    private Entity gameStateEntity;
    private EntityManager entityManager;
    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CreateGameState();
        ShowMainMenu();
    }
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
        entityManager.AddComponentData(gameStateEntity, new HackerGameDifficulty() { level = difficulty });
        entityManager.SetComponentData(gameStateEntity, new HackerGameState() { screen = HackerGameState.Screen.Password });
        ShowGameMenu();
    }
    void OnValidDifficultySubmit(int difficulty)
    {
        Destroy(mainMenu.gameObject);
        StartGame(difficulty);
    }
}
