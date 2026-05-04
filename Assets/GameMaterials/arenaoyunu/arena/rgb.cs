using UnityEngine;
using TMPro;

public class rgb_yazi : MonoBehaviour
{
    TextMeshProUGUI yazi;
    float hiz = 2f;

    void Start()
    {
        yazi = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float r = Mathf.Sin(Time.time * hiz) * 0.5f + 0.5f;
        float g = Mathf.Sin(Time.time * hiz + 2f) * 0.5f + 0.5f;
        float b = Mathf.Sin(Time.time * hiz + 4f) * 0.5f + 0.5f;

        yazi.color = new Color(r, g, b);
    }
}