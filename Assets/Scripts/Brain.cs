using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class Brain : MonoBehaviour, ITestForTarget
{
    public enum DNAType { Priority, Height, MoveForwardOrBackward, MoveRightLeftOrRight, TurnLeftOrRight, Colour }

    private enum Options {  CanSeeLeft, CanSeeRight, CanSeeCentre, CantSeeLeft, CantSeeRight, CantSeeCentre, CantSeeItUnsureWhereabouts, CantSeeAnything } 
    // cant see anything doesnt need to be multiplied by visible objects

    [Serializable]
    public class DNAGroups
    {
        public DNA movementDnaForwardBackward;
        public DNA heightDna;
        public DNA movementDnaLeftRight;
        public DNA movementDnaTurn;
        public DNA colourDna;
        public DNA priorityDna;

        public DNAGroups CopyGeneGroupStructure() => new DNAGroups() {
            movementDnaForwardBackward = DNA.CopyType(movementDnaForwardBackward, false),
            heightDna = DNA.CopyType(heightDna, false),
            movementDnaLeftRight = DNA.CopyType(movementDnaLeftRight, false),
            movementDnaTurn = DNA.CopyType(movementDnaTurn, false),
            priorityDna = DNA.CopyType(priorityDna, false),
            colourDna = DNA.CopyType(colourDna, false)
        };

        static public DNAGroups Clone(DNAGroups dndGroupOriginal)
        {
            DNAGroups dnaGroupCopy = new DNAGroups();
            dnaGroupCopy.movementDnaForwardBackward = DNA.CopyType(dndGroupOriginal.movementDnaForwardBackward, true);
            dnaGroupCopy.heightDna = DNA.CopyType(dndGroupOriginal.heightDna, true);
            dnaGroupCopy.movementDnaLeftRight = DNA.CopyType(dndGroupOriginal.movementDnaLeftRight, true);
            dnaGroupCopy.movementDnaTurn = DNA.CopyType(dndGroupOriginal.movementDnaTurn, true);
            dnaGroupCopy.priorityDna = DNA.CopyType(dndGroupOriginal.priorityDna, true);
            dnaGroupCopy.colourDna = DNA.CopyType(dndGroupOriginal.colourDna, true);
            return dnaGroupCopy;
        }
        public static void PrintDnaColour(DNAGroups dnaGroups)
        {
            float r = dnaGroups.colourDna.genes[0] / 100f;
            float g = dnaGroups.colourDna.genes[1] / 100f;
            float b = dnaGroups.colourDna.genes[2] / 100f;
            Color dnaColour = new Color(r, g, b);
            // Debug.Log($"<color=dnaColour>r{r} :  g:{g}  b:{b} </color>");
            Debug.Log (string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(dnaColour.r * 255f), (byte)(dnaColour.g * 255f), (byte)(dnaColour.b * 255f), $"r{r} :  g:{g}  b:{b}"));
        }
    }
    public float timeAlive;
    public DNAGroups dnaGroups;
    private ThirdPersonCharacter _character;
    private Vector3 _move;
    private bool _alive = true;
    public static event Action Dead;
    private Vector3 _startPos;
    private Vector3 _endPos;
    [SerializeField] private GameObject[] turnOffOnDeath;
    [SerializeField] public string[] tagsToLookFor;
    private readonly List<Color> _coloursOfTaggedItems = new List<Color>();
    [SerializeField] private string[] tagsForDie;
    [SerializeField] private GameObject lightBulb;
    [SerializeField] private RenderColourChanger ethanColour;
    [SerializeField] private GameObject star;
    private float _bonus;
    private Vector3 deathLocation;
    [SerializeField] private ParticleSystem[] explosionFX;
    [SerializeField] private ParticleSystem[] winFX;
    [SerializeField] private GameObject dnaGameObject;

    public Vector3 GetDeathLocation()
    {
        if (_alive)
        {
            deathLocation = transform.position;
        }

        return deathLocation;
    }

    public void Bonus()
    {
        foreach (ParticleSystem fx in winFX)
        {
            fx.Play();
        }
    }
    public static string DNAInfo(List<string> tags, DNA dna, int dnaIndex, int dnaValue)
    {
        string dnaMeaning = $"";
        switch (dna.dnaType)
        {
            case DNAType.Colour:
                switch (dnaIndex)
                {
                    case 0:
                        dnaMeaning += $"colour R: {(float)dnaValue / 100}";
                        break;
                    case 1:
                        dnaMeaning += $"colour G: {(float)dnaValue / 100}";
                        break;
                    case 2:
                        dnaMeaning += $"colour B: {(float)dnaValue / 100}";
                        break;
                }
                return dnaMeaning;
            case DNAType.Height:
                switch (dnaValue)
                {
                    case 0:
                        dnaMeaning += "Height Normal";
                        break;
                    case 1:
                        dnaMeaning += "Height Crouch";
                        break;
                    case 2:
                        dnaMeaning += "Height Jump";
                        break;
                }
                break;
            case DNAType.Priority:
                dnaMeaning += $"Priority {dnaValue}";
                break;
            case DNAType.MoveForwardOrBackward:
                switch (dnaValue)
                {
                    case 0:
                        dnaMeaning += "Stop Forward Backward";
                        break;
                    case 1:
                        dnaMeaning += "Move Forward";
                        break;
                    case 2:
                        dnaMeaning += "Move Backward";
                        break;
                }
                break;
            case DNAType.MoveRightLeftOrRight:
                switch (dnaValue)
                {
                    case 0:
                        dnaMeaning += "Stop Right Left";
                        break;
                    case 1:
                        dnaMeaning += "Move Right";
                        break;
                    case 2:
                        dnaMeaning += "Move Left";
                        break;
                }
                break;
            case DNAType.TurnLeftOrRight:
                switch (dnaValue)
                {
                    case 0:
                        dnaMeaning += "Stop Turn";
                        break;
                    case 1:
                        dnaMeaning += "Turn Right";
                        break;
                    case 2:
                        dnaMeaning += "Turn Left";
                        break;
                }
                break;
        }
        
        int options = (Enum.GetValues(typeof(Options)).Length - 1);
        int numberOfDNAOptions = options * tags.Count;
        if (dnaIndex > (numberOfDNAOptions)) return $"On Can't see anything: {dnaMeaning}";
        Options option;
        string tagString;
        try
        {
            option = (Options) (dnaIndex % options);
        }
        catch (Exception e)
        {
            Debug.Log($"Issue with getting DNA option: {e.Message} {e.Source}");
            return "Error in DNA information";
        }
        try
        {
            tagString = tags[dnaIndex % tags.Count];
        }
        catch (Exception e)
        {
            Debug.Log($"Issue with getting DNA object type tag: {e.Message} {e.Source}");
            return "Error in DNA information";
        }
        return $"on {option} {tagString}: {dnaMeaning}";
    }

    public bool GetIsAlive()
    {
        return _alive;
    }
    public void AddHitBonus(float val)
    {
        _bonus += val;
    }

    public void SetDeath()
    {
        if (_alive)
        {
            dnaGameObject.SetActive(false);
            _alive = false;
            Dead?.Invoke();
            deathLocation = transform.position;
            SetEndPosition();
            DeathOnOff(false);
            StarActive(false);
            foreach (ParticleSystem fx in explosionFX)
            {
                fx.Play();
            }

        }
    }

    public void ActivateMutant()
    {
        dnaGameObject.SetActive(true);
    }

    public float GetBonus()
    {
        return _bonus;
    }

    public void StarActive(bool on)
    {
        star.SetActive(on);
    }

    public float GetProgress()
    {
        if (_alive)
        {
            // Vector3 vector31 = new Vector3(0, 0, startPos.z);
            // Vector3 vector32 = new Vector3(0, 0, transform.position.z);
            // return Vector3.Distance(vector31, vector32);
            float distance = transform.position.z - _startPos.z;
            if (distance > 500)
            {
                _alive = false;
                return 0f;
            }
            return distance;
            
        }
        return 0f;
    }
    public float Distance
    {
        get => GetDistanceTraveled();
    }

    private void Awake()
    {
        _character = GetComponent<ThirdPersonCharacter>();
        foreach (string s in tagsToLookFor)
        {
            if (s == "Ethan")
            {
                _coloursOfTaggedItems.Add(new Color(0.29f, 0.09f, 0.5f));
                continue;
            }
            try
            {
                _coloursOfTaggedItems.Add(GameObject.FindGameObjectWithTag(s).GetComponent<MeshRenderer>().material.color);
            }
            catch (Exception e)
            {
                _coloursOfTaggedItems.Add(Color.grey);
                Console.WriteLine(e);
            }
        }
        
        int numberOfDNAOptions = (Enum.GetValues(typeof(Options)).Length - 1) * tagsToLookFor.Length;
        dnaGroups.movementDnaForwardBackward = new DNA((numberOfDNAOptions) + 1, 3, DNAType.MoveForwardOrBackward);
        dnaGroups.movementDnaLeftRight = new DNA((numberOfDNAOptions) + 1, 3, DNAType.MoveRightLeftOrRight);
        dnaGroups.movementDnaTurn = new DNA((numberOfDNAOptions) + 1, 3, DNAType.TurnLeftOrRight);
        dnaGroups.heightDna = new DNA((numberOfDNAOptions) + 1, 3, DNAType.Height);
        dnaGroups.priorityDna = new DNA((numberOfDNAOptions), 101, DNAType.Priority);
        dnaGroups.colourDna = new DNA((3), 101, DNAType.Colour);
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
            SetDeath();
            // _alive = false;
            // Dead?.Invoke();
            // SetEndPosition();
            // DeathOnOff(false);
        }
    }

    public void DeathOnOff(bool on)
    {
        foreach (GameObject gObject in turnOffOnDeath)
        {
            gObject.SetActive(on);
        }
    }

    private void SetEndPosition()
    {
        _endPos = transform.position;
    }

    public float GetDistanceTraveled()
    {
        if (_alive) SetEndPosition();
        float distance = _endPos.z - _startPos.z;
        if (distance > 500)
        {
            return 0f;
        }
        return distance;
    }

    // public override string ToString()
    // {
    //     return base.ToString();
    // }

    public void Init()
    {
        foreach (ParticleSystem fx in explosionFX)
        {
            fx.Stop();
        }
        foreach (ParticleSystem fx in winFX)
        {
            fx.Stop();
        }

        _bonus = 0f;
        var position = transform.position;
        _startPos = position;
        _startPos = position;
        _move = position;
        timeAlive = 0;
        _alive = true;
        float r = dnaGroups.colourDna.genes[0] / 100f;
        float g = dnaGroups.colourDna.genes[1] / 100f;
        float b = dnaGroups.colourDna.genes[2] / 100f;
        
        ethanColour.ChangeColor(new Color(r, g, b));
    }

    
    private void MoveFromDNAValue(int movementForwardBackwardValue,int heightValue, int movementLeftRight, int movementTurn)
    {
        float v = 0f;
        float h = 0f;
        bool crouch = false;
        bool jump = false;
        float r = 0;

        switch (movementForwardBackwardValue)
        {
            case 0: //  -VAL 0 = Stop
                v = 0;
                break;
            case 1: //  -VAL 1 = Move Forward
                v = 1;
                break;
            case 2: //  -VAL 2 = Move Backward
                v = -1;
                break;
        }
        switch (movementLeftRight)
        {
            case 0: //  -VAL 0 = Stop
                h = 0;
                break;
            case 1: //  -VAL 1 = Move Right
                h = 1;
                break;
            case 2: //  -VAL 2 = Move Left
                h = -1;
                break;
        }
        switch (movementTurn)
        {
            case 0: //  -VAL 0 = No Turn
                r = 0;
                break;
            case 1: //  -VAL 1 = Turn Right
                r = 90;
                break;
            case 2: //  -VAL 2 = Turn Left
                r = -90;
                break;
        }
        
        switch (heightValue)
        {
            case 0: //  -VAL 0 = NORMAL
                break;
            case 1: //  -VAL 1 = CROUCH
                crouch = true;
                break;
            case 2: //  -VAL 2 = JUMP
                jump = true;
                break;
        }
        
        _move = v * Vector3.forward + h * Vector3.right;
        Vector3 rotation = new Vector3(0, r * .5f * Time.deltaTime, 0);
        _character.transform.Rotate(rotation);
        _character.Move(_move,crouch,jump);

    }


    private void FixedUpdate()
    {
        if (!_alive) return;
        if (transform.position.y <= -5) SetDeath();
        List<GameObject> seen = new List<GameObject>();
        List<Vector3> vector3S = new List<Vector3>();
        Vector3 eye = new Vector3();
        foreach (FieldOfView fieldOfView in GetComponents<FieldOfView>())
        {
            eye = fieldOfView.Eye.transform.position;
            (List<GameObject> gameObjects, List<Vector3> objectPositions) = fieldOfView.FindVisibleTargets(this);
            seen.AddRange(gameObjects);
            vector3S.AddRange(objectPositions);
        }
        MakeDecision(seen, eye, vector3S);
        if (_alive) timeAlive += Time.deltaTime;

    }

    (bool, GameObject) FindClosestTag(string tagString)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(tagString);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        bool found = false;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            found = true;
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return (found, closest);
    }
    private void MakeDecision(List<GameObject> seen, Vector3 eye, List<Vector3> pos)
    {
        int moveFb = 0;
        int height = 0;
        int moveLr = 0;
        int moveT = 0;
        Vector3 lightBulbPosition = lightBulb.transform.position;
        List<string> seenObjects = new List<string>();
        if (seen.Count > 0)
        {
            List<(int index, int value)> dnaPos = new List<(int, int)>();
            var vals = dnaGroups.priorityDna.GetGenes();
            for (var index = 0; index < vals.Count; index++)
            {
                int val = vals[index];
                dnaPos.Add((index, val));
            }
            dnaPos.Sort((x, y) => y.value.CompareTo(x.value));
            List<(int index, float value, Vector3 pos)> options = new List<(int, float, Vector3)>();
            try
            {
                AddSeenObjects(seen, pos, seenObjects, options);
            }
            catch (Exception e)
            {
                Console.WriteLine($"See Options {e}");
            }

            try
            {
                AddNotSeenOptions(seenObjects, options, lightBulbPosition);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Not See Options {e}");
            }
            
            
            options.Sort((x, y) => y.value.CompareTo(x.value));
            moveFb = dnaGroups.movementDnaForwardBackward.GetGene(options[0].index);
            height = dnaGroups.heightDna.GetGene(options[0].index);
            moveLr = dnaGroups.movementDnaLeftRight.GetGene(options[0].index);
            moveT = dnaGroups.movementDnaTurn.GetGene(options[0].index);
            Color selectedColour = Color.white;
            if (options[0].index >= (tagsToLookFor.Length * 3))
            {
                lightBulb.GetComponent<RenderColourChanger>().ChangeColor(_coloursOfTaggedItems[options[0].index - (tagsToLookFor.Length * 3)]);
            }
            else
            {
                lightBulb.GetComponent<RenderColourChanger>().ChangeColor(_coloursOfTaggedItems[options[0].index % tagsToLookFor.Length]);
                Debug.DrawLine(eye, options[0].pos, selectedColour);
            }

        }
        else
        {
            moveFb = CantSeeAnything(eye, lightBulbPosition, out height, out moveLr, out moveT);
        }

        try
        {
            MoveFromDNAValue(moveFb, height, moveLr, moveT);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Movement {e}");
            throw;
        }
        
    }

    private int CantSeeAnything(Vector3 eye, Vector3 lightBulbPosition, out int height, out int moveLr, out int moveT)
    {
        int moveFb;
        lightBulb.GetComponent<RenderColourChanger>().ChangeColor(Color.black);
        moveFb = dnaGroups.movementDnaForwardBackward.GetGene((tagsToLookFor.Length * 7));
        height = dnaGroups.heightDna.GetGene((tagsToLookFor.Length * 7));
        moveLr = dnaGroups.movementDnaLeftRight.GetGene((tagsToLookFor.Length * 7));
        moveT = dnaGroups.movementDnaTurn.GetGene((tagsToLookFor.Length * 7));
        Debug.DrawLine(eye, lightBulbPosition, Color.red);
        return moveFb;
    }

    private void AddNotSeenOptions(List<string> seenObjects, List<(int index, float value, Vector3 pos)> options, Vector3 lightBulbPosition)
    {
        for (var i = 0; i < tagsToLookFor.Length; i++)
        {
            if (!seenObjects.Contains(tagsToLookFor[i]))
            {
                int index = (tagsToLookFor.Length * 3) + i;
                float distance = Mathf.Infinity;
                int geneIndex = i + (tagsToLookFor.Length * 6);
                (bool found, GameObject taggedObject) = FindClosestTag(tagsToLookFor[i]);
                if (found)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, taggedObject.transform.position,
                        out hit, Mathf.Infinity)) distance = Vector3.Distance(transform.position, hit.transform.position);
                    float dir = FieldOfView.AngleDir(transform.forward, hit.transform.position,
                        transform.up);
                    
                    switch (dir)
                    {
                        case 0f:
                            geneIndex = i + (tagsToLookFor.Length * 3);
                            break;
                        case -1f:
                            geneIndex = i + (tagsToLookFor.Length * 4);
                            break;
                        case 1f:
                            geneIndex = i + (tagsToLookFor.Length * 5);
                            break;
                    }
                }
                

                options.Add((index, dnaGroups.priorityDna.GetGene(geneIndex) / distance, lightBulbPosition));
            }
        }
    }

    private void AddSeenObjects(List<GameObject> seen, List<Vector3> pos, List<string> seenObjects, List<(int index, float value, Vector3 pos)> options)
    {
        for (int index = 0; index < seen.Count; index++)
        {
            GameObject visibleObject = seen[index];
            for (var i = 0; i < tagsToLookFor.Length; i++)
            {
                string stringTag = tagsToLookFor[i];
                if (visibleObject.CompareTag(stringTag))
                {
                    int geneIndex = i;
                    seenObjects.Add(tagsToLookFor[i]);
                    float dir = FieldOfView.AngleDir(transform.forward, visibleObject.transform.position,
                        transform.up);
                    switch (dir)
                    {
                        case 0f:
                            geneIndex = i;
                            break;
                        case -1f:
                            geneIndex = i + (tagsToLookFor.Length);
                            break;
                        case 1f:
                            geneIndex = i + (tagsToLookFor.Length * 2);
                            break;
                    }

                    options.Add((geneIndex,
                        dnaGroups.priorityDna.GetGene(geneIndex) /
                        Vector3.Distance(transform.position, visibleObject.transform.position), pos[index]));
                }
            }
        }
    }


    public (bool, GameObject) TestForTarget(Collider colliderTarget, List<GameObject> gameObjects)
    {
        try
        {
            if (colliderTarget.attachedRigidbody == GetComponent<Rigidbody>())
            {
                return (false, colliderTarget.gameObject);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return (tagsToLookFor.Contains(colliderTarget.tag), colliderTarget.gameObject);
    }

    public void DeactivateMutant()
    {
        dnaGameObject.SetActive(false);
    }
}
