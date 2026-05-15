using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FinalBossFinale : MonoBehaviour
{
    public static FinalBossFinale Instance { get; private set; }

    [Header("Süreler")]
    public float yavasCekimSuresi = 1.4f;
    public float yavasCekimScale = 0.22f;
    public float metinAraligi = 2.1f;
    public float sonSahneBekleme = 2.5f;

    [Header("Metinler")]
    public string[] surprizMetinleri = new string[]
    {
        "Boss yere düştü...",
        "Ama arena birden kararıyor.",
        "Bir ses fısıldıyor: \"Henüz bitmedi, Kral.\"",
        "Gökyüzü yarılıyor. Gerçek final şimdi başlıyor!",
        "Tebrikler... Manisa artık senin."
    };

    private bool oynatiliyor;
    private Transform bossTransform;
    private Canvas overlayCanvas;
    private Image flashImage;
    private TextMeshProUGUI finaleText;
    private AudioSource audioSource;

    public static bool TryStartFinale(Transform olmusBoss = null)
    {
        if (GameProgressManager.Instance == null
            || GameProgressManager.Instance.currentDistrict != "Final")
        {
            return false;
        }

        if (SceneManager.GetActiveScene().name != "final")
        {
            return false;
        }

        if (Instance == null)
        {
            GameObject finaleObj = new GameObject("FinalBossFinale");
            Instance = finaleObj.AddComponent<FinalBossFinale>();
        }

        Instance.bossTransform = olmusBoss;
        Instance.StartCoroutine(Instance.OynatFinale());
        return true;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    IEnumerator OynatFinale()
    {
        if (oynatiliyor)
        {
            yield break;
        }

        oynatiliyor = true;
        OyuncuyuDondur();
        OlusturOverlay();
        CalFinalWinSesi();

        float oncekiTimeScale = Time.timeScale;
        Time.timeScale = yavasCekimScale;
        yield return new WaitForSecondsRealtime(yavasCekimSuresi);
        Time.timeScale = 1f;

        yield return StartCoroutine(FlaşVeTitreme(0.85f, new Color(1f, 0.15f, 0.1f, 0.75f)));
        if (bossTransform != null)
        {
            StartCoroutine(BossCanlanmaEfekti());
        }

        for (int i = 0; i < surprizMetinleri.Length; i++)
        {
            yield return StartCoroutine(MetniGoster(surprizMetinleri[i]));
            yield return new WaitForSeconds(metinAraligi);
        }

        yield return StartCoroutine(FlaşVeTitreme(0.6f, new Color(1f, 0.85f, 0.2f, 0.9f)));
        yield return new WaitForSecondsRealtime(sonSahneBekleme);

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.MarkFinalVictory();
        }

        TemizleOverlay();
        oynatiliyor = false;

        if (Instance == this)
        {
            Instance = null;
        }

        Destroy(gameObject);
        SceneManager.LoadScene("endscreen");
    }

    void CalFinalWinSesi()
    {
        AudioClip finalWin = Resources.Load<AudioClip>("finalwin");
        if (finalWin != null && audioSource != null)
        {
            audioSource.PlayOneShot(finalWin);
        }
    }

    void OyuncuyuDondur()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    void OlusturOverlay()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        GameObject canvasObj = new GameObject("FinalFinaleCanvas");
        canvasObj.transform.SetParent(transform);
        overlayCanvas = canvasObj.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 500;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject flashObj = new GameObject("Flash");
        flashObj.transform.SetParent(canvasObj.transform, false);
        RectTransform flashRt = flashObj.AddComponent<RectTransform>();
        flashRt.anchorMin = Vector2.zero;
        flashRt.anchorMax = Vector2.one;
        flashRt.offsetMin = Vector2.zero;
        flashRt.offsetMax = Vector2.zero;
        flashImage = flashObj.AddComponent<Image>();
        flashImage.color = new Color(0f, 0f, 0f, 0f);

        GameObject textObj = new GameObject("FinaleText");
        textObj.transform.SetParent(canvasObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.1f, 0.35f);
        textRt.anchorMax = new Vector2(0.9f, 0.65f);
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        finaleText = textObj.AddComponent<TextMeshProUGUI>();
        finaleText.alignment = TextAlignmentOptions.Center;
        finaleText.fontSize = 42f;
        finaleText.color = new Color(1f, 0.92f, 0.55f);
        finaleText.fontStyle = FontStyles.Bold;
        finaleText.text = "";
    }

    void TemizleOverlay()
    {
        if (overlayCanvas != null)
        {
            Destroy(overlayCanvas.gameObject);
            overlayCanvas = null;
            flashImage = null;
            finaleText = null;
        }
    }

    IEnumerator MetniGoster(string metin)
    {
        if (finaleText == null)
        {
            yield break;
        }

        finaleText.text = metin;
        finaleText.alpha = 0f;

        float sure = 0.55f;
        float t = 0f;
        while (t < sure)
        {
            t += Time.unscaledDeltaTime;
            finaleText.alpha = Mathf.Lerp(0f, 1f, t / sure);
            yield return null;
        }

        finaleText.alpha = 1f;
        KameraTitret(0.25f, 0.35f);
    }

    IEnumerator FlaşVeTitreme(float siddet, Color renk)
    {
        if (flashImage == null)
        {
            yield break;
        }

        flashImage.color = renk;
        KameraTitret(siddet, 0.45f);

        float t = 0f;
        while (t < 0.45f)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(renk.a, 0f, t / 0.45f);
            flashImage.color = new Color(renk.r, renk.g, renk.b, alpha);
            yield return null;
        }

        flashImage.color = new Color(renk.r, renk.g, renk.b, 0f);
    }

    IEnumerator BossCanlanmaEfekti()
    {
        if (bossTransform == null)
        {
            yield break;
        }

        Vector3 baslangicOlcek = bossTransform.localScale;
        float t = 0f;
        while (t < 1.2f)
        {
            t += Time.unscaledDeltaTime;
            float pulse = 1f + Mathf.Sin(t * 12f) * 0.08f;
            bossTransform.localScale = baslangicOlcek * pulse;
            yield return null;
        }

        bossTransform.localScale = baslangicOlcek * 1.35f;
        yield return new WaitForSecondsRealtime(0.35f);
        bossTransform.localScale = baslangicOlcek;
    }

    void KameraTitret(float siddet, float sure)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        StartCoroutine(KameraTitretCoroutine(cam.transform, siddet, sure));
    }

    IEnumerator KameraTitretCoroutine(Transform camTransform, float siddet, float sure)
    {
        Vector3 baslangic = camTransform.localPosition;
        float t = 0f;
        while (t < sure)
        {
            t += Time.unscaledDeltaTime;
            Vector3 offset = Random.insideUnitSphere * siddet;
            camTransform.localPosition = baslangic + offset;
            yield return null;
        }

        camTransform.localPosition = baslangic;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
