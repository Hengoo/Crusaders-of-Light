using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestReachPlace : QuestBase
{
    private readonly float _radius; //Radius of "reached place" trigger
    private readonly GameObject _place; //Place to reach

    public QuestReachPlace(GameObject place, float radius, string title, string description) : base(title, description)
    {
        _place = place;
        _radius = radius;
    }

    protected override void QuestStarted()
    {
        var collider = _place.AddComponent<SphereCollider>();
        var trigger = _place.AddComponent<QuestReachPlaceTrigger>();

        collider.radius = _radius;
        collider.isTrigger = true;

        trigger.AddTriggerAction(OnQuestCompleted);
    }

    protected override void QuestCompleted()
    {
        //TODO: ??
    }
}
