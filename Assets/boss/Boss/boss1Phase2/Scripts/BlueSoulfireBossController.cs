using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class BlueSoulfireBossController : MonoBehaviour
{
    [Header("Boss Stats & Phase 2 Trigger")]
    public float maxHealth = 1000f;
    public float currentHealth;
    [Range(0.1f, 0.9f)]
    public float phase2HealthThreshold = 0.5f;
    public bool isPhase2 { get; private set; } = false;
    private bool isTransitioning = false;

    [Header("Self-Burning Mechanics (ไฟเผาผลาญตัวเองในเฟส 2)")]
    [Tooltip("พลังชีวิตของบอสที่ลดลงต่อวินาทีเมื่อเข้าสู่เฟส 2")]
    public float selfBurnDamagePerSecond = 12f;
    [Tooltip("ตัวคูณความเร็วแอนิเมชันและการเคลื่อนที่ในเฟส 2")]
    public float phase2SpeedMultiplier = 1.45f;
    [Tooltip("ตัวคูณความแรงการโจมตีในเฟส 2")]
    public float phase2DamageMultiplier = 1.5f;

    [Header("Movement & Combat")]
    public float baseMoveSpeed = 4.0f;
    public float baseAttackCooldown = 1.6f;
    public float attackRange = 2.4f;
    public float rangedAttackRange = 7.5f;
    public Transform playerTarget;

    [Header("Ranged Storm Projectile")]
    public GameObject soulfireProjectilePrefab;
    public Transform firePoint;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private float nextActionTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentHealth <= 0f || isTransitioning) return;

        // ระบบไฟเผาผลาญตัวเองต่อเนื่องในเฟส 2 (แลกเลือดกับความเร็วและความรุนแรง)
        if (isPhase2)
        {
            currentHealth -= selfBurnDamagePerSecond * Time.deltaTime;
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die();
                return;
            }
        }

        if (playerTarget == null) return;

        float dirX = Mathf.Sign(playerTarget.position.x - transform.position.x);
        sr.flipX = dirX < 0;

        float dist = Vector2.Distance(transform.position, playerTarget.position);
        float currentSpeed = isPhase2 ? baseMoveSpeed * phase2SpeedMultiplier : baseMoveSpeed;

        if (Time.time >= nextActionTime)
        {
            if (dist <= attackRange)
            {
                PerformMeleeAttack();
            }
            else if (isPhase2 && dist <= rangedAttackRange && Random.value > 0.45f)
            {
                PerformRangedStorm();
            }
            else
            {
                // พุ่งเข้าหาผู้เล่นอย่างรวดเร็ว
                rb.linearVelocity = new Vector2(dirX * currentSpeed, rb.linearVelocity.y);
                anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f || isTransitioning) return;

        currentHealth -= amount;

        // ตรวจสอบเงื่อนไขเข้าสู่เฟส 2
        if (!isPhase2 && currentHealth <= maxHealth * phase2HealthThreshold)
        {
            StartCoroutine(EnterPhase2Routine());
            return;
        }

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
        else
        {
            // เฟส 2 บอสจะไม่ชะงักนาน ใช้ท่า Hurt_Rapid แล้วสวนกลับทันที
            anim.SetTrigger(isPhase2 ? "Phase2_Hurt_Rapid" : "Hurt");
        }
    }

    private IEnumerator EnterPhase2Routine()
    {
        isTransitioning = true;
        rb.linearVelocity = Vector2.zero;

        // เล่นแอนิเมชันเสาเพลิงระเบิดพลัง Phase2_Transition
        anim.SetTrigger("Phase2_Transition");

        // รอจนกว่าแอนิเมชันเปลี่ยนร่างจบ (ปรับเวลาให้ตรงกับคลิป Transition)
        yield return new WaitForSeconds(1.2f);

        isPhase2 = true;
        isTransitioning = false;

        // เร่งความเร็วแอนิเมชันทั้งหมดของบอสในเฟส 2
        anim.speed = phase2SpeedMultiplier;
        anim.SetBool("IsPhase2", true);
    }

    private void PerformMeleeAttack()
    {
        rb.linearVelocity = Vector2.zero;
        float cd = isPhase2 ? baseAttackCooldown / phase2SpeedMultiplier : baseAttackCooldown;
        nextActionTime = Time.time + cd;

        if (isPhase2)
        {
            // สุ่มระหว่างท่าฟันสายฟ้าครามรุนแรง หรือท่าปั่นดาบพายุเพลิง
            if (Random.value > 0.4f)
                anim.SetTrigger("Phase2_Attack_Enhanced");
            else
                anim.SetTrigger("Phase2_Sword_Spin");
        }
        else
        {
            anim.SetTrigger("Attack1");
        }
    }

    private void PerformRangedStorm()
    {
        rb.linearVelocity = Vector2.zero;
        nextActionTime = Time.time + (baseAttackCooldown / phase2SpeedMultiplier);
        anim.SetTrigger("Phase2_Ranged_Storm");
    }

    // เรียกใช้ผ่าน Animation Event ในเฟรมที่ 3 ของท่า Phase2_Ranged_Storm
    public void SpawnSoulfireStormEvent()
    {
        if (soulfireProjectilePrefab == null || firePoint == null) return;

        for (int i = -2; i <= 2; i++)
        {
            float angle = i * 10f;
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);
            Instantiate(soulfireProjectilePrefab, firePoint.position, rot);
        }
    }

    private void Die()
    {
        rb.linearVelocity = Vector2.zero;
        anim.speed = 1f;
        // เฟส 2 ตายด้วยการระเบิดเพลิงวิญญาณ (Phase2_Death_Explosive)
        anim.SetTrigger(isPhase2 ? "Phase2_Death_Explosive" : "Death");
        Destroy(gameObject, 2.0f);
    }
}
