using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        public float bestDistance;
        public float bestFitnessScore;
        public float bestPossibleScore;
        public Vector3 winningDeathPos;
        public int cloneIndex;
        public float avgFitnessScore;
        public float avgPossibleScore;
        public float avgAliveTime;
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
    [SerializeField] private bool printScore = true;
    [SerializeField] private float fitnessMultiplyBonus = 1;
    [SerializeField] private float fitnessMultiplyDistance = 1;
    [SerializeField] private float fitnessMultiplyTime = 1;
    [SerializeField] private bool showStats = true;

    // Now

    private int _generation = 1;
    private float _distance;
    private static float _elapsed;
    private int _activeBots;
    private int _cloneIndex;

    // Last
    
    private float _lastBestFitnessScorePrint;
    private float _lastAvgFitnessScore;
    private float _lastBestDistance;
    private float _lastAvgDistance;
    private float _lastBestPossibleScore;
    private float _lastAvgPossibleScore;
    private float _lastTimeAlive;
    private float _lastAvgAliveTime;
    private float _lastSurvivors;
    private float _lastPopulationSize = 1f;
    private Vector3 _winningDeathPos;

    // Top
    private float _topFitnessScore;
    private float _topAvgFitnessScore;
    private float _topDistance;
    private float _topAvgDistance;
    private float _topPossibleScore;
    private float _topAvgPossibleScore;
    private float _topTimeAlive;
    private float _topAvgTimeAlive;
    private float _topSurvivors;
    private float _topAvgSurvivors;
    private float _topSurvivorsPopulationSize = 1f;
    
    // Event
    public static event Action NewRound;

    // References
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private string fileName = "Pre Run Generation 300";
    [SerializeField] private DeathSphere sphere;
    [SerializeField] private GameObject star;
    
    private readonly List<GameObject> _population = new List<GameObject>();
    private readonly GUIStyle _guiStyle = new GUIStyle();
    private List<Generation> _generations = new List<Generation>();
    private bool _generationData;
    private readonly List<Brain> _brains = new List<Brain>();
    private TextFileHandler _textFileHandler;
    private float _timer;

    void OnGUI()
    {
        if (!showStats) return;
        // guiStyle = new GUIStyle(GUI.skin.box);
        _guiStyle.fontSize = 25;
        _guiStyle.normal.textColor = Color.white;
        // guiStyle.normal.background = Texture2D.CreateExternalTexture(1500, 1500, TextureFormat.Alpha8, false, true, IntPtr.Zero);
        GUI.BeginGroup(new Rect(10,10,1500,1500));
        GUI.Box(new Rect(0,0,1500,100),$"Settings", _guiStyle);
        GUI.Label(new Rect(10,25,300,30), $"Generation: {_generation}",_guiStyle);
        GUI.Label(new Rect(10,50,300,30), $"Population: {_population.Count}",_guiStyle);
        GUI.Label(new Rect(10,75,300,30), $"Mutation: {Mathf.RoundToInt(mutationChance * 100)}%",_guiStyle);
        // GUI.Label(new Rect(10,100,300,30), $"Time: {_elapsed:0.00}" + " / " 
                                                                   // + $"Time: {trialTime:0.00}",guiStyle);
        // GUI.Label(new Rect(10,125,800,30), $"Furthest Distance {lastBestDistance} / 155",guiStyle);
        GUI.Box(new Rect(0,125,1500,700),$"Stats", _guiStyle);
        GUI.contentColor = new Color(0.81f, 1f, 0.71f);
        
        GUI.Label(new Rect(10,150,1500,30), $"Survivors: Now: {_activeBots}/" +
                                            $"{Mathf.RoundToInt(((float)_activeBots / populationSize) * 100)}% /" +
                                            $" Last : {_lastSurvivors}/" +
                                            $"{Mathf.RoundToInt((_lastSurvivors / _lastPopulationSize) * 100)}% /" +
                                            $" Top: {_topSurvivors}/" +
                                            $"{Mathf.RoundToInt((_topSurvivors / _topSurvivorsPopulationSize) * 100)}%", _guiStyle);
        GUI.contentColor = new Color(0.6f, 0.63f, 1f);
        GUI.Label(new Rect(10,200,1500,30), $"Distance: Now: {Mathf.RoundToInt(_lastBestDistance)}/155" +
                                            $" Last: {Mathf.RoundToInt(_distance)} /" +
                                            $" LastAvg: {Mathf.RoundToInt(_lastAvgDistance)}" +
                                            $" Top: {Mathf.RoundToInt(_topDistance)} /" +
                                            $" TopAvg: {Mathf.RoundToInt(_topAvgDistance)}",_guiStyle);
        GUI.contentColor = new Color(1f, 0.99f, 0.67f);
        GUI.Label(new Rect(10,250,1500,30), $"Survival Time: Now: {_elapsed:0.00}/{trialTime:0.00} /" +
                                            $" Last: {_lastTimeAlive:0.00} /" +
                                            $" LastAvg: {_lastAvgAliveTime:0.00}" +
                                            $" Top: {_topTimeAlive:0.00} /" +
                                            $" TopAvg: {_topAvgTimeAlive:0.00}",_guiStyle);
        GUI.contentColor = new Color(0.7f, 0.51f, 0.61f);
        GUI.Label(new Rect(10,300,1500,30), $"Fitness Score: Now: N/A /" +
                                            $" Last: {_lastBestFitnessScorePrint}% /" +
                                            $" LastAvg: {_lastAvgFitnessScore}% /" +
                                            $" Top: {_topFitnessScore}% /" +
                                            $" TopAvg: {_topAvgFitnessScore}%",_guiStyle);
        GUI.contentColor = new Color(0.99f, 0.8f, 0.48f);
        GUI.Label(new Rect(10,350,1500,30), $"Possible Score: Now: N/A /" +
                                            $" Last: {Mathf.RoundToInt(_lastBestPossibleScore * 100)}% /" +
                                            $" LastAvg: {Mathf.RoundToInt(_lastAvgPossibleScore * 100)}%"+
                                            $" Top: {Mathf.RoundToInt(_topPossibleScore * 100)}% /" +
                                            $" TopAvg: {Mathf.RoundToInt(_topAvgPossibleScore * 100)}%",_guiStyle);
        
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
        if (generation.bestFitnessScore > _topFitnessScore) _topFitnessScore = generation.bestFitnessScore;
        if (generation.avgFitnessScore > _topAvgFitnessScore) _topAvgFitnessScore = generation.avgFitnessScore;
        if (generation.bestDistance > _topDistance) _topDistance = generation.bestDistance;
        if (generation.avgDistance > _topAvgDistance) _topAvgDistance = generation.avgDistance;
        if (generation.bestPossibleScore > _topPossibleScore) _topPossibleScore = generation.bestPossibleScore;
        if (generation.avgPossibleScore > _topAvgPossibleScore) _topAvgPossibleScore = generation.avgPossibleScore;
        if (generation.elapsed > _topTimeAlive) _topTimeAlive = generation.elapsed;
        if (generation.avgAliveTime > _topAvgTimeAlive) _topAvgTimeAlive = generation.avgAliveTime;
        if (generation.survivors >= _topSurvivors)
        {
            _topSurvivors = generation.survivors;
            _topSurvivorsPopulationSize = generation.populationSize;
        }
        if (((float)generation.survivors / generation.populationSize) >= _topAvgSurvivors)
            _topAvgSurvivors = ((float)generation.survivors / generation.populationSize);
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

            Generation gen = _generations[_generations.Count - 1];
            var dnaValues = gen.dnaGroupsList;
            _generation = gen.generation;
            trialTime = gen.trialTime;
            mutationChance = gen.mutationChance;
            _distance = gen.bestDistance;
            populationSize = gen.populationSize;
            _lastBestFitnessScorePrint = gen.bestFitnessScore;
            _lastBestPossibleScore = gen.bestPossibleScore;
            _winningDeathPos = gen.winningDeathPos;
            Vector3 starPos = new Vector3(star.transform.position.x, star.transform.position.y, _winningDeathPos.z);
            _cloneIndex = gen.cloneIndex;
            star.transform.position = starPos;
            _lastTimeAlive = gen.elapsed;
            _lastAvgFitnessScore = gen.avgFitnessScore;
            _lastAvgPossibleScore = gen.avgPossibleScore;
            _lastAvgDistance = gen.avgDistance;
            _lastAvgAliveTime = gen.avgAliveTime;
            _lastSurvivors = gen.survivors;
            _lastPopulationSize = gen.populationSize;
            
            CreateNewPopulation();
            for (var index = 0; index < _brains.Count; index++)
            {
                _brains[index].dnaGroups = JsonConvert.DeserializeObject<Brain.DNAGroups>(dnaValues[index]);
                _brains[index].Init();
            }
            _brains[_cloneIndex].StarActive(true);
            SetBest(_generations);
        }
        else
        {
            CreateNewPopulation();
        }
        
    }

    void AddToGenerations(List<Brain> orderedPopulation)
    {
        _generationData = true;
        List<string> dnaValues = new List<string>();
        foreach (Brain pop in orderedPopulation)
        {
            string s = pop.GetDNAString();
            // Debug.Log(s);
            dnaValues.Add(s);
        }
        Generation gen = new Generation();
        gen.elapsed = _elapsed;
        gen.generation = _generation;
        gen.survivors = _activeBots;
        gen.mutationChance = mutationChance;
        gen.populationSize = populationSize;
        gen.trialTime = trialTime;
        gen.dnaGroupsList = dnaValues;
        gen.bestDistance = _distance;
        gen.bestFitnessScore = _lastBestFitnessScorePrint;
        gen.bestPossibleScore = _lastBestPossibleScore;
        gen.winningDeathPos = _winningDeathPos;
        gen.cloneIndex = _cloneIndex;
        gen.elapsed = _lastTimeAlive;
        gen.avgFitnessScore = _lastAvgFitnessScore;
        gen.avgPossibleScore = _lastAvgPossibleScore;
        gen.avgDistance = _lastAvgDistance;
        gen.avgAliveTime = _lastAvgAliveTime;
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
            _population.Add(b);
            // _textFileHandler.AddTextToFile(b.GetComponent<Brain>().GetDNAString());
        }
        _activeBots = populationSize;
    }

    private void OnApplicationQuit()
    {
        if (_generationData) _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_generations));
    }

    private void CountDead()
    {
        _activeBots--;
    }

    private GameObject CreateEthan(Vector3 position)
    {
        // var startingPosition = RandomStartPosition(position);
        GameObject b = Instantiate(botPrefab, position, transform.rotation);
        Brain brain = b.GetComponent<Brain>();
        brain.Init();
        _brains.Add(brain);
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
        offspringDnaGroups.movementDnaForwardBackward.Combine(parent1.dnaGroups.movementDnaForwardBackward,parent2.dnaGroups.movementDnaForwardBackward);
        offspringDnaGroups.movementDnaLeftRight.Combine(parent1.dnaGroups.movementDnaLeftRight,parent2.dnaGroups.movementDnaLeftRight);
        offspringDnaGroups.movementDnaTurn.Combine(parent1.dnaGroups.movementDnaTurn,parent2.dnaGroups.movementDnaTurn);
        offspringDnaGroups.priorityDna.Combine(parent1.dnaGroups.priorityDna,parent2.dnaGroups.priorityDna);
        offspringDnaGroups.heightDna.Combine(parent1.dnaGroups.heightDna,parent2.dnaGroups.heightDna);
        offspringDnaGroups.colourDna.Combine(parent1.dnaGroups.colourDna,parent2.dnaGroups.colourDna);
        if (Random.Range(0f, 1f) < mutationChance)
        {
            
            (int index, int oldValue, int newValue) dnaDetails = (0,0,0);
            DNA dnaMutate = new DNA();
            switch (Random.Range(0, 6))
            {
                case 0:
                    dnaMutate = offspringDnaGroups.movementDnaForwardBackward;
                    break;
                case 1:
                    dnaMutate = offspringDnaGroups.priorityDna;
                    break;
                case 2:
                    dnaMutate = offspringDnaGroups.heightDna;
                    break;
                case 3:
                    dnaMutate = offspringDnaGroups.movementDnaLeftRight;
                    break;
                case 4:
                    dnaMutate = offspringDnaGroups.movementDnaTurn;
                    break;
                case 5:
                    dnaMutate = offspringDnaGroups.colourDna;
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
        return Mathf.RoundToInt(((((brain.Distance / _distance) * fitnessMultiplyDistance) +
                                  (brain.timeAlive / _elapsed) +
                                  (brain.GetBonus() / FindObjectsOfType<CheckPoint>().Length)) / divScore) * 100);
    }

    private void BreedNewPopulation()
    {
        _generation++;
        Debug.Log($"Gen {_generation}: ");
        
        List<Brain> sortedList = _brains.OrderBy(o => ((o.Distance))).ToList();
        _winningDeathPos = sortedList[sortedList.Count - 1].GetDeathLocation();
        Vector3 starPos = new Vector3(star.transform.position.x, star.transform.position.y, _winningDeathPos.z);
        star.transform.position = starPos;
        // Debug.Log($"Max Distance {sortedList[sortedList.Count-1].Distance}");
        this._distance = sortedList[sortedList.Count - 1].Distance;
        sortedList = _brains.OrderBy(o => ((GetFitnessScore(o)))).ToList();
        // float divScore = fitnessMultiplyDistance + fitnessMultiplyTime + fitnessMultiplyBonus;
        // sortedList = brains.OrderBy(o =>
        //     ((((o.Distance / furthestDistance) * fitnessMultiplyDistance) +
        //       ((o.timeAlive / _elapsed) * fitnessMultiplyTime)) +
        //      ((o.GetBonus() / FindObjectsOfType<CheckPoint>().Length)) * fitnessMultiplyBonus) / divScore).ToList();
        Brain.DNAGroups topGeneGroups = Brain.DNAGroups.Clone(sortedList[sortedList.Count - 1].dnaGroups);
        Brain brain = sortedList[sortedList.Count - 1];
        _lastBestPossibleScore = UnbiasPosibleScore(brain);
        _lastBestFitnessScorePrint = GetFitnessScore(brain);
        float scores = 0f;
        float possibleScore = 0f;
        float timeAlive = 0f;
        float brainDistance = 0f;
         foreach (Brain b in sortedList)
         {
             brainDistance += b.Distance;
             timeAlive += b.timeAlive;
             float currentScore = GetFitnessScore(b);
             scores += currentScore; 
             possibleScore += ((b.Distance / 150) + (b.timeAlive / trialTime) + b.GetBonus()) / 3;
             if (printScore) Debug.Log($"Score {currentScore}%, " +
                                     $"Dis: {Mathf.RoundToInt((b.Distance / this._distance) * 100)}, Time: {Mathf.RoundToInt((b.timeAlive / _elapsed) * 100)}, Bonus: {(b.GetBonus() / FindObjectsOfType<CheckPoint>().Length)}");
        }

        _lastAvgPossibleScore = Mathf.RoundToInt(possibleScore / sortedList.Count);
        _lastAvgFitnessScore = Mathf.RoundToInt((scores / sortedList.Count));
        _lastAvgDistance = Mathf.RoundToInt(brainDistance / sortedList.Count);
        _lastAvgAliveTime = timeAlive / sortedList.Count;
        
        AddToGenerations(sortedList);
        // population.Clear();
        List<Brain.DNAGroups> offspring = new List<Brain.DNAGroups>();
        // int c = 0;
        // for (int i = (int) (sortedList.Count - (sortedList.Count / 4.0f)) - 1; i < sortedList.Count -1; i++)
        // {
        //     c+= 4;
        //     Debug.Log($"breed {c} / {i} distance {sortedList[i + 1].Distance}");
        //     offspring.Add(Breed(sortedList[i], sortedList[i + 1]));
        //     offspring.Add(Breed(sortedList[i + 1], sortedList[i]));
        //     offspring.Add(Breed(sortedList[i], sortedList[i + 1]));
        //     offspring.Add(Breed(sortedList[i + 1], sortedList[i]));
        // }

        int divideBy = 4;
        int breedingPoolQty = (int) Mathf.Ceil(((float)populationSize / 4));
        int startingRange = populationSize - breedingPoolQty;
        // Debug.Log($"divide by {divideBy} qty of breeders {breedingPoolQty}");
        for (int i = populationSize - 1; i > startingRange - 1; i--)
        {
            for (int j = 0; j < divideBy; j++)
            {
                int rand = Random.Range(startingRange, populationSize);
                offspring.Add(Breed(sortedList[i], sortedList[rand]));
            }
        }
        Debug.Log($"bred pop = {offspring.Count}");
        Vector3 randPos = RandomStartPosition(transform.position);
        
        for (int i = 0; i < sortedList.Count; i++)
        {
            _brains[i].dnaGroups = offspring[i];
            _brains[i].DeathOnOff(true);
            _brains[i].transform.position = randPos;
            _brains[i].Init();
            _brains[i].StarActive(false);
            // Destroy(sortedList[i]);
        }

        _cloneIndex = Random.Range(0, _brains.Count);
        _brains[_cloneIndex].dnaGroups = topGeneGroups;
        _brains[_cloneIndex].StarActive(true);
        _brains[_cloneIndex].Init();
        _activeBots = populationSize;
        // Brain.DNAGroups.PrintDNAColour(brains[clone].dnaGroups);
    }

    private float UnbiasPosibleScore(Brain brain)
    {
        return ((brain.Distance / 150) + (brain.timeAlive / trialTime) + (brain.GetBonus()) / FindObjectsOfType<CheckPoint>().Length);
    }

    private void BestBrain()
    {
        _timer = 0f;
        List<Brain> orderedBrains = _brains.OrderBy(o => (o.GetProgress())).ToList();
        _lastBestDistance = orderedBrains[orderedBrains.Count-1].GetProgress();
    }

    private void Update()
    {
        Time.timeScale = gameSpeed;
        _timer += Time.deltaTime;
        _elapsed += Time.deltaTime;
        if (_timer >= .5f) BestBrain();
        if (_elapsed >= trialTime || _activeBots == 0)
        {
            _lastTimeAlive = _elapsed;
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
            foreach (Collider hitCollider in colliders)
            {
                try
                {
                    if (hitCollider.attachedRigidbody.TryGetComponent(out Brain brain))
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
