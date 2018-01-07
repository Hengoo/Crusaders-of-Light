using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestPickupItem : QuestBase
{
    private readonly Item _item;

    public QuestPickupItem(Item item)
    {
        _item = item;
    }

    protected override void QuestStarted()
    {
        _item.SubscribeItemPickup(OnQuestCompleted);

        //TODO: mechanics to warn if the item is dropped
    }

    protected override void QuestCompleted()
    {
        
    }
}
