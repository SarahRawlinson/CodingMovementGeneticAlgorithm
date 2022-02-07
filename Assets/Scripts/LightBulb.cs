using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBulb : MonoBehaviour
{

    [SerializeField] private Renderer lightBulbMeshRenderer;
    // Use this for initialization
    void Start ()
    {
        foreach (Material material in lightBulbMeshRenderer.materials)
        {
            material.color = Color.black;
        }
    }

    public void ChangeColor(Color newColour)
    {
        foreach (Material material in lightBulbMeshRenderer.materials)
        {
            material.color = newColour;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
