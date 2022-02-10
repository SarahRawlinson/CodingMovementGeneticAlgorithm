using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class DNA
{
    public string dnaType;
    public int dnaLegnth = 0;
    public int maxValues = 0;
    public List<int> genes = new List<int>();
    

    public List<int> GetGenes()
    {
        return genes;
    }
    static public DNA CopyType(DNA copy)
    {
        DNA dna = new DNA(copy.dnaLegnth, copy.maxValues, copy.dnaType);
        return dna;
    }
    
    static public DNA Clone(DNA copy)
    {
        DNA dna = new DNA(copy.dnaLegnth, copy.maxValues, copy.dnaType);
        foreach (int gene in copy.genes)
        {
            dna.genes.Add(gene);
        }
        return dna;
    }

    public DNA()
    {
        
    }

    public DNA(int l, int v, string name)
    {
        dnaType = $"{name}";
        dnaLegnth = l;
        maxValues = v;
        SetRandom();
    }
    public DNA(int l, int v, string name, List<int> values)
    {
        dnaType = $"{name}";
        dnaLegnth = l;
        maxValues = v;
        genes = values;
    }

    private void SetRandom()
    {
        genes.Clear();
        for (int i = 0; i < dnaLegnth; i++)
        {
            genes.Add(Mathf.RoundToInt(Random.Range(0, maxValues + 1)));
        }
    }

    public void SetFloat(int pos, int value)
    {
        genes[pos] = value;
    }

    public void Combine(DNA dna1, DNA dna2)
    {
        int c = 0;
        for (int i = 0; i < dnaLegnth; i++)
        {
            if (i<dnaLegnth/2.0f)
            {
                c = dna1.genes[i];
            }
            else
            {
                c = dna2.genes[i];
            }
            genes[i] = c;
        }
    }

    public void Mutate()
    {
        genes[Random.Range(0, dnaLegnth)] = Random.Range(0, maxValues + 1);
    }

    public int GetGene(int pos)
    {
        return genes[pos];
    }
}
