using LLMUnity;
using UnityEngine;

public class LLM_Handler : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public Talk talk;

    private string replyMessage;

    public void Start()
    {
        //ProcessExample();
    }

    void HandleReply(string reply)
    {
        // do something with the reply from the model
        //Debug.Log(reply);
        replyMessage = reply;
    }

    void ReplyCompleted()
    {
        // do something when the reply from the model is complete
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
        _ = llmCharacter.Chat(message, HandleReply, ReplyCompleted);
    }
}
