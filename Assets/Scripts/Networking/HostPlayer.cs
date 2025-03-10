using System.Collections;
using System.Collections.Generic;
using Fusion;
using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;

public class HostPlayer : NetworkBehaviour
{
    #region Network Properties
    [Networked] private TickTimer delay { get; set; }
    [Tooltip("The name of the player")]
    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))]
    public NetworkString<_16> PlayerName { get; set; }

    [Tooltip("The amount of points earned by answering the question quickly.")]
    [Networked]
    public int TimerBonusScore { get; set; }

    [Tooltip("If true, this player will be registered as the master client and displayed visually.")]
    [Networked, OnChangedRender(nameof(OnMasterClientChanged))]
    public NetworkBool IsMasterClient { get; set; }

    [Tooltip("Which answer did the player choose. 0 is always the correct answer, but the answers are randomized locally.")]
    [Networked, OnChangedRender(nameof(OnTaskChosen))]
    public int ChosenTask { get; set; } = -1;

    [SerializeField] private GameObject popUps;
    private GameObject PopUps;

    private bool inputAllowed = true;
    private bool isSpawningTask = false;
    private Transform canvasContainer;

    #endregion
    [Tooltip("Reference to the name display object.")]
    public TextMeshProUGUI nameText;
    private NetworkCharacterController _cc;

    public static HostPlayer LocalPlayer;

    public static List<HostPlayer> PlayerRefs = new List<HostPlayer>();

    private PopUpController popUpController;

    public override void Spawned()
    {
        base.Spawned();
        _cc = GetComponent<NetworkCharacterController>();

        PlayerRefs.Add(this);
        PlayerRefs.Sort((x, y) => x.Object.StateAuthority.AsIndex - y.Object.StateAuthority.AsIndex);

        OnTaskChosen();

        transform.SetParent(FusionConnector.Instance.playerContainer, false);

        if (HasStateAuthority)
        {
            IsMasterClient = Runner.IsSharedModeMasterClient;
        }

        bool showGameButton = Runner.IsSharedModeMasterClient && NetworkedTaskManager.ManagerPresent == false;
        FusionConnector.Instance.showGameButton.SetActive(showGameButton);
        /*
        popUpController = FindObjectOfType<PopUpController>();
        if (popUpController == null)
        {
            Debug.LogError("PopUpController is not found in the scene.");
        }
        */
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        PlayerRefs.Remove(this);

        if (this == LocalPlayer)
            LocalPlayer = null;

        if (HasStateAuthority)
            IsMasterClient = runner.IsSharedModeMasterClient;

        bool showGameButton = Runner.IsSharedModeMasterClient && NetworkedTaskManager.ManagerPresent == false;
        FusionConnector.Instance.showGameButton.SetActive(showGameButton);
    }
    void OnPlayerNameChanged()
    {
        nameText.text = PlayerName.Value;
    }

    private void Start()
    {
        canvasContainer = FusionConnector.Instance.canvasContainer;
    }


    private void Update()
    {
        if (inputAllowed && !isSpawningTask && Input.GetKeyUp(KeyCode.P))
        {
            inputAllowed = false;
            Debug.Log("P pressed");
            isSpawningTask = true; // Set flag to indicate task is being spawned
            SpawnTask();
            StartCoroutine(InputDelayCoroutineA(5f));
        }
    }

    private IEnumerator InputDelayCoroutineA(float delayDuration)
    {
        yield return new WaitForSeconds(delayDuration);
        isSpawningTask = false; // Reset flag after delay
        inputAllowed = true;
    }

    public void SpawnTask()
    {
        if (canvasContainer == null)
        {
            Debug.LogError("canvasContainer is not set.");
            return;
        }

        PopUps = Instantiate(popUps, canvasContainer);

        RectTransform rectTransform = PopUps.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localPosition = new Vector3(Random.Range(-500, 500), -20, 0);
        }
        else
        {
            Debug.LogError("RectTransform component not found in instantiated prefab.");
        }
        Debug.Log("New Popup spawned");
    }

    void OnTaskChosen()
    {
        if (HasStateAuthority)
        {
            if (ChosenTask >= 0)
            {
                Debug.Log("Task chosen");
            }
        }
    }

    void OnMasterClientChanged()
    {
        // Handle master client change
    }
}
