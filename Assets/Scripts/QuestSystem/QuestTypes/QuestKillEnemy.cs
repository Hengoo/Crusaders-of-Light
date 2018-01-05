using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KillEnemyQuest", menuName = "Quests/Kill enemy")]
public class QuestKillEnemy : QuestBase
{
    public GameObject SpawnPoint;
    public CharacterEnemy Enemy;
    
    protected override void QuestStarted()
    {
        //TODO: spawn enemy
    }

    protected override void QuestCompleted()
    {
        //TODO: give rewards?
    }
}
