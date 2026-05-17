using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    const string PrefMaster = "Settings_MasterVolume";
    const string PrefSfx = "Settings_SfxVolume";
    const string PrefMusic = "Settings_MusicVolume";
    const string PrefFullscreen = "Settings_Fullscreen";
    const string PrefDifficulty = "Settings_Difficulty";

    public float masterVolume = 1f;
    public float sfxVolume = 1f;
    public float musicVolume = 0.7f;
    public bool fullscreen = true;
    public GameDifficulty difficulty = GameDifficulty.Kolay;

    public bool PanelAcikMi => panelAcik;

    GameObject panelRoot;
    Slider masterSlider;
    Slider sfxSlider;
    Slider musicSlider;
    Toggle fullscreenToggle;
    Button[] zorlukButonlari;
    bool panelAcik;
    float oncekiTimeScale = 1f;

    static readonly string[] ZorlukEtiketleri = { "Kolay", "Zor", "Kabus" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OtomatikOlustur()
    {
        if (FindAnyObjectByType<GameSettingsManager>() != null)
        {
            return;
        }

        GameObject obj = new GameObject("GameSettingsManager");
        obj.AddComponent<GameSettingsManager>();
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
        Yukle();
        OlusturPanel();
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

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

        if (panelAcik)
        {
            PanelKapat();
        }
    }

    void Update()
    {
        if (EscBasildi())
        {
            PanelAcKapa();
        }
    }

    bool EscBasildi()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            return true;
        }

        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    public void PanelAcKapa()
    {
        if (panelRoot == null)
        {
            OlusturPanel();
        }

        if (panelAcik)
        {
            PanelKapat();
        }
        else
        {
            if (CheatManager.Instance != null && CheatManager.Instance.PanelAcikMi)
            {
                CheatManager.Instance.PanelAcKapa();
            }

            panelAcik = true;
            panelRoot.SetActive(true);
            oncekiTimeScale = Time.timeScale;
            if (OyunSahnesiMi())
            {
                Time.timeScale = 0f;
            }

            GuncelleUiDegerleri();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void PanelKapat()
    {
        panelAcik = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        Time.timeScale = oncekiTimeScale > 0f ? oncekiTimeScale : 1f;

        if (OyunSahnesiMi())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    static bool OyunSahnesiMi()
    {
        string ad = SceneManager.GetActiveScene().name;
        return ad == "arena" || ad == "talimalani" || ad == "final";
    }

    public void AnaMenuyeDon()
    {
        Kaydet();
        Time.timeScale = 1f;
        panelAcik = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        SceneManager.LoadScene("home");
    }

    public void YenidenBaslat()
    {
        Kaydet();
        Time.timeScale = 1f;
        panelAcik = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Kaydet()
    {
        PlayerPrefs.SetFloat(PrefMaster, masterVolume);
        PlayerPrefs.SetFloat(PrefSfx, sfxVolume);
        PlayerPrefs.SetFloat(PrefMusic, musicVolume);
        PlayerPrefs.SetInt(PrefFullscreen, fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(PrefDifficulty, (int)difficulty);
        PlayerPrefs.Save();
        UygulaSesVeEkran();
    }

    void Yukle()
    {
        masterVolume = PlayerPrefs.GetFloat(PrefMaster, 1f);
        sfxVolume = PlayerPrefs.GetFloat(PrefSfx, 1f);
        musicVolume = PlayerPrefs.GetFloat(PrefMusic, 0.7f);
        fullscreen = PlayerPrefs.GetInt(PrefFullscreen, 1) == 1;
        difficulty = (GameDifficulty)Mathf.Clamp(PlayerPrefs.GetInt(PrefDifficulty, 0), 0, 2);
        UygulaSesVeEkran();

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.UpdateEnemyStats();
        }
    }

    void UygulaSesVeEkran()
    {
        AudioListener.volume = masterVolume;
        SesKaynaklariniGuncelle();

        Screen.fullScreen = fullscreen;
        if (fullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    void SesKaynaklariniGuncelle()
    {
        AudioSource[] kaynaklar = FindObjectsByType<AudioSource>(FindObjectsInactive.Include);
        foreach (AudioSource kaynak in kaynaklar)
        {
            if (kaynak == null)
            {
                continue;
            }

            bool muzik = kaynak.gameObject.CompareTag("Music")
                || kaynak.gameObject.name.IndexOf("music", StringComparison.OrdinalIgnoreCase) >= 0;
            kaynak.volume = Mathf.Clamp01(muzik ? musicVolume : sfxVolume);
        }
    }

    public float GetEffectiveSfxVolume()
    {
        return masterVolume * sfxVolume;
    }

    public static float GetDifficultyHealthMultiplier()
    {
        if (Instance == null) return 1f;
        switch (Instance.difficulty)
        {
            case GameDifficulty.Zor: return 2.5f;
            case GameDifficulty.Kabus: return 2.5f;
            default: return 1f;
        }
    }

    public static float GetDifficultyDamageMultiplier()
    {
        if (Instance == null) return 1f;
        switch (Instance.difficulty)
        {
            case GameDifficulty.Zor: return 2.5f;
            case GameDifficulty.Kabus: return 2.5f;
            default: return 1f;
        }
    }

    public static float GetDifficultySpeedMultiplier()
    {
        if (Instance == null) return 1f;
        switch (Instance.difficulty)
        {
            case GameDifficulty.Zor: return 2.5f;
            case GameDifficulty.Kabus: return 2.5f;
            default: return 1f;
        }
    }

    public static float GetEnemyAttackCooldownMultiplier()
    {
        if (Instance == null || Instance.difficulty == GameDifficulty.Kolay) return 1f;
        return 1.6f;
    }

    public static float GetDefenseDuration()
    {
        if (Instance == null || Instance.difficulty == GameDifficulty.Kolay) return 1.5f;
        return 1.1f;
    }

    public static float GetDefenseReuseCooldown()
    {
        if (Instance == null || Instance.difficulty == GameDifficulty.Kolay) return 0.35f;
        if (Instance.difficulty == GameDifficulty.Zor) return 3.5f;
        return 4.5f;
    }

    public static bool IsNightmareMode()
    {
        return Instance != null && Instance.difficulty == GameDifficulty.Kabus;
    }

    public static int GetNightmareEnemyCount(int seed)
    {
        System.Random rng = new System.Random(seed);
        return rng.Next(5, 11);
    }

    void GuncelleUiDegerleri()
    {
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(masterVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfxVolume);
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(musicVolume);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        GuncelleZorlukButonRenkleri();
    }

    void GuncelleZorlukButonRenkleri()
    {
        if (zorlukButonlari == null) return;

        for (int i = 0; i < zorlukButonlari.Length; i++)
        {
            if (zorlukButonlari[i] == null) continue;

            Image img = zorlukButonlari[i].GetComponent<Image>();
            if (img == null) continue;

            bool secili = (int)difficulty == i;
            // Aktifken daha canlı bir mavi, pasifken koyu, arka plana uyumlu gri-mavi
            img.color = secili
                ? new Color(0.25f, 0.55f, 0.85f, 1f) 
                : new Color(0.12f, 0.15f, 0.22f, 1f);
        }
    }

    void ZorlukSec(int index)
    {
        difficulty = (GameDifficulty)Mathf.Clamp(index, 0, 2);
        Kaydet();
        GuncelleZorlukButonRenkleri();

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.UpdateEnemyStats();
        }

        if (SceneManager.GetActiveScene().name == "arena" && ArenaDistrictVariator.Instance != null)
        {
            ArenaDistrictVariator.Instance.KabusDusmanlariniYenile();
        }
    }

    void OlusturPanel()
    {
        if (panelRoot != null) return;

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("SettingsEventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(eventSystemObj);
        }

        GameObject canvasObj = new GameObject("SettingsCanvas");
        canvasObj.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasObj);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1900;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("SettingsPanel");
        panelRoot.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRt = panelRoot.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        // Paneli daha yatay ve zarif bir boyuta getirdik
        panelRt.sizeDelta = new Vector2(850f, 650f);
        Image panelBg = panelRoot.AddComponent<Image>();
        // Modern, hafif saydam koyu lacivert/siyah arka plan
        panelBg.color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
        panelBg.raycastTarget = true;

        var baslik = OlusturBaslik(panelRoot.transform, "AYARLAR", new Vector2(0.0f, 0.88f), new Vector2(1.0f, 0.98f));
        baslik.fontSize = 38;
        baslik.color = new Color(0.9f, 0.9f, 0.95f, 1f); // Daha temiz bir beyaz

        float y = 0.78f;
        float satirBoslugu = 0.11f;

        masterSlider = OlusturSliderSatir(panelRoot.transform, "Ana Ses", y, v => { masterVolume = v; UygulaSesVeEkran(); });
        y -= satirBoslugu;
        sfxSlider = OlusturSliderSatir(panelRoot.transform, "Oyun Sesleri", y, v => { sfxVolume = v; UygulaSesVeEkran(); });
        y -= satirBoslugu;
        musicSlider = OlusturSliderSatir(panelRoot.transform, "Müzik", y, v => { musicVolume = v; UygulaSesVeEkran(); });
        y -= satirBoslugu;

        fullscreenToggle = OlusturToggleSatir(panelRoot.transform, "Tam Ekran", y, aktif =>
        {
            fullscreen = aktif;
            UygulaSesVeEkran();
        });
        
        y -= 0.13f; // Zorluk öncesi biraz daha fazla boşluk
        zorlukButonlari = OlusturZorlukButonlari(panelRoot.transform, y);

        // Alt butonları daha şık ve dengeli renklere çektik
        float altButonY = 0.08f;
        OlusturButon(panelRoot.transform, "Ana Menü", new Vector2(0.08f, altButonY), AnaMenuyeDon, new Color(0.2f, 0.25f, 0.32f, 1f));
        OlusturButon(panelRoot.transform, "Yeniden Başlat", new Vector2(0.38f, altButonY), YenidenBaslat, new Color(0.18f, 0.5f, 0.35f, 1f));
        OlusturButon(panelRoot.transform, "Kaydet & Çık (ESC)", new Vector2(0.68f, altButonY), PanelAcKapa, new Color(0.75f, 0.35f, 0.15f, 1f));
    }

    Button[] OlusturZorlukButonlari(Transform parent, float anchorY)
    {
        var zorlukLabel = OlusturMetin(parent, "ZorlukLabel", 22, new Vector2(0.0f, anchorY + 0.03f), new Vector2(1.0f, anchorY + 0.08f), new Color(0.7f, 0.75f, 0.8f));
        zorlukLabel.text = "ZORLUK SEVİYESİ";
        zorlukLabel.fontStyle = TMPro.FontStyles.Bold;
        zorlukLabel.alignment = TMPro.TextAlignmentOptions.Center;

        Button[] butonlar = new Button[ZorlukEtiketleri.Length];
        
        // Butonları tam ortaya hizalamak için X eksenindeki pozisyonları
        float[] xPositions = { 0.25f, 0.5f, 0.75f }; 

        for (int i = 0; i < ZorlukEtiketleri.Length; i++)
        {
            int index = i;
            butonlar[i] = OlusturZorlukButonu(parent, ZorlukEtiketleri[i], new Vector2(xPositions[i], anchorY - 0.08f), () => ZorlukSec(index));
        }

        return butonlar;
    }

    Button OlusturZorlukButonu(Transform parent, string yazi, Vector2 merkezAnchor, UnityEngine.Events.UnityAction aksiyon)
    {
        GameObject obj = new GameObject(yazi + "ZorlukBtn");
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = merkezAnchor;
        rt.anchorMax = merkezAnchor;
        // Kare yerine daha yatay, modern buton boyutu
        rt.sizeDelta = new Vector2(160f, 55f); 

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.12f, 0.15f, 0.22f, 1f);
        img.raycastTarget = true;

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
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.9f, 0.9f, 0.95f);
        tmp.fontStyle = TMPro.FontStyles.Bold;

        return btn;
    }

    Toggle OlusturToggleSatir(Transform parent, string etiket, float anchorY, Action<bool> degisti)
    {
        OlusturMetin(parent, etiket + "Label", 22, new Vector2(0.1f, anchorY), new Vector2(0.35f, anchorY + 0.05f), Color.white).text = etiket;

        GameObject obj = new GameObject(etiket + "Toggle");
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        // Küçük, şık bir kare checkbox
        rt.anchorMin = new Vector2(0.4f, anchorY);
        rt.anchorMax = new Vector2(0.4f, anchorY);
        rt.sizeDelta = new Vector2(36f, 36f);

        Toggle toggle = obj.AddComponent<Toggle>();
        toggle.isOn = fullscreen;

        Image arka = obj.AddComponent<Image>();
        arka.color = new Color(0.12f, 0.15f, 0.20f, 1f);
        toggle.targetGraphic = arka;

        GameObject check = new GameObject("Checkmark");
        check.transform.SetParent(obj.transform, false);
        RectTransform checkRt = check.AddComponent<RectTransform>();
        checkRt.anchorMin = Vector2.zero;
        checkRt.anchorMax = Vector2.one;
        checkRt.offsetMin = new Vector2(6f, 6f);
        checkRt.offsetMax = new Vector2(-6f, -6f);
        
        Image checkImg = check.AddComponent<Image>();
        checkImg.color = new Color(0.35f, 0.75f, 0.45f, 1f);
        toggle.graphic = checkImg;

        toggle.onValueChanged.AddListener(v => degisti(v));
        return toggle;
    }

    Slider OlusturSliderSatir(Transform parent, string etiket, float anchorY, Action<float> degisti)
    {
        OlusturMetin(parent, etiket + "Label", 22, new Vector2(0.1f, anchorY), new Vector2(0.35f, anchorY + 0.05f), Color.white).text = etiket;

        GameObject obj = new GameObject(etiket + "Slider");
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        
        // Slider alanını incelttik
        rt.anchorMin = new Vector2(0.4f, anchorY);
        rt.anchorMax = new Vector2(0.9f, anchorY + 0.05f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Slider slider = obj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.onValueChanged.AddListener(v => degisti(v));

        GameObject arka = new GameObject("Background");
        arka.transform.SetParent(obj.transform, false);
        RectTransform arkaRt = arka.AddComponent<RectTransform>();
        arkaRt.anchorMin = new Vector2(0, 0.3f); // Sadece orta kısmı kaplasın (ince çizgi)
        arkaRt.anchorMax = new Vector2(1, 0.7f);
        arkaRt.offsetMin = Vector2.zero;
        arkaRt.offsetMax = Vector2.zero;
        Image arkaImg = arka.AddComponent<Image>();
        arkaImg.color = new Color(0.12f, 0.15f, 0.20f, 1f);

        GameObject dolguAlani = new GameObject("Fill Area");
        dolguAlani.transform.SetParent(obj.transform, false);
        RectTransform dolguAlaniRt = dolguAlani.AddComponent<RectTransform>();
        dolguAlaniRt.anchorMin = new Vector2(0, 0.3f);
        dolguAlaniRt.anchorMax = new Vector2(1, 0.7f);
        dolguAlaniRt.offsetMin = Vector2.zero;
        dolguAlaniRt.offsetMax = Vector2.zero;

        GameObject dolgu = new GameObject("Fill");
        dolgu.transform.SetParent(dolguAlani.transform, false);
        RectTransform dolguRt = dolgu.AddComponent<RectTransform>();
        dolguRt.anchorMin = Vector2.zero;
        dolguRt.anchorMax = Vector2.one;
        dolguRt.offsetMin = Vector2.zero;
        dolguRt.offsetMax = Vector2.zero;
        Image dolguImg = dolgu.AddComponent<Image>();
        dolguImg.color = new Color(0.25f, 0.55f, 0.85f, 1f); // Modern mavi
        slider.fillRect = dolguRt;

        GameObject tutamacAlani = new GameObject("Handle Slide Area");
        tutamacAlani.transform.SetParent(obj.transform, false);
        RectTransform tutamacAlaniRt = tutamacAlani.AddComponent<RectTransform>();
        tutamacAlaniRt.anchorMin = Vector2.zero;
        tutamacAlaniRt.anchorMax = Vector2.one;
        tutamacAlaniRt.offsetMin = Vector2.zero;
        tutamacAlaniRt.offsetMax = Vector2.zero;

        GameObject tutamac = new GameObject("Handle");
        tutamac.transform.SetParent(tutamacAlani.transform, false);
        RectTransform tutamacRt = tutamac.AddComponent<RectTransform>();
        // Görünür, şık bir tutamaç
        tutamacRt.sizeDelta = new Vector2(20f, 20f); 
        Image tutamacImg = tutamac.AddComponent<Image>();
        tutamacImg.color = Color.white;
        slider.handleRect = tutamacRt;
        slider.targetGraphic = tutamacImg;

        return slider;
    }

    TextMeshProUGUI OlusturBaslik(Transform parent, string yazi, Vector2 min, Vector2 max)
    {
        TextMeshProUGUI tmp = OlusturMetin(parent, yazi, 34, min, max, Color.white);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = yazi;
        return tmp;
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
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        return tmp;
    }

    void OlusturButon(Transform parent, string yazi, Vector2 anchorMin, UnityEngine.Events.UnityAction aksiyon, Color renk)
    {
        GameObject obj = new GameObject(yazi + "Btn");
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        // Yüksekliği biraz düşürüp daha zarif bir buton elde ettik
        rt.anchorMax = anchorMin + new Vector2(0.24f, 0.09f); 
        rt.sizeDelta = Vector2.zero;
        
        Image img = obj.AddComponent<Image>();
        img.color = renk;
        img.raycastTarget = true;
        img.type = UnityEngine.UI.Image.Type.Sliced;
        
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
        tmp.fontSize = 22; // Çok devasa olmaması için fontu kıstık
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = TMPro.FontStyles.Bold;
    }

    void AtamaTmpFont(TextMeshProUGUI tmp)
    {
        if (tmp != null && TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }
    }
}