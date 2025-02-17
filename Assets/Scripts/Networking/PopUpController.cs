using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;
using TouchScript.Gestures;
using UnityEngine.UI;

public class PopUpController : MonoBehaviour
{
    private PopUpBtn popUpBtn;
    private Text popUpText;
    private GameObject popupInstance;

    public HostPlayerScript LocalPlayer = HostPlayerScript.LocalPlayer;

    private string code;
    private bool inputAllowed = true;
    bool taskComplete = false;

    private void Start()
    {
        popUpBtn = GetComponentInChildren<PopUpBtn>();
        popUpText = GetComponentInChildren<Text>();

        code = Random.Range(1, 11).ToString();
        popUpText.text = "Rocket landing task\nCurrent code: " + code;
    }

    private void Update()
    {
        if (inputAllowed && Input.GetKeyUp(KeyCode.O))
        {
            Debug.Log("Task refreshing");
            inputAllowed = false;
            RefreshTask();
            StartCoroutine(InputDelayCoroutineB(2f));
        }

        if (inputAllowed && Input.GetKeyUp(KeyCode.I))
        {
            Debug.Log("taskComplete now true");
            taskComplete = true;
            inputAllowed = false;
            StartCoroutine(InputDelayCoroutineB(2f));
        }
    }

    private IEnumerator InputDelayCoroutineB(float delayDuration)
    {
        yield return new WaitForSeconds(delayDuration);
        inputAllowed = true;
    }

    public void RefreshTask()
    {
        if (taskComplete)
        {
            Debug.Log("Task complete, closing popup of code " + code);
            Destroy(popupInstance); // it won't destroy X_X
        }
        else
        {
            popUpText.text = popUpText.text + "\nWork in progress";
        }

        Debug.Log("Task status refreshed");

        if (popUpBtn != null)
        {
            // Change the text of the child Text component
            Text btnText = popUpBtn.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = "Occupied";
            }
            else
            {
                Debug.LogError("Button text component not found.");
            }

            // Want to disable the button
            //popUpBtn.interactable = false;
        }
        else
        {
            Debug.LogError("popUpBtn reference is not set.");
        }
    }

    public void OnPopupBtnClicked()
    {
        if (popupInstance != null)
        {
            RPC_SendMessage(code);
            RefreshTask();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        Debug.Log("message: " + message);

        if (messageSource == FindObjectOfType<NetworkRunner>().LocalPlayer)
        {
            Debug.Log($"Host sent: {message}\n");
        }
        else
        {
            Debug.Log($"Some client said: {message}\n");
            if (message == code)
            {
                taskComplete = true;
            }
            else
            {
                Debug.Log("Failed to match message with code.");
            }
        }
    }
}

