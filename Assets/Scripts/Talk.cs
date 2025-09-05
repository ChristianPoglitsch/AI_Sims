using ReadyPlayerMe.Core;
using System.Collections;
using UnityEngine;

public class Talk : MonoBehaviour
{
    public Text2Speech speech; // assign in Inspector
    public AudioSource audioSource;
    public VoiceHandler voiceHandler;

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
                audioSource.mute = false;
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
