using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FusionConnector : MonoBehaviour
{
    public string LocalPlayerName { get; set; }

    public string LocalRoomName { get; set; }

    [SerializeField, Tooltip("The network runner prefab that will be instantiated when looking starting the game.")]
    private NetworkRunner _networkRunnerPrefab;

    [Tooltip("The canvas group that handles interactivity for the game.")]
    public CanvasGroup canvasGroup;

    [Tooltip("The GameObject that contains the main menu.")]
    public GameObject mainMenuObject;

    [Tooltip("The Game Object that handles the game itself")]
    public GameObject mainGameObject;

    [Tooltip("GameObject that appears if there is a network error when trying to join a room.")]
    public GameObject errorMessageObject;

    [Tooltip("The GameObject that displays the button to start the game.")]
    public GameObject showGameButton;

    [Tooltip("Text object that displays the room name.")]
    public TextMeshProUGUI roomName;

    [Tooltip("Prefab for the trivia game itself.")]
    public NetworkObject triviaGamePrefab;

    public Transform playerContainer;
    public Transform canvasContainer;

    [Tooltip("The message shown before starting the game.")]
    public TextMeshProUGUI preGameMessage;

    // UI presented in game
    public GameObject InGamePanel;

    public static FusionConnector Instance { get; private set; }

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        GoToMainMenu();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public async void StartGame(bool joinRandomRoom)
    {
        canvasGroup.interactable = false;

        StartGameArgs startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = joinRandomRoom ? string.Empty : LocalRoomName,
            PlayerCount = 20,
        };

        NetworkRunner newRunner = Instantiate(_networkRunnerPrefab);

        StartGameResult result = await newRunner.StartGame(startGameArgs);

        if (result.Ok)
        {
            roomName.text = "Room:  " + newRunner.SessionInfo.Name;

            GoToGame();
        }
        else
        {
            roomName.text = string.Empty;

            GoToMainMenu();

            errorMessageObject.SetActive(true);
            TextMeshProUGUI gui = errorMessageObject.GetComponentInChildren<TextMeshProUGUI>();
            if (gui)
                gui.text = result.ErrorMessage;

            Debug.LogError(result.ErrorMessage);
        }

        canvasGroup.interactable = true;
    }

    public void GoToMainMenu()
    {
        mainMenuObject.SetActive(true);
        mainGameObject.SetActive(false);
        InGamePanel.SetActive(false);
    }

    public void GoToGame()
    {
        mainMenuObject.SetActive(false);
        mainGameObject.SetActive(true);
        InGamePanel.SetActive(false);
        Debug.Log("Switched to game UI.");
    }

    internal void OnPlayerJoin(NetworkRunner runner)
    {
        // Only set pregame messages if the game hasn't started.
        if (NetworkedTaskManager.ManagerPresent)
        {
            return;
        }

        if (runner.IsSharedModeMasterClient == true)
        {
            SetPregameMessage("Game Is Ready To Start");
        }
        else
        {
            SetPregameMessage("Waiting for master client to start game.");
        }
    }
    public void SetPregameMessage(string message)
    {
        preGameMessage.text = message;
    }

    public void StartTriviaGame()
    {
        NetworkRunner runner = null;
        // If no runner has been assigned, we cannot start the game
        if (NetworkRunner.Instances.Count > 0)
        {
            runner = NetworkRunner.Instances[0];
        }

        if (runner == null)
        {
            Debug.Log("No runner found.");
            return;
        }

        // If no trivia manager has been made and we are the master mode client.
        // Redundant but being safe.
        if (runner.IsSharedModeMasterClient && !NetworkedTaskManager.ManagerPresent)
        {
            Debug.Log("Spawning trivia game prefab...");
            runner.Spawn(triviaGamePrefab, new Vector3(-1000, -1000, -1000));
            Debug.Log("Trivia game prefab spawned.");

            showGameButton.SetActive(false);
            Debug.Log("Game button hidden.");

            // Transition to main game panel
            HandleGameStart(runner);
        }
    }


    private void HandleGameStart(NetworkRunner runner)
    {
        // Transition to main game object first
        mainMenuObject.SetActive(false);
        mainGameObject.SetActive(true);
        InGamePanel.SetActive(false);
        Debug.Log("Transitioned to main game panel.");

        // Additional game start logic
        // Enable any necessary components or game objects
        // Start timers, initialize variables, etc.
        Debug.Log("Game state changed to active.");

        // Update the pre-game message
        SetPregameMessage("Game is starting...");

        // Schedule transition to in-game panel
        StartCoroutine(SwitchToInGamePanel());
    }

    private IEnumerator SwitchToInGamePanel()
    {
        yield return new WaitForSeconds(1);  // Adjust timing as needed
        mainGameObject.SetActive(false);
        InGamePanel.SetActive(true);
        Debug.Log("Transitioned to in-game panel.");
    }

}
