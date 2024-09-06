using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{

    private PlayerInput m_PlayerInput;
    private InputAction m_MoveAction;
    private InputAction m_JumpAction;
    private InputAction m_DashAction;
    private InputAction m_AttackAction; 

    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    public float dashUnits = 10f;
    private float currentDash = 0f;
    private bool enableDash = false;

    public Transform ceilingCheck;
    public Transform groundCheck;

    public LayerMask groundObject;
    public float checkRadius = 0.2f;

    public int maxJumpCount = 2;

    private Rigidbody2D rb;
    private bool facingRight = true;
    private Vector2 moveDirection;
    private bool isJumping = false; 
    private bool isGrounded;
    private int jumpCount;

    private Animator _animator;
    private bool isAttacking;

    public GameObject attackPoint;
    public float attackRadius = 5f;
    public LayerMask enemies;


    private void Awake()
    {
       rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        jumpCount = maxJumpCount;
    }

    void Update()
    {
        ProcessInputs();

        if (!isAttacking)
            Animate();  
        
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundObject);
        if (isGrounded)
        {
            jumpCount = maxJumpCount;
        }
        if(!isAttacking)
            Move();
    }

    private void ProcessInputs()
    {

        if (m_PlayerInput == null)
        {
            m_PlayerInput = GetComponent<PlayerInput>();
            m_JumpAction = m_PlayerInput.actions["jump"];
            m_MoveAction = m_PlayerInput.actions["move"];
            m_DashAction = m_PlayerInput.actions["dash"];
            m_AttackAction = m_PlayerInput.actions["attack"];
        }
        moveDirection = m_MoveAction.ReadValue<Vector2>();
        if (m_DashAction.triggered)
        {
            enableDash = true;
            _animator.SetBool("IsDashing", true);
            
        }

        if (m_JumpAction.triggered && jumpCount > 0)
        {
            isJumping = true;
        }

        if (m_AttackAction.triggered && !isAttacking)
        {
            PerformAttackAnimation();
        }
    }

    private void Move()
    {
        if (enableDash)
        {
            currentDash = moveDirection.x*dashUnits;
            if (moveDirection.x == 0)
                currentDash = -1*dashUnits;
        }
        rb.velocity = new Vector2(moveDirection.x * moveSpeed + currentDash, rb.velocity.y);
        enableDash = false;
        _animator.SetBool("IsDashing", false);
        currentDash = 0;

        if (isJumping)
        {
            rb.AddForce(new Vector2(0,jumpForce));
            jumpCount--;
        }
        isJumping= false;
    }

    private void Animate()
    {
        //1 right -1 left
        

        if (moveDirection.x > 0 && !facingRight)
        {
            FlipCharacter();
        }
        else if (moveDirection.x < 0 && facingRight)
        {
            FlipCharacter();
        }
    }

    private void PerformAttackAnimation()
    {
       if(_animator != null)
        {
            _animator.SetInteger("AttackType", 0);
            isAttacking = true;
        }
    }

    public void Attack()
    {
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRadius, enemies);
        Debug.Log("Attack called");
    }

    public void EndAttack()
    {
        if (_animator != null)
        {
            isAttacking = false;
            _animator.SetInteger("AttackType", -1);
        }
    }

    private void FlipCharacter()
    {
        facingRight = !facingRight;

        transform.Rotate(0, 180f, 0);
    }

    private void OnDrawGizmos()
    {
        if(attackPoint!= null)
        {
            Gizmos.DrawWireSphere(attackPoint.transform.position, attackRadius);
        }
        
    }

}
