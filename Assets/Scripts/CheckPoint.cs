using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CheckPoint : MonoBehaviour
{
    private Collider _collider;
    private bool bonusReceived;
    private List<Brain> _brains = new List<Brain>();
    private Brain _bonusWinner;
    [SerializeField] private int bonus = 1;
    // [SerializeField] private int normalHit = 1;
    public static event Action CheckPointReached;
    private void Start()
    {
        PopulationManager.NewRound += Reset;
        _collider = GetComponent<Collider>();
    }

    public int GetNumberPassed()
    {
        return _brains.Count;
    }

    public Brain GetBonusWinner()
    {
        return _bonusWinner;
    }

    public int GetBonus()
    {
        return bonus;
    }

    public bool BonusClaimed()
    {
        return bonusReceived;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        // Debug.Log($"collided with {other.name}");
        if (other.TryGetComponent(out Rigidbody body))
        {
            if (body.TryGetComponent(out Brain brain))
            {
                if (_brains.Contains(brain)) return;
                if (brain.GetIsAlive())
                {
                    CheckPointReached?.Invoke();
                    brain.CheckPointReached();
                    GetComponent<MeshRenderer>().enabled = false;
                    _brains.Add(brain);
                    if (bonusReceived) return;
                    brain.AddHitBonus(bonus);
                    brain.Bonus();
                    _bonusWinner = brain;
                    bonusReceived = true;
                    // Debug.Log("Check Point Reached");
                    // _collider.enabled = false;
                    
                }
            }
        }
        
    }

    private void Reset()
    {
        _brains.Clear();
        bonusReceived = false;
        GetComponent<MeshRenderer>().enabled = true;
        // _collider.enabled = true;
    }

    private void OnDestroy()
    {
        PopulationManager.NewRound -= Reset;
    }
}
