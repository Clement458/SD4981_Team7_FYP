using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Examples.Tap;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class OreMining : MonoBehaviour, IDataPersistence
{
    public float Power = 5.0f;
    public Text collectionText;

    private LongPressGesture longPressGesture;
    private PressGesture pressGesture;
    private MeshRenderer rnd;
    private bool growing = false;
    private float growingTime = 0;
    private bool isPressed;

    public static int collectedIron = 0;
    public static int collectedRocks = 0;

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

    private void pressedHandler(object sender, EventArgs e)
    {
        if (isPressed) return; 
        isPressed = true; 
        Invoke("ResetPress", 0.1f);

        startGrowing();
        if (transform.localScale.x < 0.4f)
        {
            //Debug.Log("Small object");
            if (gameObject.tag == "Iron ore")
            {
                collectedIron++;
                collectionText.text = gameObject.tag + " collected: " + collectedIron;
            }
            else if (gameObject.tag == "Lunar rocks")
            {
                collectedRocks++;
                collectionText.text = gameObject.tag + " collected: " + collectedRocks;
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("No ores collected");
        }
    }

    private void updateText()
    {

    }

    private void ResetPress() 
    { 
        isPressed = false; 
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

    public void LoadData(GameData data)
    {
        collectedIron = data.ironOre;
        collectedRocks = data.rocks;

        collectionText.text = gameObject.tag + " collected: " + collectedIron;
        collectionText.text = gameObject.tag + " collected: " + collectedRocks;
    }

    public void SaveData(ref GameData data)
    {
        data.ironOre = collectedIron;
        data.rocks = collectedRocks;
    }
}
