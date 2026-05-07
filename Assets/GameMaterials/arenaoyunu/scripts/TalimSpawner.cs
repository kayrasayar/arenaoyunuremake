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

        CreateCubeLabel(cube, "Talim Alanı");
    }

    void CreateCubeLabel(GameObject cube, string label)
    {
        GameObject textObj = new GameObject("CubeLabel");
        textObj.transform.SetParent(cube.transform, false);
        textObj.transform.localPosition = new Vector3(0f, 5f, 0f); // Daha yüksek
        textObj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 80; // Daha büyük
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        textObj.AddComponent<BillboardLabel>();
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
