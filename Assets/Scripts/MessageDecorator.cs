using System;
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
    public bool ProcessedEvaluation { get; private set; } = false;

    public string EvaluationString { get; private set; } = string.Empty;

    public void ProcessMessage(string message)
    {
        if (processMessage)
        {
            message = Regex.Match(message, @"^\d").Value; // match first digit
            Console.WriteLine(message);  // Output: 0
        }
        text.text = message;
    }

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
        llmHandler.GetLlm().AddMessage(type.ToString() + ": " + role, message);
    }

    public void SetLlmHandler(LLM_Handler handler)
    {
        this.llmHandler = handler;
    }

    public void SetEvaluationInstruction(string instruction)
    {
        EvaluationString = instruction;
    }

    public void Clear()
    {
        if(ProcessedEvaluation)
            llmHandler.GetLlm().ClearChat();
    }

    public void EvaluateConversation()
    {
        if (llmHandler == null || llmHandler.GetLlm() == null || EvaluationString == string.Empty)
        {
            Debug.Log("LLM Handler for evaluating quests is not assigned.");
            return;
        }

        //string message = llmHandler.GetLlm().prompt;
        string message = "\nBased on the chat evaluate " + EvaluationString + " Respond with exactly one character: 1 if Yes, 0 if No. If the outcome is unclear, respond with 0.";

        // Now pass it to your function
        llmHandler.ProcessMessage(message, false);
        ProcessedEvaluation = true;
    }
}
