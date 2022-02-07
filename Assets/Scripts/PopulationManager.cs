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
        _textFileHandler = new TextFileHandler($"Load 1");
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
            for (var index = 0; index < population.Count; index++)
            {
                GameObject pop = population[index];
                pop.GetComponent<Brain>()._dnaGroups = JsonConvert.DeserializeObject<Brain.DNAGroups>(dnaValues[index]);
            }
        }
        else
        {
            CreateNewPopulation();
        }
        
    }

    void AddToDictionary(List<GameObject> orderedPopulation)
    {
        dictionaryData = true;
        List<string> dnaValues = new List<string>();
        foreach (GameObject pop in orderedPopulation)
        {
            string s = pop.GetComponent<Brain>().GetDNAString();
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
        _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_generations));
        
        
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
        // if (dictionaryData) _textFileHandler.AddTextToFile(JsonUtility.ToJson(generationDictionary));
    }

    private void CountDead()
    {
        activeEthans--;
    }

    private GameObject CreateEthan(Vector3 position)
    {
        Vector3 startingPosition = new Vector3(position.x + Random.Range(-2, 2), position.y,
            position.z + Random.Range(-2, 2));
        GameObject b = Instantiate(botPrefab, startingPosition, transform.rotation);
        
        b.GetComponent<Brain>().Init();
        return b;
    }

    GameObject Breed(GameObject parent1, GameObject parent2)
    {
        var position = transform.position;
        var offspring = CreateEthan(position);
        Brain brain = offspring.GetComponent<Brain>();
        brain._dnaGroups._movementDNA.Combine(parent1.GetComponent<Brain>()._dnaGroups._movementDNA,parent2.GetComponent<Brain>()._dnaGroups._movementDNA);
        brain._dnaGroups._priorityDNA.Combine(parent1.GetComponent<Brain>()._dnaGroups._priorityDNA,parent2.GetComponent<Brain>()._dnaGroups._priorityDNA);
        brain._dnaGroups._heightDNA.Combine(parent1.GetComponent<Brain>()._dnaGroups._heightDNA,parent2.GetComponent<Brain>()._dnaGroups._heightDNA);
        if (Random.Range(0f, 1f) < mutationChance)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    brain._dnaGroups._movementDNA.Mutate();
                    break;
                case 1:
                    brain._dnaGroups._priorityDNA.Mutate();
                    break;
                case 2:
                    brain._dnaGroups._heightDNA.Mutate();
                    break;
            }
        }
        // _textFileHandler.AddTextToFile(brain.GetComponent<Brain>().GetDNAString());
        return offspring;
    }

    private void BreedNewPopulation()
    {
        generation++;
        activeEthans = populationSize;
        List<GameObject> sortedList = population.OrderBy(o => (o.GetComponent<Brain>().Distance * o.GetComponent<Brain>().timeAlive)).ToList();
        AddToDictionary(sortedList);
        population.Clear();
        for (int i = (int) (sortedList.Count / 2.0f) - 1; i < sortedList.Count -1; i++)
        {
            population.Add(Breed(sortedList[i], sortedList[i + 1]));
            population.Add(Breed(sortedList[i + 1], sortedList[i]));
        }

        for (int i = 0; i < sortedList.Count; i++)
        {
            Destroy(sortedList[i]);
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
