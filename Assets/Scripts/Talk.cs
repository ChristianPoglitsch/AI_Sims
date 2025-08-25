using ReadyPlayerMe.Core;
using System.Collections;
using UnityEngine;

public class Talk : MonoBehaviour
{
    public Text2Speech speech; // assign in Inspector
    private AudioSource audioSource;
    private VoiceHandler voiceHandler;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        voiceHandler = GetComponent<VoiceHandler>();
    }

    void Start()
    {
        // Example: speak automatically after startup
        //StartCoroutine(PlayVoice("Hello Unity, I am speaking using OpenAI TTS!"));
    }

    public void Text2Speech(string text)
    {
        StartCoroutine(PlayVoice(text));
    }

    /// <summary>
    /// Coroutine that requests speech from OpenAI and plays it.
    /// </summary>
    private IEnumerator PlayVoice(string text)
    {
        yield return StartCoroutine(speech.SpeakToClip(text, clip =>
        {
            if (clip != null)
            {
                audioSource.loop = false;
                audioSource.clip = clip;

                // If you're using ReadyPlayerMe voice animation
                if (voiceHandler != null)
                {
                    voiceHandler.PlayCurrentAudioClip();
                }
                else
                {
                    audioSource.Play();
                }
            }
        }));
    }
}
