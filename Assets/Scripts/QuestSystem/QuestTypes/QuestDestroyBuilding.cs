using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDestroyBuilding : QuestBase
{

    private CharacterEnemy _building;

    public QuestDestroyBuilding(CharacterEnemy building, string title, string description) : base(title, description)
    {
        _building = building;
    }

    protected override void QuestStarted()
    {
        _building.SubscribeDeathAction(OnQuestCompleted);
    }

    protected override void QuestCompleted()
    {
        throw new System.NotImplementedException();
    }
}
