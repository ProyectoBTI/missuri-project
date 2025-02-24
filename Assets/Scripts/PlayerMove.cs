using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerMove : MonoBehaviour
{

    public CharacterController controller;

    public float speed = 5;

    public float turnSmoothTime = 0.1f; //3

    float turnSmoothVelocity; //3

    public Transform cam;

    void Update()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y; //2

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime); //3

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            transform.rotation = Quaternion.Euler(0f, angle, 0f); // 2

            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

    }
}
