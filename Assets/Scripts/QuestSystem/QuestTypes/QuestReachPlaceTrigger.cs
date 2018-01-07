using UnityEngine;
using UnityEngine.Events;

public class QuestReachPlaceTrigger : MonoBehaviour
{
    public UnityAction TriggerEnterAction;

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.name.ToLower().Contains("player")) return;

        if(TriggerEnterAction != null)
            TriggerEnterAction.Invoke();
        else
            Debug.LogWarning("No action attached to trigger at GameObject: " + gameObject.name);
    }
}
