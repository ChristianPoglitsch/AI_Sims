using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.IO;

public class Speech2Text : MonoBehaviour
{
    [Header("OpenAI API Key")]
    [SerializeField] private string apiKeyFileName = "openai_api_key.txt"; // file name for local key

    [Header("STT Settings")]
    [SerializeField] private string sttModel = "gpt-4o-mini-transcribe";

    public LLM_Handler llm_handler;

    private string micDevice;
    private AudioClip recording;
    private bool isRecording = false;
    private string apiKey;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            Debug.Log("Using microphone: " + micDevice);
        }
        else
        {
            Debug.LogError("No microphone found!");
        }

        LoadApiKey();
    }

    private void LoadApiKey()
    {
        try
        {
            // You can place your key in StreamingAssets/openai_api_key.txt
            string path = Path.Combine(Application.streamingAssetsPath, apiKeyFileName);

            if (File.Exists(path))
            {
                apiKey = File.ReadAllText(path).Trim();
                Debug.Log("✅ OpenAI API key loaded from file.");
            }
            else
            {
                Debug.LogError("❌ API key file not found at: " + path);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Failed to load API key: " + e.Message);
        }
    }

    /// Combined function: toggle recording on/off
    public void ToggleRecording()
    {
        if (!isRecording)
        {
            // --- START recording ---
            Debug.Log("Recording started...");
            isRecording = true;
            recording = Microphone.Start(micDevice, false, 60, 16000); // buffer max 60s
        }
        else
        {
            // --- STOP recording ---
            Debug.Log("Recording stopped, sending to Whisper...");
            Microphone.End(micDevice);
            isRecording = false;

            // Convert AudioClip → WAV
            byte[] wavData = WavUtility.FromAudioClip(recording);
            StartCoroutine(SendToWhisper(wavData));
        }
    }

    private IEnumerator SendToWhisper(byte[] wavData)
    {
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
                llm_handler?.ProcessMessage(www.downloadHandler.text);
            }
        }
    }
}
