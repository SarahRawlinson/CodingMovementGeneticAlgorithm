using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class Brain : MonoBehaviour
{
    [SerializeField] private int _DNALength = 1;
    public float timeAlive;
    public DNA _dna;
    private ThirdPersonCharacter _character;
    private Vector3 _move;
    private bool _alive = true;
    public static event Action Dead;
    private Vector3 startPos;
    private Vector3 endPos;
    public float Distance
    {
        get => GetDistanceTraveled();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag($"dead"))
        {
            _alive = false;
            Dead?.Invoke();
            SetEndPosition();
        }
    }

    private void SetEndPosition()
    {
        endPos = transform.position;
    }

    public float GetDistanceTraveled()
    {
        if (_alive) SetEndPosition();
        return Vector3.Distance(startPos, endPos);
    }

    public void Init()
    {
        _dna = new DNA(_DNALength, 6);
        startPos = transform.position;
        _character = GetComponent<ThirdPersonCharacter>();
        timeAlive = 0;
        _alive = true;
    }

    private void FixedUpdate()
    {
        float h = 0;
        float v = 0;
        bool crouch = false;
        bool jump = false;
        int value = _dna.GetGene(0);
        switch (value)
        {
            case 1:
                v = 1;
                break;
            case 2:
                v = -1;
                break;
            case 3:
                h = -1;
                break;
            case 4:
                h = 1;
                break;
            case 5:
                jump = true;
                break;
            case 6:
                crouch = true;
                break; 
        }

        _move = v * Vector3.forward + h * Vector3.right;
        _character.Move(_move,crouch,jump);
        if (_alive) timeAlive += Time.deltaTime;

    }
}
