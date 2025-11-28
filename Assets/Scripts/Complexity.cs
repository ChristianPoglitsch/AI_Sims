using AiSims;
using LLMUnity;
using System;
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
public class LlmQuestEntry
{
    [Header("LLM Character Reference")]
    public LLMCharacter llmCharacter; // Reference to an LLMCharacter
    public LLM_Handler llmHandler;

    [Header("Prompts for this Quest")]
    [TextArea(5, 10), Chat]
    public List<string> promptTexts = new List<string>();

    [TextArea(5, 10), Chat]
    public List<string> quests = new List<string>();

    public float delaySeconds = 0f;
    public bool isActive = false;

    public List<GameObject> questSuccessfull;
}

[System.Serializable]
public class LlmEntry
{
    [Header("LLM Character Reference")]
    public LLMCharacter llmCharacter;

    [TextArea(5, 10), Chat] public string promptText;
    [TextArea(5, 10), Chat] public string promptTextChallenging;

    public float delaySeconds = 0f;
    public bool isActive = false;
    public DifficultyLevel difficulty = DifficultyLevel.Easy;
}

public class Complexity : MonoBehaviour
{
    [Header("NPC Entries")]
    public List<NpcEntry> npcEntries = new List<NpcEntry>();

    [Header("LLM Character Quest Entries")]
    public List<LlmQuestEntry> llmQuestEntries = new List<LlmQuestEntry>();

    [Header("LLM Character Complexity Entries")]
    public List<LlmEntry> llmEntries = new List<LlmEntry>();

    [Header("Global Difficulty Settings (Complexity Entries)")]
    public bool useGlobalDifficulty = false;
    public DifficultyLevel globalDifficulty = DifficultyLevel.Easy;

    private int currentEntry = -1;
    private int currentQuest = -1;

    private List<Quaternion> originalOrientation = new List<Quaternion>();


    void Start()
    {
        // Store original orientations of LLM Quest parents
        originalOrientation.Clear();
        foreach (var entry in llmQuestEntries)
        {
            if (entry.llmCharacter != null && entry.llmCharacter.transform.parent != null)
            {
                originalOrientation.Add(entry.llmCharacter.transform.parent.rotation);
            }
            else
            {
                originalOrientation.Add(Quaternion.identity);
                Debug.LogWarning("Could not save orientation: LLMQuestEntry missing character or parent.");
            }
        }

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
                    StartCoroutine(StartNpcMovementAfterDelay(entry));
            }
            else
            {
                Debug.LogWarning("NpcEntry has a null npc reference.");
            }
        }

        UpdateLlmInstruction();
    }


    public void UpdateLlmInstruction()
    {
        ResetOrientation();   // Reset whenever LLM instructions change

        foreach (var entry in llmEntries)
        {
            if (!entry.isActive) continue;

            if (entry.llmCharacter != null)
            {
                StartCoroutine(SendPromptAfterDelay(entry));
            }
            else
            {
                Debug.LogWarning("LlmEntry has a null character reference.");
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
            Debug.LogWarning($"NpcMovement not found on: {entry.npc.name}");
        }
    }


    private System.Collections.IEnumerator SendPromptAfterDelay(LlmEntry entry)
    {
        yield return new WaitForSeconds(entry.delaySeconds);

        DifficultyLevel effectiveDifficulty = useGlobalDifficulty ? globalDifficulty : entry.difficulty;

        string finalPrompt = (effectiveDifficulty == DifficultyLevel.Challenging)
            ? entry.promptTextChallenging
            : entry.promptText;

        if (!string.IsNullOrEmpty(finalPrompt))
        {
            entry.llmCharacter.SetPrompt(finalPrompt);
        }
        else
        {
            Debug.LogWarning($"No prompt assigned for {entry.llmCharacter.name} at difficulty {effectiveDifficulty}");
        }
    }


    public void SetLlmQuestByIndex(int index, int questIndex)
    {
        ResetOrientation();   // Reset when switching quests

        if (index < 0 || index >= llmQuestEntries.Count)
        {
            Debug.LogWarning($"Invalid index {index} for LLM Quest Entries.");
            return;
        }

        var entry = llmQuestEntries[index];

        if (questIndex < 0 || questIndex >= entry.promptTexts.Count)
        {
            Debug.LogWarning($"Invalid quest index {questIndex}.");
            return;
        }

        currentEntry = index;
        currentQuest = questIndex;

        if (entry.isActive &&
            entry.llmCharacter != null &&
            questIndex < entry.promptTexts.Count &&
            questIndex < entry.quests.Count)
        {
            string finalPrompt = entry.promptTexts[questIndex];

            if (!string.IsNullOrEmpty(finalPrompt))
            {
                entry.llmCharacter.SetPrompt(finalPrompt);
                entry.llmHandler.EvaluationString = entry.quests[questIndex];
            }
            else
            {
                Debug.LogWarning($"Prompt text missing for {entry.llmCharacter.name}.");
            }
        }
    }


    public void SetCurrentQuestSuccessful()
    {
        if (currentEntry >= 0 &&
            currentEntry < llmQuestEntries.Count &&
            currentQuest >= 0 &&
            currentQuest < llmQuestEntries[currentEntry].questSuccessfull.Count)
        {
            llmQuestEntries[currentEntry].questSuccessfull[currentQuest].SetActive(true);
        }
    }


    // ==================================================
    // RESET ORIENTATION FOR ALL QUEST CHARACTERS
    // ==================================================
    public void ResetOrientation()
    {
        if (originalOrientation.Count != llmQuestEntries.Count)
        {
            Debug.LogWarning("Orientation list mismatch — cannot reset.");
            return;
        }

        for (int i = 0; i < llmQuestEntries.Count; i++)
        {
            var entry = llmQuestEntries[i];
            if (entry.llmCharacter != null && entry.llmCharacter.transform.parent != null)
            {
                entry.llmCharacter.transform.parent.rotation = originalOrientation[i];
                entry.isActive = false;
            }
        }
    }
}
