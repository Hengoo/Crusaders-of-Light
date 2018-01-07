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
        QuestCompleted();
        QuestEndAction.Invoke();
    }
    
    protected abstract void QuestStarted();
    protected abstract void QuestCompleted();
}