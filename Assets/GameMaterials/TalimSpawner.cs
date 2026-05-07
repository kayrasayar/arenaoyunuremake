using UnityEngine;
using UnityEngine.SceneManagement;

public class TalimSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public Vector3 spawnPosition = new Vector3(0f, 2f, 80f);
    public float cubeScale = 4f;
    public string trainingSceneName = "training_arena";
    public Color cubeColor = Color.white;

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
            cube.transform.position = spawnPosition;
            cube.AddComponent<BoxCollider>();
        }

        cube.transform.localScale = Vector3.one * cubeScale;
        SetCubeColor(cube, cubeColor);

        MapCube mapCube = cube.AddComponent<MapCube>();
        mapCube.Initialize(true, trainingSceneName);
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
