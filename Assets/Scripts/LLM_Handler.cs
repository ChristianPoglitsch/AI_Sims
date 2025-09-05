using LLMUnity;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]  // makes it show up in Inspector
public class StringEvent : UnityEvent<string> { }

public class LLM_Handler : MonoBehaviour
{
    private LLMCharacter llmCharacter;
    private Talk talk;
    private string replyMessage;

    private void Start()
    {
        llmCharacter = GetComponent<LLMCharacter>();
        talk = llmCharacter.GetComponent<Talk>();
    }

    void HandleReply(string reply)
    {
        replyMessage = reply;
    }

    void ReplyCompleted()
    {
        Debug.Log(replyMessage);
        talk.Text2Speech(replyMessage);
    }

    public void ProcessExample()
    {
        string message = "Hello bot!";
        ProcessMessage(message);
    }

    public void ProcessMessage(string message)
    {
        Debug.Log(message);
        _ = llmCharacter.Chat(message, HandleReply, ReplyCompleted);
    }
}
