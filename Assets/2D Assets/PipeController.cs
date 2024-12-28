using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("Filled", false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PipeHandler()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.0f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Water"))
            {
                if (gameObject.tag == "Pipette" && !animator.GetBool("Filled"))
                {
                    animator.SetBool("Filled", true);
                    gameObject.tag = "WaterPipette";
                }
                else
                {
                    Debug.Log("Cannot fill pipette!");
                }
            }
            else if (collider.CompareTag("Pots"))
            {
                if (gameObject.tag == "WaterPipette" && animator.GetBool("Filled"))
                {
                    animator.SetBool("Filled", false);
                    gameObject.tag = "Pipette";
                }
                else
                {
                    Debug.Log("Cannot water!");
                }
            }
        }
    }
}
