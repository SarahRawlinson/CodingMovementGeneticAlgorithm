using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSphere : MonoBehaviour
{
    [SerializeField] private float captureRadius = .5f;
    [SerializeField] Color gizmoColor = Color.cyan;
    [SerializeField] private GameObject captureSphere;
    float scale;
    public float CaptureRadius
    {
        get => captureRadius;
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = gizmoColor;
    //     Gizmos.DrawWireSphere(transform.position, captureRadius);
    // }

    private void Awake()
    {
        scale = transform.localScale.x;
        captureRadius = .5f;
    }
    

    public void DrawSphere(float radius)
    {
        captureRadius = radius;
        float size = (radius * 2) / scale;
        captureSphere.transform.localScale = new Vector3(size, size, size);
    }
}
