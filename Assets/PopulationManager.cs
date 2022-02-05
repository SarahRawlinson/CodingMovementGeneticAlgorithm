using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private int populationSize = 50;
    private readonly List<GameObject> population = new List<GameObject>();
    private static float _elapsed;
    [SerializeField] private float trialTime = 10;
    private int generation = 1;
    private GUIStyle guiStyle = new GUIStyle();
    private int activeEthans;

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
        activeEthans = populationSize;
        Brain.Dead += CountDead;
        for (int i = 0; i < populationSize; i++)
        {
            var position = transform.position;
            var b = CreateEthan(position);
            population.Add(b);
        }
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
        brain._movementDNA.Combine(parent1.GetComponent<Brain>()._movementDNA,parent2.GetComponent<Brain>()._movementDNA);
        brain._priorityDNA.Combine(parent1.GetComponent<Brain>()._priorityDNA,parent2.GetComponent<Brain>()._priorityDNA);
        brain._heightDNA.Combine(parent1.GetComponent<Brain>()._heightDNA,parent2.GetComponent<Brain>()._heightDNA);
        if (Random.Range(0f, 1f) < mutationChance)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    brain._movementDNA.Mutate();
                    break;
                case 1:
                    brain._priorityDNA.Mutate();
                    break;
                case 2:
                    brain._heightDNA.Mutate();
                    break;
            }
        }
        return offspring;
    }

    private void BreedNewPopulation()
    {
        generation++;
        activeEthans = populationSize;
        List<GameObject> sortedList = population.OrderBy(o => (o.GetComponent<Brain>().Distance + o.GetComponent<Brain>().timeAlive)).ToList();
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
        _elapsed += Time.deltaTime;
        if (_elapsed >= trialTime || activeEthans == 0)
        {
            BreedNewPopulation();
            _elapsed = 0f;
        }
    }
}
