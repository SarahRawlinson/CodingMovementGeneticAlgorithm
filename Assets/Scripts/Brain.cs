using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class Brain : MonoBehaviour, ITestForTarget
{
    [Serializable]
    public class DNAGroups
    {
        public DNA _movementDNA;
        public DNA _heightDNA;
        public DNA _priorityDNA;
        

        // public DNAGroups(DNA movementDNA, DNA heightDNA, DNA priorityDNA)
        // {
        //     _movementDNA = movementDNA;
        //     _heightDNA = heightDNA;
        //     _priorityDNA = priorityDNA;
        // }
    }
    public float timeAlive;

    public DNAGroups _dnaGroups;
    private ThirdPersonCharacter _character;
    private Vector3 _move;
    private bool _alive = true;
    public static event Action Dead;
    private Vector3 startPos;
    private Vector3 endPos;
    [SerializeField] private GameObject[] turnOffOnDeath;
    [SerializeField] private string[] tagsToLookFor;
    private List<Color> coloursOfTaggedItems = new List<Color>();
    [SerializeField] private string[] tagsForDie;
    [SerializeField] private GameObject lightBulb;
    public float Distance
    {
        get => GetDistanceTraveled();
    }

    private void Awake()
    {
        foreach (string tag in tagsToLookFor)
        {
            try
            {
                coloursOfTaggedItems.Add(GameObject.FindGameObjectWithTag(tag).GetComponent<MeshRenderer>().material.color);
            }
            catch (Exception e)
            {
                coloursOfTaggedItems.Add(Color.grey);
                Console.WriteLine(e);
            }
        }
    }

    public string GetDNAString()
    {
        return JsonConvert.SerializeObject(_dnaGroups);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!_alive) return;
        if (tagsForDie.Contains(other.gameObject.tag))
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
        // _dnaGroups = new DNAGroups(new DNA((tagsToLookFor.Length * 2) + 1, 7, "Movement"),
        //     new DNA((tagsToLookFor.Length * 2) + 1, 3, "Height"),
        //     new DNA((tagsToLookFor.Length * 2), 100, "Priority"));
        
        _dnaGroups._movementDNA = new DNA((tagsToLookFor.Length * 2) + 1, 7, "Movement");
        _dnaGroups._heightDNA = new DNA((tagsToLookFor.Length * 2) + 1, 3, "Height");
        _dnaGroups._priorityDNA = new DNA((tagsToLookFor.Length * 2), 100, "Priority");
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
        Vector3 lightBulbPosition = lightBulb.transform.position;
        List<string> seenObjects = new List<string>();
        if (seen.Count > 0)
        {
            List<(int index, int value)> dnaPos = new List<(int, int)>();
            var vals = _dnaGroups._priorityDNA.GetGenes();
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
                        seenObjects.Add(tagsToLookFor[i]);
                        options.Add((i,_dnaGroups._priorityDNA.GetGene(i),pos[index]));
                        // if (i == dnaPos[0].index)
                        // {
                        //     break;
                        // }
                    }
                }
            }

            
            for (var i = 0; i < tagsToLookFor.Length; i++)
            {
                if (!seenObjects.Contains(tagsToLookFor[i]))
                {
                    
                    int index = tagsToLookFor.Length + i;
                    // Debug.Log($"Added cant see object {tagsToLookFor[i]}");
                    options.Add((index,_dnaGroups._priorityDNA.GetGene(index),lightBulbPosition));
                    // if (i == dnaPos[0].index)
                    // {
                    //     break;
                    // }
                }
                // Debug.Log($"can see object {tagsToLookFor[i]}");
            }
            options.Sort((x, y) => y.value.CompareTo(x.value));
            move = _dnaGroups._movementDNA.GetGene(options[0].Item1);
            height = _dnaGroups._heightDNA.GetGene(options[0].Item1);
            Color selectedColour = Color.white;
            if (options[0].index >= tagsToLookFor.Length)
            {
                // selectedColour = coloursOfTaggedItems[options[0].index - tagsToLookFor.Length];
                lightBulb.GetComponent<LightBulb>().ChangeColor(coloursOfTaggedItems[options[0].index - tagsToLookFor.Length]);
            }
            else
            {
                lightBulb.GetComponent<LightBulb>().ChangeColor(Color.black);
                Debug.DrawLine(eye, options[0].Item3, selectedColour);
            }
            
            // Debug.Log($"Priority {options[0].index} of value {options[0].value}");
            // for (int i = 0; i < options.Count; i++)
            // {
            //     Debug.Log($"option: {i}, dna: {options[0].index}, value: {options[0].value}");
            // }
        }
        else
        {
            move = _dnaGroups._movementDNA.GetGene((tagsToLookFor.Length * 2));
            height = _dnaGroups._heightDNA.GetGene((tagsToLookFor.Length * 2));
            Debug.DrawLine(eye, lightBulbPosition, Color.red);
        }

        moveFromDNAValue(move, height);
    }


    public (bool, GameObject) TestForTarget(Collider collider, List<GameObject> gameObjects)
    {
        return (tagsToLookFor.Contains(collider.tag), collider.gameObject);
    }
}
