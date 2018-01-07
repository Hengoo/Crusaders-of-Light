using UnityEngine;
using UnityEngine.Events;

public class QuestReachPlaceTrigger : MonoBehaviour
{
    public UnityAction TriggerEnterAction;

    public void AddTriggerAction(UnityAction action)
    {
        TriggerEnterAction += action;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.name.ToLower().Contains("player")) return;

        if (TriggerEnterAction != null)
        {
            TriggerEnterAction.Invoke();
            TriggerEnterAction = null;
        }
        else
            Debug.LogWarning("No action added to trigger at GameObject: " + gameObject.name);
    }
}
