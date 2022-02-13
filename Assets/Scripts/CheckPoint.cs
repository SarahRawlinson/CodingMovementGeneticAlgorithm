using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CheckPoint : MonoBehaviour
{
    private Collider _collider;
    private void Start()
    {
        PopulationManager.NewRound += Reset;
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log($"collided with {other.name}");
        if (other.TryGetComponent(out Rigidbody body))
        {
            if (body.TryGetComponent(out Brain brain))
            {
                if (brain.GetIsAlive())
                {
                    brain.AddHitBonus(1);
                    GetComponent<MeshRenderer>().enabled = false;
                    // Debug.Log("Check Point Reached");
                    _collider.enabled = false;
                }
            }
        }
        
    }

    private void Reset()
    {
        GetComponent<MeshRenderer>().enabled = true;
        _collider.enabled = true;
    }
}
