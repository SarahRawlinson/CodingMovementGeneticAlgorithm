using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class DNA
{
    public Brain.DNAType dnaType;
    public int dnaLegnth = 0;
    public int maxValues = 0;
    public List<int> genes = new List<int>();
    

    public List<int> GetGenes()
    {
        return genes;
    }
    static public DNA CopyType(DNA copy, bool clone)
    {
        DNA dna = new DNA(copy.dnaLegnth, copy.maxValues, copy.dnaType);
        if (clone) dna.Clone(copy);
        return dna;
    }
    
    public void Clone(DNA copy)
    {
        // DNA dna = new DNA(copy.dnaLegnth, copy.maxValues, copy.dnaType);
        for (var index = 0; index < copy.genes.Count; index++)
        {
            genes[index] = copy.genes[index];
        }
    }

    public DNA()
    {
        
    }

    public DNA(int l, int v, Brain.DNAType type)
    {
        dnaType = type;
        dnaLegnth = l;
        maxValues = v;
        SetRandom();
    }
    public DNA(int l, int v, Brain.DNAType type, List<int> values)
    {
        dnaType = type;
        dnaLegnth = l;
        maxValues = v;
        genes = values;
    }

    private void SetRandom()
    {
        genes.Clear();
        for (int i = 0; i < dnaLegnth; i++)
        {
            int val = Random.Range(0, maxValues);
            genes.Add(Mathf.RoundToInt(val));
            if (val > (maxValues - 1)) Debug.Log("Error Value more than max");
        }
    }

    // public void SetFloat(int pos, int value)
    // {
    //     if (value > (maxValues - 1)) Debug.Log("Error Value more than max");
    //     genes[pos] = value;
    // }

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

    public (int index, int oldValue, int newValue) Mutate()
    {
        int geneIndex = Random.Range(0, dnaLegnth);
        int newValue = Random.Range(0, maxValues);
        int originalValue = genes[geneIndex];
        genes[geneIndex] = newValue;
        // Debug.Log($"DNA Gene Mutate : Old Val: {originalValue}, New Val: {newValue} : Max Val: {maxValues - 1}");
        return (geneIndex, originalValue, newValue);
    }

    public int GetGene(int pos)
    {
        return genes[pos];
    }
}
