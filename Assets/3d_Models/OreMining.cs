using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class OreMining : MonoBehaviour
{
    public float Power = 5.0f;
    public Text collectionText;

    private LongPressGesture longPressGesture;
    private PressGesture pressGesture;
    private MeshRenderer rnd;
    private bool growing = false;
    private float growingTime = 0;

    private static int collectedItems = 0;

    private Vector3[] directions =
    {
        new Vector3(5, 4, -10),
        new Vector3(-5, 4, -10),
        new Vector3(10, 4, -10),
        new Vector3(-10, 4, -10)
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

    private void pressedHandler(object sender, EventArgs e)
    {
        startGrowing();
        if (transform.localScale.x < 0.5f)
        {
            //Debug.Log("Small object");
            collectedItems += 1;
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("No ores collected");
        }
        collectionText.text = gameObject.tag + " collected: " + collectedItems;
    }

    private void longPressedHandler(object sender, GestureStateChangeEventArgs e)
    {
        if (e.State == Gesture.GestureState.Recognized)
        {
            // if we are not too small
            if (transform.localScale.x > 0.5f)
            {
                // break this cube into 8 parts
                for (int i = 0; i < 4; i++)
                {
                    var obj = Instantiate(gameObject) as GameObject;
                    var cube = obj.transform;
                    
                    cube.parent = transform.parent;
                    cube.name = "Cube";
                    cube.localScale = 0.3f * transform.localScale;
                    cube.position = transform.TransformPoint(directions[i] / 4);
                    cube.GetComponent<Rigidbody>().AddForce(Power * Random.insideUnitSphere, ForceMode.Impulse);
                    cube.GetComponent<Renderer>().material.color = Color.white;
                }
            }
        }
        else if (e.State == Gesture.GestureState.Failed)
        {
            stopGrowing();
        }
    }
}
