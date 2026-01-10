using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static QuestEventStore;

[Serializable]
public class QuestEventEntry
{
    public QuestEvent questEvent;
    public bool finished;
}

[Serializable]
public class QuestEvents
{
    public List<QuestEvent> events = new();
}

public enum QuestEvent
{
    PickedUpScooter
}

public class QuestEventStore : MonoBehaviour
{
    public static QuestEventStore Instance { get; private set; }

    [SerializeField]
    private List<QuestEventEntry> availableQuestEvents = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool ProvideQuestEvent(QuestEvent forThisQuestEvent)
    {
        var questEvent = availableQuestEvents.FirstOrDefault(f => f.questEvent == forThisQuestEvent);

        return questEvent != null && questEvent.finished;
    }

    public void SetQuestEvent(QuestEvent forThisQuestEvent, bool triggered)
    {
        var questEvent = availableQuestEvents.FirstOrDefault(f => f.questEvent == forThisQuestEvent);

        if (questEvent != null)
        {
            questEvent.finished = triggered;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        EnsureAllQuestEventsExist();
    }
#endif

    private void EnsureAllQuestEventsExist()
    {
        var seen = new HashSet<QuestEvent>();

        for (int i = availableQuestEvents.Count - 1; i >= 0; i--)
        {
            if (seen.Contains(availableQuestEvents[i].questEvent))
            {
                availableQuestEvents.RemoveAt(i);
            }
            else
            {
                seen.Add(availableQuestEvents[i].questEvent);
            }
        }

        foreach (QuestEvent questEvent in Enum.GetValues(typeof(QuestEvent)))
        {
            if (!seen.Contains(questEvent))
            {
                availableQuestEvents.Add(new QuestEventEntry { questEvent = questEvent, finished = false });
            }
        }
    }

    public bool FinishedAllQuestEvents(List<QuestEvent> checkTheseEvents)
    {
        foreach (var questEvent in checkTheseEvents)
        {
            var toCheck = availableQuestEvents.FirstOrDefault(e => e.questEvent == questEvent);

            if (toCheck == null || !toCheck.finished)
            {
                return false;
            }
        }

        return true;
    }
}
