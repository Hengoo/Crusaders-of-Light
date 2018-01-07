using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class QuestKillEnemy : QuestBase
{
    private readonly Transform _spawnPoint;
    private readonly GameObject _enemyPrefab;

    public QuestKillEnemy(Transform spawnPoint, GameObject enemyPrefab)
    {
        _spawnPoint = spawnPoint;
        _enemyPrefab = enemyPrefab;
    }

    protected override void QuestStarted()
    {
        var enemy = Object.Instantiate(_enemyPrefab);
        enemy.GetComponent<CharacterEnemy>().SubscribeDeathAction(OnQuestCompleted);
    }

    protected override void QuestCompleted()
    {
        //TODO: give rewards?
    }
}
