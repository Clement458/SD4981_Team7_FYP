using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PopUpBtn : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public bool isPressed;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CallParentMethod);
    }

    void CallParentMethod()
    {
        // Find the parent GameObject
        GameObject parent = transform.parent.gameObject;

        // Get the ParentScript component from the parent
        PopUpController parentScript = parent.GetComponent<PopUpController>();

        // Call the method from the parent script
        if (parentScript != null)
        {
            parentScript.OnPopupBtnClicked();
        }
        else
        {
            Debug.LogError("PopUpController not found in parent UI.");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        // Debug.Log("pressed");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        // Debug.Log("release");
    }
}
