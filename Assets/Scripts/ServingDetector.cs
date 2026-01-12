using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ServingDetector : MonoBehaviour
{
    [SerializeField]
    private AudioSource soundEffect;

    [SerializeField]
    private List<string> allowedTags;

    [SerializeField]
    private bool playOnlyOnce = true;

    [SerializeField]
    private float requiredTime = 0.5f;

    [SerializeField]
    private GameObject visuals;

    [SerializeField]
    private List<QuestEvent> questEvents;

    private bool played;
    private readonly Dictionary<Transform, float> timers = new();

    private void OnTriggerStay(Collider collider)
    {
        if (played && playOnlyOnce || soundEffect == null)
        {
            return;
        }

        var root = collider.attachedRigidbody
            ? collider.attachedRigidbody.transform
            : collider.transform;

        if (!allowedTags.Any(root.CompareTag))
        {
            return;
        }

        var grabInteractable = root.GetComponentInChildren<XRGrabInteractable>(true);

        if (grabInteractable == null)
        {
            return;
        }

        var rigidbody = root.GetComponent<Rigidbody>();

        if (!grabInteractable.isSelected)
        {
            if (!timers.ContainsKey(root))
            {
                timers[root] = 0f;
            }

            timers[root] += Time.deltaTime;

            if (timers[root] >= requiredTime)
            {
                visuals.SetActive(false);
                soundEffect.Play();
                played = true;
                grabInteractable.enabled = false;
                rigidbody.isKinematic = true;

                var returnGrabObject = root.GetComponentInChildren<ReturnGrabObject>(true);
                if (returnGrabObject)
                {
                    returnGrabObject.enabled = false;
                }

                foreach (var questEvent in questEvents)
                {
                    QuestEventStore.Instance.SetQuestEvent(questEvent, true);
                }
            }
        }
        else
        {
            timers[root] = 0f;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        var root = collider.attachedRigidbody
            ? collider.attachedRigidbody.transform
            : collider.transform;

        timers.Remove(root);
    }
}
