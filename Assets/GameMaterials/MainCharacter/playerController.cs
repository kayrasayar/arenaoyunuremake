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

    [Header("Kamera Ayarları")]
    public Transform cameraPivot;
    public float mouseHassasiyet = 2f;
    public bool isAttack = false;

    private float yVelocity;
    private bool kosuyorMu;
    public bool Die = false;
    public bool isDead = false;
    public bool isDefending = false;
    public float defansSure = 1.5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        mevcutCan = maxCan; // ✅ EKLENDİ
        CanBarıGuncelle(); // ✅ EKLENDİ

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        if (isDefending)
        {
            Debug.Log("Player defans yaptı, hasar engellendi.");
            return;
        }

        mevcutCan -= hasarMiktari;

        Debug.Log("Player hasar aldı: " + hasarMiktari);

        CanBarıGuncelle();

        if (mevcutCan <= 0)
        {
            isDead = true;
            Die = true;
            if (animator != null)
            {
                animator.SetBool("Die", true);
            }
            // Kaybetme: XP kaybı
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.LoseGame();
            }
            else
            {
                StartCoroutine(OyuncuOlmeCoroutine());
            }
        }
    }

    public void OnDefense(InputValue value)
    {
        if (value.isPressed && !isDefending && !isDead)
        {
            isDefending = true;
            animator?.SetTrigger("Def");
            Invoke(nameof(DefansKapat), defansSure);
        }
    }

    void DefansKapat() => isDefending = false;

    // ✅ EKLENDİ
    void CanBarıGuncelle()
    {
        if (canBarıGorseli != null)
        {
            canBarıGorseli.fillAmount = (float)mevcutCan / maxCan;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (isDead) return;

        if (value.isPressed && !isAttack)
        {
            animator?.SetTrigger("Attack");
            Invoke("HitboxAc", 0.3f);
            Invoke("HitboxKapat", 0.5f);
        }
    }

    void HitboxAc() => isAttack = true;
    void HitboxKapat() => isAttack = false;

    void Update()
    {
        if (isDead) return;

        kosuyorMu = Keyboard.current.leftShiftKey.isPressed;

        if (!isDefending && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            StartDefense();
        }

        Hareket();
        KameraKontrol();
        Animasyon();
    }

    void StartDefense()
    {
        if (isDefending || isDead) return;

        isDefending = true;
        animator?.SetTrigger("Def");
        Invoke(nameof(DefansKapat), defansSure);
    }

    void KameraKontrol()
    {
        float mouseX = lookInput.x * mouseHassasiyet;

        transform.Rotate(Vector3.up * mouseX);

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    void Hareket()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += yercekimi * Time.deltaTime;

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
    }

    IEnumerator OyuncuOlmeCoroutine()
    {
        Debug.Log("Oyuncu öldü!");
        if (controller != null)
        {
            controller.enabled = false;
        }
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("losescreen");
    }
}