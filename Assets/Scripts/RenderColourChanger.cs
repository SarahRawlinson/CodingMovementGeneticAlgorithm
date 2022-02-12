using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderColourChanger : MonoBehaviour
{

    [SerializeField] private Renderer MeshRenderer;
    // Use this for initialization
    void Awake ()
    {
        foreach (Material material in MeshRenderer.materials)
        {
            material.color = Color.black;
        }
    }

    public void ChangeColor(Color newColour)
    {
        foreach (Material material in MeshRenderer.materials)
        {
            material.color = newColour;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
