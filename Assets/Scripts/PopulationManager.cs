using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Newtonsoft.Json;
using UnityEngine.Serialization;


[RequireComponent(typeof(GenerationStats))]
public class PopulationManager : MonoBehaviour
{

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
    [SerializeField] private float fitnessMultiplyCheckPoint = 1;
    [SerializeField] private bool showStats = true;
    [SerializeField] private float maxDistance = 150;
    [SerializeField] private int numberOfClones = 1;

    private List<int> clones = new List<int>();
    private List<int> mutants = new List<int>();

    // Event
    public static event Action NewRound;

    // References
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private string fileName = "Pre Run Generation 300";
    [SerializeField] private DeathSphere sphere;
    [SerializeField] private GameObject star;
    [SerializeField] private CheckPoint endCheckPoint;
    private readonly List<GameObject> _population = new List<GameObject>();
    
    private List<Generation> _generations = new List<Generation>();
    private bool _generationData;
    private readonly List<Brain> _brains = new List<Brain>();
    private TextFileHandler _textFileHandler;
    private float _timer;

    GenerationStats generationStats;

    

    void SetBest(List<Generation> generations)
    {
        foreach (Generation generation in generations)
        {
            CheckForBest(generation);
            generationStats._totalCheckPointCount += generation.checkPointCount;
        }
    }

    private void CheckForBest(Generation generation)
    {

    // private float topTimeAlive = 0;
    // private float topAvgTimeAlive = 0;
        if (generation.bestFitnessScore > generationStats._topFitnessScore) generationStats._topFitnessScore = generation.bestFitnessScore;
        if (generation.avgFitnessScore > generationStats._topAvgFitnessScore) generationStats._topAvgFitnessScore = generation.avgFitnessScore;
        if (generation.bestDistance > generationStats._topDistance) generationStats._topDistance = generation.bestDistance;
        if (generation.avgDistance > generationStats._topAvgDistance) generationStats._topAvgDistance = generation.avgDistance;
        if (generation.bestPossibleScore > generationStats._topPossibleScore) generationStats._topPossibleScore = generation.bestPossibleScore;
        if (generation.avgPossibleScore > generationStats._topAvgPossibleScore) generationStats._topAvgPossibleScore = generation.avgPossibleScore;
        if (generation.elapsed > generationStats._topTimeAlive) generationStats._topTimeAlive = generation.elapsed;
        if (generation.avgAliveTime > generationStats._topAvgTimeAlive) generationStats._topAvgTimeAlive = generation.avgAliveTime;
        if (generation.checkPointCount > generationStats._topCheckPointCount) generationStats._topCheckPointCount = generation.checkPointCount;
        if (generation.avgCheckPointCount > generationStats._topAvgCheckPointCount) generationStats._topAvgCheckPointCount = generation.avgCheckPointCount;
        generationStats._totalBrainsCreated += generation.populationSize;
        generationStats._totalFitnessScore += generation.avgFitnessScore;
        generationStats._totalPossibleScore += generation.avgPossibleScore;
        if (generation.survivors >= generationStats._topSurvivors)
        {
            generationStats._topSurvivors = generation.survivors;
            generationStats._topSurvivorsPopulationSize = generation.populationSize;
        }
        if (((float)generation.survivors / generation.populationSize) >= generationStats._topAvgSurvivors) generationStats._topAvgSurvivors = ((float)generation.survivors / generation.populationSize);
    }

    private void Awake()
    {
        generationStats = GetComponent<GenerationStats>();
        generationStats._totalBrainsCreated = populationSize;
        sphere.GetComponent<Renderer>().enabled = false;
        sphere.DrawSphere(deathSphereStartSize);
        _textFileHandler = new TextFileHandler(fileName);
        Brain.Dead += CountDead;
        endCheckPoint.CheckPointReached += AddToCheckpointTally;
        (bool exists, string fileText) = _textFileHandler.GetFileText();
        if (exists)
        {
            _generations = JsonConvert.DeserializeObject<List<Generation>>(fileText);
            CreateNewPopulation();
            
            LoadGeneration(_generations.Count - 1);

            SetBest(_generations);
        }
        else
        {
            
            CreateNewPopulation();
        }
        
    }

    private void LoadGeneration(int genIndex)
    {
        if (genIndex > 0)
        {
            Generation lastGen = _generations[genIndex - 1];
            generationStats._lastBestFitnessScore = lastGen.bestFitnessScore;
            generationStats._lastBestPossibleScore = lastGen.bestPossibleScore;
            generationStats._winningDeathPos = lastGen.winningDeathPos;
            Vector3 starPos = new Vector3(star.transform.position.x, star.transform.position.y, generationStats._winningDeathPos.z);
            star.transform.position = starPos;
            generationStats._lastTimeAlive = lastGen.elapsed;
            generationStats._lastAvgFitnessScore = lastGen.avgFitnessScore;
            generationStats._lastAvgPossibleScore = lastGen.avgPossibleScore;
            generationStats._lastAvgDistance = lastGen.avgDistance;
            generationStats._lastAvgAliveTime = lastGen.avgAliveTime;
            generationStats._lastSurvivors = lastGen.survivors;
            generationStats._lastPopulationSize = lastGen.populationSize;
            generationStats._lastCheckPointCount = lastGen.checkPointCount;
        }
        
        Generation gen = _generations[genIndex];
        var dnaValues = gen.dnaGroupsList;
        generationStats._generation = gen.generation;
        trialTime = gen.trialTime;
        mutationChance = gen.mutationChance;
        generationStats._distance = gen.bestDistance;
        populationSize = gen.populationSize;
        clones = gen.cloneIndex;
        mutants = gen.mutants;

        Vector3 randPos = RandomStartPosition(transform.position);
        for (var index = 0; index < _brains.Count; index++)
        {
            _brains[index].dnaGroup = JsonConvert.DeserializeObject<DNAGroup>(dnaValues[index]);
            _brains[index].Init();
            _brains[index].StarActive(false);
            _brains[index].MutateActive(false);
            _brains[index].DeathOnOff(true);
            _brains[index].transform.position = randPos;
        }
        foreach (int i in mutants)
        {
            _brains[i].MutateActive(true);
        }
        foreach (int i in clones)
        {
            _brains[i].StarActive(true);
        }
        NewRound?.Invoke();
    }

    private void AddToCheckpointTally()
    {
        generationStats._checkPointCount += 1;
        generationStats._totalCheckPointCount += 1;
    }

    void AddToGenerations(List<Brain> orderedPopulation)
    {
        var gen = new Generation();
        gen = UpdateGeneration(orderedPopulation, gen);
        _generations.Add(gen);
        CheckForBest(gen);
        // _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_generations));
    }

    private Generation UpdateGeneration(List<Brain> orderedPopulation, Generation gen)
    {
        _generationData = true;
        List<string> dnaValues = new List<string>();
        foreach (Brain pop in orderedPopulation)
        {
            string s = pop.GetDNAString();
            // Debug.Log(s);
            dnaValues.Add(s);
        }
        
        gen.elapsed = generationStats._elapsed;
        gen.generation = generationStats._generation;
        gen.survivors = generationStats._activeBots;
        gen.mutationChance = mutationChance;
        gen.populationSize = populationSize;
        gen.trialTime = trialTime;
        gen.dnaGroupsList = dnaValues;
        gen.bestDistance = generationStats._distance;
        gen.bestFitnessScore = generationStats._lastBestFitnessScore;
        gen.bestPossibleScore = generationStats._lastBestPossibleScore;
        gen.winningDeathPos = generationStats._winningDeathPos;
        gen.cloneIndex = clones;
        gen.elapsed = generationStats._lastTimeAlive;
        gen.avgFitnessScore = generationStats._lastAvgFitnessScore;
        gen.avgPossibleScore = generationStats._lastAvgPossibleScore;
        gen.avgDistance = generationStats._lastAvgDistance;
        gen.avgAliveTime = generationStats._lastAvgAliveTime;
        gen.avgCheckPointCount = ((float) generationStats._lastCheckPointCount / generationStats._lastPopulationSize) * 100;
        gen.checkPointCount = generationStats._lastCheckPointCount;
        gen.mutants = mutants;
        return gen;
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
        generationStats._activeBots = populationSize;
    }

    private void OnApplicationQuit()
    {
        if (_generationData) _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_generations));
    }

    private void CountDead()
    {
        generationStats._activeBots--;
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

    (bool, DNAGroup) Breed(DNAGroup parent1, DNAGroup parent2, List<string> tags)
    {
        
        bool mutate = false;
        // var position = transform.position;
        DNAGroup offspringDNAGroup = parent1.CopyGeneGroupStructure();
        // var offspring = CreateEthan(position);
        // Brain brain = offspring.GetComponent<Brain>();
        offspringDNAGroup.movementDnaForwardBackward.Combine(parent1.movementDnaForwardBackward,parent2.movementDnaForwardBackward);
        offspringDNAGroup.movementDnaLeftRight.Combine(parent1.movementDnaLeftRight,parent2.movementDnaLeftRight);
        offspringDNAGroup.movementDnaTurn.Combine(parent1.movementDnaTurn,parent2.movementDnaTurn);
        offspringDNAGroup.priorityDna.Combine(parent1.priorityDna,parent2.priorityDna);
        offspringDNAGroup.heightDna.Combine(parent1.heightDna,parent2.heightDna);
        offspringDNAGroup.colourDna.Combine(parent1.colourDna,parent2.colourDna);
        if (Random.Range(0f, 1f) < mutationChance)
        {
            mutate = true;
            (int index, int oldValue, int newValue) dnaDetails = (0,0,0);
            DNA dnaMutate = new DNA();
            switch (Random.Range(0, 6))
            {
                case 0:
                    dnaMutate = offspringDNAGroup.movementDnaForwardBackward;
                    break;
                case 1:
                    dnaMutate = offspringDNAGroup.priorityDna;
                    break;
                case 2:
                    dnaMutate = offspringDNAGroup.heightDna;
                    break;
                case 3:
                    dnaMutate = offspringDNAGroup.movementDnaLeftRight;
                    break;
                case 4:
                    dnaMutate = offspringDNAGroup.movementDnaTurn;
                    break;
                case 5:
                    dnaMutate = offspringDNAGroup.colourDna;
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
                printString += Brain.DNAInfo(tags,
                    dnaMutate,
                    dnaDetails.index, dnaDetails.oldValue);
                printString += " Has now been changed to ";
                printString += Brain.DNAInfo(tags,
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
        return (mutate, offspringDNAGroup);
    }

    float GetFitnessScore(Brain brain)
    {
        float bonus = 0;
        float checkPoints = 0;
        foreach (CheckPoint check in FindObjectsOfType<CheckPoint>())
        {
            if (check.BonusClaimed())
            {
                checkPoints += 1;
                bonus += check.GetBonus();
            }
        }
        if (bonus == 0) bonus = 1;
        if (checkPoints == 0) checkPoints = 1;
        float divScore = TotalFitnessModifiers();
        if (printScore) Debug.Log($"Fitness = " +
                                $"Distance: ({brain.Distance} / {generationStats._distance}) * {fitnessMultiplyDistance}, " +
                                $"Time: ({brain.timeAlive} / {generationStats._elapsed}) * {fitnessMultiplyTime}, " +
                                $"Bonus: ({brain.GetBonus()} / {bonus}) * {fitnessMultiplyBonus}, " +
                                $"CheckPoints ({brain.GetCheckPoints()} / {checkPoints}) * {fitnessMultiplyCheckPoint}");
        return (((((brain.Distance / generationStats._distance) * fitnessMultiplyDistance) +
                                  ((brain.timeAlive / generationStats._elapsed) * fitnessMultiplyTime) +
                                  ((brain.GetBonus() / bonus) * fitnessMultiplyBonus))  +
                                  ((brain.GetCheckPoints() / checkPoints) * fitnessMultiplyCheckPoint)) / divScore) * 100;
    }

    private float TotalFitnessModifiers()
    {
        return fitnessMultiplyDistance + fitnessMultiplyTime + fitnessMultiplyBonus + fitnessMultiplyCheckPoint;
    }

    float GetPossibleScore(Brain brain)
    {
        float bonus = 0;
        float checkPoints = 0;
        foreach (CheckPoint check in FindObjectsOfType<CheckPoint>())
        {
            checkPoints += 1;
            bonus += check.GetBonus();
        }
        float divScore = TotalFitnessModifiers();
        if (printScore) Debug.Log($"Possible Fitness = " + 
                                    $"Distance: ({brain.Distance} / {maxDistance}) * {fitnessMultiplyDistance}, " +
                                    $"Time: ({brain.timeAlive} / {trialTime}) * {fitnessMultiplyTime}, " +
                                    $"Bonus: ({brain.GetBonus()} / {bonus}) * {fitnessMultiplyBonus}, " +
                                    $"CheckPoints ({brain.GetCheckPoints()} / {checkPoints}) * {fitnessMultiplyCheckPoint}");
        return (((((brain.Distance / maxDistance) * fitnessMultiplyDistance) +
                                  ((brain.timeAlive / trialTime) * fitnessMultiplyTime) +
                                  ((brain.GetBonus() / bonus)) * fitnessMultiplyBonus)  +
                                  ((brain.GetCheckPoints() / checkPoints) * fitnessMultiplyCheckPoint)) / divScore) * 100;
    }

    private void CheckPopulationsFitness()
    {
        
        List<Brain> sortedList = _brains.OrderBy(o => ((o.Distance))).ToList();
        generationStats._winningDeathPos = sortedList[sortedList.Count - 1].GetDeathLocation();
        Vector3 starPos = new Vector3(star.transform.position.x, star.transform.position.y, generationStats._winningDeathPos.z);
        generationStats._distance = sortedList[sortedList.Count - 1].Distance;
        // sortedList = _brains.OrderBy(o => ((GetPossibleScore(o)))).ToList();
        // Brain brain = sortedList[sortedList.Count - 1];
        float topFitness = 0f;
        float avgFitness = 0f;
        foreach (Brain brain in _brains)
        {
            float fitnessTestResult = GetPossibleScore(brain);
            avgFitness += fitnessTestResult;
            if (fitnessTestResult > topFitness) topFitness = fitnessTestResult;
        }

        avgFitness = avgFitness / _brains.Count;
        if (Mathf.RoundToInt(topFitness) >= Mathf.RoundToInt(generationStats._lastBestPossibleScore) || Mathf.RoundToInt(avgFitness) >= Mathf.RoundToInt(generationStats._lastAvgPossibleScore))
        {
            Debug.Log($"Generation Improved! Fitness: {Mathf.RoundToInt(topFitness)}," +
                      $" Last Fitness: {Mathf.RoundToInt(generationStats._lastBestPossibleScore)}," +
                      $" Avg: {Mathf.RoundToInt(avgFitness)}," +
                      $" Last Avg: {Mathf.RoundToInt(generationStats._lastAvgPossibleScore)}");
            generationStats._lastBestPossibleScore = topFitness;
            sortedList = _brains.OrderBy(o => ((GetFitnessScore(o)))).ToList();
            Brain brain = sortedList[sortedList.Count - 1];
            generationStats._lastBestFitnessScore = GetFitnessScore(brain);
            star.transform.position = starPos;
            generationStats._lastCheckPointCount = generationStats._checkPointCount;
            generationStats._lastTimeAlive = generationStats._elapsed;
            DNAGroup topGeneGroup = DNAGroup.Clone(sortedList[sortedList.Count - 1].dnaGroup);
            BreedNewPopulation(sortedList, topGeneGroup);
        }
        else
        {
            BreedLastGeneration(topFitness, avgFitness);
            // ReloadLastGeneration(topFitness, avgFitness, sortedList);
        }

        generationStats._activeBots = populationSize;
        // Brain.DNAGroups.PrintDNAColour(brains[clone].dnaGroups);
    }

    void BreedLastGeneration(float fitnessTestResult, float avgFitness)
    {
        Debug.Log($"Generation did not make improvements, breed last best again. Fitness: {Mathf.RoundToInt(fitnessTestResult)}," +
                  $" Last Fitness: {Mathf.RoundToInt(generationStats._lastBestPossibleScore)}," +
                  $" Avg: {Mathf.RoundToInt(avgFitness)}," +
                  $" Last Avg: {Mathf.RoundToInt(generationStats._lastAvgPossibleScore)}");
        
        List<string> dnaValues = new List<string>();
        List<DNAGroup> dnaGroups = new List<DNAGroup>();
        try
        {
            Generation gen = _generations[generationStats._generation - 2];
            try
            {
                dnaValues = gen.dnaGroupsList;
                try
                {
                    for (var index = 0; index < dnaValues.Count; index++)
                    {
                        dnaGroups.Add(JsonConvert.DeserializeObject<DNAGroup>(dnaValues[index]));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Couldn't DeserializeObject DNA Values: {e}");
                }

                try
                {
                    BreedBest(dnaGroups, dnaGroups[gen.cloneIndex[0]]);
                }
                catch (Exception e)
                {
                    Debug.Log($"Couldn't run Breed Best: {e}");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"DNA Values not found: {e}");
            }

        }
        catch (Exception e)
        {
            Debug.Log($"Generation not found: {e}");
        }

    }

    private void ReloadLastGeneration(float fitnessTestResult, float avgFitness, List<Brain> sortedList)
    {
        // _generation--;
        Debug.Log($"Generation did not make improvements, retry test. Fitness: {Mathf.RoundToInt(fitnessTestResult)}," +
                  $" Last Fitness: {Mathf.RoundToInt(generationStats._lastBestPossibleScore)}," +
                  $" Avg: {Mathf.RoundToInt(avgFitness)}," +
                  $" Last Avg: {Mathf.RoundToInt(generationStats._lastAvgPossibleScore)}");
        Vector3 randPos = RandomStartPosition(transform.position);
        for (int i = 0; i < sortedList.Count; i++)
        {
            _brains[i].DeathOnOff(true);
            _brains[i].transform.position = randPos;
            _brains[i].Init();
        }
        
    }

    private void BreedNewPopulation(List<Brain> sortedList, DNAGroup topGeneGroup)
    {
        
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
            possibleScore += GetPossibleScore(b);
            
             if (printScore) Debug.Log($"Score {currentScore}%, " +
                          $"Dis: {Mathf.RoundToInt((b.Distance / generationStats._distance) * 100)}, " +
                          $"Time: {Mathf.RoundToInt((b.timeAlive / generationStats._elapsed) * 100)}, " +
                          $"Bonus: {(b.GetBonus() / FindObjectsOfType<CheckPoint>().Length)}");
        }
        
        generationStats._lastAvgPossibleScore = Mathf.RoundToInt(possibleScore / sortedList.Count);
        generationStats._lastAvgFitnessScore = Mathf.RoundToInt((scores / sortedList.Count));
        generationStats._lastAvgDistance = Mathf.RoundToInt(brainDistance / sortedList.Count);
        generationStats._lastAvgAliveTime = timeAlive / sortedList.Count;
        
        if (generationStats._generation == _generations.Count)
        {
            UpdateGeneration(_brains, _generations[generationStats._generation - 1]);
        }
        else
        {
            AddToGenerations(sortedList);
        }

        generationStats._generation++;
        Debug.Log($"Gen {generationStats._generation}: ");
        List<DNAGroup> dnaGroups = new List<DNAGroup>();
        sortedList.ForEach(b => dnaGroups.Add(b.dnaGroup));
        BreedBest(dnaGroups, topGeneGroup);
    }

    // TODO change this to be a list of DNAGroup rather than Brain
    private void BreedBest(List<DNAGroup> sortedList, DNAGroup topGeneGroup)
    {
        List<DNAGroup> offspring = new List<DNAGroup>();
        Debug.Log("Breeding started");

        int divideBy = 4;
        int breedingPoolQty = (int) Mathf.Ceil(((float) populationSize / 4));
        int startingRange = populationSize - breedingPoolQty;
        mutants.Clear();
        for (int i = populationSize - 1; i > startingRange - 1; i--)
        {
            for (int j = 0; j < divideBy; j++)
            {
                int rand = Random.Range(startingRange, populationSize);
                (bool mutation, DNAGroup dnaGroups) = Breed(sortedList[i], sortedList[rand], botPrefab.GetComponent<Brain>().tagsToLookFor.ToList());
                offspring.Add(dnaGroups);
                if (mutation) mutants.Add(offspring.Count - 1);
            }
        }

        Debug.Log($"bred pop = {offspring.Count}");
        Vector3 randPos = RandomStartPosition(transform.position);

        for (int i = 0; i < sortedList.Count; i++)
        {
            if (mutants.Contains(i)) _brains[i].MutateActive(true);
            else
            {
                _brains[i].MutateActive(false);
            }

            _brains[i].dnaGroup = offspring[i];
            _brains[i].DeathOnOff(true);
            _brains[i].transform.position = randPos;
            _brains[i].Init();
            _brains[i].StarActive(false);
        }

        clones.Clear();
        
        for (int i = 0; i < numberOfClones; i++)
        {
            int _cloneIndex = Random.Range(0, _brains.Count);
            while (clones.Contains(_cloneIndex) || clones.Count >= populationSize)
            {
                _cloneIndex = Random.Range(0, _brains.Count);
            }

            clones.Add(_cloneIndex);
            _brains[_cloneIndex].dnaGroup = topGeneGroup;
            _brains[_cloneIndex].StarActive(true);
            _brains[_cloneIndex].Init();
        }
    }

    private void BestBrain()
    {
        _timer = 0f;
        List<Brain> orderedBrains = _brains.OrderBy(o => (o.GetProgress())).ToList();
        generationStats._lastBestDistance = orderedBrains[orderedBrains.Count-1].GetProgress();
    }

    private void Update()
    {
        generationStats.populationSize = populationSize;
        generationStats.mutationChance = mutationChance;
        generationStats.trialTime = trialTime;
        Time.timeScale = gameSpeed;
        _timer += Time.deltaTime;
        generationStats._elapsed += Time.deltaTime;
        if (_timer >= .5f) BestBrain();
        if (generationStats._elapsed >= trialTime || generationStats._activeBots == 0)
        {
            
            sphere.GetComponent<Renderer>().enabled = false;
            sphere.DrawSphere(deathSphereStartSize);
            CheckPopulationsFitness();
            generationStats._elapsed = 0f;
            generationStats._checkPointCount = 0;
            NewRound?.Invoke();
        }
        if (generationStats._elapsed >= deathSphereStartTime)
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
                catch { }
                
            }
        }
        
    }
}
