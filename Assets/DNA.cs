using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DNA 
{
    private List<int> genes = new List<int>();
    private int dnaLegnth = 0;
    private int maxValues = 0;
    

    public DNA(int l, int v)
    {
        dnaLegnth = l;
        maxValues = v;
        SetRandom();
    }

    private void SetRandom()
    {
        genes.Clear();
        for (int i = 0; i < dnaLegnth; i++)
        {
            genes.Add(Mathf.RoundToInt(Random.Range(0f, maxValues)));
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
        genes[Random.Range(0, dnaLegnth)] = Random.Range(0, maxValues);
    }

    public int GetGene(int pos)
    {
        return genes[pos];
    }
}
