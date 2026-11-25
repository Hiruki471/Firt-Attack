using System.Collections;
using UnityEngine;

public class BossPhase1Controller : MonoBehaviour
{
    [Header("เป้าหมายและพลังชีวิตเฟส 1")]
    public Transform player;
    public int maxHealth = 200;
    public int currentHealth;

    [Header("การเคลื่อนที่ (เดินก่อน -> ปลดล็อกวิ่งหลังใช้ Special)")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5.5f;
    public float chaseRange = 14f;
    public float attackRange = 2.6f;
    public float attackCooldown = 1.5f;

    [Header("ท่าพิเศษ Boss_Special และการปลดล็อกวิ่ง")]
    public float specialCooldown = 7f;
    [Range(0f, 1f)]
    public float specialUseChance = 0.35f;
    [Tooltip("0 = ปลดล็อกวิ่งถาวรตลอดเฟส 1 หลังใช้ Boss_Special")]
    public float runDurationAfterSpecial = 0f;
    public float specialCastDuration = 0.85f;
    public float specialHitDelay = 0.35f;
    public float specialDamageRadius = 3.2f;
    public int specialDamage = 28;

    [Header("การโจมตีปกติ (Attack 1 - 4)")]
    public Transform attackPoint;
    public float normalAttackRadius = 1.5f;
    public LayerMask playerLayer;
    public int normalAttackDamage = 15;
    [Tooltip("หน่วงเวลาก่อนดาเมจออกอัตโนมัติในโค้ด (ถ้าใช้ Animation Event อยู่แล้ว ตั้งค่านี้ได้ตามจังหวะฟัน)")]
    public float autoHitDelay = 0.25f;
    public bool useAutoHitInCode = true;

    [Header("ตั้งค่าเมื่อบอสเฟส 1 เลือดหมด -> เข้าสู่เฟส 2")]
    public float phase2SpawnDelay = 0.6f;
    public Vector3 phase2SpawnOffset = Vector3.zero;
    public GameObject phase2BossPrefab;
    public GameObject existingPhase2BossObject;
    public MonoBehaviour phase2ScriptToEnable;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;
    private float baseScaleX;
    private float lastAttackTime = -999f;
    private float lastSpecialTime = -999f;
    private float runUnlockedUntilTime = -999f;

    private bool canRun = false;
    private bool isUsingSpecial = false;
    private bool isDefeatedPhase1 = false;
    private bool hasDealtCurrentSpecialHit = false;
    private bool hasDealtCurrentNormalHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        if (maxHealth <= 0) maxHealth = 200;
        currentHealth = maxHealth;
        baseScaleX = Mathf.Abs(transform.localScale.x);

        FindPlayerIfMissing();

        if (existingPhase2BossObject != null)
            existingPhase2BossObject.SetActive(false);

        if (phase2ScriptToEnable != null)
            phase2ScriptToEnable.enabled = false;
    }

    void FindPlayerIfMissing()
    {
        if (player != null) return;

        GameObject p = GameObject.Find("Player1");
        if (p == null) p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isDefeatedPhase1) return;

        if (player == null)
        {
            FindPlayerIfMissing();
            if (player == null) return;
        }

        if (canRun && runDurationAfterSpecial > 0f && Time.time > runUnlockedUntilTime)
        {
            canRun = false;
        }

        if (isUsingSpecial)
        {
            SetMoveSpeed(0f);
            return;
        }

        float distX = player.position.x - transform.position.x;
        float absDist = Mathf.Abs(distX);
        float dirX = Mathf.Sign(distX);

        // หันหน้าเข้าหาผู้เล่นโดยคงขนาด Scale เดิม
        if (absDist > 0.1f)
        {
            transform.localScale = new Vector3(
                dirX > 0 ? baseScaleX : -baseScaleX,
                transform.localScale.y,
                transform.localScale.z
            );
        }

        if (absDist <= attackRange)
        {
            SetMoveSpeed(0f);
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformNormalAttack();
            }
        }
        else if (absDist <= chaseRange)
        {
            // โอกาสใช้ท่า Boss_Special เพื่อสร้างดาเมจพิเศษและปลดล็อกการวิ่ง
            if (Time.time - lastSpecialTime >= specialCooldown &&
                Random.value < specialUseChance * Time.deltaTime * 3f)
            {
                StartCoroutine(SpecialMoveRoutine());
                return;
            }

            // ตอนปกติเดินได้อย่างเดียว (walkSpeed) จนกว่าจะใช้ Boss_Special ถึงจะวิ่งได้ (runSpeed)
            float activeSpeed = canRun ? runSpeed : walkSpeed;
            SetMoveSpeed(dirX * activeSpeed);
        }
        else
        {
            SetMoveSpeed(0f);
        }
    }

    void SetMoveSpeed(float vx)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);

        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(vx));
    }

    IEnumerator SpecialMoveRoutine()
    {
        if (isDefeatedPhase1 || isUsingSpecial) yield break;

        isUsingSpecial = true;
        hasDealtCurrentSpecialHit = false;
        lastSpecialTime = Time.time;
        SetMoveSpeed(0f);

        if (anim != null)
        {
            anim.ResetTrigger("SpecialTrigger");
            anim.SetTrigger("SpecialTrigger");
        }

        if (specialHitDelay > 0f)
            yield return new WaitForSeconds(specialHitDelay);

        if (!isDefeatedPhase1 && !hasDealtCurrentSpecialHit && useAutoHitInCode)
        {
            OnSpecialAttackHitAndUnlockRun();
        }

        float remainingTime = Mathf.Max(0f, specialCastDuration - specialHitDelay);
        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);

        FinishSpecialMove();
    }

    // เรียกได้ทั้งจากโค้ดอัตโนมัติ และจาก Animation Event ในคลิป Boss_Special
    public void OnSpecialAttackHitAndUnlockRun()
    {
        if (isDefeatedPhase1 || hasDealtCurrentSpecialHit) return;
        hasDealtCurrentSpecialHit = true;

        canRun = true;
        if (runDurationAfterSpecial > 0f)
        {
            runUnlockedUntilTime = Time.time + runDurationAfterSpecial;
        }

        Vector2 center = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, specialDamageRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            hit.SendMessage("TakeDamage", specialDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    // เรียกได้ทั้งจากโค้ดอัตโนมัติ และจาก Animation Event เฟรมสุดท้ายของ Boss_Special
    public void FinishSpecialMove()
    {
        isUsingSpecial = false;
    }

    void PerformNormalAttack()
    {
        lastAttackTime = Time.time;
        hasDealtCurrentNormalHit = false;
        int attackIdx = Random.Range(1, 5); // สุ่ม Attack 1 - 4

        if (anim != null)
        {
            anim.SetInteger("AttackIndex", attackIdx);
            anim.ResetTrigger("AttackTrigger");
            anim.SetTrigger("AttackTrigger");
        }

        if (useAutoHitInCode)
        {
            StartCoroutine(AutoNormalHitRoutine());
        }
    }

    IEnumerator AutoNormalHitRoutine()
    {
        if (autoHitDelay > 0f)
            yield return new WaitForSeconds(autoHitDelay);

        DealNormalAttackDamage();
    }

    // เรียกได้ทั้งจากโค้ดอัตโนมัติ และจาก Animation Event ในคลิป Boss_Attack1 - 4
    public void DealNormalAttackDamage()
    {
        if (isDefeatedPhase1 || hasDealtCurrentNormalHit) return;
        hasDealtCurrentNormalHit = true;

        Vector2 center = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, normalAttackRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            hit.SendMessage("TakeDamage", normalAttackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDefeatedPhase1 || damage <= 0) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(TransitionToPhase2OnDeath());
        }
        else if (anim != null)
        {
            anim.ResetTrigger("HurtTrigger");
            anim.SetTrigger("HurtTrigger");
        }
    }

    IEnumerator TransitionToPhase2OnDeath()
    {
        isDefeatedPhase1 = true;
        isUsingSpecial = false;
        StopAllCoroutines();
        SetMoveSpeed(0f);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (col != null)
            col.enabled = false;

        if (anim != null)
        {
            anim.SetBool("IsDead", true);
        }

        if (phase2SpawnDelay > 0f)
        {
            yield return new WaitForSeconds(phase2SpawnDelay);
        }

        Vector3 spawnPos = transform.position + phase2SpawnOffset;

        if (phase2BossPrefab != null)
        {
            GameObject p2 = Instantiate(phase2BossPrefab, spawnPos, Quaternion.identity);
            if (player != null)
            {
                p2.SendMessage("SetPlayerTarget", player, SendMessageOptions.DontRequireReceiver);
            }
        }
        else if (existingPhase2BossObject != null)
        {
            existingPhase2BossObject.transform.position = spawnPos;
            existingPhase2BossObject.SetActive(true);
            existingPhase2BossObject.SendMessage("SetPlayerTarget", player, SendMessageOptions.DontRequireReceiver);
        }
        else if (phase2ScriptToEnable != null)
        {
            if (col != null) col.enabled = true;
            if (anim != null) anim.SetBool("IsDead", false);
            phase2ScriptToEnable.enabled = true;
            this.enabled = false;
            yield break;
        }

        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Vector2 center = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, normalAttackRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, specialDamageRadius);
    }
}