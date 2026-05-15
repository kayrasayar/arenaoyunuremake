using UnityEngine;
using TMPro;

public static class DistrictMapLabel
{
    public static void Create(Transform parent, string text, float localHeight, float fontSize, Color color, float localScale = 1f)
    {
        Transform existingLabel = parent.Find("CubeLabel");
        if (existingLabel != null)
        {
            Object.Destroy(existingLabel.gameObject);
        }

        GameObject labelObject = new GameObject("CubeLabel");
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = new Vector3(0f, localHeight, 0f);
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale = Vector3.one * localScale;

        TextMeshPro label = labelObject.AddComponent<TextMeshPro>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.horizontalAlignment = HorizontalAlignmentOptions.Center;
        label.verticalAlignment = VerticalAlignmentOptions.Middle;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
        label.rectTransform.sizeDelta = new Vector2(8f, 2f);

        MeshRenderer meshRenderer = label.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 50;
        }

        labelObject.AddComponent<BillboardLabel>();
    }
}
