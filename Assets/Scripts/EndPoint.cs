using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class EndPoint : MonoBehaviour
{
    private Collider _collider;
    private void Start()
    {
        PopulationManager.NewRound += Reset;
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody body))
        {
            if (body.TryGetComponent(out Brain brain))
            {
                brain.AddHitBonus(1);
                _collider.enabled = false;
            }
        }
        
    }

    private void Reset()
    {
        _collider.enabled = true;
    }
}
