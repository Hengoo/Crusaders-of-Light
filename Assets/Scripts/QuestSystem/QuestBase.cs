using UnityEngine.Events;


public abstract class QuestBase
{
    public string Title;
    public string Description;
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