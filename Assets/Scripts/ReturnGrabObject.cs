using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class ReturnGrabObject : MonoBehaviour
{
    [Header("Return Motion")]
    public float returnDelay = 3f;
    public float returnDuration = 0.7f;
    public float arcHeight = 0.15f;

    [Header("Optional FX")]
    public ParticleSystem returnParticles;
    public AudioSource returnAudio;

    [Header("Auto Return When Moved (e.g., knocked over)")]
    [SerializeField] 
    private bool returnWhenMoved = true;

    [SerializeField] 
    private float movedDistanceThreshold = 0.15f;

    [SerializeField] 
    private float movedVelocityThreshold = 0.25f;

    [Header("Stability / Anti-Loop")]
    [SerializeField] 
    private float captureStartPoseDelay = 2f;

    [SerializeField] 
    private float homeRadius = 0.05f;

    [SerializeField] 
    private float cooldownAfterReturn = 0.75f;

    [SerializeField]
    private bool muteScript;

    private XRGrabInteractable grab;
    private Rigidbody rigidBody;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Coroutine returnRoutine;
    private bool hasStartPose;

    private float ignoreMovedCheckUntil;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(CaptureStartPoseDelayed());
    }

    private IEnumerator CaptureStartPoseDelayed()
    {
        yield return new WaitForSeconds(captureStartPoseDelay);

        startPosition = transform.position;
        startRotation = transform.rotation;
        hasStartPose = true;
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

    private void Update()
    {
        if (!returnWhenMoved || !hasStartPose || muteScript)
            return;

        if (Time.time < ignoreMovedCheckUntil)
            return;

        if (grab != null && grab.isSelected)
            return;

        if (returnRoutine != null)
            return;

        float distSqr = (transform.position - startPosition).sqrMagnitude;
        float velSqr = rigidBody != null ? rigidBody.linearVelocity.sqrMagnitude : 0f;

        float distThSqr = movedDistanceThreshold * movedDistanceThreshold;
        float velThSqr = movedVelocityThreshold * movedVelocityThreshold;
        float homeSqr = homeRadius * homeRadius;

        bool farEnough = distSqr >= distThSqr;
        bool movingAndNotAtHome = distSqr >= homeSqr && velSqr >= velThSqr;

        if (farEnough || movingAndNotAtHome)
        {
            returnRoutine = StartCoroutine(ReturnAfterDelay());
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        rigidBody.isKinematic = false;
        rigidBody.WakeUp();

        returnParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        returnAudio?.Stop();

        ignoreMovedCheckUntil = 0f;
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

        if (grab != null && grab.isSelected)
        {
            returnRoutine = null;
            yield break;
        }

        yield return StartCoroutine(SmoothReturn());

        returnRoutine = null;
    }

    private IEnumerator SmoothReturn()
    {
        if (muteScript)
        {
            if (rigidBody != null)
            {
                rigidBody.isKinematic = false;
                rigidBody.WakeUp();
            }
            yield break;
        }

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
            if (grab != null && grab.isSelected)
            {
                returnParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                returnAudio?.Stop();

                rigidBody.isKinematic = false;
                rigidBody.WakeUp();
                yield break;
            }

            float t = elapsed / returnDuration;
            float smoothT = t * t * (3f - 2f * t);

            Vector3 basePos = Vector3.Lerp(fromPos, startPosition, smoothT);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * arcHeight;
            basePos.y += heightOffset;

            transform.position = basePos;
            transform.rotation = Quaternion.Slerp(fromRot, startRotation, smoothT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.SetPositionAndRotation(startPosition, startRotation);

        returnParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        returnAudio?.Stop();

        rigidBody.isKinematic = false;
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.Sleep();

        ignoreMovedCheckUntil = Time.time + cooldownAfterReturn;
    }

    public void SetMuted(bool muted)
    {
        muteScript = muted;

        if (muted && returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        if (muted && rigidBody != null)
        {
            rigidBody.isKinematic = false;
            rigidBody.WakeUp();
        }
    }
}