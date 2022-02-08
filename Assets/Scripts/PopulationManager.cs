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
        public int servers;
        public float mutationChance;

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

    private List<Brain> brains = new List<Brain>();

    public event Action NewRound; 

    private TextFileHandler _textFileHandler;

    
    [Range(0f, 1f)] [SerializeField] private float mutationChance = 0.05f;

    void OnGUI()
    {
        guiStyle.fontSize = 25;
        guiStyle.normal.textColor = Color.white;
        GUI.BeginGroup(new Rect(10,10,250,150));
        GUI.Box(new Rect(0,0,140,140),$"Stats", guiStyle);
        GUI.Label(new Rect(10,25,200,30), $"Generation: {generation}",guiStyle);
        GUI.Label(new Rect(10,50,200,30), string.Format("Time: {0:0.00}",_elapsed),guiStyle);
        GUI.Label(new Rect(10,75,200,30), $"Population: {population.Count}",guiStyle);
        GUI.Label(new Rect(10,100,200,30), $"Alive: {activeEthans}",guiStyle);
        GUI.EndGroup();
    }

    private void Start()
    {
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
            CreateNewPopulation();
            for (var index = 0; index < brains.Count; index++)
            {
                GameObject pop = population[index];
                brains[index].dnaGroups = JsonConvert.DeserializeObject<Brain.DNAGroups>(dnaValues[index]);
            }
        }
        else
        {
            CreateNewPopulation();
        }
        
    }

    void AddToDictionary(List<Brain> orderedPopulation)
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
        gen.servers = activeEthans;
        gen.mutationChance = mutationChance;
        gen.populationSize = populationSize;
        gen.trialTime = trialTime;
        gen.dnaGroupsList = dnaValues;
        _generations.Add(gen);
        // _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_generations));
        
        
    }

    private void CreateNewPopulation()
    {
        
        activeEthans = populationSize;
        for (int i = 0; i < populationSize; i++)
        {
            var position = transform.position;
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
        var startingPosition = RandomStartPosition(position);
        GameObject b = Instantiate(botPrefab, startingPosition, transform.rotation);
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
        Brain.DNAGroups offspringDnaGroups = parent1.dnaGroups.Clone();
        // var offspring = CreateEthan(position);
        // Brain brain = offspring.GetComponent<Brain>();
        offspringDnaGroups._movementDNA.Combine(parent1.dnaGroups._movementDNA,parent2.dnaGroups._movementDNA);
        offspringDnaGroups._priorityDNA.Combine(parent1.dnaGroups._priorityDNA,parent2.dnaGroups._priorityDNA);
        offspringDnaGroups._heightDNA.Combine(parent1.dnaGroups._heightDNA,parent2.dnaGroups._heightDNA);
        if (Random.Range(0f, 1f) < mutationChance)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    offspringDnaGroups._movementDNA.Mutate();
                    break;
                case 1:
                    offspringDnaGroups._priorityDNA.Mutate();
                    break;
                case 2:
                    offspringDnaGroups._heightDNA.Mutate();
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
        List<Brain> sortedList = brains.OrderBy(o => (o.Distance * o.timeAlive)).ToList();
        AddToDictionary(sortedList);
        // population.Clear();
        List<Brain.DNAGroups> Offsping = new List<Brain.DNAGroups>();
        for (int i = (int) (sortedList.Count / 2.0f) - 1; i < sortedList.Count -1; i++)
        {
            Offsping.Add(Breed(sortedList[i], sortedList[i + 1]));
            Offsping.Add(Breed(sortedList[i + 1], sortedList[i]));
        }

        for (int i = 0; i < sortedList.Count; i++)
        {
            brains[i].dnaGroups = Offsping[i];
            brains[i].transform.position = RandomStartPosition(transform.position);
            brains[i].Init();
            // Destroy(sortedList[i]);
        }
    }

    private void Update()
    {
        Time.timeScale = gameSpeed;
        _elapsed += Time.deltaTime;
        if (_elapsed >= trialTime || activeEthans == 0)
        {
            BreedNewPopulation();
            _elapsed = 0f;
        }
    }
}
