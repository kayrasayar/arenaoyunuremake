using UnityEngine;
using UnityEngine.SceneManagement;

public class TalimSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public Vector3 spawnPosition = new Vector3(0f, 2f, 80f);
    public float cubeScale = 40f;
    public string trainingSceneName = "talimalani";
    public Color cubeColor = Color.white;
    public string cubeLabelText = "Talim Alanı";

    void Start()
    {
        SpawnTrainingCube();
    }

    void SpawnTrainingCube()
    {
        GameObject cube;

        if (cubePrefab != null)
        {
            cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.AddComponent<BoxCollider>();
        }

        cube.name = "TalimCube";
        cube.transform.position = spawnPosition;
        cube.transform.localScale = Vector3.one * cubeScale;

        SetCubeColor(cube, cubeColor);

        MapCube mapCube = cube.GetComponent<MapCube>();
        if (mapCube == null)
            mapCube = cube.AddComponent<MapCube>();

        mapCube.Initialize(true, trainingSceneName);

        CreateCubeLabel(cube, cubeLabelText);
    }

    void CreateCubeLabel(GameObject cube, string label)
    {
        float labelHeight = cubeScale * 0.05f + 3f;
        float labelLocalScale = 1.2f / Mathf.Max(cubeScale, 0.01f);
        DistrictMapLabel.Create(cube.transform, label, labelHeight, 24f, Color.white, labelLocalScale);
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