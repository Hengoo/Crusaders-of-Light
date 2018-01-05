using UnityEngine;
using UnityEngine.Events;

public abstract class PuzzleBase : ScriptableObject
{
    private UnityAction _onPuzzleCompletedAction;

    public void SubscribeAction(UnityAction action)
    {
        if (action != null)
            _onPuzzleCompletedAction += action;
    }

    public void UnsubscribeAction(UnityAction action)
    {
        if (action != null && _onPuzzleCompletedAction != null)
            _onPuzzleCompletedAction -= action;
    }

    public void OnPuzzleStarted()
    {
        PuzzleStartedAction();
    }

    public void OnPuzzleCompleted()
    {
        PuzzleCompletedAction();
    }

    protected abstract void PuzzleStartedAction();
    protected abstract void PuzzleCompletedAction();
}