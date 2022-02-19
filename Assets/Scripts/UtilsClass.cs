using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsClass : MonoBehaviour
{
    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPostion, int fontSize,
        Color color, TextAnchor textAnchor, TextAlignment textAlignment)
    {
        var gameObject = new GameObject("World_Text", typeof(TextMesh));
        var transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPostion;
        var textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        return textMesh;
    }

    public static Vector3 GetMouseWorldPosition()
    {
        var vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0.0f;
        return vec;
    }

    private static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        var worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }
}
