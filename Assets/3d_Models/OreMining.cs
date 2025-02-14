using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TouchScript.Gestures;
using System;

public class OreMining : MonoBehaviour
{
    public float Power = 5.0f;

    private LongPressGesture longPressGesture;
    private PressGesture pressGesture;
    private MeshRenderer rnd;
    private bool growing = false;
    private float growingTime = 0;
    private bool isPressed;

    // spawning directions
    private Vector3[] directions =
    {
        new Vector3(15, 6, -10),
        new Vector3(-15, 6, -10),
        new Vector3(10, 6, -10),
        new Vector3(-10, 6, -10)
    };

    private void OnEnable()
    {
        rnd = GetComponent<MeshRenderer>();
        longPressGesture = GetComponent<LongPressGesture>();
        pressGesture = GetComponent<PressGesture>();

        longPressGesture.StateChanged += longPressedHandler;
        pressGesture.Pressed += pressedHandler;
    }

    private void OnDisable()
    {
        longPressGesture.StateChanged -= longPressedHandler;
        pressGesture.Pressed -= pressedHandler;
    }

    private void Update()
    {
        if (growing)
        {
            growingTime += Time.unscaledDeltaTime;
            rnd.material.color = Color.Lerp(Color.white, Color.red, growingTime);
        }
    }

    private void startGrowing()
    {
        growing = true;
    }

    private void stopGrowing()
    {
        growing = false;
        growingTime = 0;
        rnd.material.color = Color.white;
    }

    // short press to collect materials
    private void pressedHandler(object sender, EventArgs e)
    {
        if (isPressed) return;
        isPressed = true;
        Invoke("ResetPress", 0.1f);

        startGrowing();
        // if the rocks are small (aka spawned ones)
        if (transform.localScale.x < 0.4f)
        {
            if (gameObject.tag == "Iron ore")
            {
                GameManager.instance.AddCollectedIron(1);
            }
            else if (gameObject.tag == "Lunar rocks")
            {
                GameManager.instance.AddCollectedRocks(1);
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("No ores collected");
        }
    }

    private void ResetPress()
    {
        isPressed = false;
    }

    // long press and spawn smaller materials for collection
    private void longPressedHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            if (transform.localScale.x > 0.5f)
            {
                for (int i = 0; i < 4; i++)
                {
                    var obj = Instantiate(gameObject) as GameObject;
                    var cube = obj.transform;

                    cube.parent = transform.parent;
                    cube.name = "Cube";
                    cube.localScale = 0.3f * transform.localScale;
                    cube.position = transform.TransformPoint(directions[i] / 4);
                    cube.GetComponent<Rigidbody>().AddForce(Power * UnityEngine.Random.insideUnitSphere, ForceMode.Impulse);
                    cube.GetComponent<Renderer>().material.color = Color.white;

                    cube.gameObject.isStatic = false;
                    Rigidbody rb = cube.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.constraints = RigidbodyConstraints.None;
                    }
                }
            }
        }
        else if (e.State == Gesture.GestureState.Failed)
        {
            stopGrowing();
        }
    }
}