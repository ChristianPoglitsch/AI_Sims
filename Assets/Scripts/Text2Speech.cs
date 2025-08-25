using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class Text2Speech : MonoBehaviour
{
    [Header("OpenAI Settings")]
    [SerializeField] private string apiKey = "YOUR_API_KEY";   // set in Inspector
    [SerializeField] private string voice = "alloy";           // OpenAI TTS voices
    [SerializeField] private string model = "gpt-4o-mini-tts"; // fast + cheap

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Coroutine: Request speech from OpenAI and return AudioClip.
    /// Usage: yield return StartCoroutine(SpeakToClip("Hello", clip => { ... }));
    /// </summary>
    public IEnumerator SpeakToClip(string text, Action<AudioClip> onReady)
    {
        string url = "https://api.openai.com/v1/audio/speech";
        string jsonBody = "{\"model\":\"gpt-4o-mini-tts\",\"voice\":\"alloy\",\"input\":\"" + text + "\",\"response_format\":\"wav\"}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("TTS Error: " + www.error);
                onReady?.Invoke(null);
            }
            else
            {
                byte[] wavData = www.downloadHandler.data;
                AudioClip clip = WavToAudioClip(wavData, "TTSClip");
                onReady?.Invoke(clip);
            }
        }
    }

    /// <summary>
    /// Converts WAV bytes into a Unity AudioClip.
    /// </summary>
    private AudioClip WavToAudioClip(byte[] wavFile, string name)
    {
        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int bitsPerSample = BitConverter.ToInt16(wavFile, 34);
        int dataIndex = 44; // PCM WAV header size

        int sampleCount = (wavFile.Length - dataIndex) / (bitsPerSample / 8);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(wavFile, dataIndex + i * 2);
            samples[i] = sample / 32768f;
        }

        AudioClip audioClip = AudioClip.Create(name, sampleCount, channels, sampleRate, false);
        audioClip.SetData(samples, 0);
        return audioClip;
    }
}
