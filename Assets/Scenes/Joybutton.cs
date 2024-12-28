using UnityEngine;
using UnityEngine.EventSystems;

public class Joybutton : MonoBehaviour, IPointerUpHandler,IPointerDownHandler
{
    [HideInInspector]
    public bool Pressed;

    public string buttonName;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed = true;
        
        Debug.Log("pressed" + " " + buttonName);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Pressed = false;
        Debug.Log("release");
    }

}
