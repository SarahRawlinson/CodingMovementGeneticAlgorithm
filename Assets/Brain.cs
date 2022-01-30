using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class Brain : MonoBehaviour, ITestForTarget
{
    [SerializeField] private int _DNALength = 10;
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
        _dna = new DNA(_DNALength, 2);
        startPos = transform.position;
        _character = GetComponent<ThirdPersonCharacter>();
        timeAlive = 0;
        _alive = true;
    }

    private void FixedUpdate()
    {
        if (!_alive) return;
        bool stop = GetDNAValue(_dna.GetGene(1)) == 1;
        if (GetComponent<FieldOfView>().FindVisibleTargets(this).Count > 0) {
            if (stop)
            {
                try
                {
                     return;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            else
            {
                _character.transform.rotation = Quaternion.Euler(0,Random.Range(-180,180),0);
            }
            
        }
        float v = GetDNAValue(_dna.GetGene(2));
        float h = GetDNAValue(_dna.GetGene(3));
        bool crouch = GetDNAValue(_dna.GetGene(4)) == 1;
        bool jump = GetDNAValue(_dna.GetGene(5)) == 1;
        _move = v * Vector3.forward + h * Vector3.right;
        _character.Move(_move,crouch,jump);
        if (_alive) timeAlive += Time.deltaTime;

    }

    private static int GetDNAValue(int value)
    {
        switch (value)
        {
            case 0:
                return 1;
            case 1:
                return -1;
        }

        return 0;
    }

    public (bool, GameObject) TestForTarget(Collider collider, List<GameObject> gameObjects)
    {
        return (true, collider.gameObject);
    }
}
