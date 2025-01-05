using System.Collections;
using UnityEngine;

public class GoodRobot : MonoBehaviour
{
    public Transform[] targets; // Array of target game objects
    public float speed = 2.0f; // Movement speed
    public float stopDistance = 1.0f; // Distance to stop from target

    private int currentTargetIndex = 0; // Current target index
    private int firstValue = 0; // First value to be incremented
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
        while (true)
        {
            // Get the current target position
            Transform currentTarget = targets[currentTargetIndex];

            // Move towards the current target
            while (Vector3.Distance(transform.position, currentTarget.position) > stopDistance)
            {
                transform.LookAt(currentTarget);
                anim.SetBool("Walk_Anim", true);
                transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            if (Vector3.Distance(transform.position, currentTarget.position) <= stopDistance) 
            {
                anim.SetBool("Walk_Anim", false);

            }

            // Wait at the current target and increment the value
            if (currentTargetIndex == 0)
            {
                yield return StartCoroutine(IncrementValue(() => OreMining.collectedOre++));
            }
            else if (currentTargetIndex == 1)
            {
                yield return StartCoroutine(IncrementValue(() => secondValue++));
            }

            // Move to the next target if there are more targets
            currentTargetIndex++;
            if (currentTargetIndex >= targets.Length)
            {
                yield break; // Exit the coroutine if all targets are visited
            }
        }
    }

    IEnumerator IncrementValue(System.Action incrementAction)
    {
        for (int i = 0; i < 5; i++)
        {
            incrementAction();
            Debug.Log("Value: " + (currentTargetIndex == 0 ? OreMining.collectedOre : secondValue));
            float randomDelay = Random.Range(0.5f, 2.0f); // Random delay between 0.5 and 2 seconds
            yield return new WaitForSeconds(randomDelay);
        }
    }
}
