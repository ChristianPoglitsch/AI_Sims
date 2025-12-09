using UnityEngine;

public class ComplexityAudioManager : MonoBehaviour
{
    public AudioSource difficultyEasySource;
    public AudioSource difficultyChallengingSource;

    public Complexity complexity;

    public DifficultyLevel currentDifficulty = DifficultyLevel.Easy;

    private void Start()
    {
        UpdateAudioForDifficulty(true);
    }

    private void Update()
    {
        UpdateAudioForDifficulty();
    }

    private void UpdateAudioForDifficulty(bool force = false)
    {
        if (complexity == null)
        {
            return;
        }

        var difficulty = complexity.globalDifficulty;

        if (!force && difficulty == currentDifficulty)
        {
            return;
        }

        currentDifficulty = difficulty;

        bool isEasy = (currentDifficulty == DifficultyLevel.Easy);

        SetSourceActive(difficultyEasySource, isEasy);
        SetSourceActive(difficultyChallengingSource, !isEasy);
    }

    private void SetSourceActive(AudioSource src, bool active)
    {
        if (src == null)
        {
            return;
        }

        src.gameObject.SetActive(active);
    }
}
