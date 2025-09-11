using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class MessageDecorator : MonoBehaviour
{
    public TMP_Text text;
    public bool processMessage = false;
    
    private LLM_Handler llmHandler;
    private List<string> messages = new List<string>();

    public void ProcessMessage(string message)
    {
        if (processMessage)
        {
            message = Regex.Match(message, @"^\d").Value; // match first digit
            Console.WriteLine(message);  // Output: 0
        }
        text.text = message;
    }
    
    //public string FilterMessage(string message)
    //{
    //    // Remove all text between parentheses, including the parentheses
    //    //message = Regex.Replace(message, @"\([^)]*\)", "");
    //    return message;
    //}

    public void AddMessage(string m)
    {
        messages.Add(m);
    }

    public List<string> GetMessages() 
    { 
        return messages;
    }

    public void SetLlmHandler(LLM_Handler handler)
    {
        this.llmHandler = handler;
    }

    public void EvaluateConversation()
    {
        llmHandler.GetLlm().ClearChat();

        string combined = string.Join("\n\n", messages);
        combined += "\nBased on the chat history, was the user friendly?";

        // Now pass it to your function
        llmHandler.ProcessMessage(combined, false);
    }
}
