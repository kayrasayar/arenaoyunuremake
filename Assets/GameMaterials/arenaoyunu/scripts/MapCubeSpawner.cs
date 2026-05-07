using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct DistrictBonus
{
    public string name;
    public int enemyHpBonus;
    public int xpBonus;
    public int speedPenalty;
    public int playerHpBonus;

    public string GetBonusText()
    {
        string positives = "";
        string negatives = "";

        if (enemyHpBonus > 0) positives += $"+{enemyHpBonus} düşman HP, ";
        if (xpBonus > 0) positives += $"+{xpBonus} XP, ";
        if (playerHpBonus > 0) positives += $"+{playerHpBonus} oyuncu HP, ";

        if (speedPenalty > 0) negatives += $"-{speedPenalty} hız, ";

        positives = positives.TrimEnd(',', ' ');
        negatives = negatives.TrimEnd(',', ' ');

        return $"{positives} ; {negatives}";
    }
}

public class MapCubeSpawner : MonoBehaviour
{
    public static MapCubeSpawner Instance;

    [Header("Küp Ayarları")]
    public GameObject cubePrefab;
    public int cubeCount = 8;
    public Vector3[] cubePositions = new Vector3[8]
    {
        new Vector3(-40f, 2f, -40f),
        new Vector3(40f, 2f, -40f),
        new Vector3(-40f, 2f, 40f),
        new Vector3(40f, 2f, 40f),
        new Vector3(0f, 2f, -30f),
        new Vector3(0f, 2f, 30f),
        new Vector3(-30f, 2f, 0f),
        new Vector3(30f, 2f, 0f)
    };
    public float cubeScale = 2f;
    public string[] cubeNames = { "Köprübaşı", "Akhisar", "Demirci", "Esenler", "Beylikdüzü", "Üsküdar", "Çıkrıkçı", "Turgutlu" };
    public DistrictBonus[] districtBonuses = new DistrictBonus[8]
    {
        new DistrictBonus { name = "Köprübaşı", enemyHpBonus = 5, xpBonus = 20, speedPenalty = 5, playerHpBonus = 10 },
        new DistrictBonus { name = "Akhisar", enemyHpBonus = 10, xpBonus = 15, speedPenalty = 0, playerHpBonus = 5 },
        new DistrictBonus { name = "Demirci", enemyHpBonus = 0, xpBonus = 30, speedPenalty = 10, playerHpBonus = 0 },
        new DistrictBonus { name = "Esenler", enemyHpBonus = 15, xpBonus = 10, speedPenalty = 0, playerHpBonus = 15 },
        new DistrictBonus { name = "Beylikdüzü", enemyHpBonus = 8, xpBonus = 25, speedPenalty = 3, playerHpBonus = 8 },
        new DistrictBonus { name = "Üsküdar", enemyHpBonus = 12, xpBonus = 18, speedPenalty = 7, playerHpBonus = 12 },
        new DistrictBonus { name = "Çıkrıkçı", enemyHpBonus = 3, xpBonus = 35, speedPenalty = 12, playerHpBonus = 3 },
        new DistrictBonus { name = "Turgutlu", enemyHpBonus = 20, xpBonus = 5, speedPenalty = 0, playerHpBonus = 20 }
    };
    public float labelHeight = 2.5f;
    public float labelFontSize = 40f;
    public float labelScale = 1.2f;
    public Color labelColor = Color.white;

    [Header("Harita Sınırları")]
    public float minX = -100f;
    public float maxX = 100f;
    public float minZ = -100f;
    public float maxZ = 100f;

    private Canvas tooltipCanvas;
    public Text tooltipText;
    public GameObject tooltipObj;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CreateTooltipUI();
        SpawnCubes();
    }

    void CreateTooltipUI()
    {
        // Canvas oluştur
        GameObject canvasObj = new GameObject("TooltipCanvas");
        tooltipCanvas = canvasObj.AddComponent<Canvas>();
        tooltipCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Tooltip container
        tooltipObj = new GameObject("Tooltip");
        tooltipObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rt = tooltipObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 100);
        rt.anchoredPosition = Vector2.zero;

        // Background
        GameObject bgObj = new GameObject("TooltipBackground");
        bgObj.transform.SetParent(tooltipObj.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f); // Yarı şeffaf siyah
        bg.rectTransform.sizeDelta = new Vector2(400, 200); // Daha büyük
        bg.rectTransform.anchoredPosition = Vector2.zero;

        // Text oluştur
        GameObject textObj = new GameObject("TooltipText");
        textObj.transform.SetParent(tooltipObj.transform, false);
        tooltipText = textObj.AddComponent<Text>();
        tooltipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tooltipText.fontSize = 36; // Daha büyük
        tooltipText.color = Color.white; // Beyaz text
        tooltipText.alignment = TextAnchor.MiddleCenter;
        tooltipText.supportRichText = true;
        tooltipText.rectTransform.sizeDelta = new Vector2(400, 200); // Daha büyük
        tooltipText.rectTransform.anchoredPosition = Vector2.zero;

        tooltipObj.SetActive(false);
    }

    void SpawnCubes()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("cubePrefab atanmamış! Inspector'dan küp prefab'ını ata.");
            return;
        }

        var availableNames = new System.Collections.Generic.List<string>(cubeNames);
        Debug.Log(cubePositions.Length);
        Debug.Log(cubeCount);
        int spawnCount = Mathf.Min(cubeCount, cubePositions.Length);

        Debug.Log("Spawn işlemi başlıyor: " + spawnCount + " küp oluşturulacak");

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 position = cubePositions[i];
            GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
            cube.transform.localScale = Vector3.one * cubeScale;
            cube.AddComponent<BoxCollider>(); // Hover için collider ekle
            MapCube mapCube = cube.AddComponent<MapCube>();

            string label = GetNextCubeLabel(availableNames, i);
            cube.name = label;
            CreateCubeLabel(cube, label);

            // Bonus eşleştir
            DistrictBonus bonus = GetBonusForDistrict(label);
            mapCube.Initialize(false, "arena", bonus);

            // Kazanıldıysa yeşil yap
            if (GameProgressManager.Instance != null && GameProgressManager.Instance.completedDistricts.Contains(label))
            {
                Renderer renderer = cube.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.green;
                }
                mapCube.isCompleted = true;
            }

            Debug.Log("Küp oluşturuldu: " + label + " konumda: " + position);
        }
    }

    string GetNextCubeLabel(System.Collections.Generic.List<string> availableNames, int index)
    {
        if (availableNames.Count > 0)
        {
            string label = availableNames[index % availableNames.Count];
            availableNames.RemoveAt(index % availableNames.Count);
            return label;
        }
        return "Unknown";
    }

    DistrictBonus GetBonusForDistrict(string districtName)
    {
        foreach (var bonus in districtBonuses)
        {
            if (bonus.name == districtName)
            {
                return bonus;
            }
        }
        return default;
    }

    void CreateCubeLabel(GameObject cube, string label)
    {
        GameObject textObj = new GameObject("CubeLabel");
        textObj.transform.SetParent(cube.transform, false);
        textObj.transform.localPosition = new Vector3(0f, labelHeight, 0f);
        textObj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = (int)labelFontSize;
        textMesh.color = labelColor;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        textObj.AddComponent<BillboardLabel>();
    }

    void Update()
    {
        if (tooltipObj != null && tooltipObj.activeSelf)
        {
            // Mouse pozisyonuna tooltip'ı taşı
            Vector3 mousePos = Input.mousePosition;
            RectTransform rt = tooltipObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.position = mousePos + new Vector3(0, 125, 0); // Offset ekle
            }
        }
    }
}

public class MapCube : MonoBehaviour
{
    private bool isTrainingCube = false;
    private bool isFinalCube = false;
    private string sceneName = "arena";
    private string warningMessage = "Önce talim alanına gitmelisin!";
    private DistrictBonus bonus;
    public bool isCompleted = false;

    public void Initialize(bool trainingCube, string destinationScene = "arena", DistrictBonus districtBonus = default, bool finalCube = false)
    {
        isTrainingCube = trainingCube;
        isFinalCube = finalCube;
        sceneName = destinationScene;
        bonus = districtBonus;
        if (isFinalCube)
        {
            warningMessage = "Önce tüm ilçeleri bitirmelisin!";
        }
    }

    private void OnMouseEnter()
    {
        if (MapCubeSpawner.Instance != null && MapCubeSpawner.Instance.tooltipText != null)
        {
            string positives = "";
            string negatives = "";

            if (bonus.enemyHpBonus > 0) negatives += $"+{bonus.enemyHpBonus} düşman HP\n";
            if (bonus.playerHpBonus > 0) positives += $"+{bonus.playerHpBonus} oyuncu HP\n";

            if (bonus.speedPenalty > 0) negatives += $"-{bonus.speedPenalty} hız\n";

            positives = positives.TrimEnd('\n');
            negatives = negatives.TrimEnd('\n');

            string tooltipTextContent;
            if (isTrainingCube)
            {
                tooltipTextContent = "Talim Alanı";
            }
            else if (isFinalCube)
            {
                tooltipTextContent = "Final Alanı - Son Mücadele";
                if (GameProgressManager.Instance != null && GameProgressManager.Instance.completedDistricts.Count < MapCubeSpawner.Instance.cubeCount)
                {
                    tooltipTextContent += "\nÖnce tüm bölgeleri yenmelisin!";
                }
            }
            else if (GameProgressManager.Instance != null && GameProgressManager.Instance.needsTraining)
            {
                tooltipTextContent = "Önce talim alanına gitmelisin!";
            }
            else
            {
                tooltipTextContent = $"{bonus.name}\n";
                if (!string.IsNullOrEmpty(positives)) tooltipTextContent += $"<color=#00FF00>{positives}</color>\n";
                if (!string.IsNullOrEmpty(negatives)) tooltipTextContent += $"<color=#FF0000>{negatives}</color>\n";
                if (bonus.xpBonus > 0) tooltipTextContent += $"<color=#000080>+{bonus.xpBonus} XP</color>";
            }

            MapCubeSpawner.Instance.tooltipText.text = tooltipTextContent;
            MapCubeSpawner.Instance.tooltipObj.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (MapCubeSpawner.Instance != null && MapCubeSpawner.Instance.tooltipText != null)
        {
            MapCubeSpawner.Instance.tooltipObj.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (isCompleted)
        {
            return;
        }

        if (isFinalCube)
        {
            if (GameProgressManager.Instance == null || GameProgressManager.Instance.completedDistricts.Count < MapCubeSpawner.Instance.cubeCount)
            {
                Debug.LogWarning(warningMessage);
                return;
            }
        }

        if (GameProgressManager.Instance != null && GameProgressManager.Instance.needsTraining && !isTrainingCube && !isFinalCube)
        {
            Debug.LogWarning(warningMessage);
            return;
        }

        StartGame();
    }

    void StartGame()
    {
        if (isTrainingCube && GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CompleteTraining();
        }
        else if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.currentDistrict = bonus.name;
        }

        SceneManager.LoadScene(sceneName);
    }
}

public class BillboardLabel : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0f, 180f, 0f);
    }
}