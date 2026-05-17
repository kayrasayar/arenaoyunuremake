using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // ✅ EKLENDİ
using UnityEngine.SceneManagement; // ✅ EKLENDİ

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;
    private AudioSource footstepSource;
    private float footstepTimer = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;

    [Header("Hareket Ayarları")]
    public float hiz = 5f;
    public float kosmaCarpani = 2f;
    public float yercekimi = -20f;
    public float ziplaGucu = 1.5f;

    [Header("Can Ayarları")]
    public int maxCan = 100;
    public int mevcutCan; // ✅ EKLENDİ
    public Image canBarıGorseli; // ✅ EKLENDİ

    [Header("Sesler")]
    public AudioClip ouchClip;
    public AudioClip dieClip;
    public AudioClip footstepClip;

    [Header("Hasar Ayarları")]
    public int baseHasar = 10; // ✅ EKLENDİ
    public Transform attackPoint;
    public float attackRadius = 4f;
    public LayerMask enemyLayers;
    public Hitbox hitbox;

    [Header("Kamera Ayarları")]
    public Transform cameraPivot;
    public Transform cameraRig;
    public float mouseHassasiyet = 2f;
    [Tooltip("Karakter omzundaki dönme ekseni (yerel konum).")]
    public Vector3 kameraOrbitPivotYerel = new Vector3(0f, 1.55f, 0f);
    [Tooltip("Dikey bakış: eksen etrafında dönme hızı (derece).")]
    public float dikeyBakisHassasiyeti = 0.18f;
    [Tooltip("Aşağı bakış limiti (derece). 180° dönüş olmaz.")]
    public float minPitchAcisi = -38f;
    [Tooltip("Yukarı bakış limiti (derece).")]
    public float maxPitchAcisi = 34f;
    public float kameraCarpismaYaricapi = 0.25f;
    public float kameraCarpismaMesafesi = 0.15f;
    public LayerMask kameraEngelKatmanlari = ~0;
    public bool isAttack = false;
    public float attackCooldown = 0.5f; // ✅ COOLDOWN EKLENDİ
    private float lastAttackTime; // ✅ COOLDOWN İÇİN

    private float yVelocity;
    private bool kosuyorMu;
    public bool Die = false;
    public bool isDead = false;
    public bool isDefending = false;
    public float defansSure = 1.5f;

    private int baseMaxCan = 100;
    private float sonrakiDefansZamani;
    private float varsayilanDefansSure;

    private Vector3 varsayilanKameraRigKonumu;
    private Vector3 varsayilanOrbitKolu;
    private float kameraPitchAcisi;
    private float birinciSahisPitch;
    private int oyuncuKatmani;
    private float varsayilanYercekimi;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;

        if (ouchClip == null)
        {
            ouchClip = Resources.Load<AudioClip>("ouch");
        }

        if (dieClip == null)
        {
            dieClip = Resources.Load<AudioClip>("die");
        }

        if (footstepClip == null)
        {
            footstepClip = Resources.Load<AudioClip>("footstep");
        }

        mevcutCan = maxCan; // ✅ EKLENDİ
        CanBarıGuncelle(); // ✅ EKLENDİ
        ApplyLevelStats(); // ✅ EKLENDİ

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (hitbox == null)
        {
            hitbox = GetComponentInChildren<Hitbox>();
        }

        oyuncuKatmani = gameObject.layer;
        varsayilanYercekimi = yercekimi;
        varsayilanDefansSure = defansSure;
        GuncelleDefansAyarlarini();
        KameraRiginiHazirla();
    }

    void GuncelleDefansAyarlarini()
    {
        defansSure = GameSettingsManager.GetDefenseDuration();
    }

    void KameraRiginiHazirla()
    {
        if (cameraPivot == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraPivot = mainCam.transform;
            }
        }

        if (cameraRig == null && cameraPivot != null)
        {
            cameraRig = cameraPivot.parent;
        }

        if (cameraRig == null)
        {
            return;
        }

        varsayilanKameraRigKonumu = cameraRig.localPosition;
        varsayilanOrbitKolu = varsayilanKameraRigKonumu - kameraOrbitPivotYerel;
        kameraPitchAcisi = 0f;

        cameraRig.localRotation = Quaternion.identity;
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.identity;
        }
    }

    // INPUT
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();

    public void OnJump(InputValue value)
    {
        if (value.isPressed && controller.isGrounded)
        {
            yVelocity = Mathf.Sqrt(ziplaGucu * -2f * yercekimi);
            animator?.SetTrigger("Jump");
        }
    }

    // ✅ DÜZELTİLDİ
    public void HasarAl(int hasarMiktari)
    {
        if (isDead)
            return;

        if (CheatManager.Instance != null && CheatManager.Instance.OlumsuzlukAktif)
        {
            mevcutCan = maxCan;
            CanBarıGuncelle();
            return;
        }

        if (isDefending)
        {
            Debug.Log("Player defans yaptı, hasar engellendi.");
            return;
        }

        mevcutCan -= hasarMiktari;

        Debug.Log("Player hasar aldı: " + hasarMiktari);

        if (audioSource != null && ouchClip != null)
        {
            audioSource.PlayOneShot(ouchClip);
        }

        CanBarıGuncelle();

        if (mevcutCan <= 0)
        {
            isDead = true;
            Die = true;
            if (animator != null)
            {
                animator.SetBool("Die", true);
            }

            if (audioSource != null && dieClip != null)
            {
                audioSource.PlayOneShot(dieClip);
            }

            // Kaybetme: 4 saniye ölüm animasyonu, sonra XP kaybı
            StartCoroutine(OyuncuOlmeCoroutine());
        }
    }

    public void OnDefense(InputValue value)
    {
        if (value.isPressed)
        {
            StartDefense();
        }
    }

    void DefansKapat()
    {
        isDefending = false;
        sonrakiDefansZamani = Time.time + GameSettingsManager.GetDefenseReuseCooldown();
    }

    // ✅ CAN BAR GÜNCELLEME
    void CanBarıGuncelle()
    {
        if (canBarıGorseli != null)
        {
            canBarıGorseli.fillAmount = (float)mevcutCan / maxCan;
        }
    }

    void TriggerAttack()
    {
        if (isAttack || Time.time - lastAttackTime < attackCooldown) return; // ✅ COOLDOWN KONTROLÜ

        lastAttackTime = Time.time; // ✅ COOLDOWN BAŞLAT
        animator?.SetTrigger("Attack"); // ✅ SADECE TRIGGER KULLAN
        isAttack = true;
        hitbox?.ResetHit();
        Invoke("HitboxKapat", 0.5f);
        Invoke("PerformAttack", 0.3f);
    }

    void HitboxKapat() => isAttack = false;

    void PerformAttack()
    {
        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position + transform.forward * (attackRadius * 0.5f);
        LayerMask mask = enemyLayers.value == 0 ? Physics.AllLayers : enemyLayers;
        Collider[] hits = Physics.OverlapSphere(origin, attackRadius, mask, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>() ?? hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                int hasar = baseHasar;
                if (CheatManager.Instance != null)
                {
                    hasar = CheatManager.Instance.GetSaldiriHasari(hasar);
                }

                enemy.HasarAl(hasar);
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        if ((CheatManager.Instance != null && CheatManager.Instance.PanelAcikMi)
            || (GameSettingsManager.Instance != null && GameSettingsManager.Instance.PanelAcikMi))
        {
            return;
        }

        kosuyorMu = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        bool silahModu = CheatManager.Instance != null && CheatManager.Instance.SilahAktif;

        if (!isDefending && !silahModu && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            StartDefense();
        }

        if (!isDead && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (silahModu)
            {
                CheatManager.Instance.SilahAtesEt(this);
            }
            else
            {
                TriggerAttack();
            }
        }

        Hareket();
        KameraKontrol();
        Animasyon();
    }

    void StartDefense()
    {
        if (isDefending || isDead || Time.time < sonrakiDefansZamani)
        {
            return;
        }

        GuncelleDefansAyarlarini();
        isDefending = true;
        animator?.SetTrigger("Def");
        CancelInvoke(nameof(DefansKapat));
        Invoke(nameof(DefansKapat), defansSure);
    }

    void KameraKontrol()
    {
        if (CheatManager.Instance != null && CheatManager.Instance.BirinciSahisAktif)
        {
            BirinciSahisKamera();
            return;
        }

        float mouseX = lookInput.x * mouseHassasiyet;
        transform.Rotate(Vector3.up * mouseX);

        if (cameraRig == null)
        {
            return;
        }

        float mouseY = lookInput.y * dikeyBakisHassasiyeti;
        kameraPitchAcisi = Mathf.Clamp(kameraPitchAcisi - mouseY, minPitchAcisi, maxPitchAcisi);
        OrbitKamerayiUygula();
    }

    void BirinciSahisKamera()
    {
        float mouseX = lookInput.x * mouseHassasiyet;
        float mouseY = lookInput.y * dikeyBakisHassasiyeti;
        transform.Rotate(Vector3.up * mouseX);

        if (cameraRig == null)
        {
            return;
        }

        birinciSahisPitch = Mathf.Clamp(birinciSahisPitch - mouseY, -85f, 85f);
        cameraRig.localPosition = new Vector3(0f, 1.65f, 0.12f);
        cameraRig.localRotation = Quaternion.identity;

        if (cameraPivot != null)
        {
            cameraPivot.localPosition = Vector3.zero;
            cameraPivot.localRotation = Quaternion.Euler(birinciSahisPitch, 0f, 0f);
        }
    }

    void OrbitKamerayiUygula()
    {
        Quaternion pitchRotasyonu = Quaternion.AngleAxis(kameraPitchAcisi, Vector3.right);
        Vector3 donmusKol = pitchRotasyonu * varsayilanOrbitKolu;
        cameraRig.localPosition = kameraOrbitPivotYerel + donmusKol;

        Transform kameraTransformu = cameraPivot != null ? cameraPivot : cameraRig;
        Vector3 orbitPivotDunya = transform.TransformPoint(kameraOrbitPivotYerel);
        Vector3 kameraDunyaKonumu = kameraTransformu.position;

        kameraTransformu.LookAt(orbitPivotDunya);

        Vector3 yon = kameraDunyaKonumu - orbitPivotDunya;
        float mesafe = yon.magnitude;
        if (mesafe > 0.01f)
        {
            int engelMaskesi = kameraEngelKatmanlari.value & ~(1 << oyuncuKatmani);
            if (Physics.SphereCast(
                    orbitPivotDunya,
                    kameraCarpismaYaricapi,
                    yon.normalized,
                    out RaycastHit carpisma,
                    mesafe,
                    engelMaskesi,
                    QueryTriggerInteraction.Ignore))
            {
                Vector3 guvenliKonum = carpisma.point - yon.normalized * kameraCarpismaMesafesi;
                cameraRig.position = guvenliKonum;
                kameraTransformu.LookAt(orbitPivotDunya);
            }
        }
    }

    void Hareket()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        float aktifYercekimi = yercekimi;
        if (CheatManager.Instance != null)
        {
            aktifYercekimi = CheatManager.Instance.GetYercekimi(varsayilanYercekimi);
        }

        if (controller.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        if (CheatManager.Instance != null && CheatManager.Instance.JetpackUcuyorMu())
        {
            yVelocity = CheatManager.Instance.GetJetpackGucu();
        }
        else
        {
            yVelocity += aktifYercekimi * Time.deltaTime;
        }

        float aktifHiz = kosuyorMu ? hiz * kosmaCarpani : hiz;

        Vector3 finalMove = move * aktifHiz;
        finalMove.y = yVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    void Animasyon()
    {
        if (animator == null) return;

        float rawSpeed = moveInput.magnitude;

        float finalSpeed = 0f;

        if (rawSpeed > 0.05f)
        {
            finalSpeed = kosuyorMu ? 1.5f : 1f;
        }

        animator.SetFloat("Speed", finalSpeed, 0.1f, Time.deltaTime);
        animator.SetBool("Walk", finalSpeed > 0f);
        animator.SetBool("Run", kosuyorMu && rawSpeed > 0.1f);

        // Footstep sound with 1 second delay
        if (finalSpeed > 0f)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= 1f && footstepClip != null && footstepSource != null && !footstepSource.isPlaying)
            {
                footstepSource.clip = footstepClip;
                footstepSource.Play();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
            if (footstepSource != null && footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }

    // ✅ EKLENDİ: Seviyeye göre karakteri güçlendir
    public void ApplyLevelStats()
    {
        if (GameProgressManager.Instance == null) return;

        int level = GameProgressManager.Instance.currentLevel;
        float levelMultiplier = (float)level / GameProgressManager.Instance.maxLevel;

        // Karakterin boyutunu artır (1x - 2x)
        float scaleMultiplier = 1.0f + levelMultiplier * 1.0f;
        transform.localScale = Vector3.one * scaleMultiplier;

        // Hasar artır (10 - 25)
        baseHasar = Mathf.RoundToInt(10 + levelMultiplier * 15f);

        // Can artır (100 - 300)
        baseMaxCan = Mathf.RoundToInt(100 + levelMultiplier * 200f);
        maxCan = baseMaxCan;
        mevcutCan = maxCan;
        CanBarıGuncelle();

        Debug.Log("Player güçlendirildi - Seviye: " + level + " | Scale: " + scaleMultiplier + " | Hasar: " + baseHasar + " | Max Can: " + maxCan);
    }

    IEnumerator OyuncuOlmeCoroutine()
    {
        Debug.Log("Oyuncu öldü!");
        if (controller != null)
        {
            controller.enabled = false;
        }
        yield return new WaitForSeconds(4f);
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.LoseGame();
        }
        else
        {
            SceneManager.LoadScene("losescreen");
        }
    }
}