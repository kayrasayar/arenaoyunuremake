using UnityEngine;
using UnityEngine.SceneManagement;

public class TalimSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public Vector3 spawnPosition = new Vector3(0f, 2f, 80f);
    public float cubeScale = 40f;
    public string trainingSceneName = "training_arena";
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
        Transform existingLabel = cube.transform.Find("CubeLabel");
        if (existingLabel != null)
        {
            TextMesh existingText = existingLabel.GetComponent<TextMesh>();
            if (existingText != null)
                existingText.text = label;
            return;
        }

        GameObject textObj = new GameObject("CubeLabel");
        textObj.transform.SetParent(cube.transform, false);
        textObj.transform.localPosition = new Vector3(0f, cubeScale * 0.05f + 3f, 0f);
        textObj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 20;
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