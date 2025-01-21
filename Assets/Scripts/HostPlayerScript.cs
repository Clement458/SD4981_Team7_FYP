using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostPlayerScript : NetworkBehaviour
{
    #region Network Properties
    [Networked] private TickTimer delay { get; set; }
    [Tooltip("The name of the player")]
    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))]
    public NetworkString<_16> PlayerName { get; set; }

    [Tooltip("What is the player's score.")]
    [Networked, OnChangedRender(nameof(OnScoreChanged))]
    public int Score { get; set; }

    [Tooltip("What is the player's score.")]
    [Networked, OnChangedRender(nameof(OnScorePopupChanged))]
    public ScorePopUp scorePopUps { get; set; }

    [Tooltip("The amount of points earned by answering the question quickly.")]
    [Networked]
    public int TimerBonusScore { get; set; }

    [Tooltip("If true, this player will be registered as the master client and displayed visually.")]
    [Networked, OnChangedRender(nameof(OnMasterClientChanged))]
    public NetworkBool IsMasterClient { get; set; }

    [Tooltip("Which answer did the player choose.  0 is always the correct answer, but the answers are randomized locally.")]
    [Networked, OnChangedRender(nameof(OnTaskChosen))]
    public int ChosenTask { get; set; } = -1;

    #endregion

    [SerializeField] private GameObject popUps;
    static public string code;
    public string clientAns;
    public GameObject PopUps;
    protected PopUpBtn PopUpBtn;
    protected int sessionResources = 0;
    private NetworkCharacterController _cc;
    bool taskSpawned;

    [Tooltip("Reference to the name display object.")]
    public TextMeshProUGUI nameText;

    [Tooltip("Reference to the score display object.")]
    public TextMeshProUGUI scoreText;

    /// <summary>
    /// Unsure if this pattern is okay, but static references to the local player and a list of all players.
    /// </summary>
    public static HostPlayerScript LocalPlayer;

    /// <summary>
    /// A list of all players currently in the game.
    /// </summary>
    public static List<HostPlayerScript> PlayerRefs = new List<HostPlayerScript>();

    // private void Awake()
    // {
    //     _cc = GetComponent<NetworkCharacterController>();
    // }
    /// <summary>
    /// When a character is spawned, we have to do the checks that a user would do in case someone spawns late.
    /// </summary>
    public override void Spawned()
    {
        base.Spawned();
        _cc = GetComponent<NetworkCharacterController>();

        // Adds this player to a list of player refs and then sorts the order by index
        PlayerRefs.Add(this);
        PlayerRefs.Sort((x, y) => x.Object.StateAuthority.AsIndex - y.Object.StateAuthority.AsIndex);

        // The OnRenderChanged functions are called during spawn to make sure they are set properly for players who have already joined the room.

        OnTaskChosen();
        OnScoreChanged();
        OnPlayerNameChanged();

        transform.SetParent(FusionConnector.Instance.playerContainer, false);

        // Sets the master client value on spawn
        if (HasStateAuthority)
        {
            IsMasterClient = Runner.IsSharedModeMasterClient;
        }

        // We show the "Start Game Button" for the master client only, regardless of the number of players in the room.
        bool showGameButton = Runner.IsSharedModeMasterClient && NetworkManager.ManagerPresent == false;
        FusionConnector.Instance.showGameButton.SetActive(showGameButton);
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Removes the player from the list
        PlayerRefs.Remove(this);

        // Sets the local test play to null
        if (this == LocalPlayer)
            LocalPlayer = null;

        if (HasStateAuthority)
            IsMasterClient = runner.IsSharedModeMasterClient;

        bool showGameButton = Runner.IsSharedModeMasterClient && NetworkManager.ManagerPresent == false;
        FusionConnector.Instance.showGameButton.SetActive(showGameButton);
    }
    void OnTaskChosen()
    {
        if (HasStateAuthority)
        {
            if (ChosenTask >= 0)
            {
                Debug.Log("Task choose");
            }
            else
            {
                // Debug.Log("Task not chosen");
            }
        }
    }

    void OnPlayerNameChanged()
    {
        nameText.text = PlayerName.Value;
    }
    void OnScoreChanged()
    {
        scoreText.text = Score.ToString();
    }
    void OnScorePopupChanged()
    {
        if (scorePopUps.Score > 0)
        {
            /* _scorePopUpText.text = string.Format("+{0}", ScorePopUp.Score);
             _scorePopUpAnimator.SetTrigger("CorrectAnswer"); */
        }
        else
        {
            /* _scorePopUpText.text = "X";
            _scorePopUpAnimator.SetTrigger("WrongAnswer"); */
        }
    }

    void OnMasterClientChanged()
    {
        // masterClientIcon.enabled = IsMasterClient;
    }
    private void FixedUpdate()
    {
        if (Object.HasInputAuthority && code == null)
        {
            SpawnTask();
        }
        if (Object.HasInputAuthority && PopUpBtn.isPressed) //Input.GetKeyDown(KeyCode.R)
        {
            RPC_SendMessage(code);
        }
        if (Object.HasInputAuthority && !taskSpawned)
        {
            Debug.Log("refreshing");
            sessionResources += 1;
            // StatsManager.instance.resources = sessionResources;
            RefreshTask();
            clientAns = null;
        }
    }
    public void SpawnTask()
    {
        PopUps = Instantiate(popUps, new Vector3(Random.Range(-10, 26), Random.Range(-13, -9), 40), Quaternion.identity);
        PopUpBtn = PopUps.GetComponentInChildren<PopUpBtn>();
        code = Random.Range(1, 11).ToString();
        taskSpawned = true;
    }
    public void RefreshTask()
    {
        if (Object.HasInputAuthority)
        {
            PopUpBtn.transform.position = new Vector3(Random.Range(-10, 26), Random.Range(-13, -9), 40);
            code = Random.Range(1, 11).ToString();
            _messages.text = null;
            Debug.Log("button refreshed");
            taskSpawned = true;
        }
    }

    [SerializeField] private TMP_Text _messages;
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        // Debug.Log("message: " + message);

        if (_messages == null)
            _messages = FindObjectOfType<TMP_Text>();

        if (messageSource == Runner.LocalPlayer)
        {
            _messages.text = message;
            Debug.Log($"Host send: {message}\n");
        }
        else
        {
            Debug.Log($"Some client said: {message}\n");
            if (message == code)
            {
                clientAns = message;
                taskSpawned = false;
            }
            else
            {
                Debug.Log("fail matching");
            }
        }

        // Debug.Log(message);
    }
}
