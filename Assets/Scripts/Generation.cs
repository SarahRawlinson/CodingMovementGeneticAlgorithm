
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class Generation
    {
        public int generation;
        public List<string> dnaGroupsList;
        public string bestDNA;
        public float trialTime = 10;
        public int populationSize = 50;
        public float elapsed;
        public int survivors;
        public float mutationChance;
        public float bestDistance;
        public float bestFitnessScore;
        public float bestPossibleScore;
        public Vector3 winningDeathPos;
        public List<int> cloneIndex;
        public float avgFitnessScore;
        public float avgPossibleScore;
        public float avgAliveTime;
        public float avgDistance;
        public int checkPointCount;
        public float avgCheckPointCount;
        public List<int> mutants;
    }
