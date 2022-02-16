using UnityEngine;

public class GenerationStats : MonoBehaviour
{

    [SerializeField] private bool showStats = true;
    public int populationSize = 1;
    public int _generation = 1;
    public float _distance = 0;
    public float _elapsed = 0;
    public int _activeBots = 0;
    public int _checkPointCount = 0;
    public float _lastBestFitnessScore = 0;
    public float _lastAvgFitnessScore = 0;
    public float _lastBestDistance = 0;
    public float _lastAvgDistance = 0;
    public float _lastBestPossibleScore = 0;
    public float _lastAvgPossibleScore = 0;
    public float _lastTimeAlive = 0;
    public float _lastAvgAliveTime = 0;
    public float _lastSurvivors = 0;
    public int _lastPopulationSize = 1;
    public Vector3 _winningDeathPos;
    public int _lastCheckPointCount = 0;
    public float _topFitnessScore = 0;
    public float _topAvgFitnessScore = 0;
    public float _topDistance = 0;
    public float _topAvgDistance = 0;
    public float _topPossibleScore = 0;
    public float _topAvgPossibleScore = 0;
    public float _topTimeAlive = 0;
    public float _topAvgTimeAlive = 0;
    public float _topSurvivors = 0;
    public float _topAvgSurvivors = 0;
    public float _topSurvivorsPopulationSize = 1f;
    public int _topCheckPointCount = 0;
    public float _topAvgCheckPointCount = 0;
    public int _totalCheckPointCount = 0;
    public int _totalBrainsCreated = 0;
    public float _totalFitnessScore = 0;
    public float _totalPossibleScore = 0;
    
    
    private readonly GUIStyle _guiStyle = new GUIStyle();
    public float mutationChance { get; set; }
    public float trialTime { get; set; }

    void OnGUI()
    {
        if (!showStats) return;
        // guiStyle = new GUIStyle(GUI.skin.box);
        _guiStyle.fontSize = 25;
        _guiStyle.normal.textColor = Color.white;
        // guiStyle.normal.background = Texture2D.CreateExternalTexture(1500, 1500, TextureFormat.Alpha8, false, true, IntPtr.Zero);
        GUI.BeginGroup(new Rect(10,10,1500,1500));
        GUI.Box(new Rect(0,0,1500,100),$"Settings", _guiStyle);
        GUI.Label(new Rect(10,25,300,30), $"Generation: {_generation}",_guiStyle);
        GUI.Label(new Rect(10,50,300,30), $"Population: {populationSize}, Total Created: {_totalBrainsCreated}",_guiStyle);
        GUI.Label(new Rect(10,75,300,30), $"Mutation: {Mathf.RoundToInt(mutationChance * 100)}%",_guiStyle);
        // GUI.Label(new Rect(10,100,300,30), $"Time: {_elapsed:0.00}" + " / " 
                                                                   // + $"Time: {trialTime:0.00}",guiStyle);
        // GUI.Label(new Rect(10,125,800,30), $"Furthest Distance {lastBestDistance} / 155",guiStyle);
        GUI.Box(new Rect(0,125,1500,700),$"Stats", _guiStyle);
        GUI.contentColor = new Color(0.81f, 1f, 0.71f);
        
        GUI.Label(new Rect(10,150,1500,30), $"Survivors: Now: {_activeBots}/" +
                                            $"{Mathf.RoundToInt(((float) _activeBots / populationSize) * 100)}% /" +
                                            $" Last : {_lastSurvivors}/" +
                                            $"{Mathf.RoundToInt((_lastSurvivors / _lastPopulationSize) * 100)}% /" +
                                            $" Top: {_topSurvivors}/" +
                                            $"{Mathf.RoundToInt((_topSurvivors / _topSurvivorsPopulationSize) * 100)}%", _guiStyle);
        GUI.contentColor = new Color(0.6f, 0.63f, 1f);
        GUI.Label(new Rect(10,200,1500,30), $"Distance: Now: {Mathf.RoundToInt(_lastBestDistance)}/155" +
                                            $" Last: {Mathf.RoundToInt(_distance)} /" +
                                            $" LastAvg: {Mathf.RoundToInt(_lastAvgDistance)}" +
                                            $" Top: {Mathf.RoundToInt(_topDistance)} /" +
                                            $" TopAvg: {Mathf.RoundToInt(_topAvgDistance)}",_guiStyle);
        GUI.contentColor = new Color(1f, 0.99f, 0.67f);
        GUI.Label(new Rect(10,250,1500,30), $"Survival Time: Now: {_elapsed:0.00}/{trialTime:0.00} /" +
                                            $" Last: {_lastTimeAlive:0.00} /" +
                                            $" LastAvg: {_lastAvgAliveTime:0.00}" +
                                            $" Top: {_topTimeAlive:0.00} /" +
                                            $" TopAvg: {_topAvgTimeAlive:0.00}",_guiStyle);
        GUI.contentColor = new Color(0.7f, 0.51f, 0.61f);
        GUI.Label(new Rect(10,300,1500,30), $"Fitness Score: Now: N/A /" +
                                            $" Last: {Mathf.RoundToInt(_lastBestFitnessScore)}% /" +
                                            $" LastAvg: {_lastAvgFitnessScore}% /" +
                                            $" Top: {Mathf.RoundToInt(_topFitnessScore)}% /" +
                                            $" TopAvg: {_topAvgFitnessScore}% /" +
                                            $" RunningAvg: {Mathf.RoundToInt((_totalFitnessScore) / _generation)}%",_guiStyle);
        GUI.contentColor = new Color(0.99f, 0.8f, 0.48f);
        GUI.Label(new Rect(10,350,1500,30), $"Possible Score: Now: N/A /" +
                                            $" Last: {Mathf.RoundToInt(_lastBestPossibleScore)}% /" +
                                            $" LastAvg: {_lastAvgPossibleScore}%"+
                                            $" Top: {Mathf.RoundToInt(_topPossibleScore)}% /" +
                                            $" TopAvg: {_topAvgPossibleScore}% /" +
                                            $" RunningAvg: {Mathf.RoundToInt(((_totalPossibleScore) / _generation ))}%",_guiStyle);
        GUI.Label(new Rect(10,400,1500,30), $"Check Point: Now: {_checkPointCount} /" +
                                            $" Last: {_lastCheckPointCount} /" +
                                            $" LastAvg: {Mathf.RoundToInt(((float) _lastCheckPointCount / _lastPopulationSize) * 100)}%"+
                                            $" Top: {_topCheckPointCount} /" +
                                            $" TopAvg: {_topAvgCheckPointCount}% /" +
                                            $" Running Total: {_totalCheckPointCount} /" +
                                            $" Running Avg: {Mathf.RoundToInt(((float) _totalCheckPointCount/ _totalBrainsCreated) * 100)}%",_guiStyle);
        
        GUI.EndGroup();
    }
}