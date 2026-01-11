using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScooterUprightDetector : MonoBehaviour
{
    [SerializeField]
    private AudioSource soundEffect;

    [SerializeField]
    private string scooterTag = "Scooter";

    [SerializeField]
    private float maxAllowedTiltDegrees = 5f;

    [SerializeField]
    private float requiredUprightTime = 0.5f;

    [SerializeField]
    private float maxAllowedMovement = 0.1f;

    [SerializeField]
    private bool playOnlyOnce = true;

    [SerializeField]
    private GameObject visuals;

    private bool played;
    private readonly Dictionary<Transform, float> uprightTimers = new();

    private void OnTriggerStay(Collider collider)
    {
        if (played && playOnlyOnce || soundEffect == null)
        {
            return;
        }

        var root = collider.attachedRigidbody
            ? collider.attachedRigidbody.transform
            : collider.transform;

        if (!root.CompareTag(scooterTag))
        {
            return;
        }

        var grabInteractable = root.GetComponentInChildren<XRGrabInteractable>(true);

        if (grabInteractable == null || (grabInteractable != null && grabInteractable.isSelected))
        {
            return;
        }

        float tilt = Vector3.Angle(root.up, Vector3.up);
        bool isUpright = tilt <= maxAllowedTiltDegrees;
        bool isNotMoving = false;

        var rigidbody = root.GetComponent<Rigidbody>();

        if (rigidbody != null)
        {
            isNotMoving = rigidbody.linearVelocity.magnitude <= maxAllowedMovement;
        }

        if (isUpright && isNotMoving)
        {
            if (!uprightTimers.ContainsKey(root))
            {
                uprightTimers[root] = 0f;
            }

            uprightTimers[root] += Time.deltaTime;

            if (uprightTimers[root] >= requiredUprightTime)
            {
                visuals.SetActive(false);
                soundEffect.Play();
                played = true;
                grabInteractable.enabled = false;
                rigidbody.isKinematic = true;
                QuestEventStore.Instance.SetQuestEvent(QuestEvent.PickedUpScooter, true);
            }
        }
        else
        {
            uprightTimers[root] = 0f;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        var root = collider.attachedRigidbody 
            ? collider.attachedRigidbody.transform 
            : collider.transform;

        uprightTimers.Remove(root);
    }
}
