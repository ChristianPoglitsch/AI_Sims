using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public class MessageDecorator : MonoBehaviour
{
    public StringEvent sendMessageTo;
    public bool processMessage = true;

    public void ProcessMessage(string message)
    {
        Debug.Log(message);
        if (processMessage)
        {
            message = Regex.Match(message, @"^\d").Value; // match first digit
            Console.WriteLine(message);  // Output: 0
        }
        sendMessageTo.Invoke(message);
    }
}
