using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("ข้อมูลบอสและเป้าหมาย")]
    public Transform player;
    public int maxHealth = 200;
    public int currentHealth;

    [Header("การเคลื่อนที่และระยะโจมตี")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5.0f;
    public float chaseRange = 10f;
    public float attackRange = 2.2f;
    public float attackCooldown = 1.6f;

    [Header("จุดเช็คดาเมจ (Hitbox)")]
    public Transform attackPoint;
    public float attackRadius = 1.2f;
    public LayerMask playerLayer;
    public int normalAttackDamage = 15;
    public int specialAttackDamage = 30;

    [Header("การเปลี่ยนสู่เฟส 2")]
    [Range(0.1f, 0.9f)]
    public float phase2HealthRatio = 0.5f;
    public GameObject phase2VisualOrEffect; // เอฟเฟกต์หรือร่างเฟส 2 (ถ้ามี)
    public MonoBehaviour phase2BehaviorScript; // สคริปต์เฟส 2 แยกต่างหาก (ถ้ามี)

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private float baseScaleX;
    private float lastAttackTime = -999f;
    private bool isPhase2 = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        baseScaleX = Mathf.Abs(transform.localScale.x);

        if (player == null)
        {
            GameObject p = GameObject.Find("Player1");
            if (p != null) player = p.transform;
        }

        if (phase2BehaviorScript != null)
            phase2BehaviorScript.enabled = false;
    }

    void Update()
    {
        if (isDead || player == null) return;

        // เช็คเงื่อนไขเข้าสู่เฟส 2 เมื่อเลือดลดลงถึงเกณฑ์ที่กำหนด
        if (!isPhase2 && currentHealth <= maxHealth * phase2HealthRatio)
        {
            EnterPhase2();
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float directionX = Mathf.Sign(player.position.x - transform.position.x);

        // หันหน้าเข้าหาผู้เล่น
        if (Mathf.Abs(player.position.x - transform.position.x) > 0.1f)
        {
            transform.localScale = new Vector3(
                directionX > 0 ? baseScaleX : -baseScaleX,
                transform.localScale.y,
                transform.localScale.z
            );
        }

        if (distanceToPlayer <= attackRange)
        {
            // หยุดเดินเพื่อโจมตี
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null)
                anim.SetFloat("Speed", 0f);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                ChooseAndPerformAttack();
            }
        }
        else if (distanceToPlayer <= chaseRange)
        {
            // เดินหรือวิ่งเข้าหาผู้เล่น (เฟส 2 วิ่งเร็วขึ้น)
            float moveSpeed = isPhase2 ? runSpeed * 1.2f : walkSpeed;
            if (rb != null)
                rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);
            if (anim != null)
                anim.SetFloat("Speed", moveSpeed);
        }
        else
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null)
                anim.SetFloat("Speed", 0f);
        }
    }

    void ChooseAndPerformAttack()
    {
        lastAttackTime = Time.time;

        // ในเฟส 2 มีโอกาสใช้ท่า Special หรือคอมโบถี่ขึ้น
        if (isPhase2 && Random.value < 0.35f)
        {
            if (anim != null)
            {
                anim.ResetTrigger("SpecialTrigger");
                anim.SetTrigger("SpecialTrigger");
            }
            return;
        }

        int attackIdx = isPhase2 ? Random.Range(2, 5) : Random.Range(1, 4);
        if (anim != null)
        {
            anim.SetInteger("AttackIndex", attackIdx);
            anim.ResetTrigger("AttackTrigger");
            anim.SetTrigger("AttackTrigger");
        }
    }

    // เรียกจาก Animation Event ตอนจังหวะดาบฟันโดน
    public void DealAttackDamage()
    {
        if (attackPoint == null || isDead) return;

        int dmg = isPhase2 ? Mathf.RoundToInt(normalAttackDamage * 1.3f) : normalAttackDamage;
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D hit in hitPlayers)
        {
            hit.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void DealSpecialDamage()
    {
        if (attackPoint == null || isDead) return;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius * 1.4f, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            hit.SendMessage("TakeDamage", specialAttackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        attackCooldown = Mathf.Max(0.7f, attackCooldown * 0.7f);

        // ปรับสี/ออร่าของบอสเฟส 1 ก่อนเปลี่ยนไปสคริปต์เฟส 2 หรือเพิ่มความดุ
        if (sr != null)
            sr.color = new Color(0.65f, 0.85f, 1f);

        if (phase2VisualOrEffect != null)
            phase2VisualOrEffect.SetActive(true);

        if (anim != null)
        {
            anim.ResetTrigger("SpecialTrigger");
            anim.SetTrigger("SpecialTrigger");
        }

        if (phase2BehaviorScript != null)
        {
            phase2BehaviorScript.enabled = true;
            this.enabled = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            if (anim != null)
                anim.SetBool("IsDead", true);
        }
        else
        {
            if (anim != null)
            {
                anim.ResetTrigger("HurtTrigger");
                anim.SetTrigger("HurtTrigger");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}