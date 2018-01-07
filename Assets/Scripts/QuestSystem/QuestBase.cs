using System.Diagnostics;
using UnityEngine.Events;


public abstract class QuestBase
{
    public readonly string Title;
    public readonly string Description;
    public UnityAction QuestEndAction;

    protected QuestBase(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public void OnQuestStarted()
    {
        QuestStarted();
    }

    public void OnQuestCompleted()
    {
        UnityEngine.Debug.Log("QuestCompleted");
        QuestCompleted();
        QuestEndAction.Invoke();
    }
    
    protected abstract void QuestStarted();
    protected abstract void QuestCompleted();
}