using LLMUnity;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;

public class LLM_Handler : MonoBehaviour
{
    public VoiceHandler voiceHandler;
    public ConversationManager conversationManager;
    public string voice = "alloy";

    private NpcConnection connection;
    private LLMCharacter llmCharacter;
    private string replyMessage;
    private string userMessage;

    bool addToHistory = false;

    private void Start()
    {
        llmCharacter = GetComponent<LLMCharacter>();
        connection = GetComponent<NpcConnection>();
    }

    public string GetVoiceName()
    {
        return voice;
    }

    public string GetUserMessage()
    {
        return userMessage;
    }

    public NpcConnection GetNpcConnection()
    {
        return connection;
    }

    public LLMCharacter GetLlm() 
    { 
        return llmCharacter;
    }

    void HandleReply(string reply)
    {
        replyMessage = reply;
    }

    void ReplyCompleted()
    {
        Debug.Log(replyMessage);
        conversationManager.TalkNpc(replyMessage, voiceHandler, addToHistory);
    }

    public void ProcessMessage(string message, bool addToHist = true)
    {
        Debug.Log(message);
        userMessage = message;
        addToHistory = addToHist;
        _ = llmCharacter.Chat(message, HandleReply, ReplyCompleted, addToHistory);
    }
}
