using AiSims;
using LLMUnity;
using System.Collections.Generic;
using UnityEngine;

// Difficulty levels for LLM Characters
public enum DifficultyLevel
{
    Easy,
    Challenging
}

[System.Serializable]
public class NpcEntry
{
    [Header("NPC Reference")]
    public GameObject npc; // The GameObject to control
    public float delaySeconds = 2f; // Delay before this NPC starts moving
    public string startAnimation;
    public bool isActive = false; // Enable/disable this entry
}

[System.Serializable]
public class LlmEntry
{
    [Header("LLM Character Reference")]
    public LLMCharacter llmCharacter; // Reference to an LLMCharacter

    [TextArea(5, 10), Chat] public string promptText; // Easy prompt
    [TextArea(5, 10), Chat] public string promptTextChallenging; // Challenging prompt

    public float delaySeconds = 0f; // Delay before this prompt is triggered
    public bool isActive = false; // Enable/disable this entry
    public DifficultyLevel difficulty = DifficultyLevel.Easy; // Default difficulty
}

public class Complexity : MonoBehaviour
{
    [Header("NPC Entries")]
    public List<NpcEntry> npcEntries = new List<NpcEntry>();

    [Header("LLM Character Entries")]
    public List<LlmEntry> llmEntries = new List<LlmEntry>();

    [Header("Global Difficulty Settings")]
    public bool useGlobalDifficulty = false; // Toggle to override all entries
    public DifficultyLevel globalDifficulty = DifficultyLevel.Easy; // Global difficulty level

    void Start()
    {
        // Handle NPCs
        foreach (var entry in npcEntries)
        {
            if (entry.npc != null)
            {
                NpcMovement movement = entry.npc.GetComponent<NpcMovement>();
                if (movement != null && !string.IsNullOrEmpty(entry.startAnimation))
                {
                    movement.SetAnimation(entry.startAnimation);
                }

                if (entry.isActive)
                {
                    StartCoroutine(StartNpcMovementAfterDelay(entry));
                }
            }
            else
            {
                Debug.LogWarning("NpcEntry has a null npc reference.");
            }
        }

        // Handle LLM Characters
        foreach (var entry in llmEntries)
        {
            if (!entry.isActive) continue; // Skip inactive entries

            if (entry.llmCharacter != null)
            {
                StartCoroutine(SendPromptAfterDelay(entry));
            }
            else
            {
                Debug.LogWarning("LlmEntry has a null LLMCharacter reference.");
            }
        }
    }

    private System.Collections.IEnumerator StartNpcMovementAfterDelay(NpcEntry entry)
    {
        yield return new WaitForSeconds(entry.delaySeconds);

        NpcMovement movement = entry.npc.GetComponent<NpcMovement>();
        if (movement != null)
        {
            movement.StartMovement();
        }
        else
        {
            Debug.LogWarning($"NpcMovement component not found on {entry.npc.name}");
        }
    }

    private System.Collections.IEnumerator SendPromptAfterDelay(LlmEntry entry)
    {
        yield return new WaitForSeconds(entry.delaySeconds);

        // Determine which difficulty setting to use
        DifficultyLevel effectiveDifficulty = useGlobalDifficulty ? globalDifficulty : entry.difficulty;

        string finalPrompt = string.Empty;

        if (effectiveDifficulty == DifficultyLevel.Easy)
        {
            finalPrompt = entry.promptText;
        }
        else if (effectiveDifficulty == DifficultyLevel.Challenging)
        {
            finalPrompt = entry.promptTextChallenging;
        }

        if (!string.IsNullOrEmpty(finalPrompt))
        {
            entry.llmCharacter.SetPrompt(finalPrompt);
        }
        else
        {
            Debug.LogWarning($"No prompt text specified for {entry.llmCharacter.name} (Difficulty: {effectiveDifficulty})");
        }
    }
}
