using UnityEngine;
using UnityEngine.SceneManagement;

public class MapCubeSpawner : MonoBehaviour
{
    [Header("Küp Ayarları")]
    public GameObject cubePrefab;
    public int cubeCount = 10;
    public float spawnRadius = 50f;
    public float minY = 0f;
    public float maxY = 10f;

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
        for (int i = 0; i < cubeCount; i++)
        {
            Vector3 randomPos = GetRandomPosition();
            GameObject cube = Instantiate(cubePrefab, randomPos, Quaternion.identity);
            cube.AddComponent<MapCube>();
        }
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        float z = Random.Range(minZ, maxZ);
        return new Vector3(x, y, z);
    }
}

public class MapCube : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Küpe tıklandığında oyun başlasın
        StartGame();
    }

    void StartGame()
    {
        // Ana oyun sahnesine geç
        SceneManager.LoadScene("arena"); // Sahne adını değiştir
    }
}