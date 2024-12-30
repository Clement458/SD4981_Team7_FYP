using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : MonoBehaviour
{
    private bool moving;

    private float startPosX;
    private float startPosY;

    private Vector3 resetPosition;

    // Start is called before the first frame update
    void Start()
    {
        resetPosition = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            Vector3 mousePos;
            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            this.gameObject.transform.localPosition = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY, this.gameObject.transform.localPosition.z);
        }
    }

    private void OnMouseDown()
    {
        Vector3 mousePos;
        mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        startPosX = mousePos.x - this.transform.localPosition.x;
        startPosY = mousePos.y - this.transform.localPosition.y;

        moving = true;
    }

    private void OnMouseUp()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.0f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Pots"))
            {
                collider.gameObject.SendMessage("PotAction");
            }
            else if ((gameObject.tag == "Cutters" || gameObject.tag == "WaterPipette" ) && collider.CompareTag("Veggies"))
            {
                collider.gameObject.SendMessage("VeggiesAction");
            }
            else if (gameObject.tag == "Pipette" && collider.CompareTag("Water"))
            {
                gameObject.SendMessage("PipeHandler");
            }
            else if (gameObject.tag == "WaterPipette" && collider.CompareTag("Water"))
            {
                Debug.Log("Pipette already filled!");
            }
            else
            {
                Debug.Log("MoveSystem cannot identify action");
            }
        }

        moving = false;

        this.transform.localPosition = new Vector3(resetPosition.x, resetPosition.y, resetPosition.z);
    }
}
