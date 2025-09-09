using LLMUnity;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;

public class LLM_Handler : MonoBehaviour
{
    public VoiceHandler voiceHandler;
    public ConversationManager conversationManager;

    private LLMCharacter llmCharacter;
    private string replyMessage;    

    private void Start()
    {
        llmCharacter = GetComponent<LLMCharacter>();
    }

    void HandleReply(string reply)
    {
        replyMessage = reply;
    }

    void ReplyCompleted()
    {
        Debug.Log(replyMessage);
        conversationManager.TalkNpc(replyMessage, voiceHandler);
    }

    public void ProcessMessage(string message)
    {
        Debug.Log(message);
        _ = llmCharacter.Chat(message, HandleReply, ReplyCompleted);
    }
}
