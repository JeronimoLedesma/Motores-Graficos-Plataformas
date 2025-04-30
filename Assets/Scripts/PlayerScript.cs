using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float jumpForce;
    [SerializeField] Rigidbody rb;
    bool canJump;
    Vector2 moveInput;
    [SerializeField] Transform cameraPlayer;
    [SerializeField] float rotationSpeed;
    [SerializeField] float dashForce;
    [SerializeField] float dashUpward;
    Vector3 cameraForward;
    Vector3 cameraRight;
    Vector3 cameraUp;
    Vector3 move;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        canJump = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        cameraForward = cameraPlayer.transform.forward;
        cameraRight = cameraPlayer.transform.right;
        cameraUp = cameraPlayer.transform.up;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        //Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        move = (cameraForward * moveInput.y + cameraRight * moveInput.x);
        Vector3 moveRelative = transform.TransformDirection(move) * speed * Time.deltaTime;
        

        
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            rb.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        rb.MovePosition(rb.position + moveRelative);


    }

    public void OnJump (InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (canJump)
            {
                rb.AddForce(0f, jumpForce, 0f, ForceMode.Impulse);
                canJump = false;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 forceToApply = cameraForward * dashForce * moveInput.y + cameraUp * dashUpward;
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = true;
        }
    }
}
