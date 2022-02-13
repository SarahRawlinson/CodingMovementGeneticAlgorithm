using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
// ReSharper disable once InconsistentNaming
public class DNA
{
    public Brain.DNAType dnaType;
    public int dnaLength = 0;
    public int maxValues = 0;
    public List<int> genes = new List<int>();
    

    public List<int> GetGenes()
    {
        return genes;
    }
    public static DNA CopyType(DNA copy, bool clone)
    {
        DNA dna = new DNA(copy.dnaLength, copy.maxValues, copy.dnaType);
        if (clone) dna.Clone(copy);
        return dna;
    }
    
    public void Clone(DNA copy)
    {
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
        dnaLength = l;
        maxValues = v;
        SetRandom();
    }
    public DNA(int l, int v, Brain.DNAType type, List<int> values)
    {
        dnaType = type;
        dnaLength = l;
        maxValues = v;
        genes = values;
    }

    private void SetRandom()
    {
        genes.Clear();
        for (int i = 0; i < dnaLength; i++)
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
        for (int i = 0; i < dnaLength; i++)
        {
            int g = 0;
            switch (Random.Range(0,2))
            {
                case 0:
                    g = dna1.genes[i];
                    break;
                case 1:
                    g = dna2.genes[i];
                    break;
                default:
                    Debug.Log("Error no DNA chosen");
                    break;
                    
            }
            // int c = i<dnaLength/2.0f ? dna1.genes[i] : dna2.genes[i];
            genes[i] = g;
        }
    }

    public (int index, int oldValue, int newValue) Mutate()
    {
        int geneIndex = Random.Range(0, dnaLength);
        int newValue = Random.Range(0, maxValues);
        int originalValue = genes[geneIndex];
        while (newValue == originalValue)
        {
            newValue = Random.Range(0, maxValues);
        }
        genes[geneIndex] = newValue;
        // Debug.Log($"DNA Gene Mutate : Old Val: {originalValue}, New Val: {newValue} : Max Val: {maxValues - 1}");
        return (geneIndex, originalValue, newValue);
    }

    public int GetGene(int pos)
    {
        return genes[pos];
    }
}
