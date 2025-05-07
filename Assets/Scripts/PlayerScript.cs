using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class PlayerScript : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float baseSpeed;
    [SerializeField] float speed;
    [SerializeField] float jumpForce;
    [SerializeField] Rigidbody rb;
    bool canJump;
    Vector2 moveInput;
    Vector3 move;

    [Header("Dash")]
    [SerializeField] Transform cameraPlayer;
    [SerializeField] float rotationSpeed;
    [SerializeField] float dashForce;
    [SerializeField] float dashUpward;
    [SerializeField] float dashDuration;
    bool isDashing;

    [Header("Camara")]
    Vector3 cameraForward;
    Vector3 cameraRight;
    Vector3 cameraUp;
    
    [Header("Ground")]
    public bool grounded;
    public LayerMask ground;
    public float height;
    public float groundDrag;

    [Header("WallClimb")]
    [SerializeField] float climbSpeed;
    float climbTimer;
    [SerializeField] float climbMaxTime;
    bool isClimbing;

    [Header("WallDetect")]
    [SerializeField] float detectionLength;
    [SerializeField] float sphereCastRadius;
    float wallAngle;
    [SerializeField] float wallAngleMax;
    public LayerMask wall;
    RaycastHit frontWall;
    public bool isWall;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        canJump = true;
        isDashing = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        //Definicion de camara y direccion
        cameraForward = cameraPlayer.transform.forward;
        cameraRight = cameraPlayer.transform.right;
        cameraUp = cameraPlayer.transform.up;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        //Definicion de "en el suelo"
        grounded = Physics.Raycast(transform.position, Vector3.down, height * 0.5f + 0.2f, ground);

        //genera drag
        if (grounded && !isDashing)
        {
            rb.linearDamping = groundDrag;
            climbTimer = climbMaxTime;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        //Deteccion de Paredes
        isWall = Physics.SphereCast(transform.position, sphereCastRadius, cameraForward, out frontWall, detectionLength, wall);
        wallAngle = Vector3.Angle(cameraForward, -frontWall.normal);

        //Manejo de escalada por paredes
        StateOfClimb();
        if(isClimbing) ClimbingMove();

        //Definicion de movimiento
        move = (cameraForward * moveInput.y + cameraRight * moveInput.x);
        rb.AddForce(move.normalized * speed, ForceMode.Force);

        //Limita velocidad
        SpeedControl();

        
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        


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
            isDashing = true;
            Vector3 forceToApply = cameraForward * dashForce * moveInput.y + cameraUp * dashUpward;
            rb.AddForce(forceToApply, ForceMode.Impulse);
            speed = dashForce;
            Invoke(nameof(ResetDash), dashDuration);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = true;
        }
        if (collision.gameObject.CompareTag("Death"))
        {
            SceneManager.LoadScene(0);
        }
        if (collision.gameObject.CompareTag("Goal"))
        {
            SceneManager.LoadScene(1);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatSpeed = new Vector3 (rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatSpeed.magnitude > speed)
        {
            Vector3 limit = flatSpeed.normalized * speed;
            rb.linearVelocity = new Vector3(limit.x, rb.linearVelocity.y, limit.z);
        }
    }

    private void StateOfClimb()
    {
        if (moveInput.y > 0 && wallAngle < wallAngleMax && isWall)
        {
            if (!isClimbing && climbTimer > 0) StartClimb();

            if(climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimb();
        }

        else
        {
            if(isClimbing) StopClimb();
        }
    }

    private void ResetDash()
    {
        isDashing = false;
        speed = baseSpeed;
    }

    private void StartClimb()
    {
        isClimbing = true;
    }

    private void ClimbingMove()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);
    }

    private void StopClimb()
    {
        isClimbing = false;
    }
}
