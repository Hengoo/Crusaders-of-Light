using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class QuestBase : ScriptableObject
{
    public string Title;
    public string Description;
    public QuestBase[] SubQuests;
    public UnityAction QuestEndAction;

    public void OnQuestStarted()
    {
        QuestStarted();
    }

    public void OnQuestCompleted()
    {
        QuestCompleted();
        QuestEndAction.Invoke();
    }
    
    protected abstract void QuestStarted();
    protected abstract void QuestCompleted();
}