using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

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
        if (!_place) return;

        var collider = _place.AddComponent<SphereCollider>();
        var trigger = _place.AddComponent<QuestReachPlaceTrigger>();

        collider.radius = _radius;
        collider.isTrigger = true;

        trigger.AddTriggerAction(() =>
        {
            OnQuestCompleted();
            Object.Destroy(collider);
            Object.Destroy(trigger);
        });
    }

    protected override void QuestCompleted()
    {
        //TODO: ??
    }
}
