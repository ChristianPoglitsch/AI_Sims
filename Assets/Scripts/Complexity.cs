using AiSims;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class NavMeshEntry
{
    public GameObject npc; // The GameObject to control
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
                if (movement != null)
                {
                    movement.StartMovement();
                }
                else
                {
                    Debug.LogWarning($"NpcMovement component not found on {entry.npc.name}");
                }
            }
            else
            {
                Debug.LogWarning("NavMeshEntry has a null npc reference.");
            }
        }
    }
}
