using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.Characters.ThirdPerson;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class Brain : MonoBehaviour, ITestForTarget
{
    [Serializable]
    public class DNAGroups
    {
        public DNA _movementDNAForwardBackward;
        public DNA _heightDNA;
        public DNA _priorityDNA;
        public DNA _movementDNALeftRight;
        public DNA _movementDNATurn;

        public DNAGroups Clone() => new DNAGroups() {
            _movementDNAForwardBackward = _movementDNAForwardBackward,
            _heightDNA = _heightDNA,
            _priorityDNA = _priorityDNA,
            _movementDNALeftRight = _movementDNALeftRight,
            _movementDNATurn = _movementDNATurn
        };
    }
    public float timeAlive;
    public DNAGroups dnaGroups;
    private ThirdPersonCharacter _character;
    private Vector3 _move;
    private bool _alive = true;
    public static event Action Dead;
    private Vector3 startPos;
    private Vector3 endPos;
    [SerializeField] private GameObject[] turnOffOnDeath;
    [SerializeField] private string[] tagsToLookFor;
    private List<Color> _coloursOfTaggedItems = new List<Color>();
    [SerializeField] private string[] tagsForDie;
    [SerializeField] private GameObject lightBulb;

    public float GetProgress()
    {
        if (_alive) return Vector3.Distance(startPos, transform.position);
        return 0f;
    }
    public float Distance
    {
        get => GetDistanceTraveled();
    }

    private void Awake()
    {
        foreach (string tag in tagsToLookFor)
        {
            if (tag == "Ethan")
            {
                _coloursOfTaggedItems.Add(new Color(117f, 6f, 255f));
                continue;
            }
            try
            {
                _coloursOfTaggedItems.Add(GameObject.FindGameObjectWithTag(tag).GetComponent<MeshRenderer>().material.color);
            }
            catch (Exception e)
            {
                _coloursOfTaggedItems.Add(Color.grey);
                Console.WriteLine(e);
            }
        }
        dnaGroups._movementDNAForwardBackward = new DNA((tagsToLookFor.Length * 2) + 1, 3, "MovementForwardBackward");
        dnaGroups._movementDNALeftRight = new DNA((tagsToLookFor.Length * 2) + 1, 3, "MovementLeftRight");
        dnaGroups._movementDNATurn = new DNA((tagsToLookFor.Length * 2) + 1, 3, "MovementForwardBackward");
        dnaGroups._heightDNA = new DNA((tagsToLookFor.Length * 2) + 1, 3, "Height");
        dnaGroups._priorityDNA = new DNA((tagsToLookFor.Length * 2), 100, "Priority");
    }

    public string GetDNAString()
    {
        return JsonConvert.SerializeObject(dnaGroups);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!_alive) return;
        if (tagsForDie.Contains(other.gameObject.tag))
        {
            _alive = false;
            Dead?.Invoke();
            SetEndPosition();
            DeathOnOff(false);
        }
    }

    private void DeathOnOff(bool on)
    {
        foreach (GameObject gObject in turnOffOnDeath)
        {
            gObject.SetActive(on);
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
        // _dnaGroups = new DNAGroups(new DNA((tagsToLookFor.Length * 2) + 1, 7, "Movement"),
        //     new DNA((tagsToLookFor.Length * 2) + 1, 3, "Height"),
        //     new DNA((tagsToLookFor.Length * 2), 100, "Priority"));
        DeathOnOff(true);
        startPos = transform.position;
        
        startPos = transform.position;
        _move = transform.position;
        _character = GetComponent<ThirdPersonCharacter>();
        timeAlive = 0;
        _alive = true;
    }

    private void moveFromDNAValue(int movementForwardBackwardValue,int heightValue, int movementLeftRight, int movementTurn)
    {
        float v = 0f;
        float h = 0f;
        bool crouch = false;
        bool jump = false;
        float r = 0;

        switch (movementForwardBackwardValue)
        {
            case 0: //  -VAL 1 = STOP
                v = 0;
                break;
            case 1: //  -VAL 2 = TURN LEFT
                v = 1;
                break;
            case 2: //  -VAL 3 = TURN RIGHT
                v = -1;
                break;
        }
        switch (movementLeftRight)
        {
            case 0: //  -VAL 1 = STOP
                h = 0;
                break;
            case 1: //  -VAL 2 = TURN LEFT
                h = 1;
                break;
            case 2: //  -VAL 3 = TURN RIGHT
                h = -1;
                break;
        }
        switch (movementTurn)
        {
            case 0: //  -VAL 1 = STOP
                r = 0;
                break;
            case 1: //  -VAL 2 = TURN LEFT
                r = 90;
                break;
            case 2: //  -VAL 3 = TURN RIGHT
                r = -90;
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
        int moveFB = 0;
        int height = 0;
        int moveLR = 0;
        int moveT = 0;
        Vector3 lightBulbPosition = lightBulb.transform.position;
        List<string> seenObjects = new List<string>();
        if (seen.Count > 0)
        {
            List<(int index, int value)> dnaPos = new List<(int, int)>();
            var vals = dnaGroups._priorityDNA.GetGenes();
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
                    if (visibleObject.CompareTag(tag))
                    {
                        seenObjects.Add(tagsToLookFor[i]);
                        options.Add((i,dnaGroups._priorityDNA.GetGene(i),pos[index]));

                    }
                }
            }

            
            for (var i = 0; i < tagsToLookFor.Length; i++)
            {
                if (!seenObjects.Contains(tagsToLookFor[i]))
                {
                    int index = tagsToLookFor.Length + i;
                    options.Add((index,dnaGroups._priorityDNA.GetGene(index),lightBulbPosition));

                }
            }
            options.Sort((x, y) => y.value.CompareTo(x.value));
            moveFB = dnaGroups._movementDNAForwardBackward.GetGene(options[0].Item1);
            height = dnaGroups._heightDNA.GetGene(options[0].Item1);
            moveLR = dnaGroups._movementDNALeftRight.GetGene(options[0].Item1);
            moveT = dnaGroups._movementDNATurn.GetGene(options[0].Item1);
            Color selectedColour = Color.white;
            if (options[0].index >= tagsToLookFor.Length)
            {
                lightBulb.GetComponent<LightBulb>().ChangeColor(_coloursOfTaggedItems[options[0].index - tagsToLookFor.Length]);
            }
            else
            {
                lightBulb.GetComponent<LightBulb>().ChangeColor(_coloursOfTaggedItems[options[0].index]);
                Debug.DrawLine(eye, options[0].Item3, selectedColour);
            }

        }
        else
        {
            lightBulb.GetComponent<LightBulb>().ChangeColor(Color.black);
            moveFB = dnaGroups._movementDNAForwardBackward.GetGene((tagsToLookFor.Length * 2));
            height = dnaGroups._heightDNA.GetGene((tagsToLookFor.Length * 2));
            moveLR = dnaGroups._movementDNALeftRight.GetGene((tagsToLookFor.Length * 2));
            moveT = dnaGroups._movementDNATurn.GetGene((tagsToLookFor.Length * 2));
            Debug.DrawLine(eye, lightBulbPosition, Color.red);
        }

        moveFromDNAValue(moveFB, height, moveLR, moveT);
    }
    


    public (bool, GameObject) TestForTarget(Collider collider, List<GameObject> gameObjects)
    {
        return (tagsToLookFor.Contains(collider.tag), collider.gameObject);
    }
}
