using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton network behavoir that manages the game and is managed by the shared mode master client.
/// </summary>
public class NetworkManager : NetworkBehaviour, IStateAuthorityChanged
{
    [Tooltip("Scriptable object that contains the trivia question.")]
    public MiniTaskSet miniTasks;

    [Tooltip("Container for the trivial question elements")]
    public GameObject questionElements = null;

    #region Networked Properties
    [Networked, Tooltip("Timer used for asking questions and transitioning between different states.")]
    public TickTimer timer { get; set; }

    [Networked, Tooltip("The length of the timer, used to help get a percentage when rendering timers.")]
    public float timerLength { get; set; }

    [Tooltip("The index of the current question.  -1 means no question is being currently asked.")]
    [Networked, OnChangedRender(nameof(UpdateCurrentTask))]
    public int CurrentTask { get; set; } = -1;

    [Tooltip("The number of questions asked asked so far.")]
    [Networked, OnChangedRender(nameof(UpdateTaskGeneratedText))]
    public int TaskGenerated { get; set; } = 0;

    [Tooltip("The current state of the game.")]
    [Networked, OnChangedRender(nameof(OnGameStateChanged))]
    public GameState gameState { get; set; } = GameState.Intro;


    /// <summary>
    /// A randomized array of each question index.
    /// </summary>
    [Networked, Capacity(50)]
    public NetworkArray<int> randomizedTaskList => default;

    #endregion

    #region UI ELEMENETS

    /// <summary>
    /// Question, answer, and answer highlights
    /// </summary>
    public TextMeshProUGUI question;
    public TextMeshProUGUI[] answers;
    public Image[] answerHighlights;

    /// <summary>
    /// The visual used to display the game
    /// </summary>
    public Image timerVisual;

    [Tooltip("Gradient used to color the timer based on percentage.")]
    public Gradient timerVisualGradient;

    /// <summary>
    /// Displays which questions out of 10 we are on.
    /// </summary>
    public TextMeshProUGUI questionIndicatorText;

    /// <summary>
    /// Text message shown when the game changes state.
    /// </summary>
    public TextMeshProUGUI triviaMessage;

    [Tooltip("Button displayed to leave the game after a round ends.")]
    public GameObject leaveGameBtn;

    [Tooltip("Button displayed, only to the master client, to start a new game.")]
    public GameObject startNewGameBtn;

    [Tooltip("MonoBehaviour that displays winner at the end of a game.")]
    public EndGame endGameObject;

    #endregion

    [Header("Game Rules")]
    [Tooltip("The maximum number of questions to ask in a round.")]
    [Min(1)]
    public int maxMiniTasks = 2;

    [Tooltip("The amount of time the questions will be asked for.")]
    public float questionLength = 30;

    [Tooltip("The minimum number of points earned for getting a question correct")]
    public int pointsPerMiniTasks;

    [Tooltip("The amount of points earned based on the percentage of remaining time.  So 100 would make the player earn 50 points if they answered at the 50% mark.")]
    public int timeBonus;

    #region SFX
    [Header("SFX Audio Sources")]
    [SerializeField, Tooltip("AudioSource played when the local player gets an answer correct.")]
    private AudioSource _correctSFX;

    [SerializeField, Tooltip("AudioSource played when the local player gets an answer incorrect.")]
    private AudioSource _incorrectSFX;

    [SerializeField, Tooltip("AudioSource played when the local player selects an answer.")]
    private AudioSource _confirmSFX;

    [SerializeField, Tooltip("AudioSource played when the local player selects an answer.")]
    private AudioSource _errorSFX;
    #endregion

    /// <summary>
    /// Has a trivia manager been made; set to true on spawn and false on despawn
    /// </summary>
    public static bool ManagerPresent { get; private set; } = false;

    /// <summary>
    /// The different states of the trivia game.  Made as a byte since there are not that many.
    /// </summary>
    public enum GameState : byte
    {
        Intro = 0,
        ShowQuestion = 1,
        ShowAnswer = 2,
        GameOver = 3,
        NewRound = 4,
    }

    public override void Spawned()
    {
        if (TaskGenerated == 0)
            questionIndicatorText.text = "";
        else
            questionIndicatorText.text = "Question: " + TaskGenerated + " / " + maxMiniTasks;

        // Disallows players from joining once the game is started.
        if (Runner.IsSharedModeMasterClient)
        {
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;
        }

        // If we have state authority, we set an intro time and randomized the question list.
        if (HasStateAuthority)
        {
            // Sets an initial intro timer
            // The initial timer for the game is only 3 seconds.
            timerLength = 3f;
            timer = TickTimer.CreateFromSeconds(Runner, timerLength);

            ShuffleTask();
        }

        ManagerPresent = true;

        FusionConnector.Instance?.SetPregameMessage(string.Empty);

        OnGameStateChanged();
        UpdateCurrentTask();
        UpdateTaskGeneratedText();
    }

    /// <summary>
    /// Shuffles the questions around.
    /// </summary>
    private void ShuffleTask()
    {
        Debug.Log("SHUFFLINF QUESTIONG!");

        // Creates a temp list, adding every index avaiable
        List<int> taskAvailable = new List<int>();
        for (int i = 0; i < miniTasks.MiniTasks.Count; i++)
            taskAvailable.Add(i);

        // Sets the fifty questions
        for (int i = 0; i < randomizedTaskList.Length; i++)
        {
            int c = taskAvailable[Random.Range(0, taskAvailable.Count)];
            taskAvailable.Remove(c);
            randomizedTaskList.Set(i, c);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        ManagerPresent = false;
    }

    /// <summary>
    /// Note, was a bit confused because only the shared mode master client seems to run this.
    /// Game worked fine, but unsure if every client should be running this in their simulation or not.
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        // When the timer expires...
        if (timer.Expired(Runner))
        {
            // If we are showing a question, we then show an answer...
            if (gameState == GameState.ShowQuestion)
            {
                timerLength = 3f;
                timer = TickTimer.CreateFromSeconds(Runner, timerLength);
                gameState = GameState.ShowAnswer;
                return;
            }
            else if (TaskGenerated < maxMiniTasks)
            {
                HostPlayerScript.LocalPlayer.ChosenTask = -1;

                // This means we are at the end of the question list and want to reshuffle the answers
                if (CurrentTask + 1 >= randomizedTaskList.Length)
                {
                    ShuffleTask();
                    CurrentTask = 0;
                }
                else
                {
                    CurrentTask++;
                }

                TaskGenerated++;

                timerLength = questionLength;
                timer = TickTimer.CreateFromSeconds(Runner, timerLength);
                gameState = GameState.ShowQuestion;
            }
            else
            {
                timer = TickTimer.None;
                gameState = GameState.GameOver;
            }

            return;
        }

        // We check to see if every player has chosen answer, and if so, go to the show answer state.
        if (gameState == GameState.ShowQuestion)
        {
            int totalAnswers = 0;
            for (int i = 0; i < HostPlayerScript.PlayerRefs.Count; i++)
            {
                if (HostPlayerScript.PlayerRefs[i].ChosenTask >= 0)
                {
                    totalAnswers++;
                }
            }
            if (totalAnswers == HostPlayerScript.PlayerRefs.Count)
            {
                timerLength = 3f;
                timer = TickTimer.CreateFromSeconds(Runner, timerLength);
                gameState = GameState.ShowAnswer;
            }
        }
    }

    /// <summary>
    /// Called when a player picks an answer.
    /// </summary>
    /// <param name="index"></param>
    public void PickAnswer(int index)
    {
        // If we are in the question state and the local player has not picked an answer...
        if (gameState == GameState.ShowQuestion)
        {
            // For now, if Chosen Answer is less than 0, this means they haven't picked an answer.
            // We don't allow players to pick new answers at this time.
            if (HostPlayerScript.LocalPlayer.ChosenTask < 0)
            {
                _confirmSFX.Play();

                HostPlayerScript.LocalPlayer.ChosenTask = index;

                // Colors the highlighted question cyan.
                answerHighlights[index].color = Color.cyan;

                float? remainingTime = timer.RemainingTime(Runner);
                if (remainingTime.HasValue)
                {
                    float percentage = remainingTime.Value / this.timerLength;
                    HostPlayerScript.LocalPlayer.TimerBonusScore = Mathf.RoundToInt(timeBonus * percentage);
                }
                else
                {
                    HostPlayerScript.LocalPlayer.TimerBonusScore = 0;
                }
            }
            else
            {
                _errorSFX.Play();
            }
        }
    }

    /// <summary>
    /// Update function that updates the timer visual.
    /// </summary>
    public void Update()
    {
        // Updates the timer visual
        float? remainingTime = timer.RemainingTime(Runner);
        if (remainingTime.HasValue)
        {
            float percent = remainingTime.Value / timerLength;
            timerVisual.fillAmount = percent;
            timerVisual.color = timerVisualGradient.Evaluate(percent);
        }
        else
        {
            timerVisual.fillAmount = 0f;
        }
    }

    private void OnGameStateChanged()
    {
        // If showin an answer, we show which players got the question correct and increase their score.
        if (gameState == GameState.Intro || gameState == GameState.NewRound)
        {
            triviaMessage.text = gameState == GameState.Intro ? "Select The Correct Answer\nStarting Game Soon" : "New Game Starting Soon!";

            endGameObject.Hide();
        }
        else if (gameState == GameState.ShowAnswer)
        {
            //OnGameStateShowAnswer();

            endGameObject.Hide();
        }
        else if (gameState == GameState.GameOver)
        {
            OnGameStateGameOver();
        }
        else if (gameState == GameState.ShowQuestion)
        {
            // Otherwise, we clear the color of the answers
            for (int i = 0; i < answers.Length; i++)
            {
                answerHighlights[i].color = Color.clear;
            }

            triviaMessage.text = string.Empty;

            endGameObject.Hide();
        }

        leaveGameBtn.SetActive(gameState == GameState.GameOver);
        startNewGameBtn.SetActive(gameState == GameState.GameOver && Runner.IsSharedModeMasterClient == true);
    }

    private void OnGameStateGameOver()
    {
        // Hides the question elements and then shows the game elements / final score / winner elements
        questionElements.SetActive(false);

        // Removes the correct answer highlight
        for (int i = 0; i < answers.Length; i++)
        {
            answerHighlights[i].color = Color.clear;
        }

        // Sorts all players in a list and keeps the three highest players.
        /*
        List<HostPlayerScript> winners = new List<HostPlayerScript>(HostPlayerScript.PlayerRefs);
        winners.RemoveAll(x => x.Score == 0);
        winners.Sort((x, y) => y.Score - x.Score);
        if (winners.Count > 3)
            winners.RemoveRange(3, winners.Count - 3);

        endGameObject.Show(winners);

        if (winners.Count == 0)
        {
            triviaMessage.text = "No winners";
        }
        else
        {
            triviaMessage.text = winners[0].PlayerName.Value + " Wins!";
        }
        */
    }


    private void UpdateCurrentTask()
    {
        // If we are asking a question, we set the answers.
        if (CurrentTask >= 0)
        {
            questionElements.SetActive(true);

            int taskIndex = randomizedTaskList[CurrentTask];

            question.text = miniTasks.MiniTasks[taskIndex].miniTask;

            answers[0].text = miniTasks.MiniTasks[taskIndex].correctAnswer;

            answers[1].text = miniTasks.MiniTasks[taskIndex].decoyAnswers[0];
            answers[2].text = miniTasks.MiniTasks[taskIndex].decoyAnswers[1];
            answers[3].text = miniTasks.MiniTasks[taskIndex].decoyAnswers[2];

            // Scrambles the answers.  This can be different per player.
            List<int> answerIndices = new List<int>() { 0, 1, 2, 3 };
            while (answerIndices.Count > 0)
            {
                int r = Random.Range(0, answerIndices.Count);
                answers[answerIndices[r]].transform.parent.SetSiblingIndex(0);
                answerIndices.RemoveAt(r);
            }

            // Clears the trivia message
            triviaMessage.text = string.Empty;

            // Deisgnate that the local player has not chosen an answer yet.
            HostPlayerScript.LocalPlayer.ChosenTask = -1;

            // Change the game state
            if (HasStateAuthority)
            {
                gameState = GameState.ShowQuestion;
            }
        }

        // We hide the question element in case a player late joins at the end of the game.
        if (gameState != GameState.ShowAnswer && gameState != GameState.ShowQuestion)
        {
            questionElements.SetActive(false);
        }
    }

    private void UpdateTaskGeneratedText()
    {
        if (TaskGenerated == 0)
            questionIndicatorText.text = "";
        else
            questionIndicatorText.text = "Question: " + TaskGenerated + " / " + maxMiniTasks;
    }

    public async void LeaveGame()
    {
        await Runner.Shutdown(true, ShutdownReason.Ok);

        FusionConnector fc = GameObject.FindObjectOfType<FusionConnector>();
        if (fc)
        {
            fc.mainMenuObject.SetActive(true);
            fc.mainGameObject.SetActive(false);
        }
    }

    public void StartNewGame()
    {
        if (HasStateAuthority == false)
            return;

        gameState = GameState.NewRound;

        TaskGenerated = 0;

        // Sets an initial intro timer
        timerLength = 3f;
        timer = TickTimer.CreateFromSeconds(Runner, timerLength);
    }

    public void StateAuthorityChanged()
    {
        if (gameState == GameState.GameOver)
        {
            startNewGameBtn.SetActive(Runner.IsSharedModeMasterClient);
        }
    }
}
