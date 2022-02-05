using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;
    [SerializeField] private float minZ;
    [SerializeField] private float maxZ;
    [SerializeField] private float speed;
    [SerializeField] private float reachedDistance;
    private Vector3 startingPosition;
    private float timeMoved = 0f;
    
    private bool min = true;

    private void Start()
    {
        startingPosition = transform.position;
    }

    void Update()
    {
        timeMoved += Time.deltaTime;
        Vector3 move;
        if (min)
        {
            move = new Vector3(startingPosition.x + minX, startingPosition.y + minY, startingPosition.z + minZ);
        }
        else
        {
            move = new Vector3(startingPosition.x + maxX, startingPosition.y + maxY, startingPosition.z + maxZ);
        }
        if (Vector3.Distance(transform.position, move) < reachedDistance)
        {
            timeMoved = 0f;
            min = !min;
        }
        transform.position = Vector3.Lerp(transform.position, move, speed * timeMoved);
    }
}
