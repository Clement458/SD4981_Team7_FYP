using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flight : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    public Button jumpButton;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        jumpButton.onClick.AddListener(Jump);
        Debug.Log("listener added");
    }

    void Update()
    {
        // Move in all directions based on WASD keys
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput) * moveSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        rb.AddForce(Physics.gravity * rb.mass);

        if (Input.GetButton("Jump"))
        {
            Jump();
        }
    }

    void Jump()
    {
        Debug.Log("jump pressed");
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
