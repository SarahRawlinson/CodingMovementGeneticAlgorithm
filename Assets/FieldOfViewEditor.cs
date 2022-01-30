using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    static FieldOfViewEditor()
    {
        Debug.Log("FieldOfViewEditor Working");
    }

    private void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView)target;
        if (fow.Eye == null) return;
        Handles.color = fow.Colour;
        var position = fow.Eye.position;
        Handles.DrawWireArc(position, fow.Eye.up, fow.Eye.forward, 360, fow.ViewRadius);
        Vector3 viewAngleA =  fow.DirectionFromAngle(-fow.ViewAngle / 2, true);
        Vector3 viewAngleB =  fow.DirectionFromAngle(fow.ViewAngle / 2, true);
        Handles.DrawLine(position, position + viewAngleA * fow.ViewRadius);
        Handles.DrawLine(position, position + viewAngleB * fow.ViewRadius);
        
    }
}
