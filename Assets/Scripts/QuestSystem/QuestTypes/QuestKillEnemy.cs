using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class QuestKillEnemy : QuestBase
{
    private readonly Transform _spawnPoint;
    private readonly GameObject[] _enemyPrefabs;

    private int _killsRemaining;

    public QuestKillEnemy(Transform spawnPoint, GameObject[] enemyPrefabs, string title, string description) : base(title, description)
    {
        _spawnPoint = spawnPoint;
        _enemyPrefabs = enemyPrefabs;
        _killsRemaining = enemyPrefabs.Length;
    }

    protected override void QuestStarted()
    {
        var circleAngle = 360f / _enemyPrefabs.Length;
        for (var i = 0; i < _enemyPrefabs.Length; i++)
        {
            var enemy = Object.Instantiate(_enemyPrefabs[i]);
            enemy.SetActive(true);
            enemy.GetComponent<CharacterEnemy>().SpawnAndEquipStartingWeapons();
            enemy.GetComponent<CharacterEnemy>().SubscribeDeathAction(CheckCompletion);

            var circlePosition = Quaternion.AngleAxis(circleAngle * i, Vector3.up) * Vector3.forward * 2;
            enemy.transform.position = _spawnPoint.position + circlePosition + Vector3.up * 2;
        }
    }

    protected override void QuestCompleted()
    {
        //TODO: give rewards?
    }

    private void CheckCompletion()
    {
        _killsRemaining--;
        if(_killsRemaining <= 0)
            OnQuestCompleted();
    }
}
