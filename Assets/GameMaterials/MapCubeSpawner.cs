using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MapCubeSpawner : MonoBehaviour
{
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
    public float labelHeight = 2.5f;
    public float labelFontSize = 40f;
    public float labelScale = 1.2f;
    public Color labelColor = Color.white;
    public TMP_FontAsset labelFont;

    [Header("Harita Sınırları")]
    public float minX = -100f;
    public float maxX = 100f;
    public float minZ = -100f;
    public float maxZ = 100f;

    void Start()
    {
        SpawnCubes();
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
            MapCube mapCube = cube.AddComponent<MapCube>();
            mapCube.Initialize(false, "arena");

            string label = GetNextCubeLabel(availableNames, i);
            cube.name = label;
            CreateCubeLabel(cube, label);
            Debug.Log("Küp oluşturuldu: " + label + " konumda: " + position);
        }
    }

    void CreateCubeLabel(GameObject cube, string label)
    {
        GameObject textObj = new GameObject("CubeLabel");
        textObj.transform.SetParent(cube.transform, false);
        textObj.transform.localPosition = new Vector3(0f, labelHeight, 0f);
        textObj.transform.localRotation = Quaternion.identity;

        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = label;
        textMesh.fontSize = labelFontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = labelColor;
        textMesh.enableAutoSizing = true;
        textMesh.fontSizeMin = 20f;
        textMesh.fontSizeMax = 80f;
        textMesh.textWrappingMode = TextWrappingModes.NoWrap;
        textMesh.rectTransform.sizeDelta = new Vector2(10f, 3f);
        textMesh.transform.localScale = Vector3.one * labelScale;

        if (labelFont != null)
        {
            textMesh.font = labelFont;
        }

        textObj.AddComponent<BillboardLabel>();
    }

    string GetNextCubeLabel(System.Collections.Generic.List<string> availableNames, int index)
    {
        if (availableNames != null && availableNames.Count > 0)
        {
            int randomIndex = Random.Range(0, availableNames.Count);
            string label = availableNames[randomIndex];
            availableNames.RemoveAt(randomIndex);
            return label;
        }

        // İsim listesi biterse tekrar eden baz isimlere numara ekle
        string baseName = cubeNames.Length > 0 ? cubeNames[index % cubeNames.Length] : "Küp";
        return baseName + " " + (index + 1);
    }
}

public class MapCube : MonoBehaviour
{
    private bool isTrainingCube = false;
    private string sceneName = "arena";
    private string warningMessage = "Önce talim alanına gitmelisin!";

    public void Initialize(bool trainingCube, string destinationScene = "arena")
    {
        isTrainingCube = trainingCube;
        sceneName = destinationScene;
    }

    private void OnMouseDown()
    {
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.needsTraining && !isTrainingCube)
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