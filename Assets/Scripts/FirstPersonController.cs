using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 5f;
    public float lookSpeed = 2f;
    public float lookXLimit = 60f;
    public float gravity = 10f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = walkSpeed * Input.GetAxis("Vertical");
        float curSpeedY = walkSpeed * Input.GetAxis("Horizontal");
        float movementDirectionY = moveDirection.y;
        
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (!characterController.isGrounded)
        {
            moveDirection.y = movementDirectionY - (gravity * Time.deltaTime);
        }
        else
        {
            moveDirection.y = -0.5f; 
        }

        characterController.Move(moveDirection * Time.deltaTime);

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}