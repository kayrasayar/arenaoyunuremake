using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndScreenManager : MonoBehaviour
{
    [Header("Jenerik")]
    public string baslangicSahnesi = "video";
    public float jenerikKaymaHizi = 85f;
    public float jenerikSonuBekleme = 2f;

    [TextArea(4, 12)]
    public string jenerikMetni =
        "OYUN TAMAMLANDI\n\n" +
        "YAPIMCI\n\n" +
        "ATIF CEYUN SİVRİ\n\n" +
        "Teşekkürler oynadığın için, Kral.";

    private bool yenidenBaslatildi;
    private GameObject siyahEkran;
    private RectTransform jenerikMetinRt;
    private TextMeshProUGUI jenerikYazi;
    private Canvas jenerikCanvas;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        EskiWinEkraniniGizle();
        OlusturSiyahJenerik();
        StartCoroutine(JenerikOynat());
    }

    void EskiWinEkraniniGizle()
    {
        GameObject eskiWin = GameObject.Find("winscreen");
        if (eskiWin != null)
        {
            eskiWin.SetActive(false);
        }

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas.gameObject.name.Contains("winscreen", System.StringComparison.OrdinalIgnoreCase))
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }

    void OlusturSiyahJenerik()
    {
        GameObject canvasObj = new GameObject("JenerikCanvas");
        jenerikCanvas = canvasObj.AddComponent<Canvas>();
        jenerikCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        jenerikCanvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        siyahEkran = new GameObject("SiyahEkran");
        siyahEkran.transform.SetParent(canvasObj.transform, false);
        RectTransform siyahRt = siyahEkran.AddComponent<RectTransform>();
        siyahRt.anchorMin = Vector2.zero;
        siyahRt.anchorMax = Vector2.one;
        siyahRt.offsetMin = Vector2.zero;
        siyahRt.offsetMax = Vector2.zero;
        Image siyah = siyahEkran.AddComponent<Image>();
        siyah.color = Color.black;

        GameObject metinObj = new GameObject("JenerikMetin");
        metinObj.transform.SetParent(canvasObj.transform, false);
        jenerikMetinRt = metinObj.AddComponent<RectTransform>();
        jenerikMetinRt.anchorMin = new Vector2(0.5f, 0f);
        jenerikMetinRt.anchorMax = new Vector2(0.5f, 0f);
        jenerikMetinRt.pivot = new Vector2(0.5f, 0f);
        jenerikMetinRt.sizeDelta = new Vector2(1400f, 900f);
        jenerikMetinRt.anchoredPosition = new Vector2(0f, -200f);

        jenerikYazi = metinObj.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            jenerikYazi.font = TMP_Settings.defaultFontAsset;
        }

        jenerikYazi.text = jenerikMetni;
        jenerikYazi.fontSize = 46f;
        jenerikYazi.alignment = TextAlignmentOptions.Center;
        jenerikYazi.color = Color.white;
        jenerikYazi.lineSpacing = 18f;
        jenerikYazi.fontStyle = FontStyles.Bold;
    }

    IEnumerator JenerikOynat()
    {
        float metinYuksekligi = jenerikYazi.preferredHeight + 200f;
        float baslangicY = -metinYuksekligi;
        float hedefY = 1200f + metinYuksekligi;
        jenerikMetinRt.anchoredPosition = new Vector2(0f, baslangicY);

        while (jenerikMetinRt.anchoredPosition.y < hedefY)
        {
            if (HerhangiTusaBasildi())
            {
                break;
            }

            float yeniY = jenerikMetinRt.anchoredPosition.y + jenerikKaymaHizi * Time.deltaTime;
            jenerikMetinRt.anchoredPosition = new Vector2(0f, yeniY);
            yield return null;
        }

        yield return new WaitForSeconds(jenerikSonuBekleme);
        YenidenBaslat();
    }

    bool HerhangiTusaBasildi()
    {
        if (Input.anyKeyDown)
        {
            return true;
        }

        return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
    }

    public void YenidenBaslat()
    {
        if (yenidenBaslatildi)
        {
            return;
        }

        yenidenBaslatildi = true;
        StopAllCoroutines();

        if (jenerikCanvas != null)
        {
            Destroy(jenerikCanvas.gameObject);
        }

        if (FinalBossFinale.Instance != null)
        {
            Destroy(FinalBossFinale.Instance.gameObject);
        }

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetAndRestartFromBeginning(baslangicSahnesi);
            return;
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(baslangicSahnesi);
    }
}
