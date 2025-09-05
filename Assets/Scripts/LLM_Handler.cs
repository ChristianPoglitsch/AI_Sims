using LLMUnity;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]  // makes it show up in Inspector
public class StringEvent : UnityEvent<string> { }

public class LLM_Handler : MonoBehaviour
{
    public LLMCharacter llmCharacter;

    // Now UnityEvent can take a string parameter
    public StringEvent sendMessageTo;

    private string replyMessage;

    void HandleReply(string reply)
    {
        replyMessage = reply;
    }

    void ReplyCompleted()
    {
        Debug.Log(replyMessage);
        // Invoke UnityEvent with replyMessage as parameter
        sendMessageTo.Invoke(replyMessage);
    }

    public void ProcessExample()
    {
        string message = "Hello bot!";
        ProcessMessage(message);
    }

    public void ProcessMessage(string message)
    {
        _ = llmCharacter.Chat(message, HandleReply, ReplyCompleted);
    }
}
