using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public enum MessageTypes
{
    system = 0,
    assistant = 1,
    user = 2
}

public class MessageDecorator : MonoBehaviour
{
    public TMP_Text text;

    private bool processMessage = false;
    private LLM_Handler llmHandler;

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

    public void AddMessage(string message, MessageTypes type)
    {
        if (llmHandler == null || llmHandler.GetLlm() == null)
        {
            Debug.Log("LLM Handler for evaluating quests is not assigned.");
            return;
        }

        // Convert enum to string role
        string role = type.ToString(); // "system", "assistant", "user"

        // Call LLM with role and message
        llmHandler.GetLlm().AddMessage(role, message);
    }

    public void SetLlmHandler(LLM_Handler handler)
    {
        this.llmHandler = handler;
    }

    public void EvaluateConversation()
    {
        if (llmHandler == null || llmHandler.GetLlm() == null)
        {
            Debug.Log("LLM Handler for evaluating quests is not assigned.");
            return;
        }

        string message = "\nProvide a concise one-sentence answer. Based on the chat history, determine the amount of included small talk.";

        // Now pass it to your function
        llmHandler.ProcessMessage(message, false);

        llmHandler.GetLlm().ClearChat();
    }
}
