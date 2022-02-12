using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Newtonsoft.Json;

public class PopulationManager : MonoBehaviour
{
    [Serializable]
    class Generation
    {
        public int generation;
        public List<string> dnaGroupsList;
        public float trialTime = 10;
        public int populationSize = 50;
        public float elapsed;
        public int survivors;
        public float mutationChance;
        public float furthestDistance;
        public string bestFitnessScore;
        public float bestPossibleScore;
        public Vector3 winningDeathPos;
        public int cloneIndex;
        public float AvgFitnessScore;
        public float AvgLastPossibleScore;
        public float AvgAliveTime;
        public float AvgDistance;
    }
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private int populationSize = 50;
    [SerializeField] private string fileName = "Pre Run Generation 300";
    private readonly List<GameObject> population = new List<GameObject>();
    private static float _elapsed;
    [SerializeField] private float trialTime = 10;
    private int generation = 1;
    private GUIStyle guiStyle = new GUIStyle();
    private int activeEthans;
    // private List<string> generationList = new List<string>();
    private List<Generation> _generations = new List<Generation>();
    private bool dictionaryData = false;
    [Range(0, 20)]
    [SerializeField] float gameSpeed;
    [SerializeField] private float deathSphereSpeed = 1f;
    [SerializeField] private float deathSphereStartTime = 30f;   
    [SerializeField] private float deathSphereStartSize = .5f; 
    [SerializeField] private DeathSphere sphere;
    [SerializeField] private GameObject star;
    private string scorePrint = "No Best";
    [SerializeField] private bool PrintScore = true;
    private float bestPosibleScore;
    private Vector3 winningDeathPos;
    private List<Brain> brains = new List<Brain>();
    private float furthestDistance = 0;
    private int cloneIndex;
    private float lastElapstTime;
    private float bestDistance;
    private float timer;
    private float AvgLastScore = 0f;
    private float AvgLastPossibleScore = 0f;
    private float AvgLastAliveTime = 0f;
    private float AvgLastDistance = 0f;
    public static event Action NewRound; 

    private TextFileHandler _textFileHandler;

    
    [Range(0f, 1f)] [SerializeField] private float mutationChance = 0.05f;

    void OnGUI()
    {
        guiStyle.fontSize = 25;
        guiStyle.normal.textColor = Color.white;
        GUI.BeginGroup(new Rect(10,10,800,500));
        GUI.Box(new Rect(0,0,800,500),$"Stats", guiStyle);
        GUI.Label(new Rect(10,25,300,30), $"Generation: {generation}",guiStyle);
        GUI.Label(new Rect(10,50,300,30), $"Population: {population.Count}",guiStyle);
        GUI.Label(new Rect(10,75,300,30), $"Mutation: {Mathf.RoundToInt(mutationChance * 100)}%",guiStyle);
        GUI.Label(new Rect(10,100,300,30), $"Time: {_elapsed:0.00}" + " / " 
                                                                   + $"Time: {trialTime:0.00}",guiStyle);
        GUI.Label(new Rect(10,125,800,30), $"Furthest Distance {bestDistance} / 155",guiStyle);
        
        GUI.Label(new Rect(10, 150, 300, 30),
            $"Alive: {activeEthans} / {Mathf.RoundToInt(((float)activeEthans / populationSize) * 100)}%", guiStyle);
        GUI.Label(new Rect(10,175,800,30), $"Last Best Distance: {Mathf.RoundToInt(furthestDistance)} / 155 / Avg: {AvgLastDistance}",guiStyle);
        GUI.Label(new Rect(10,200,800,30), $"Last Survival Time: {lastElapstTime:0.00} / {trialTime:0.00} / Avg: {AvgLastAliveTime:0.00}",guiStyle);
        GUI.Label(new Rect(10,225,800,30), $"{scorePrint} / Avg: {AvgLastScore}%",guiStyle);
        GUI.Label(new Rect(10,250,800,30), $"Last Possible Score: {Mathf.RoundToInt(bestPosibleScore * 100)}% / 100% / Avg: {AvgLastPossibleScore}%",guiStyle);
        
        GUI.EndGroup();
    }

    private void Start()
    {
        sphere.GetComponent<Renderer>().enabled = false;
        sphere.DrawSphere(deathSphereStartSize);
        _textFileHandler = new TextFileHandler(fileName);
        Brain.Dead += CountDead;
        (bool exists, string fileText) = _textFileHandler.GetFileText();
        if (exists)
        {
            _generations = JsonConvert.DeserializeObject<List<Generation>>(fileText);
            
            List<string> dnaValues = new List<string>();
            Generation gen = _generations[_generations.Count - 1];
            dnaValues = gen.dnaGroupsList;
            generation = gen.generation;
            trialTime = gen.trialTime;
            mutationChance = gen.mutationChance;
            furthestDistance = gen.furthestDistance;
            populationSize = gen.populationSize;
            scorePrint = gen.bestFitnessScore;
            bestPosibleScore = gen.bestPossibleScore;
            winningDeathPos = gen.winningDeathPos;
            Vector3 starPos = new Vector3(winningDeathPos.x, star.transform.position.y, winningDeathPos.z);
            cloneIndex = gen.cloneIndex;
            star.transform.position = starPos;
            lastElapstTime = gen.elapsed;
            AvgLastScore = gen.AvgFitnessScore;
            AvgLastPossibleScore = gen.AvgLastPossibleScore;
            AvgLastDistance = gen.AvgDistance;
            AvgLastAliveTime = gen.AvgAliveTime;
            
            CreateNewPopulation();
            for (var index = 0; index < brains.Count; index++)
            {
                GameObject pop = population[index];
                brains[index].dnaGroups = JsonConvert.DeserializeObject<Brain.DNAGroups>(dnaValues[index]);
                brains[index].Init();
            }
            brains[cloneIndex].StarActive(true);
        }
        else
        {
            CreateNewPopulation();
        }
        
    }

    void AddToGenerations(List<Brain> orderedPopulation)
    {
        dictionaryData = true;
        List<string> dnaValues = new List<string>();
        foreach (Brain pop in orderedPopulation)
        {
            string s = pop.GetDNAString();
            // Debug.Log(s);
            dnaValues.Add(s);
        }
        Generation gen = new Generation();
        gen.elapsed = _elapsed;
        gen.generation = generation;
        gen.survivors = activeEthans;
        gen.mutationChance = mutationChance;
        gen.populationSize = populationSize;
        gen.trialTime = trialTime;
        gen.dnaGroupsList = dnaValues;
        gen.furthestDistance = furthestDistance;
        gen.bestFitnessScore = scorePrint;
        gen.bestPossibleScore = bestPosibleScore;
        gen.winningDeathPos = winningDeathPos;
        gen.cloneIndex = cloneIndex;
        gen.elapsed = lastElapstTime;
        gen.AvgFitnessScore = AvgLastScore;
        gen.AvgLastPossibleScore = AvgLastPossibleScore;
        gen.AvgDistance = AvgLastDistance;
        gen.AvgAliveTime = AvgLastAliveTime;
        _generations.Add(gen);
        // _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_generations));
        
        
    }

    private void CreateNewPopulation()
    {
        
        activeEthans = populationSize;
        Vector3 randPos = RandomStartPosition(transform.position);
        for (int i = 0; i < populationSize; i++)
        {
            var position = randPos;
            var b = CreateEthan(position);
            population.Add(b);
            // _textFileHandler.AddTextToFile(b.GetComponent<Brain>().GetDNAString());
        }
    }

    private void OnApplicationQuit()
    {
        if (dictionaryData) _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_generations));
    }

    private void CountDead()
    {
        activeEthans--;
    }

    private GameObject CreateEthan(Vector3 position)
    {
        // var startingPosition = RandomStartPosition(position);
        GameObject b = Instantiate(botPrefab, position, transform.rotation);
        Brain brain = b.GetComponent<Brain>();
        brain.Init();
        brains.Add(brain);
        return b;
    }

    private static Vector3 RandomStartPosition(Vector3 position)
    {
        Vector3 startingPosition = new Vector3(position.x + Random.Range(-2, 2), position.y,
            position.z + Random.Range(-2, 2));
        return startingPosition;
    }

    Brain.DNAGroups Breed(Brain parent1, Brain parent2)
    {
        NewRound?.Invoke();
        // var position = transform.position;
        Brain.DNAGroups offspringDnaGroups = parent1.dnaGroups.CopyGeneGroupStructure();
        // var offspring = CreateEthan(position);
        // Brain brain = offspring.GetComponent<Brain>();
        offspringDnaGroups._movementDNAForwardBackward.Combine(parent1.dnaGroups._movementDNAForwardBackward,parent2.dnaGroups._movementDNAForwardBackward);
        offspringDnaGroups._movementDNALeftRight.Combine(parent1.dnaGroups._movementDNALeftRight,parent2.dnaGroups._movementDNALeftRight);
        offspringDnaGroups._movementDNATurn.Combine(parent1.dnaGroups._movementDNATurn,parent2.dnaGroups._movementDNATurn);
        offspringDnaGroups._priorityDNA.Combine(parent1.dnaGroups._priorityDNA,parent2.dnaGroups._priorityDNA);
        offspringDnaGroups._heightDNA.Combine(parent1.dnaGroups._heightDNA,parent2.dnaGroups._heightDNA);
        offspringDnaGroups._colourDNA.Combine(parent1.dnaGroups._colourDNA,parent2.dnaGroups._colourDNA);
        if (Random.Range(0f, 1f) < mutationChance)
        {
            switch (Random.Range(0, 6))
            {
                case 0:
                    offspringDnaGroups._movementDNAForwardBackward.Mutate();
                    Debug.Log("Mutation in Movement Forward Backwards");
                    break;
                case 1:
                    offspringDnaGroups._priorityDNA.Mutate();
                    Debug.Log("Mutation in priority");
                    break;
                case 2:
                    offspringDnaGroups._heightDNA.Mutate();
                    Debug.Log("Mutation in height");
                    break;
                case 3:
                    offspringDnaGroups._movementDNALeftRight.Mutate();
                    Debug.Log("Mutation in Movement Left Right");
                    break;
                case 4:
                    offspringDnaGroups._movementDNATurn.Mutate();
                    Debug.Log("Mutation in Movement Turn");
                    break;
                case 5:
                    offspringDnaGroups._colourDNA.Mutate();
                    Debug.Log("Mutation in Colour");
                    break;
            }
        }
        // _textFileHandler.AddTextToFile(brain.GetComponent<Brain>().GetDNAString());
        return offspringDnaGroups;
    }

    private void BreedNewPopulation()
    {
        generation++;
        activeEthans = populationSize;
        // List<Brain> brains = new List<Brain>();
        // foreach (GameObject brain in population)
        // {
        //     brains.Add(brain.GetComponent<Brain>());
        // }
        List<Brain> sortedList = brains.OrderBy(o => ((o.Distance))).ToList();
        winningDeathPos = sortedList[sortedList.Count - 1].transform.position;
        Vector3 starPos = new Vector3(winningDeathPos.x, star.transform.position.y, winningDeathPos.z);
        star.transform.position = starPos;
        // Brain.DNAGroups.PrintDNAColour(sortedList[sortedList.Count - 1].dnaGroups);
        // Brain.DNAGroups.PrintDNAColour(topGeneGroups);
        Debug.Log(sortedList[sortedList.Count-1].Distance);
        furthestDistance = sortedList[sortedList.Count - 1].Distance;
        sortedList = brains.OrderBy(o => ((((o.Distance / furthestDistance) * 5) + (o.timeAlive / _elapsed)) + o.GetBonus()) / 7).ToList();
        Brain.DNAGroups topGeneGroups = Brain.DNAGroups.Clone(sortedList[sortedList.Count - 1].dnaGroups);
        Brain brain = sortedList[sortedList.Count - 1];
        bestPosibleScore = ((brain.Distance / 150) + (brain.timeAlive / trialTime) + brain.GetBonus()) / 3;
        scorePrint = $"Score: {Mathf.RoundToInt(((((brain.Distance / furthestDistance) * 5) + (brain.timeAlive / _elapsed) + brain.GetBonus())/7) * 100)}%, " +
                     $"Dis: {Mathf.RoundToInt((brain.Distance / furthestDistance) * 100)}, Time: {Mathf.RoundToInt((brain.timeAlive / _elapsed) * 100)}, Bonus: {brain.GetBonus()}";
        float scores = 0f;
        float possibleScore = 0f;
        float timeAlive = 0f;
        float distance = 0f;
         foreach (Brain b in sortedList)
         {
             distance += b.Distance;
             timeAlive += b.timeAlive;
             float currentScore =
                 Mathf.RoundToInt(
                     ((((b.Distance / furthestDistance) * 5) + (b.timeAlive / _elapsed) + b.GetBonus()) / 7) * 100);
             scores += currentScore; 
             possibleScore += ((b.Distance / 150) + (b.timeAlive / trialTime) + b.GetBonus()) / 3;
             if (PrintScore) Debug.Log($"Score {currentScore}%, " +
                                     $"Dis: {Mathf.RoundToInt((b.Distance / furthestDistance) * 100)}, Time: {Mathf.RoundToInt((b.timeAlive / _elapsed) * 100)}, Bonus: {b.GetBonus()}");
        }

        AvgLastPossibleScore = Mathf.RoundToInt(possibleScore / sortedList.Count);
        AvgLastScore = Mathf.RoundToInt((scores / sortedList.Count));
        AvgLastDistance = Mathf.RoundToInt(distance / sortedList.Count);
        AvgLastAliveTime = timeAlive / sortedList.Count;
        
        AddToGenerations(sortedList);
        // population.Clear();
        List<Brain.DNAGroups> Offsping = new List<Brain.DNAGroups>();
        for (int i = (int) (sortedList.Count / 2.0f) - 1; i < sortedList.Count -1; i++)
        {
            Offsping.Add(Breed(sortedList[i], sortedList[i + 1]));
            Offsping.Add(Breed(sortedList[i + 1], sortedList[i]));
        }
        Vector3 randPos = RandomStartPosition(transform.position);
        
        for (int i = 0; i < sortedList.Count; i++)
        {
            brains[i].dnaGroups = Offsping[i];
            brains[i].DeathOnOff(true);
            brains[i].transform.position = randPos;
            brains[i].Init();
            brains[i].StarActive(false);
            // Destroy(sortedList[i]);
        }

        cloneIndex = Random.Range(0, brains.Count);
        brains[cloneIndex].dnaGroups = topGeneGroups;
        brains[cloneIndex].StarActive(true);
        brains[cloneIndex].Init();
        // Brain.DNAGroups.PrintDNAColour(brains[clone].dnaGroups);
    }
    
    private void BestBrain()
    {
        timer = 0f;
        List<Brain> orderedBrains = brains.OrderBy(o => (o.GetProgress())).ToList();
        bestDistance = orderedBrains[orderedBrains.Count-1].GetProgress();
    }

    private void Update()
    {
        Time.timeScale = gameSpeed;
        timer += Time.deltaTime;
        _elapsed += Time.deltaTime;
        if (timer >= .5f) BestBrain();
        if (_elapsed >= trialTime || activeEthans == 0)
        {
            lastElapstTime = _elapsed;
            sphere.GetComponent<Renderer>().enabled = false;
            sphere.DrawSphere(deathSphereStartSize);
            BreedNewPopulation();
            _elapsed = 0f;
        }
        if (_elapsed >= deathSphereStartTime)
        {
            sphere.GetComponent<Renderer>().enabled = true;
            float radius = sphere.CaptureRadius + (Time.deltaTime * deathSphereSpeed);
            sphere.DrawSphere(radius);
            Collider[] colliders = Physics.OverlapSphere(sphere.transform.position, radius);
            foreach (Collider collider in colliders)
            {
                try
                {
                    if (collider.attachedRigidbody.TryGetComponent(out Brain brain))
                    {
                        brain.SetDeath();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
        }
        
    }
}
