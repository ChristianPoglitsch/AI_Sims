using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public class MessageDecorator : MonoBehaviour
{
    public StringEvent sendMessageTo;

    public void ProcessMessage(string message)
    {
        Debug.Log(message);
        string replyMessage = Regex.Match(message, @"^\d").Value; // match first digit
        Console.WriteLine(replyMessage);  // Output: 0
        sendMessageTo.Invoke(replyMessage);
    }
}
