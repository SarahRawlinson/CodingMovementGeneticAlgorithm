using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class Brain : MonoBehaviour, ITestForTarget
{
    public float timeAlive;
    public DNA _movementDNA;
    public DNA _heightDNA;
    public DNA _priorityDNA;
    private ThirdPersonCharacter _character;
    private Vector3 _move;
    private bool _alive = true;
    public static event Action Dead;
    private Vector3 startPos;
    private Vector3 endPos;
    [SerializeField] private GameObject[] turnOffOnDeath;
    [SerializeField] private string[] tagsToLookFor;
    public float Distance
    {
        get => GetDistanceTraveled();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!_alive) return;
        if (other.gameObject.CompareTag($"dead") || other.gameObject.CompareTag($"MovingObject"))
        {
            _alive = false;
            Dead?.Invoke();
            SetEndPosition();
            foreach (GameObject gObject in turnOffOnDeath)
            {
                gObject.SetActive(false);
            }
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
        _movementDNA = new DNA(tagsToLookFor.Length + 1, 7);
        _heightDNA = new DNA(tagsToLookFor.Length + 1, 3);
        _priorityDNA = new DNA(tagsToLookFor.Length, 100);
        startPos = transform.position;
        _character = GetComponent<ThirdPersonCharacter>();
        timeAlive = 0;
        _alive = true;
    }

    private void moveFromDNAValue(int movementValue,int heightValue)
    {
        float v = 0f;
        float h = 0f;
        bool crouch = false;
        bool jump = false;
        float r = 0;

        switch (movementValue)
        {
            case 0: //  -VAL 1 = STOP
                break;
            case 1: //  -VAL 2 = TURN LEFT
                r = 90;
                break;
            case 2: //  -VAL 3 = TURN RIGHT
                r = -90;
                break;
            case 3: //  -VAL 4 = MOVE FORWARD
                v = 1;
                break;
            case 4: //  -VAL 5 = MOVE BACKWARD
                v = -1;
                break;
            case 5: //  -VAL 6 = MOVE LEFT
                h = 1;
                break;
            case 6: //  -VAL 7 = MOVE RIGHT
                h = -1;
                break;
        }
        
        switch (heightValue)
        {
            case 0: //  -VAL 1 = NORMAL
                break;
            case 1: //  -VAL 2 = CROUCH
                crouch = true;
                break;
            case 2: //  -VAL 3 = JUMP
                jump = true;
                break;
        }
        _character.transform.rotation =
            Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, r, 0), Time.time * 0.1f);
        _move = v * Vector3.forward + h * Vector3.right;
        _character.Move(_move,crouch,jump);
    }


    private void FixedUpdate()
    {
        if (!_alive) return;
        List<GameObject> seen = new List<GameObject>();
        List<Vector3> vector3s = new List<Vector3>();
        Vector3 eye = new Vector3();
        foreach (FieldOfView fieldOfView in GetComponents<FieldOfView>())
        {
            eye = fieldOfView.Eye.transform.position;
            (List<GameObject> gameObjects, List<Vector3> objectPositions) = fieldOfView.FindVisibleTargets(this);
            seen.AddRange(gameObjects);
            vector3s.AddRange(objectPositions);
        }
        MakeDecision(seen, eye, vector3s);
        if (_alive) timeAlive += Time.deltaTime;

    }

    private void MakeDecision(List<GameObject> seen, Vector3 eye, List<Vector3> pos)
    {
        int move = 0;
        int height = 0;
        if (seen.Count > 0)
        {
            List<(int index, int value)> dnaPos = new List<(int, int)>();
            var vals = _priorityDNA.GetGenes();
            for (var index = 0; index < vals.Count; index++)
            {
                int val = vals[index];
                dnaPos.Add((index, val));
            }
            dnaPos.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            List<(int index, int value, Vector3 position)> options = new List<(int, int, Vector3)>();
            for (int index = 0; index < seen.Count; index++)
            {
                GameObject visibleObject = seen[index];
                for (var i = 0; i < tagsToLookFor.Length; i++)
                {
                    string tag = tagsToLookFor[i];
                    if (tag == visibleObject.tag)
                    {
                        options.Add((i,_priorityDNA.GetGene(i),pos[index]));
                        if (i == dnaPos[0].index)
                        {
                            break;
                        }
                    }
                }
            }
            options.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            move = _movementDNA.GetGene(options[0].Item1);
            height = _heightDNA.GetGene(options[0].Item1);
            Debug.DrawLine(eye, options[0].Item3, Color.magenta);
        }
        else
        {
            move = _movementDNA.GetGene(tagsToLookFor.Length);
            height = _heightDNA.GetGene(tagsToLookFor.Length);
        }

        moveFromDNAValue(move, height);
    }


    public (bool, GameObject) TestForTarget(Collider collider, List<GameObject> gameObjects)
    {
        
        return (tagsToLookFor.Contains(collider.tag), collider.gameObject);
    }
}
