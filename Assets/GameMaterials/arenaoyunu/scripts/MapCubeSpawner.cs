using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
    public int cubeCount = 7;
    public Vector3[] cubePositions = new Vector3[7]
    {
        new Vector3(-40f, 2f, -40f),
        new Vector3(40f, 2f, -40f),
        new Vector3(-40f, 2f, 40f),
        new Vector3(40f, 2f, 40f),
        new Vector3(0f, 2f, -30f),
        new Vector3(0f, 2f, 30f),
        new Vector3(-30f, 2f, 0f)
    };
    public float cubeScale = 2f;
    public string[] cubeNames = { "Köprübaşı", "Akhisar", "Demirci", "Esenler", "Beylikdüzü", "Atıfın Dünyası", "Selendi" };
    public DistrictBonus[] districtBonuses = new DistrictBonus[7]
    {
        new DistrictBonus { name = "Köprübaşı", enemyHpBonus = 5, xpBonus = 20, speedPenalty = 5, playerHpBonus = 10 },
        new DistrictBonus { name = "Akhisar", enemyHpBonus = 10, xpBonus = 15, speedPenalty = 0, playerHpBonus = 5 },
        new DistrictBonus { name = "Demirci", enemyHpBonus = 0, xpBonus = 30, speedPenalty = 10, playerHpBonus = 0 },
        new DistrictBonus { name = "Esenler", enemyHpBonus = 15, xpBonus = 10, speedPenalty = 0, playerHpBonus = 15 },
        new DistrictBonus { name = "Beylikdüzü", enemyHpBonus = 8, xpBonus = 25, speedPenalty = 3, playerHpBonus = 8 },
        new DistrictBonus { name = "Atıfın Dünyası", enemyHpBonus = 12, xpBonus = 18, speedPenalty = 4, playerHpBonus = 7 },
        new DistrictBonus { name = "Selendi", enemyHpBonus = 7, xpBonus = 22, speedPenalty = 6, playerHpBonus = 9 }
    };
    public float labelHeight = 2.5f;
    public float labelFontSize = 30f;
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

    private GameObject cubeContainer;

    void OnValidate()
    {
        ValidateMapData();
    }

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
        ValidateMapData();
        // İmleci görünür yap
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

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
        if (cubeContainer != null)
        {
            Destroy(cubeContainer);
        }

        cubeContainer = new GameObject("MapCubeContainer");

        if (cubePrefab == null)
        {
            Debug.LogError("cubePrefab atanmamış! Inspector'dan küp prefab'ını ata.");
            return;
        }

        var availableNames = new System.Collections.Generic.List<string>(cubeNames);
        int maxValidCount = Mathf.Min(cubePositions.Length, cubeNames.Length, districtBonuses.Length);
        int spawnCount = Mathf.Min(cubeCount, maxValidCount);

        if (cubeCount > maxValidCount)
        {
            Debug.LogWarning($"cubeCount değeri ({cubeCount}) kullanılabilir dizilerin boyutundan büyük. spawnCount {spawnCount} ile sınırlandı.");
        }

        Debug.Log("Spawn işlemi başlıyor: " + spawnCount + " küp oluşturulacak");

        for (int i = 0; i < spawnCount; i++)
        {
            string label = GetNextCubeLabel(availableNames);
            Vector3 position = cubePositions[i];
            if (label == "Selendi")
            {
                position = new Vector3(1000f, 2f, 1000f); // Selendi çok uzak bir yere taşındı
            }

            GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
            cube.transform.SetParent(cubeContainer.transform, false);
            cube.transform.localScale = Vector3.one * cubeScale;
            if (cube.GetComponent<BoxCollider>() == null)
            {
                cube.AddComponent<BoxCollider>(); // Hover için collider ekle
            }
            MapCube mapCube = cube.GetComponent<MapCube>();
            if (mapCube == null)
            {
                mapCube = cube.AddComponent<MapCube>();
            }

            cube.name = label;
            CreateCubeLabel(cube, label);

            // Bonus eşleştir
            DistrictBonus bonus = GetBonusForDistrict(label);
            bool isFinal = (i == spawnCount - 1); // Son küp final
            mapCube.Initialize(false, "arena", bonus, isFinal);

            // Selendi otomatik olarak tamamlandı
            if (label == "Selendi")
            {
                Renderer renderer = cube.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.green;
                }
                mapCube.isCompleted = true;
                if (GameProgressManager.Instance != null && !GameProgressManager.Instance.completedDistricts.Contains(label))
                {
                    GameProgressManager.Instance.completedDistricts.Add(label);
                }
            }

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

            Debug.Log("Küp oluşturuldu: " + label + " konumda: " + position + (isFinal ? " (Final)" : ""));
        }
    }

    string GetNextCubeLabel(System.Collections.Generic.List<string> availableNames)
    {
        if (availableNames.Count > 0)
        {
            string label = availableNames[0];
            availableNames.RemoveAt(0);
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

    void ValidateMapData()
    {
        string[] defaultNames = { "Köprübaşı", "Akhisar", "Demirci", "Esenler", "Beylikdüzü", "Atıfın Dünyası", "Selendi" };
        DistrictBonus[] defaultBonuses = new DistrictBonus[7]
        {
            new DistrictBonus { name = "Köprübaşı", enemyHpBonus = 5, xpBonus = 20, speedPenalty = 5, playerHpBonus = 10 },
            new DistrictBonus { name = "Akhisar", enemyHpBonus = 10, xpBonus = 15, speedPenalty = 0, playerHpBonus = 5 },
            new DistrictBonus { name = "Demirci", enemyHpBonus = 0, xpBonus = 30, speedPenalty = 10, playerHpBonus = 0 },
            new DistrictBonus { name = "Esenler", enemyHpBonus = 15, xpBonus = 10, speedPenalty = 0, playerHpBonus = 15 },
            new DistrictBonus { name = "Beylikdüzü", enemyHpBonus = 8, xpBonus = 25, speedPenalty = 3, playerHpBonus = 8 },
            new DistrictBonus { name = "Atıfın Dünyası", enemyHpBonus = 12, xpBonus = 18, speedPenalty = 4, playerHpBonus = 7 },
            new DistrictBonus { name = "Selendi", enemyHpBonus = 7, xpBonus = 22, speedPenalty = 6, playerHpBonus = 9 }
        };
        Vector3[] defaultPositions = new Vector3[7]
        {
            new Vector3(-40f, 2f, -40f),
            new Vector3(40f, 2f, -40f),
            new Vector3(-40f, 2f, 40f),
            new Vector3(40f, 2f, 40f),
            new Vector3(0f, 2f, -30f),
            new Vector3(0f, 2f, 30f),
            new Vector3(-30f, 2f, 0f)
        };

        if (cubeNames == null || cubeNames.Length != defaultNames.Length)
        {
            Debug.LogWarning("MapCubeSpawner: cubeNames dizisi beklenen 7 elemanla eşleşmiyor; varsayılana geri döndü.");
            cubeNames = defaultNames;
        }

        if (districtBonuses == null || districtBonuses.Length != defaultBonuses.Length)
        {
            Debug.LogWarning("MapCubeSpawner: districtBonuses dizisi beklenen 7 elemanla eşleşmiyor; varsayılana geri döndü.");
            districtBonuses = defaultBonuses;
        }

        if (cubePositions == null || cubePositions.Length != defaultPositions.Length)
        {
            Debug.LogWarning("MapCubeSpawner: cubePositions dizisi beklenen 7 elemanla eşleşmiyor; varsayılana geri döndü.");
            cubePositions = defaultPositions;
        }

        int maxValid = Mathf.Min(cubeNames.Length, districtBonuses.Length, cubePositions.Length);
        if (cubeCount > maxValid)
        {
            Debug.LogWarning($"MapCubeSpawner: cubeCount ({cubeCount}) fazla, {maxValid} ile sınırlandı.");
        }

        cubeCount = Mathf.Clamp(cubeCount, 0, maxValid);
    }

    void CreateCubeLabel(GameObject cube, string label)
    {
        Transform existingLabel = cube.transform.Find("CubeLabel");
        if (existingLabel != null)
        {
            Destroy(existingLabel.gameObject);
        }

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
    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
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
            GameProgressManager.Instance.currentDistrict = isFinalCube ? "Final" : bonus.name;
        }

        if (isFinalCube)
        {
            AudioClip finalClip = Resources.Load<AudioClip>("finalgiris");
            if (finalClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(finalClip);
                StartCoroutine(LoadSceneAfterSound(finalClip.length));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    IEnumerator LoadSceneAfterSound(float delay)
    {
        yield return new WaitForSeconds(delay);
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