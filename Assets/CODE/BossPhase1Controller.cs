using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossPhase1Controller : MonoBehaviour
{
    [Header("เลือด")]
    public int maxHealth = 400;
    [Range(0.1f, 0.9f)] public float phase2Threshold = 0.5f;

    [Header("เฟส 1")]
    public float p1MoveSpeed = 2f;
    public int p1Damage = 10;
    public float p1AttackCooldown = 2f;

    [Header("เฟส 2 (โกรธ)")]
    public float p2MoveSpeed = 4f;
    public int p2Damage = 20;
    public float p2AttackCooldown = 1f;
    public Color phase2Color = new Color(1f, 0.4f, 0.4f);

    [Header("โจมตี")]
    public Transform attackPoint;
    public float attackRadius = 1f;
    public float attackTriggerDistance = 1.8f;
    public float attackWindup = 0.4f;
    public float attackRecovery = 0.4f;
    public LayerMask playerLayer;

    [Header("สกิลเฟส 2")]
    public float skillRadius = 4f;
    public int skillDamage = 30;
    public float skillWindup = 0.8f;
    public float skillRecovery = 0.6f;
    public bool repeatSkill = true;
    public float skillInterval = 8f;
    public GameObject skillEffectPrefab;

    [Header("อ้างอิง")]
    public Transform player;

    [Header("จบเกมเมื่อบอสตาย")]
    public float endGameDelay = 2.5f;      // รอให้ท่าตายเล่นจบก่อน
    public GameObject victoryPanel;        // ลาก Panel "ชนะ" ใส่ (ไม่ใส่ก็ได้)
    public string victorySceneName = "";   // หรือใส่ชื่อ Scene ที่จะโหลด

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private int currentHealth;
    private int phase = 1;
    private bool isDead;
    private bool isTransitioning;
    private bool isAttacking;
    private float nextAttackTime;
    private float nextSkillTime;
    private float baseScaleX;
    private Coroutine attackCo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
        baseScaleX = Mathf.Abs(transform.localScale.x);

        if (player == null)
        {
            SamuraiController s = FindAnyObjectByType<SamuraiController>();
            if (s != null) player = s.transform;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (isTransitioning || isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            SetSpeedAnim(0);
            return;
        }

        float dx = player.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        transform.localScale = new Vector3(dx >= 0 ? baseScaleX : -baseScaleX,
                                           transform.localScale.y, transform.localScale.z);

        // เฟส 2: ใช้สกิลซ้ำตามเวลา
        if (phase == 2 && repeatSkill && Time.time >= nextSkillTime)
        {
            StartCoroutine(SkillRoutine());
            return;
        }

        if (dist > attackTriggerDistance)
        {
            float speed = (phase == 1) ? p1MoveSpeed : p2MoveSpeed;
            rb.linearVelocity = new Vector2(Mathf.Sign(dx) * speed, rb.linearVelocity.y);
            SetSpeedAnim(speed);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            SetSpeedAnim(0);

            if (Time.time >= nextAttackTime)
                attackCo = StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        if (anim != null)
        {
            anim.ResetTrigger("AttackTrigger");
            anim.SetTrigger("AttackTrigger");
        }

        yield return new WaitForSeconds(attackWindup);

        if (!isDead && !isTransitioning && attackPoint != null)
        {
            Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
            if (hit != null)
            {
                SamuraiController p = hit.GetComponentInParent<SamuraiController>();
                if (p != null)
                    p.TakeDamage(phase == 1 ? p1Damage : p2Damage);
            }
        }

        yield return new WaitForSeconds(attackRecovery);

        nextAttackTime = Time.time + (phase == 1 ? p1AttackCooldown : p2AttackCooldown);
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isTransitioning) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (phase == 1 && currentHealth <= maxHealth * phase2Threshold)
        {
            StartCoroutine(EnterPhase2());
            return;
        }

        if (anim != null && !isAttacking)
            anim.SetTrigger("HurtTrigger");
    }

    IEnumerator EnterPhase2()
    {
        phase = 2;
        isTransitioning = true;

        if (attackCo != null) StopCoroutine(attackCo);
        isAttacking = false;

        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetTrigger("Phase2Trigger");

        yield return new WaitForSeconds(1.5f);

        if (sr != null) sr.color = phase2Color;
        isTransitioning = false;

        yield return StartCoroutine(SkillRoutine());
    }

    IEnumerator SkillRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.ResetTrigger("SkillTrigger");
            anim.SetTrigger("SkillTrigger");
        }

        yield return new WaitForSeconds(skillWindup);

        if (!isDead)
        {
            if (skillEffectPrefab != null)
                Instantiate(skillEffectPrefab, transform.position, Quaternion.identity);

            Collider2D hit = Physics2D.OverlapCircle(transform.position, skillRadius, playerLayer);
            if (hit != null)
            {
                SamuraiController p = hit.GetComponentInParent<SamuraiController>();
                if (p != null) p.TakeDamage(skillDamage);
            }
        }

        yield return new WaitForSeconds(skillRecovery);

        nextSkillTime = Time.time + skillInterval;
        nextAttackTime = Time.time + 0.5f;
        isAttacking = false;
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("IsDead", true);
        foreach (Collider2D c in GetComponents<Collider2D>()) c.enabled = false;

        StartCoroutine(EndGameRoutine());
    }

    IEnumerator EndGameRoutine()
    {
        yield return new WaitForSeconds(endGameDelay);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f;               // หยุดเกม
        }
        else if (!string.IsNullOrEmpty(victorySceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(victorySceneName);
        }
    }

    void SetSpeedAnim(float v)
    {
        if (anim != null) anim.SetFloat("Speed", v);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackTriggerDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, skillRadius);
    }
    
}
