using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using System.IO;

public class Text2Speech : MonoBehaviour
{
    [Header("OpenAI Settings")]
    [SerializeField] private string apiKeyFileName = "openai_api_key.txt"; // file name for local key
    [SerializeField] private string model = "gpt-4o-mini-tts"; // fast + cheap

    private AudioSource audioSource;
    private string apiKey;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
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

    /// <summary>
    /// Coroutine: Request speech from OpenAI and return AudioClip.
    /// Usage: yield return StartCoroutine(SpeakToClip("Hello", clip => { ... }));
    /// </summary>
    public IEnumerator SpeakToClip(string text, string voice, Action<AudioClip> onReady)
    {
        string url = "https://api.openai.com/v1/audio/speech";
        string jsonBody = $"{{\"model\":\"gpt-4o-mini-tts\",\"voice\":\"{voice}\",\"input\":\"{text}\",\"response_format\":\"wav\"}}";

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
