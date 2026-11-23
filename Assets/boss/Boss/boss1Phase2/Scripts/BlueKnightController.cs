using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class BlueKnightController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.5f;
    public float jumpForce = 11f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private bool isGrounded;
    private int comboStep = 0;
    private float lastAttackTime;
    public float comboResetDelay = 1.0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;

        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);

        if (moveX != 0)
            sr.flipX = moveX < 0;

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
        }

        if (Time.time - lastAttackTime > comboResetDelay)
            comboStep = 0;

        if (Input.GetMouseButtonDown(0))
        {
            comboStep = (comboStep % 4) + 1;
            lastAttackTime = Time.time;
            anim.SetTrigger("Attack" + comboStep);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("Special");
        }

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsRunning", isRunning && Mathf.Abs(moveX) > 0.01f);
        anim.SetBool("IsGrounded", isGrounded);
    }
}
