using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WorldMapQuestPanel : MonoBehaviour
{
    public static WorldMapQuestPanel Instance { get; private set; }

    private const string WorldMapSceneName = "worldscreen";

    [Header("Konum")]
    public Vector2 panelBoyutu = new Vector2(340f, 210f);
    public Vector2 sagUstOffset = new Vector2(-18f, -18f);

    private TextMeshProUGUI baslikText;
    private TextMeshProUGUI gorevText;
    private GameObject panelKok;

    public static void WorldscreendeOlustur()
    {
        if (!WorldMapSahnesiMi())
        {
            return;
        }

        TemizlePanel();

        if (FindFirstObjectByType<WorldMapQuestPanel>() != null)
        {
            return;
        }

        GameObject obj = new GameObject("WorldMapQuestPanel");
        obj.AddComponent<WorldMapQuestPanel>();
    }

    public static void TemizlePanel()
    {
        if (!WorldMapSahnesiMi())
        {
            WorldMapQuestPanel panel = FindFirstObjectByType<WorldMapQuestPanel>();
            if (panel != null)
            {
                Destroy(panel.gameObject);
            }
        }
    }

    static bool WorldMapSahnesiMi()
    {
        return SceneManager.GetActiveScene().name == WorldMapSceneName;
    }

    void Awake()
    {
        if (!WorldMapSahnesiMi())
        {
            Destroy(gameObject);
            return;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        OlusturUI();
        GuncelleGorevler();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= SahneYuklendi;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += SahneYuklendi;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SahneYuklendi;
    }

    void SahneYuklendi(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != WorldMapSceneName && Instance == this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GuncelleGorevler();
    }

    public void GuncelleGorevler()
    {
        if (!WorldMapSahnesiMi() || gorevText == null)
        {
            return;
        }

        gorevText.text = OlusturGorevMetni();
    }

    string OlusturGorevMetni()
    {
        if (GameProgressManager.Instance == null)
        {
            return "Görevler yükleniyor...";
        }

        List<string> tumIlceler = GameProgressManager.Instance.GetRequiredDistrictNames();
        if (tumIlceler.Count == 0)
        {
            tumIlceler = new List<string>
            {
                "Köprübaşı", "Akhisar", "Demirci", "Esenler",
                "Beylikdüzü", "Üsküdar", "Çırıkçı", "Turgutlu"
            };
        }

        int toplam = tumIlceler.Count;
        int tamamlanan = 0;
        foreach (string ilce in tumIlceler)
        {
            if (GameProgressManager.Instance.completedDistricts.Contains(ilce))
            {
                tamamlanan++;
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<color=#FFD700><b>HİKAYE</b></color>");
        sb.AppendLine("Manisa tehlikede. İlçeleri fethet,");
        sb.AppendLine("Final'de kaderi değiştir, Kral.");
        sb.AppendLine();
        sb.AppendLine("<color=#87CEFA><b>GÖREVLER</b></color>");

        if (GameProgressManager.Instance.needsTraining)
        {
            sb.AppendLine("• <color=#FF6B6B>ÖNCELİK:</color> Talime git");
            sb.AppendLine("• İlçe / Final kilitli.");
            return sb.ToString();
        }

        if (tamamlanan < toplam)
        {
            sb.AppendLine($"• İlçe küplerine tıkla ({tamamlanan}/{toplam})");
            sb.AppendLine("• Kalan:");
            foreach (string ilce in tumIlceler)
            {
                if (!GameProgressManager.Instance.completedDistricts.Contains(ilce))
                {
                    sb.AppendLine("  - " + ilce);
                }
            }

            sb.AppendLine("• Kaybedince talim gerekir.");
            return sb.ToString();
        }

        sb.AppendLine("• Tüm ilçeler tamam!");
        sb.AppendLine("• Final'e git, boss'u yen!");
        return sb.ToString();
    }

    void OlusturUI()
    {
        GameObject canvasObj = new GameObject("GorevCanvas");
        canvasObj.transform.SetParent(transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        panelKok = new GameObject("GorevPaneli");
        panelKok.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRt = panelKok.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(1f, 1f);
        panelRt.anchorMax = new Vector2(1f, 1f);
        panelRt.pivot = new Vector2(1f, 1f);
        panelRt.sizeDelta = panelBoyutu;
        panelRt.anchoredPosition = sagUstOffset;

        Image panelBg = panelKok.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.72f);
        panelBg.raycastTarget = false;

        Outline outline = panelKok.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.84f, 0.2f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject baslikObj = new GameObject("Baslik");
        baslikObj.transform.SetParent(panelKok.transform, false);
        RectTransform baslikRt = baslikObj.AddComponent<RectTransform>();
        baslikRt.anchorMin = new Vector2(0.05f, 0.82f);
        baslikRt.anchorMax = new Vector2(0.95f, 0.98f);
        baslikRt.offsetMin = Vector2.zero;
        baslikRt.offsetMax = Vector2.zero;
        baslikText = baslikObj.AddComponent<TextMeshProUGUI>();
        AtamaFont(baslikText);
        baslikText.text = "GÖREV LİSTESİ";
        baslikText.fontSize = 20f;
        baslikText.fontStyle = FontStyles.Bold;
        baslikText.alignment = TextAlignmentOptions.Center;
        baslikText.color = new Color(1f, 0.88f, 0.35f);
        baslikText.raycastTarget = false;

        GameObject gorevObj = new GameObject("Gorevler");
        gorevObj.transform.SetParent(panelKok.transform, false);
        RectTransform gorevRt = gorevObj.AddComponent<RectTransform>();
        gorevRt.anchorMin = new Vector2(0.05f, 0.04f);
        gorevRt.anchorMax = new Vector2(0.95f, 0.8f);
        gorevRt.offsetMin = Vector2.zero;
        gorevRt.offsetMax = Vector2.zero;
        gorevText = gorevObj.AddComponent<TextMeshProUGUI>();
        AtamaFont(gorevText);
        gorevText.fontSize = 13f;
        gorevText.alignment = TextAlignmentOptions.TopLeft;
        gorevText.color = new Color(0.95f, 0.95f, 0.95f);
        gorevText.richText = true;
        gorevText.lineSpacing = 2f;
        gorevText.raycastTarget = false;
    }

    void AtamaFont(TextMeshProUGUI tmp)
    {
        if (tmp != null && TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }
    }
}
