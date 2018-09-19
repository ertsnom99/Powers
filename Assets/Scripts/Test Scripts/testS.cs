using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testS : MonoBehaviour {

    Vector3 movement;
    Rigidbody rb;
    float movementSpeed = 2;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        movement.Set(h, 0f, v);

        movement = movement * movementSpeed * Time.fixedDeltaTime;

        rb.MovePosition(transform.position + movement);
    }
}
