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
        var go = new GameObject("ReachPlaceTrigger");
        var collider = go.AddComponent<SphereCollider>();
        var trigger = go.AddComponent<QuestReachPlaceTrigger>();
        go.transform.parent = _place.transform;
        go.transform.localPosition = Vector3.zero;
        go.tag = "Spawner";
        go.layer = 10;

        collider.radius = _radius;
        collider.isTrigger = true;

        trigger.AddTriggerAction(() =>
        {
            OnQuestCompleted();
            Object.Destroy(go);
        });
    }

    protected override void QuestCompleted()
    {
        //TODO: ??
    }
}
