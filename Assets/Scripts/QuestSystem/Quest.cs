using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Quest : ScriptableObject
{
    public void OnQuestStarted()
    {
        QuestStartedAction();
    }

    public void OnQuestCompleted()
    {
        QuestCompletedAction();
    }
    
    protected abstract void QuestStartedAction();
    protected abstract void QuestCompletedAction();
}