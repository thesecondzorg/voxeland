using System;
using System.Collections;
using System.Collections.Generic;
using Client;
using Mirror;
using UnityEngine;

public class PlayerInputSystem : NetworkBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    public Camera playerCamera;
    public Transform playerHead;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 75.0f;

    [SerializeField] private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    [HideInInspector] public bool canMove = true;

    private GameObject[] inventory;
    private int selected;
    private float gravityTmp = 20.0f;
    private bool lockLook = false;
    private Nullable<Vector3> newPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            
            playerCamera = Camera.main;
            Camera.main.orthographic = false;
            Camera.main.transform.SetParent(playerHead.transform);
            Camera.main.transform.localPosition = new Vector3(0f, 1f, -0.2f);
            Camera.main.transform.localEulerAngles = transform.forward;
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            gravityTmp = gravity;
            gravity = 0;
        }
        else
        {
            foreach (SelectTargetSystem component in gameObject.GetComponents<SelectTargetSystem>())
            {
                Destroy(component);
            }
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        OnDisable();
    }

    private void OnDisable()
    {
        if (isLocalPlayer)
        {
            Camera.main.orthographic = true;
            Camera.main.transform.SetParent(null);
            Camera.main.transform.localPosition = new Vector3(0f, 70f, 0f);
            Camera.main.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer )
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.L))
        {
            
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible ;
            lockLook = !lockLook;
        }

        if (lockLook)
        {
            return;
        }
        
        if (newPosition.HasValue)
        {
            transform.position = newPosition.Value;
            newPosition = null;
        } 
        Move();
        if (Input.GetKeyUp(KeyCode.P))
        {
            Vector3 position = transform.position;
            position = new Vector3(position.x , 100, position.z);
            transform.position = position;
        }
    }

    private void Move()
    {
        

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerHead.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    public void EnableGravity()
    {
        gravity = gravityTmp;
    }
}