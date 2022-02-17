using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[Serializable]
public class PopulationSettings : MonoBehaviour
{
    public enum TrialType
    {
        BreedLastGenerationOnFailure,
        ReRunTrialOnFailure,
        ReRunLastGenerationOnFailure,
        NoFailure
    }
    public enum FailureType
    {
        IfGenerationFailsToMatchTopAvg,
        IfGenerationFailsToMatchTopBest,
        IfGenerationFailsToMatchTopAvgAndTopBest,
        IfGenerationFailsToMatchLastAvg,
        IfGenerationFailsToMatchLastBest,
        IfGenerationFailsToMatchLastAvgAndLastBest,
        NoFailure
    }
    
    [SerializeField] public string fileName = "Load 1";
    [Range(1, 200)] [SerializeField] public int populationSize = 50; // slider created but not ready
    [Range(0f, 1f)] [SerializeField] public float mutationChance = 0.05f; // Done
    [Range(1, 100)] [SerializeField] public float topPercent = 25; // Done
    [SerializeField] public int numberOfClones = 1; // Done
    [SerializeField] public float fitnessMultiplyBonus = 1; // Done
    [SerializeField] public float fitnessMultiplyDistance = 1; // Done
    [SerializeField] public float fitnessMultiplyTime = 1; // Done
    [SerializeField] public float fitnessMultiplyCheckPoint = 1; // Done
    [SerializeField] public float trialTime = 120; // Done
    [SerializeField] public float deathSphereSpeed = 1f; // Done
    [SerializeField] public float deathSphereStartTime = 30f; // Done
    [SerializeField] public float deathSphereStartSize = .5f; // Done
    [Range(0, 2)] [SerializeField] public float gameSpeed = 1;
    [SerializeField] public bool printScore = true;
    
    [SerializeField] private TMP_Text _topPercentText;
    [SerializeField] private Slider _topPercentSlider;
    
    [SerializeField] private TMP_Text _trialTimeText;
    [SerializeField] private Slider _trialTimeSlider;
    
    [SerializeField] private TMP_Text _clonesText;
    [SerializeField] private Slider _clonesSlider;
    
    // [SerializeField] private TMP_Text _populationText;
    // [SerializeField] private Slider _populationSlider;
    
    [SerializeField] private TMP_Text _mutationText;
    [SerializeField] private Slider _mutationSlider;

    [SerializeField] private TMP_InputField _bonusMultipyer;
    [SerializeField] private TMP_InputField _timeMultipyer;
    [SerializeField] private TMP_InputField _checkPointMultipyer;
    [SerializeField] private TMP_InputField _distanceMultipyer;

    [SerializeField] private TMP_InputField _deathSphereSizeInput;
    [SerializeField] private TMP_InputField _deathSphereStartTimeInput;
    [SerializeField] private TMP_InputField _deathSphereSpeedInput;
    
    [SerializeField] private TMP_Text _gameSpeedText;
    [SerializeField] private Slider _gameSpeedSlider;

    [SerializeField] private Toggle settings;
    [SerializeField] private GameObject settingsObject;

    [SerializeField] private TMP_Dropdown _trialType;
    [SerializeField] private TMP_Dropdown _trialFailureType;

    public TrialType trialType;
    public FailureType failureType;

    void UpdateTrialMode()
    {
        trialType = (TrialType)_trialType.value;
        failureType = (FailureType) _trialFailureType.value;
    }

    public bool TestFailed(GenerationStats stats)
    {
        return FailureMode(stats);
    }
    
    public int CheckPercent(int val)
    {
        if (val > 100 ) return 100;
        if (val < 1) return 1;
        return val;
    }
    public int CheckPercent(float val)
    {
        if (val > 100 ) return 100;
        if (val < 1) return 0;
        return Mathf.RoundToInt(val);
    }

    public void UpdateTopPercentFromSlider()
    {
        topPercent = CheckPercent(Mathf.RoundToInt(_topPercentSlider.value));
        _topPercentText.text = $"Breed The Top {topPercent.ToString()}%";
    }
    
    public void UpdateMutationFromSlider()
    {
        mutationChance = _mutationSlider.value / 100;
        _mutationText.text = $"Mutation {Mathf.RoundToInt(mutationChance * 100)}%";
    }
    
    public void UpdateClonesSlider()
    {
        numberOfClones = Mathf.RoundToInt((populationSize / 100) * _clonesSlider.value);
        
        // Debug.Log(Mathf.RoundToInt((populationSize / 100) * _clonesSlider.value));
        _clonesText.text = $"Number of Clones {numberOfClones.ToString()}";
    }
    
    public void UpdateTrialTimeFromSlider()
    {
        trialTime = _trialTimeSlider.value;
        _trialTimeText.text = $"Trial Time: {trialTime:0.00} seconds ({Mathf.RoundToInt(trialTime/60)} minutes)";
    }
    
    public void UpdateGameSpeedFromSlider()
    {
        gameSpeed = _gameSpeedSlider.value;
        _gameSpeedText.text = $"Game Speed: {Mathf.RoundToInt((gameSpeed / 1) * 100)}%)";
    }

    // public void TrailMode()
    // {
    //     switch (trialType)
    //     {
    //         case TrialType.NoFailure:
    //             break;
    //         case TrialType.BreedLastGenerationOnFailure:
    //             break;
    //         case TrialType.ReRunTrialOnFailure:
    //             break;
    //         case TrialType.ReRunLastGenerationOnFailure:
    //             break;
    //     }
    // }
    
    public bool FailureMode(GenerationStats stats)
    {
        switch (failureType)
        {
            case FailureType.IfGenerationFailsToMatchLastAvg:
                return stats.currentAvgPossibleScore < stats._lastAvgPossibleScore;
            case FailureType.IfGenerationFailsToMatchLastBest:
                return stats.currentPossibleScore < stats._lastBestPossibleScore;
            case FailureType.IfGenerationFailsToMatchLastAvgAndLastBest:
                return (stats.currentAvgPossibleScore < stats._lastAvgPossibleScore) || (stats.currentPossibleScore < stats._lastBestPossibleScore);
            case FailureType.IfGenerationFailsToMatchTopAvg:
                return stats.currentAvgPossibleScore < stats._topAvgPossibleScore;
            case FailureType.IfGenerationFailsToMatchTopBest:
                return stats.currentPossibleScore < stats._topPossibleScore;
            case FailureType.IfGenerationFailsToMatchTopAvgAndTopBest:
                return (stats.currentAvgPossibleScore < stats._topAvgPossibleScore) || (stats.currentPossibleScore < stats._topPossibleScore);
            case FailureType.NoFailure:
                return false;
        }
        return false;
    }
    
    // public void UpdatePopulationFromSlider()
    // {
    //     populationSize = Mathf.RoundToInt(_populationSlider.value);
    //     _populationText.text = $"Population: {populationSize}";
    // }

    public void UpdateBonusMultiplier()
    {
        fitnessMultiplyBonus = float.Parse(_bonusMultipyer.text);
    }
    
    public void UpdateCheckPointMultiplier()
    {
        fitnessMultiplyCheckPoint = float.Parse(_checkPointMultipyer.text);
    }
    
    public void UpdateDistanceMultiplier()
    {
        fitnessMultiplyDistance = float.Parse(_distanceMultipyer.text);
    }
    
    public void UpdateTimeMultiplier()
    {
        fitnessMultiplyTime = float.Parse(_timeMultipyer.text);
    }
    
    public void UpdateDeathSphereSize()
    {
        deathSphereStartSize = float.Parse(_deathSphereSizeInput.text);
    }
    
    public void UpdateDeathSphereSpeed()
    {
        deathSphereSpeed = float.Parse(_deathSphereSpeedInput.text);
    }
    
    public void UpdateDeathSphereStartTime()
    {
        deathSphereStartTime = float.Parse(_deathSphereStartTimeInput.text);
    }
    
    private void Start()
    {
        UpdateSlicersAndInputsToValues();
    }

    public void UpdateSlicersAndInputsToValues()
    {
        _topPercentSlider.value = topPercent;
        _trialTimeSlider.value = trialTime;
        _clonesSlider.value = Mathf.RoundToInt(((float)numberOfClones / populationSize) * 100);
        _mutationSlider.value = mutationChance * 100;
        _bonusMultipyer.text = fitnessMultiplyBonus.ToString();
        _checkPointMultipyer.text = fitnessMultiplyCheckPoint.ToString();
        _distanceMultipyer.text = fitnessMultiplyDistance.ToString();
        _timeMultipyer.text = fitnessMultiplyTime.ToString();
        _deathSphereSizeInput.text = deathSphereStartSize.ToString();
        _deathSphereSpeedInput.text = deathSphereSpeed.ToString();
        _deathSphereStartTimeInput.text = deathSphereStartTime.ToString();
        _gameSpeedSlider.value = gameSpeed;
        // _populationSlider.value = populationSize;
        // Debug.Log(Mathf.RoundToInt(((float)numberOfClones / populationSize) * 100));
    }

    private void Update()
    {
        if (settings.isOn)
        {
            settingsObject.SetActive(true);
            UpdateMutationFromSlider();
            UpdateTopPercentFromSlider();
            UpdateTrialTimeFromSlider();
            UpdateClonesSlider();
            UpdateBonusMultiplier();
            UpdateCheckPointMultiplier();
            UpdateDistanceMultiplier();
            UpdateTimeMultiplier();
            UpdateDeathSphereSize();
            UpdateDeathSphereSpeed();
            UpdateDeathSphereStartTime();
            UpdateGameSpeedFromSlider();
            // UpdatePopulationFromSlider();
        }
        else
        {
            settingsObject.SetActive(false);
        }
        
    }
}