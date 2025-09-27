using AiSims;
using LLMUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterHandlerPair
{
    public LLMCharacter character;
    public LLM_Handler handler;
}

[System.Serializable]
public class DifficultyConfig
{
    public string difficultyName; // e.g. "Easy", "Medium", "Hard"
    public List<CharacterHandlerPair> characterHandlerList;
}

public class Complexity : MonoBehaviour
{
    public float Visualization = 0.0f;
    public float StoryTelling = 0.0f;

    // Multiple configs, one per difficulty
    public List<DifficultyConfig> difficultyConfigs;
}
