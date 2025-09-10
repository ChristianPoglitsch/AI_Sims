using ReadyPlayerMe.Core;
using System;
using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;

public class Talk : MonoBehaviour
{
    public Text2Speech speech; // assign in Inspector
    public AudioSource audioSource;

    // Event to notify when speech finished
    public event Action OnSpeechFinished;

    /// <summary>
    /// Start text-to-speech and optionally notify when done
    /// </summary>
    public void Text2Speech(string text, VoiceHandler voiceHandler, string voice)
    {
        StartCoroutine(PlayVoice(text, voiceHandler, voice));
    }

    private IEnumerator PlayVoice(string text, VoiceHandler voiceHandler, string voice)
    {
        AudioClip generatedClip = null;

        // Request speech clip
        yield return StartCoroutine(speech.SpeakToClip(text, voice, clip =>
        {
            generatedClip = clip;
        }));

        if (generatedClip != null)
        {
            audioSource.mute = false;
            audioSource.loop = false;
            audioSource.clip = generatedClip;

            if (voiceHandler != null)
            {
                voiceHandler.PlayCurrentAudioClip();
            }
            else
            {
                audioSource.Play();
            }

            // Wait until playback finishes
            yield return new WaitForSeconds(generatedClip.length);

            // Fire the event
            OnSpeechFinished?.Invoke();
        }
    }
}
