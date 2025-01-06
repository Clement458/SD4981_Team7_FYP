using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostPlayerScript : NetworkBehaviour
{
    [Networked] private TickTimer delay { get; set; }
    [SerializeField] private GameObject popUps;
    static public string code;
    public string clientAns;
    public GameObject PopUps;
    protected PopUpBtn PopUpBtn;
    protected int sessionResources = 0;
    private NetworkCharacterController _cc;
    bool taskSpawned;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
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
