using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestPickupItemTrigger : MonoBehaviour {

    public UnityAction TriggerPickupAction;

    public void AddTriggerAction(UnityAction action)
    {
        TriggerPickupAction += action;
    }
}
