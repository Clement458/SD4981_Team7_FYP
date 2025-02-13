using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodRobot : MonoBehaviour
{
    public Transform[] targets; // Array of target game objects
    public float speed = 2.0f; // Movement speed
    public float stopDistance = 1.0f; // Distance to stop from target

    private int currentTargetIndex = 0; // Current target index
    private int secondValue = 0; // Second value to be incremented
    Animator anim;

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        // Start moving towards the first target
        StartCoroutine(MoveToTargets());
    }

    IEnumerator MoveToTargets()
    {
        while (currentTargetIndex < targets.Length)
        {
            // Move towards the current target
            Transform currentTarget = targets[currentTargetIndex];
            yield return StartCoroutine(MoveTowardsTarget(currentTarget));

            // Wait at the current target and increment the value
            if (currentTargetIndex == 0)
            {
                yield return StartCoroutine(IncrementValue(() => GameManager.instance.SetCollectedRocks(GameManager.instance.GetCollectedRocks() + 1)));
            }
            else if (currentTargetIndex == 1)
            {
                yield return StartCoroutine(IncrementValue(() => secondValue++));
            }

            // Move to the next target if there are more targets
            currentTargetIndex++;
        }
    }

    IEnumerator MoveTowardsTarget(Transform target)
    {
        while (Vector3.Distance(transform.position, target.position) > stopDistance)
        {
            transform.LookAt(target);
            anim.SetBool("Walk_Anim", true);
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        anim.SetBool("Walk_Anim", false);
    }

    IEnumerator IncrementValue(System.Action incrementAction)
    {
        for (int i = 0; i < 5; i++)
        {
            incrementAction();
            Debug.Log("Value: " + (currentTargetIndex == 0 ? GameManager.instance.GetCollectedRocks() : secondValue));
            float randomDelay = UnityEngine.Random.Range(0.5f, 2.0f); // Random delay between 0.5 and 2 seconds
            yield return new WaitForSeconds(randomDelay);
        }
    }
}
