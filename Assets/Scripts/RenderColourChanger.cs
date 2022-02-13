using UnityEngine;
using UnityEngine.Serialization;

public class RenderColourChanger : MonoBehaviour
{

    [FormerlySerializedAs("MeshRenderer")] [SerializeField] private Renderer meshRenderer;
    // Use this for initialization
    void Awake ()
    {
        foreach (Material material in meshRenderer.materials)
        {
            material.color = Color.black;
        }
    }

    public void ChangeColor(Color newColour)
    {
        foreach (Material material in meshRenderer.materials)
        {
            material.color = newColour;
        }
    }
}
