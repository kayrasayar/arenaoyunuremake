using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Kamera Ayarları")]
    public Transform cameraPivot;
    public float mouseHassasiyet = 2f;
    public bool isAttack = false;

    private float yVelocity;
    private bool kosuyorMu;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

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

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && !isAttack)
        {
            animator?.SetTrigger("Attack");
            // Animasyonun tam vuruş anına gelmesi için 0.3 saniye bekle ve hasarı aç
            Invoke("HitboxAc", 0.3f);
            // 0.5 saniye sonra (vuruş bitince) hasarı geri kapat
            Invoke("HitboxKapat", 0.5f);
        }
    }

    void HitboxAc() => isAttack = true;
    void HitboxKapat() => isAttack = false;
    void Update()
    {
        // 🔥 SHIFT kontrolünü direkt buradan alıyoruz (bug yok)
        kosuyorMu = Keyboard.current.leftShiftKey.isPressed;

        Hareket();
        KameraKontrol();
        Animasyon();
    }

    void KameraKontrol()
    {
        float mouseX = lookInput.x * mouseHassasiyet;

        // sadece sağ-sol dön
        transform.Rotate(Vector3.up * mouseX);

        // kamera sabit
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
}