using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Action<Player> CoolantReceived;
    public Action<Player> HazardHit;
    public Action<Player> PlayerReady;
    private Vector2 move;

    [SerializeField]
    private Rigidbody2D rigidBody;

    [SerializeField]
    private InputActionReference moveInput;

    [SerializeField]
    private InputActionReference jumpInput;

    [SerializeField]
    private AudioSource walkingAudioSource;
    private float gravityScale = 1f;
    private float fallingGravityScale = 3f;
    private bool facingRight = true;
    private bool jumping = false;
    private bool isGrounded = true;
    private bool fall = false;
    
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private AudioSource hitSound;

    [SerializeField]
    private AudioSource coolantSound;
    
    private void Awake() =>
        rigidBody.freezeRotation = true;

    private void OnEnable()
    {
        moveInput.action.performed += OnMove;
        moveInput.action.canceled += OnMove;
        moveInput.action.Enable();
        
        jumpInput.action.performed += OnJump;
        jumpInput.action.canceled += OnJump;
        jumpInput.action.Enable();
    }

    private void OnDisable()
    {
        moveInput.action.performed -= OnMove;
        moveInput.action.canceled -= OnMove;
        moveInput.action.Disable();
        
        jumpInput.action.performed -= OnJump;
        jumpInput.action.canceled -= OnJump;
        jumpInput.action.Disable();
    }
    
    public void Initialize()
    {
        transform.position = new Vector2(0, -3);
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;        
        fall = false;
        animator.Play("Idle", 0, 0f);
    }

    private void OnMove(InputAction.CallbackContext context) =>
        move = context.ReadValue<Vector2>();

    private void OnJump(InputAction.CallbackContext context)
    {   
        if (jumping || fall)
        {
            return;
        }

        var jumpHeight = 8f;
        rigidBody.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
        jumping = true;
        isGrounded = false;
        animator.SetBool("Jumping", true);
        walkingAudioSource.Stop();
    }
    
    public void MovePlayer()
    {
        var moveX = fall ? 0 : move.x;
        FlipSprite(moveX);

        animator.SetFloat("Speed", Math.Abs(moveX));

        if (moveX == 0)
        {
            walkingAudioSource.Stop();
        }
        else if (!walkingAudioSource.isPlaying && !jumping)
        {
            walkingAudioSource.PlayDelayed(0.15f);
        }
        
        rigidBody.linearVelocity = new Vector2(moveX * moveSpeed, rigidBody.linearVelocity.y);
        rigidBody.gravityScale = rigidBody.linearVelocity.y >= 0
            ? gravityScale
            : fallingGravityScale;
            
        if (Math.Abs(rigidBody.linearVelocity.y) < 0.01)
        {
            jumping = false;
            animator.SetBool("Jumping", false);
        }

        if (isGrounded)
        {
            rigidBody.sharedMaterial = null;
        }
    }

    private void FlipSprite(float moveDirection)
    {
        if(moveDirection == 0)
        {
            return;
        }

        var isNowFacingRight = moveDirection > 0;
        if (facingRight != isNowFacingRight)
        {
            var localScale = gameObject.transform.localScale;
            gameObject.transform.localScale = new Vector3(localScale.x * -1, localScale.y, localScale.z);
            facingRight = isNowFacingRight;
        }
    }
    
    private void FixedUpdate() =>
        rigidBody.angularVelocity = 0;

    private IEnumerator WaitPlayerRecovery()
    {
        yield return new WaitForSeconds(2f);
        
        fall = false;
        animator.Play("Idle", 0, 0f);
        PlayerReady?.Invoke(this);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Coolant"))
        {
            CoolantReceived?.Invoke(this);
            coolantSound.Play();
        }

        if (col.gameObject.CompareTag("Hazard"))
        {
            fall = true;
            animator.Play("Fall", 0, 0f);
            HazardHit?.Invoke(this);
            StartCoroutine(WaitPlayerRecovery());
            hitSound.Play();
        }
    }
}
