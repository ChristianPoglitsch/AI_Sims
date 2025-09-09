using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class MessageDecorator : MonoBehaviour
{
    public TMP_Text text;
    public bool processMessage = false;

    public void ProcessMessage(string message)
    {
        if (processMessage)
        {
            message = Regex.Match(message, @"^\d").Value; // match first digit
            Console.WriteLine(message);  // Output: 0
        }
        text.text = message;
    }
}
