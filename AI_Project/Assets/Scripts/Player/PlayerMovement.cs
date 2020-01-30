﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float walkSpeed = 3;
    public float runSpeed = 7;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public bool isClimbing = false;

    public float maxInteractionDistance = 1f;

    Vector3 velocity;

    public Camera playerCam, freeCam;

    private bool isCamFree = false;

    // Start is called before the first frame update
    void Start()
    {
        freeCam.gameObject.SetActive(false);
        playerCam.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        UnlockCam();
        if (isCamFree) {
            FreeMove();
        }
        else {
            Move();
            CheckInteraction();
        }
    }

    private void Move() {
        if (isClimbing) {
            Climb();
        }
        else {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            if (controller.isGrounded && velocity.y < 0) {
                velocity.y = 0f;
            }

            // Walk
            Vector3 move = Vector3.Normalize(transform.right * x + transform.forward * z) * Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
            if (Input.GetKey(KeyCode.LeftShift)) { // is running?
                move *= runSpeed;
            }
            else {
                move *= walkSpeed;
            }

            // Jump
            if (Input.GetButton("Jump") && controller.isGrounded) {
                velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            }

            // apply gravity
            velocity.y += gravity * Time.deltaTime;

            // apply movement
            controller.Move((move + velocity) * Time.deltaTime);
        }
    }

    private void Climb() {
        float y = Input.GetAxis("Vertical");
        Vector3 move = transform.up * y;
        move *= walkSpeed;

        // Jump
        if (Input.GetButton("Jump")) {
            velocity.y = Mathf.Sqrt(jumpHeight * -1 * gravity);
            isClimbing = false;
            controller.Move(velocity * Time.deltaTime);
        }

        controller.Move(move * Time.deltaTime);
    }

    private void CheckInteraction() {
        Ray forward = playerCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(forward, out hit, maxInteractionDistance)) {
            InteractiveObject o = hit.collider.gameObject.GetComponent<InteractiveObject>();
            if (o != null) {
                o.highlighted = true;
                if (Input.GetKeyDown(KeyCode.E)) {
                    o.DoSomething();
                }
            }
        }
    }

    private void FreeMove() {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = Input.GetAxis("Height");

        Vector3 move = freeCam.transform.right * x + freeCam.transform.up * y + freeCam.transform.forward * z;
        move = move.normalized * Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z));

        freeCam.transform.Translate(move * runSpeed * Time.deltaTime);
    }

    private void UnlockCam() {
        if (freeCam && Input.GetKeyDown(KeyCode.F)) {
            isCamFree = !isCamFree;
            if (isCamFree) {
                playerCam.gameObject.SetActive(false);
                freeCam.gameObject.SetActive(true);
                //freeCam.transform.SetPositionAndRotation(playerCam.transform.position, playerCam.transform.rotation);
                freeCam.transform.position = playerCam.transform.position;
            }
            else {
                playerCam.gameObject.SetActive(true);
                freeCam.gameObject.SetActive(false);
            }
        }
    }
}
