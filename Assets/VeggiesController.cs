using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VeggiesController : MonoBehaviour
{
    public bool collected = false;
    public bool watered = false;
    public Text VeggieText;

    private Animator animator;

    private static int collectedVeggies = 0;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        if (!collected)
        {
            animator.SetBool("Cut", false);
            animator.SetBool("Watered", false);
        }
        else
        {
            if (!watered)
            {
                animator.SetBool("Cut", true);
                animator.SetBool("Watered", false);
            }
            else
            {
                animator.SetBool("Cut", true);
                animator.SetBool("Watered", true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        VeggieText.text = "Vegatables collected: " + collectedVeggies;
    }

    void VeggiesAction()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag("Cutters")) 
            {
                if (!collected)
                {
                    collected = true;
                    animator.SetBool("Cut", true);
                    animator.SetBool("Watered", false);
                    //Debug.Log("Veggie collected!");
                    collectedVeggies++;
                }
                else
                {
                    Debug.Log("Cannot collect veggie!");
                }
                break;
            }
            else if (collider.gameObject.CompareTag("WaterPipette"))
            {
                if (!watered)
                {
                    if (collected)
                    {
                        animator.SetBool("Cut", true);
                        animator.SetBool("Watered", true);
                        //Debug.Log("Watered veggie!");
                    }
                    else
                    {
                        Debug.Log("Cannot water veggie!");
                    }
                }
                else
                {
                    Debug.Log("Already watered veggie!");
                }
                break;
            }
        }
    }
}
