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
        public float BestDistance;
        public float bestFitnessScore;
        public float bestPossibleScore;
        public Vector3 winningDeathPos;
        public int cloneIndex;
        public float avgFitnessScore;
        public float avgPossibleScore;
        public float AvgAliveTime;
        public float avgDistance;
    }
    
    // Settings
    
    [SerializeField] private int populationSize = 50;
    [Range(0, 5)]
    [SerializeField] float gameSpeed;
    [SerializeField] private float deathSphereSpeed = 1f;
    [SerializeField] private float deathSphereStartTime = 30f;   
    [SerializeField] private float deathSphereStartSize = .5f; 
    [SerializeField] private float trialTime = 10;
    [Range(0f, 1f)] [SerializeField] private float mutationChance = 0.05f;
    [SerializeField] private bool PrintScore = true;
    [SerializeField] private float fitnessMultiplyBonus = 1;
    [SerializeField] private float fitnessMultiplyDistance = 1;
    [SerializeField] private float fitnessMultiplyTime = 1;
    [SerializeField] private bool ShowStats = true;

    // Now

    private int generation = 1;
    private float distance = 0;
    private static float _elapsed;
    private int activeEthans;
    private int cloneIndex;

    // Last
    
    private float lastBestFitnessScorePrint = 0f;
    private float lastAvgFitnessScore = 0f;
    private float lastBestDistance;
    private float lastAvgDistance = 0f;
    private float lastBestPosibleScore;
    private float lastAvgPossibleScore = 0f;
    private float lastElapstTime;
    private float lastAvgAliveTime = 0f;
    private float lastSurvivors = 0f;
    private float lastPopulationSize = 1f;
    private Vector3 winningDeathPos;

    // Top
    private float topFitnessScore = 0;
    private float topAvgFitnessScore = 0;
    private float topDistance = 0;
    private float topAvgDistance;
    private float topPossibleScore = 0;
    private float topAvgPossibleScore = 0;
    private float topTimeAlive = 0;
    private float topAvgTimeAlive = 0;
    private float topSurvivors = 0;
    private float topAvgSurvivors = 0;
    private float topSurvivorsPopulationSize = 1f;
    
    // Event
    public static event Action NewRound;

    // References
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private string fileName = "Pre Run Generation 300";
    [SerializeField] private DeathSphere sphere;
    [SerializeField] private GameObject star;
    
    private readonly List<GameObject> population = new List<GameObject>();
    private GUIStyle guiStyle = new GUIStyle();
    private List<Generation> _generations = new List<Generation>();
    private bool dictionaryData = false;
    private List<Brain> brains = new List<Brain>();
    private TextFileHandler _textFileHandler;
    private float timer;

    void OnGUI()
    {
        if (!ShowStats) return;
        // guiStyle = new GUIStyle(GUI.skin.box);
        guiStyle.fontSize = 25;
        guiStyle.normal.textColor = Color.white;
        // guiStyle.normal.background = Texture2D.CreateExternalTexture(1500, 1500, TextureFormat.Alpha8, false, true, IntPtr.Zero);
        GUI.BeginGroup(new Rect(10,10,1500,1500));
        GUI.Box(new Rect(0,0,1500,100),$"Settings", guiStyle);
        GUI.Label(new Rect(10,25,300,30), $"Generation: {generation}",guiStyle);
        GUI.Label(new Rect(10,50,300,30), $"Population: {population.Count}",guiStyle);
        GUI.Label(new Rect(10,75,300,30), $"Mutation: {Mathf.RoundToInt(mutationChance * 100)}%",guiStyle);
        // GUI.Label(new Rect(10,100,300,30), $"Time: {_elapsed:0.00}" + " / " 
                                                                   // + $"Time: {trialTime:0.00}",guiStyle);
        // GUI.Label(new Rect(10,125,800,30), $"Furthest Distance {lastBestDistance} / 155",guiStyle);
        GUI.Box(new Rect(0,125,1500,700),$"Stats", guiStyle);
        GUI.contentColor = new Color(0.81f, 1f, 0.71f);
        
        GUI.Label(new Rect(10,150,1500,30), $"Survivors: Now: {activeEthans}/" +
                                            $"{Mathf.RoundToInt(((float)activeEthans / populationSize) * 100)}% /" +
                                            $" Last : {lastSurvivors}/" +
                                            $"{Mathf.RoundToInt(((float)lastSurvivors / lastPopulationSize) * 100)}% /" +
                                            $" Top: {topSurvivors}/" +
                                            $"{Mathf.RoundToInt(((float)topSurvivors / topSurvivorsPopulationSize) * 100)}%", guiStyle);
        GUI.contentColor = new Color(0.6f, 0.63f, 1f);
        GUI.Label(new Rect(10,200,1500,30), $"Distance: Now: {Mathf.RoundToInt(lastBestDistance)}/155" +
                                            $" Last: {Mathf.RoundToInt(distance)} /" +
                                            $" LastAvg: {Mathf.RoundToInt(lastAvgDistance)}" +
                                            $" Top: {Mathf.RoundToInt(topDistance)} /" +
                                            $" TopAvg: {Mathf.RoundToInt(topAvgDistance)}",guiStyle);
        GUI.contentColor = new Color(1f, 0.99f, 0.67f);
        GUI.Label(new Rect(10,250,1500,30), $"Survival Time: Now: {_elapsed:0.00}/{trialTime:0.00} /" +
                                            $" Last: {lastElapstTime:0.00} /" +
                                            $" LastAvg: {lastAvgAliveTime:0.00}" +
                                            $" Top: {topTimeAlive:0.00} /" +
                                            $" TopAvg: {topAvgTimeAlive:0.00}",guiStyle);
        GUI.contentColor = new Color(0.7f, 0.51f, 0.61f);
        GUI.Label(new Rect(10,300,1500,30), $"Fitness Score: Now: N/A /" +
                                            $" Last: {lastBestFitnessScorePrint}% /" +
                                            $" LastAvg: {lastAvgFitnessScore}% /" +
                                            $" Top: {topFitnessScore}% /" +
                                            $" TopAvg: {topAvgFitnessScore}%",guiStyle);
        GUI.contentColor = new Color(0.99f, 0.8f, 0.48f);
        GUI.Label(new Rect(10,350,1500,30), $"Possible Score: Now: N/A /" +
                                            $" Last: {Mathf.RoundToInt(lastBestPosibleScore * 100)}% /" +
                                            $" LastAvg: {Mathf.RoundToInt(lastAvgPossibleScore * 100)}%"+
                                            $" Top: {Mathf.RoundToInt(topPossibleScore * 100)}% /" +
                                            $" TopAvg: {Mathf.RoundToInt(topAvgPossibleScore * 100)}%",guiStyle);
        
        GUI.EndGroup();
    }

    void SetBest(List<Generation> generations)
    {
        foreach (Generation generation in generations)
        {
            CheckForBest(generation);
        }
    }

    private void CheckForBest(Generation generation)
    {

    // private float topTimeAlive = 0;
    // private float topAvgTimeAlive = 0;
        if (generation.bestFitnessScore > topFitnessScore) topFitnessScore = generation.bestFitnessScore;
        if (generation.avgFitnessScore > topAvgFitnessScore) topAvgFitnessScore = generation.avgFitnessScore;
        if (generation.BestDistance > topDistance) topDistance = generation.BestDistance;
        if (generation.avgDistance > topAvgDistance) topAvgDistance = generation.avgDistance;
        if (generation.bestPossibleScore > topPossibleScore) topPossibleScore = generation.bestPossibleScore;
        if (generation.avgPossibleScore > topAvgPossibleScore) topAvgPossibleScore = generation.avgPossibleScore;
        if (generation.elapsed > topTimeAlive) topTimeAlive = generation.elapsed;
        if (generation.AvgAliveTime > topAvgTimeAlive) topAvgTimeAlive = generation.AvgAliveTime;
        if (generation.survivors >= topSurvivors)
        {
            topSurvivors = generation.survivors;
            topSurvivorsPopulationSize = generation.populationSize;
        }
        if ((generation.survivors / generation.populationSize) >= topAvgSurvivors) topAvgSurvivors = (generation.survivors / generation.populationSize);
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
            distance = gen.BestDistance;
            populationSize = gen.populationSize;
            lastBestFitnessScorePrint = gen.bestFitnessScore;
            lastBestPosibleScore = gen.bestPossibleScore;
            winningDeathPos = gen.winningDeathPos;
            Vector3 starPos = new Vector3(star.transform.position.x, star.transform.position.y, winningDeathPos.z);
            cloneIndex = gen.cloneIndex;
            star.transform.position = starPos;
            lastElapstTime = gen.elapsed;
            lastAvgFitnessScore = gen.avgFitnessScore;
            lastAvgPossibleScore = gen.avgPossibleScore;
            lastAvgDistance = gen.avgDistance;
            lastAvgAliveTime = gen.AvgAliveTime;
            lastSurvivors = gen.survivors;
            lastPopulationSize = gen.populationSize;
            
            CreateNewPopulation();
            for (var index = 0; index < brains.Count; index++)
            {
                GameObject pop = population[index];
                brains[index].dnaGroups = JsonConvert.DeserializeObject<Brain.DNAGroups>(dnaValues[index]);
                brains[index].Init();
            }
            brains[cloneIndex].StarActive(true);
            SetBest(_generations);
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
        gen.BestDistance = distance;
        gen.bestFitnessScore = lastBestFitnessScorePrint;
        gen.bestPossibleScore = lastBestPosibleScore;
        gen.winningDeathPos = winningDeathPos;
        gen.cloneIndex = cloneIndex;
        gen.elapsed = lastElapstTime;
        gen.avgFitnessScore = lastAvgFitnessScore;
        gen.avgPossibleScore = lastAvgPossibleScore;
        gen.avgDistance = lastAvgDistance;
        gen.AvgAliveTime = lastAvgAliveTime;
        _generations.Add(gen);
        CheckForBest(gen);
        // _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_generations));
        
        
    }

    private void CreateNewPopulation()
    {
        Vector3 randPos = RandomStartPosition(transform.position);
        for (int i = 0; i < populationSize; i++)
        {
            var position = randPos;
            var b = CreateEthan(position);
            population.Add(b);
            // _textFileHandler.AddTextToFile(b.GetComponent<Brain>().GetDNAString());
        }
        activeEthans = populationSize;
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
            
            (int index, int oldValue, int newValue) dnaDetails = (0,0,0);
            DNA dnaMutate = new DNA();
            switch (Random.Range(0, 6))
            {
                case 0:
                    dnaMutate = offspringDnaGroups._movementDNAForwardBackward;
                    break;
                case 1:
                    dnaMutate = offspringDnaGroups._priorityDNA;
                    break;
                case 2:
                    dnaMutate = offspringDnaGroups._heightDNA;
                    break;
                case 3:
                    dnaMutate = offspringDnaGroups._movementDNALeftRight;
                    break;
                case 4:
                    dnaMutate = offspringDnaGroups._movementDNATurn;
                    break;
                case 5:
                    dnaMutate = offspringDnaGroups._colourDNA;
                    break;
            }

            try
            {
                dnaDetails = dnaMutate.Mutate();
            }
            catch (Exception e)
            {
                Debug.Log($"issue during mutation: {e.Message} {e.Source}");
            }
            try
            {
                string printString = "Mutation ";
                printString += Brain.DNAInfo(parent1.tagsToLookFor.ToList(),
                    dnaMutate,
                    dnaDetails.index, dnaDetails.oldValue);
                printString += " Has now been changed to ";
                printString += Brain.DNAInfo(parent1.tagsToLookFor.ToList(),
                    dnaMutate,
                    dnaDetails.index, dnaDetails.newValue);
                Debug.Log(printString);
            }
            catch (Exception e)
            {
                Debug.Log($"issue during mutation information from DNA: {e.Message} {e.Source}");
            }
            
            
            
        }
        // _textFileHandler.AddTextToFile(brain.GetComponent<Brain>().GetDNAString());
        return offspringDnaGroups;
    }

    float GetFitnessScore(Brain brain)
    {
        float divScore = fitnessMultiplyDistance + fitnessMultiplyTime + fitnessMultiplyBonus;
        return Mathf.RoundToInt(((((brain.Distance / distance) * fitnessMultiplyDistance) +
                                  (brain.timeAlive / _elapsed) +
                                  (brain.GetBonus() / FindObjectsOfType<CheckPoint>().Length)) / divScore) * 100);
    }

    private void BreedNewPopulation()
    {
        generation++;
        Debug.Log($"Gen {generation}: ");
        
        List<Brain> sortedList = brains.OrderBy(o => ((o.Distance))).ToList();
        winningDeathPos = sortedList[sortedList.Count - 1].transform.position;
        Vector3 starPos = new Vector3(winningDeathPos.x, star.transform.position.y, winningDeathPos.z);
        star.transform.position = starPos;
        // Debug.Log($"Max Distance {sortedList[sortedList.Count-1].Distance}");
        this.distance = sortedList[sortedList.Count - 1].Distance;
        sortedList = brains.OrderBy(o => ((GetFitnessScore(o)))).ToList();
        // float divScore = fitnessMultiplyDistance + fitnessMultiplyTime + fitnessMultiplyBonus;
        // sortedList = brains.OrderBy(o =>
        //     ((((o.Distance / furthestDistance) * fitnessMultiplyDistance) +
        //       ((o.timeAlive / _elapsed) * fitnessMultiplyTime)) +
        //      ((o.GetBonus() / FindObjectsOfType<CheckPoint>().Length)) * fitnessMultiplyBonus) / divScore).ToList();
        Brain.DNAGroups topGeneGroups = Brain.DNAGroups.Clone(sortedList[sortedList.Count - 1].dnaGroups);
        Brain brain = sortedList[sortedList.Count - 1];
        lastBestPosibleScore = UnbiasPosibleScore(brain);
        lastBestFitnessScorePrint = GetFitnessScore(brain);
        float scores = 0f;
        float possibleScore = 0f;
        float timeAlive = 0f;
        float distance = 0f;
         foreach (Brain b in sortedList)
         {
             distance += b.Distance;
             timeAlive += b.timeAlive;
             float currentScore = GetFitnessScore(b);
             scores += currentScore; 
             possibleScore += ((b.Distance / 150) + (b.timeAlive / trialTime) + b.GetBonus()) / 3;
             if (PrintScore) Debug.Log($"Score {currentScore}%, " +
                                     $"Dis: {Mathf.RoundToInt((b.Distance / this.distance) * 100)}, Time: {Mathf.RoundToInt((b.timeAlive / _elapsed) * 100)}, Bonus: {(b.GetBonus() / FindObjectsOfType<CheckPoint>().Length)}");
        }

        lastAvgPossibleScore = Mathf.RoundToInt(possibleScore / sortedList.Count);
        lastAvgFitnessScore = Mathf.RoundToInt((scores / sortedList.Count));
        lastAvgDistance = Mathf.RoundToInt(distance / sortedList.Count);
        lastAvgAliveTime = timeAlive / sortedList.Count;
        
        AddToGenerations(sortedList);
        // population.Clear();
        List<Brain.DNAGroups> offspring = new List<Brain.DNAGroups>();
        for (int i = (int) (sortedList.Count / 2.0f) - 1; i < sortedList.Count -1; i++)
        {
            offspring.Add(Breed(sortedList[i], sortedList[i + 1]));
            offspring.Add(Breed(sortedList[i + 1], sortedList[i]));
        }
        Vector3 randPos = RandomStartPosition(transform.position);
        
        for (int i = 0; i < sortedList.Count; i++)
        {
            brains[i].dnaGroups = offspring[i];
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
        activeEthans = populationSize;
        // Brain.DNAGroups.PrintDNAColour(brains[clone].dnaGroups);
    }

    private float UnbiasPosibleScore(Brain brain)
    {
        return ((brain.Distance / 150) + (brain.timeAlive / trialTime) + (brain.GetBonus()) / FindObjectsOfType<CheckPoint>().Length);
    }

    private void BestBrain()
    {
        timer = 0f;
        List<Brain> orderedBrains = brains.OrderBy(o => (o.GetProgress())).ToList();
        lastBestDistance = orderedBrains[orderedBrains.Count-1].GetProgress();
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
