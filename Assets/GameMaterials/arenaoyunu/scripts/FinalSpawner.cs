using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public Vector3 spawnPosition = new Vector3(-200f, 6f, 200f);
    public float cubeScale = 30f;
    public string finalSceneName = "final";
    public Color cubeColor = Color.white;

    void Start()
    {
        spawnPosition = new Vector3(-100f, 6f, 200f); // Inspectorda eski değeri geçersiz kıl
        Debug.Log("FinalSpawner will spawn at: " + spawnPosition);
        SpawnFinalCube();
    }

    void SpawnFinalCube()
    {
        GameObject cube;

        if (cubePrefab != null)
        {
            cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = spawnPosition;
            cube.AddComponent<BoxCollider>();
        }

        cube.transform.localScale = Vector3.one * cubeScale;
        SetCubeColor(cube, cubeColor);

        MapCube mapCube = cube.AddComponent<MapCube>();
        mapCube.Initialize(false, finalSceneName, default, true);

        CreateCubeLabel(cube, "Final");
    }

    void CreateCubeLabel(GameObject cube, string label)
    {
        float labelLocalScale = 1.5f / Mathf.Max(cubeScale, 0.01f);
        DistrictMapLabel.Create(cube.transform, label, 2f, 28f, Color.white, labelLocalScale);
    }

    void SetCubeColor(GameObject cube, Color color)
    {
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer == null) return;

        Material material;
        if (renderer.sharedMaterial != null)
        {
            material = new Material(renderer.sharedMaterial);
        }
        else
        {
            material = new Material(Shader.Find("Standard"));
        }

        material.color = color;
        renderer.material = material;
    }
}
