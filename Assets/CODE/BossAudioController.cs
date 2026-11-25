using UnityEngine;

public class BossAudioController : MonoBehaviour
{
    [Header("แหล่งกำเนิดเสียง")]
    public AudioSource audioSource;

    [Header("ไฟล์เสียงของบอส (SFX)")]
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip phase2Clip;
    public AudioClip skillClip;
    public AudioClip deathClip;

    [Header("ตั้งค่าระดับเสียง")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0.8f, 1.2f)] public float minPitch = 0.96f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.04f;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, sfxVolume);
        }
    }

    // เรียกได้ทั้งจากสคริปต์อื่น และจาก Animation Event ในคลิปอนิเมชัน
    public void PlayAttack() => PlayClip(attackClip);
    public void PlayHurt() => PlayClip(hurtClip);
    public void PlayPhase2() => PlayClip(phase2Clip);
    public void PlaySkill() => PlayClip(skillClip);
    public void PlayDeath() => PlayClip(deathClip);
}