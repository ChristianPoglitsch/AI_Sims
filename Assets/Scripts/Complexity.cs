using AiSims;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NavMeshEntry
{
    public GameObject npc; // The GameObject to control
    public float delaySeconds = 2f; // delay before this NPC starts moving
    public string startAnimation;
}

public class Complexity : MonoBehaviour
{
    public List<NavMeshEntry> entries = new List<NavMeshEntry>();

    void Start()
    {
        foreach (var entry in entries)
        {
            if (entry.npc != null)
            {
                NpcMovement movement = entry.npc.GetComponent<NpcMovement>();
                if (movement != null && entry.startAnimation != string.Empty)
                {
                    movement.SetAnimation(entry.startAnimation);
                }

                StartCoroutine(StartMovementAfterDelay(entry));
            }
            else
            {
                Debug.LogWarning("NavMeshEntry has a null npc reference.");
            }
        }
    }

    private System.Collections.IEnumerator StartMovementAfterDelay(NavMeshEntry entry)
    {
        yield return new WaitForSeconds(entry.delaySeconds);

        NpcMovement movement = entry.npc.GetComponent<NpcMovement>();
        if (movement != null)
        {
            movement.StartMovement();
        }
        else
        {
            Debug.LogWarning($"NpcMovement component not found on {entry.npc.name}");
        }
    }
}
