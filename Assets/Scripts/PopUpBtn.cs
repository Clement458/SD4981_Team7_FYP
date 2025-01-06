using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class PopUpBtn : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public bool isPressed;
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
