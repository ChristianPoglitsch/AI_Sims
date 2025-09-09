using ReadyPlayerMe.Core;
using System.Collections;
using UnityEngine;

public class Talk : MonoBehaviour
{
    public Text2Speech speech; // assign in Inspector
    public AudioSource audioSource;

    public void Text2Speech(string text, VoiceHandler voiceHandler)
    {
        StartCoroutine(PlayVoice(text, voiceHandler));
    }

    /// <summary>
    /// Coroutine that requests speech from OpenAI and plays it.
    /// </summary>
    private IEnumerator PlayVoice(string text, VoiceHandler voiceHandler)
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
