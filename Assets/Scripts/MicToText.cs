using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Net;

public class MicToText : MonoBehaviour
{
    [Header("OpenAI API Key")]
    [SerializeField] private string apiKey = "YOUR_API_KEY";

    [Header("STT Settings")]
    [SerializeField] private string sttModel = "gpt-4o-mini-transcribe";

    private string micDevice;

    void Start()
    {
        // Pick first microphone available
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            Debug.Log("Using microphone: " + micDevice);
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }

    // Call this from a button or another script
    public void StartRecording(int recordSeconds = 5)
    {
        if (micDevice == null) return;
        StartCoroutine(RecordAndSend(recordSeconds));
    }

    private IEnumerator RecordAndSend(int duration)
    {
        // Record microphone audio
        AudioClip recording = Microphone.Start(micDevice, false, duration, 16000);
        Debug.Log("Recording...");

        yield return new WaitForSeconds(duration);

        Microphone.End(micDevice);
        Debug.Log("Recording stopped, sending to Whisper...");

        // Convert AudioClip → WAV bytes
        byte[] wavData = WavUtility.FromAudioClip(recording);

        // Create multipart form for Whisper API
        WWWForm form = new WWWForm();
        form.AddField("model", sttModel);
        form.AddBinaryData("file", wavData, "recording.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", form))
        {
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Whisper STT Error: " + www.error);
            }
            else
            {
                Debug.Log("Whisper Response: " + www.downloadHandler.text);
            }
        }
    }
}
