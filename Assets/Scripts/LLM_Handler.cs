using LLMUnity;
using UnityEngine;

public class MyScript : MonoBehaviour
{
    public LLMCharacter llmCharacter;

    private string replyMessage;

    public void Start()
    {
        Game();
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
    }

    void Game()
    {
        // your game function
        string message = "Hello bot!";
        _ = llmCharacter.Chat(message, HandleReply, ReplyCompleted);
    }
}
