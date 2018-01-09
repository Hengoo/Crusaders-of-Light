﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class QuestPickupItem : QuestBase
{
    private readonly Item _item;
    private readonly Vector3 _spawnPosition = Vector3.zero;
    private bool _isAchieved = false;

    public QuestPickupItem(Item item, string title, string description) : base(title, description)
    {
        _item = item;
    }

    public QuestPickupItem(Item item, string title, string description, Vector3 spawnPosition) : base(title, description)
    {
        _item = item;
        _spawnPosition = spawnPosition;
    }

    protected override void QuestStarted()
    {
        foreach (var player in LevelController.Instance.Players)
        {
            var player1 = player;
            player.SubscribeItemPickupAction(() =>
            {
                foreach (var weapon in player1.WeaponSlots)
                {
                    if(!weapon) continue;
                    
                    //Avoid all the clone sufixes
                    if (this.CheckIfSameItem(weapon.name) && !_isAchieved) 
                    {
                        _isAchieved = true;
                        foreach (var others in LevelController.Instance.Players)
                            others.ClearItemPickupActions();
                        OnQuestCompleted();
                    }
                }
            });
        }
    }

    public bool CheckIfSameItem(string otherName)
    {
        return otherName.Contains(_item.name);
    }

    protected override void QuestCompleted()
    {
        //TODO: something else?
    }
}