using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CheatManager : MonoBehaviour
{
    public static CheatManager Instance { get; private set; }

    [Header("Jetpack")]
    public float jetpackGucu = 7f;
    public float jetpackYercekimi = -6f;

    [Header("Silah")]
    public float silahMenzili = 80f;
    public int silahHasari = 75;

    public bool OlumsuzlukAktif { get; private set; }
    public bool TekAtmaAktif { get; private set; }
    public bool JetpackAktif { get; private set; }
    public bool BirinciSahisAktif { get; private set; }
    public bool SilahAktif { get; private set; }

    private GameObject panelRoot;
    private TMP_InputField kodInput;
    private TextMeshProUGUI durumText;
    private TextMeshProUGUI yardimText;
    public bool PanelAcikMi => panelAcik;
    private bool panelAcik;
    private GameObject silahGorseli;
    private AudioClip silahSesi;
    private AudioSource silahAudio;

    private static readonly Dictionary<string, string> KodTablosu = new Dictionary<string, string>
    {
        { "ILCELER", "TumIlceleriKazan" },
        { "TUMILCELER", "TumIlceleriKazan" },
        { "MANISA", "TumIlceleriKazan" },
        { "OLUMSUZ", "Olumsuzluk" },
        { "GOD", "Olumsuzluk" },
        { "TANRI", "Olumsuzluk" },
        { "TEKAT", "TekAtma" },
        { "ONEHIT", "TekAtma" },
        { "KILL", "TekAtma" },
        { "JETPACK", "Jetpack" },
        { "UCUS", "Jetpack" },
        { "FLY", "Jetpack" },
        { "FPS", "BirinciSahis" },
        { "BIRINCI", "BirinciSahis" },
        { "SAHIS", "BirinciSahis" },
        { "SILAH", "Silah" },
        { "KILIC", "Silah" },
        { "SWORD", "Silah" },
        { "KAPAT", "Kapat" },
        { "OFF", "Kapat" },
        { "YARDIM", "Yardim" },
        { "HELP", "Yardim" }
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OtomatikOlustur()
    {
        if (FindAnyObjectByType<CheatManager>() != null)
        {
            return;
        }

        GameObject obj = new GameObject("CheatManager");
        obj.AddComponent<CheatManager>();
        DontDestroyOnLoad(obj);
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

        if (panelRoot == null)
        {
            OlusturPanel();
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        silahSesi = Resources.Load<AudioClip>("silah");
        silahAudio = gameObject.AddComponent<AudioSource>();
        silahAudio.playOnAwake = false;
        silahAudio.spatialBlend = 0f;

        SceneManager.sceneLoaded += SahneYuklendi;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= SahneYuklendi;
    }

    void SahneYuklendi(Scene scene, LoadSceneMode mode)
    {
        if (panelRoot == null)
        {
            OlusturPanel();
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        if (scene.name == "final")
        {
            FinalSahneSesiniHazirla();
        }

        if (SilahAktif)
        {
            SilahGorseliniAyarla();
        }
    }

    void FinalSahneSesiniHazirla()
    {
        if (FindAnyObjectByType<FinalSceneEntryAudio>() != null)
        {
            return;
        }

        GameObject sesObj = new GameObject("FinalSceneEntryAudio");
        sesObj.AddComponent<FinalSceneEntryAudio>();
    }

    void Update()
    {
        if (PanelTusuBasildi())
        {
            if (GameSettingsManager.Instance != null && GameSettingsManager.Instance.PanelAcikMi)
            {
                return;
            }

            PanelAcKapa();
        }

        if (panelAcik && EnterBasildi())
        {
            KoduUygula();
        }
    }

    bool PanelTusuBasildi()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            return true;
        }

        if (Input.GetKeyDown(KeyCode.Quote) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            return true;
        }

        bool shiftBasili = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shiftBasili && Input.GetKeyDown(KeyCode.Alpha2))
        {
            return true;
        }

        if (Keyboard.current == null)
        {
            return false;
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            return true;
        }

        if (Keyboard.current.quoteKey.wasPressedThisFrame
            || Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            return true;
        }

        bool shift = Keyboard.current.shiftKey.isPressed
            || Keyboard.current.leftShiftKey.isPressed
            || Keyboard.current.rightShiftKey.isPressed;
        if (shift && Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    bool EnterBasildi()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            return true;
        }

        return Keyboard.current != null
            && (Keyboard.current.enterKey.wasPressedThisFrame
                || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
    }

    public void PanelAcKapa()
    {
        if (panelRoot == null)
        {
            OlusturPanel();
        }

        if (panelRoot == null)
        {
            Debug.LogError("CheatManager: Panel oluşturulamadı.");
            return;
        }

        panelAcik = !panelAcik;
        panelRoot.SetActive(panelAcik);

        if (panelAcik)
        {
            if (GameSettingsManager.Instance != null && GameSettingsManager.Instance.PanelAcikMi)
            {
                GameSettingsManager.Instance.PanelAcKapa();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (kodInput != null)
            {
                kodInput.ActivateInputField();
                kodInput.Select();
            }
        }
        else if (SceneManager.GetActiveScene().name != "home"
            && SceneManager.GetActiveScene().name != "worldscreen"
            && SceneManager.GetActiveScene().name != "winscreen"
            && SceneManager.GetActiveScene().name != "losescreen"
            && SceneManager.GetActiveScene().name != "endscreen")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void KoduUygula()
    {
        if (kodInput == null)
        {
            return;
        }

        string kod = kodInput.text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(kod))
        {
            MesajGoster("Kod yaz ve Enter'a bas.");
            return;
        }

        if (!KodTablosu.TryGetValue(kod, out string aksiyon))
        {
            MesajGoster("Bilinmeyen kod: " + kod);
            return;
        }

        switch (aksiyon)
        {
            case "TumIlceleriKazan":
                TumIlceleriKazan();
                break;
            case "Olumsuzluk":
                OlumsuzlukAktif = !OlumsuzlukAktif;
                MesajGoster(OlumsuzlukAktif ? "Ölümsüzlük AÇIK" : "Ölümsüzlük KAPALI");
                break;
            case "TekAtma":
                TekAtmaAktif = !TekAtmaAktif;
                MesajGoster(TekAtmaAktif ? "Tek atma AÇIK" : "Tek atma KAPALI");
                break;
            case "Jetpack":
                JetpackAktif = !JetpackAktif;
                MesajGoster(JetpackAktif ? "Jetpack AÇIK (Space)" : "Jetpack KAPALI");
                break;
            case "BirinciSahis":
                BirinciSahisAktif = !BirinciSahisAktif;
                MesajGoster(BirinciSahisAktif ? "1. şahıs AÇIK" : "1. şahıs KAPALI");
                break;
            case "Silah":
                SilahAktif = !SilahAktif;
                SilahGorseliniAyarla();
                MesajGoster(SilahAktif ? "Silah AÇIK (sol tık)" : "Silah KAPALI");
                break;
            case "Kapat":
                TumHileleriKapat();
                MesajGoster("Tüm hileler kapatıldı.");
                break;
            case "Yardim":
                MesajGoster(YardimMetni());
                break;
        }
    }

    void TumIlceleriKazan()
    {
        if (GameProgressManager.Instance == null)
        {
            MesajGoster("GameProgressManager yok.");
            return;
        }

        List<string> ilceler = GameProgressManager.Instance.GetRequiredDistrictNames();
        if (ilceler.Count == 0)
        {
            ilceler = VarsayilanIlceListesi();
        }

        foreach (string ilce in ilceler)
        {
            if (!GameProgressManager.Instance.completedDistricts.Contains(ilce))
            {
                GameProgressManager.Instance.completedDistricts.Add(ilce);
            }
        }

        GameProgressManager.Instance.needsTraining = false;
        PlayerPrefs.SetInt("NeedsTraining", 0);
        GameProgressManager.Instance.SaveProgress();

        if (SceneManager.GetActiveScene().name == "worldscreen")
        {
            MapCubeSpawner spawner = FindAnyObjectByType<MapCubeSpawner>();
            if (spawner != null)
            {
                spawner.HaritayiYenile();
            }
        }

        MesajGoster("Tüm ilçeler kazanıldı! Final açıldı.");
    }

    List<string> VarsayilanIlceListesi()
    {
        return new List<string>
        {
            "Köprübaşı", "Akhisar", "Demirci", "Kula",
            "Selendi", "Ahmetli", "Çırıkçı", "Turgutlu"
        };
    }

    void TumHileleriKapat()
    {
        OlumsuzlukAktif = false;
        TekAtmaAktif = false;
        JetpackAktif = false;
        BirinciSahisAktif = false;
        SilahAktif = false;
        SilahGorseliniAyarla();
    }

    public int GetSaldiriHasari(int normalHasar)
    {
        if (TekAtmaAktif)
        {
            return 99999;
        }

        return normalHasar;
    }

    public float GetYercekimi(float normalYercekimi)
    {
        return JetpackAktif ? jetpackYercekimi : normalYercekimi;
    }

    public bool JetpackUcuyorMu()
    {
        return JetpackAktif && Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
    }

    public float GetJetpackGucu()
    {
        return jetpackGucu;
    }

    void SilahGorseliniAyarla()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null)
        {
            return;
        }

        if (!SilahAktif)
        {
            if (silahGorseli != null)
            {
                silahGorseli.SetActive(false);
            }
            return;
        }

        if (silahGorseli == null || silahGorseli.transform.parent != player.transform)
        {
            if (silahGorseli != null)
            {
                Destroy(silahGorseli);
            }

            silahGorseli = GameObject.CreatePrimitive(PrimitiveType.Cube);
            silahGorseli.name = "CheatSilah";
            Destroy(silahGorseli.GetComponent<Collider>());
            silahGorseli.transform.SetParent(player.transform, false);
            silahGorseli.transform.localPosition = new Vector3(0.35f, 1.2f, 0.45f);
            silahGorseli.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            silahGorseli.transform.localScale = new Vector3(0.08f, 0.08f, 0.55f);
            Renderer r = silahGorseli.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = new Color(0.2f, 0.2f, 0.25f);
            }
        }

        silahGorseli.SetActive(true);
    }

    public void SilahAtesEt(PlayerController player)
    {
        if (!SilahAktif || player == null)
        {
            return;
        }

        if (silahSesi != null && silahAudio != null)
        {
            silahAudio.PlayOneShot(silahSesi);
        }

        Vector3 origin = player.transform.position + Vector3.up * 1.4f;
        Vector3 yon = player.transform.forward;
        int hasar = TekAtmaAktif ? 99999 : silahHasari;
        Vector3 hedefNokta = origin + yon * silahMenzili;

        if (Physics.Raycast(origin, yon, out RaycastHit hit, silahMenzili, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            hedefNokta = hit.point;
            Enemy enemy = hit.collider.GetComponent<Enemy>() ?? hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.HasarAl(hasar);
            }
        }

        StartCoroutine(KirmiziMermiAnimasyonu(origin, hedefNokta));
    }

    IEnumerator KirmiziMermiAnimasyonu(Vector3 baslangic, Vector3 bitis)
    {
        GameObject mermi = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mermi.name = "CheatMermi";
        Destroy(mermi.GetComponent<Collider>());
        mermi.transform.localScale = Vector3.one * 0.18f;

        Renderer renderer = mermi.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }

        float sure = 0.12f;
        float t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            mermi.transform.position = Vector3.Lerp(baslangic, bitis, t / sure);
            yield return null;
        }

        Destroy(mermi);
    }

    void MesajGoster(string mesaj)
    {
        if (durumText != null)
        {
            durumText.text = mesaj;
        }

        Debug.Log("[Hile] " + mesaj);
    }

    string YardimMetni()
    {
        return "ILCELER | OLUMSUZ | TEKAT | JETPACK | FPS | SILAH | KAPAT";
    }

    void OlusturPanel()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("CheatEventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(eventSystemObj);
        }
        else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        GameObject canvasObj = new GameObject("CheatCanvas");
        canvasObj.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasObj);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("CheatPanel");
        panelRoot.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRt = panelRoot.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(620f, 420f);
        Image panelBg = panelRoot.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.08f, 0.92f);

        durumText = OlusturMetin(panelRoot.transform, "Durum", 22, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.95f), Color.green);
        durumText.text = "Hile paneli — kod yaz ve Uygula'ya bas.";

        yardimText = OlusturMetin(panelRoot.transform, "Yardim", 16, new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.76f), new Color(0.85f, 0.85f, 0.9f));
        yardimText.text =
            "Kodlar:\n" +
            "ILCELER — tüm ilçeleri kazan\n" +
            "OLUMSUZ — ölümsüzlük\n" +
            "TEKAT — her şeye tek atma\n" +
            "JETPACK — Space ile uç\n" +
            "FPS — 1. şahıs kamera\n" +
            "SILAH — menzilli silah (sol tık)\n" +
            "KAPAT — hileleri kapat\n" +
            "YARDIM — listeyi göster\n\n" +
            "Panel: \" veya Shift+2 veya F9";

        kodInput = OlusturInput(panelRoot.transform);
        OlusturButon(panelRoot.transform, "Uygula", new Vector2(0.55f, 0.08f), KoduUygula);
        OlusturButon(panelRoot.transform, "Kapat", new Vector2(0.05f, 0.08f), PanelAcKapa);
    }

    TextMeshProUGUI OlusturMetin(Transform parent, string ad, int boyut, Vector2 min, Vector2 max, Color renk)
    {
        GameObject obj = new GameObject(ad);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        AtamaTmpFont(tmp);
        tmp.fontSize = boyut;
        tmp.color = renk;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        return tmp;
    }

    void AtamaTmpFont(TextMeshProUGUI tmp)
    {
        if (tmp == null)
        {
            return;
        }

        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }
    }

    TMP_InputField OlusturInput(Transform parent)
    {
        GameObject obj = new GameObject("KodInput");
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.22f);
        rt.anchorMax = new Vector2(0.95f, 0.36f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(10f, 5f);
        textRt.offsetMax = new Vector2(-10f, -5f);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        AtamaTmpFont(text);
        text.fontSize = 24;
        text.color = Color.white;

        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(obj.transform, false);
        RectTransform phRt = placeholderObj.AddComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = new Vector2(10f, 5f);
        phRt.offsetMax = new Vector2(-10f, -5f);
        TextMeshProUGUI placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
        AtamaTmpFont(placeholder);
        placeholder.text = "Hile kodu yaz...";
        placeholder.fontSize = 22;
        placeholder.color = new Color(0.6f, 0.6f, 0.65f);

        TMP_InputField input = obj.AddComponent<TMP_InputField>();
        input.textComponent = text;
        input.placeholder = placeholder;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterValidation = TMP_InputField.CharacterValidation.None;
        return input;
    }

    void OlusturButon(Transform parent, string yazi, Vector2 anchorMin, UnityEngine.Events.UnityAction aksiyon)
    {
        GameObject obj = new GameObject(yazi + "Btn");
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMin + new Vector2(0.4f, 0.12f);
        rt.sizeDelta = Vector2.zero;
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.25f, 0.45f, 0.75f, 1f);
        Button btn = obj.AddComponent<Button>();
        btn.onClick.AddListener(aksiyon);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        AtamaTmpFont(tmp);
        tmp.text = yazi;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }
}
