using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class ReturnGrabObject : MonoBehaviour
{
    public float returnDelay = 3f;
    public float returnDuration = 0.7f;
    public float arcHeight = 0.15f;

    [Header("Optional FX")]
    public ParticleSystem returnParticles;
    public AudioSource returnAudio;

    private XRGrabInteractable grab;
    private Rigidbody rigidBody;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Coroutine returnRoutine;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rigidBody = GetComponent<Rigidbody>();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        returnParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        returnAudio?.Stop();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
        }

        returnRoutine = StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        if (grab.isSelected)
        {
            returnRoutine = null;
            yield break;
        }

        yield return StartCoroutine(SmoothReturn());

        returnRoutine = null;
    }

    private IEnumerator SmoothReturn()
    {
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.isKinematic = true;

        returnParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        returnParticles?.Play(true);

        returnAudio?.Stop();
        returnAudio?.Play();

        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;

        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            float smoothT = t * t * (3f - 2f * t);

            Vector3 basePos = Vector3.Lerp(fromPos, startPosition, smoothT);

            float heightOffset = Mathf.Sin(t * Mathf.PI) * arcHeight;
            basePos.y += heightOffset;

            transform.position = basePos;
            transform.rotation = Quaternion.Slerp(fromRot, startRotation, smoothT);

            elapsed += Time.deltaTime;
            yield return null;

            if (grab.isSelected)
            {
                returnParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                returnAudio?.Stop();

                rigidBody.isKinematic = false;
                yield break;
            }
        }

        transform.SetPositionAndRotation(startPosition, startRotation);

        returnParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        returnAudio?.Stop();

        rigidBody.isKinematic = false;
    }
}