using ReadyPlayerMe.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talk : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitAndPlaySound());
    }

    private IEnumerator WaitAndPlaySound()
    {
        yield return new WaitForSeconds(1);
        GetComponent<AudioSource>().loop = true;
        GetComponent<VoiceHandler>().PlayCurrentAudioClip();
    }
}
