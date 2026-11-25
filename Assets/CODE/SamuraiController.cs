using System.Collections;
using UnityEngine;

public class SamuraiController : MonoBehaviour
{
    [Header("ค่าพลังและการเคลื่อนที่")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.5f;
    public float jumpForce = 11f;
    public int maxHealth = 100;
    public int currentHealth;

    [Header("เช็คพื้น (Ground Check)")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("การทำดาเมจใส่ศัตรู / บอส")]
    public Transform attackPoint;
    public float attackRange = 1.4f;
    public LayerMask enemyLayer;
    public int attackDamage = 25;
    public float hitDelay = 0.15f;
    public bool useAutoHitInCode = true;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool canDoubleJump;
    private bool isBlocking;
    private bool isDead;
    private float moveInput;
    private bool isRunning;
    private float baseScaleX;
    private bool hasDealtCurrentSwingDamage;

    // ระบบคอมโบโจมตี 3 จังหวะ
    private int comboStep = 0;
    private float lastAttackTime = 0f;
    public float comboResetTime = 1.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        if (isDead) return;

        // 1. เช็คว่ายืนบนพื้นไหม
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
            isGrounded = false;

        // รีเซ็ต Double Jump เมื่อแตะพื้นและไม่ได้พุ่งขึ้น
        if (isGrounded && rb != null && rb.linearVelocity.y <= 0.05f)
        {
            canDoubleJump = true;
        }

        if (anim != null)
            anim.SetBool("IsGrounded", isGrounded);

        // 2. กดป้องกัน (คลิกขวาค้าง หรือปุ่ม K)
        isBlocking = (Input.GetMouseButton(1) || Input.GetKey(KeyCode.K)) && isGrounded;
        if (anim != null)
            anim.SetBool("IsBlocking", isBlocking);

        if (isBlocking)
        {
            moveInput = 0f;
            isRunning = false;
            if (anim != null)
                anim.SetFloat("Speed", 0);
            return;
        }

        // 3. รับค่าเดินซ้าย-ขวา (A/D หรือลูกศร) และวิ่ง (กด Shift ค้าง)
        moveInput = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = moveInput != 0 ? (isRunning ? runSpeed : walkSpeed) : 0f;

        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(targetSpeed));

        // กลับด้านตัวละครซ้าย-ขวา โดยคงขนาด Scale เดิมไว้
        if (moveInput > 0)
            transform.localScale = new Vector3(baseScaleX, transform.localScale.y, transform.localScale.z);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-baseScaleX, transform.localScale.y, transform.localScale.z);

        // 4. กระโดดบนพื้น + Double Jump กลางอากาศได้แค่ 1 ครั้ง (Spacebar)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded && rb != null && rb.linearVelocity.y <= 0.1f)
            {
                isGrounded = false;
                canDoubleJump = true;
                Jump();
            }
            else if (canDoubleJump)
            {
                canDoubleJump = false;
                Jump();
            }
        }

        // 5. โจมตีคอมโบ 3 จังหวะ (คลิกซ้าย หรือปุ่ม J)
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
        {
            PerformAttack();
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (isDead || isBlocking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (anim != null)
        {
            anim.SetBool("IsGrounded", false);
            anim.ResetTrigger("JumpTrigger");
            anim.SetTrigger("JumpTrigger");
        }
    }

    void PerformAttack()
    {
        lastAttackTime = Time.time;
        hasDealtCurrentSwingDamage = false;
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        if (anim != null)
        {
            anim.SetInteger("AttackIndex", comboStep);
            anim.ResetTrigger("AttackTrigger");
            anim.SetTrigger("AttackTrigger");
        }

        if (useAutoHitInCode)
        {
            StartCoroutine(AutoDealAttackDamageRoutine());
        }
    }

    IEnumerator AutoDealAttackDamageRoutine()
    {
        if (hitDelay > 0f)
            yield return new WaitForSeconds(hitDelay);

        DealPlayerAttackDamage();
    }

    // เรียกได้ทั้งจากโค้ดอัตโนมัติ และจาก Animation Event ตอนดาบผู้เล่นฟันโดน
    public void DealPlayerAttackDamage()
    {
        if (isDead || hasDealtCurrentSwingDamage) return;
        hasDealtCurrentSwingDamage = true;

        Vector2 center = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(center, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    // เรียกใช้ฟังก์ชันนี้เมื่อบอสหรือศัตรูตีโดนตัวละคร
    public void TakeDamage(int damage)
    {
        if (isDead || isBlocking || damage <= 0) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            if (anim != null)
                anim.SetBool("IsDead", true);
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
        else
        {
            if (anim != null)
                anim.SetTrigger("HurtTrigger");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Vector2 atkCenter = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(atkCenter, attackRange);
    }
}