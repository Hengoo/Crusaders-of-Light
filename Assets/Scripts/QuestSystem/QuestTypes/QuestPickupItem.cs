using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestPickupItem : QuestBase
{
    private Item _item;

    public QuestPickupItem(Item item)
    {
        _item = item;
    }

    protected override void QuestStarted()
    {
        
    }

    protected override void QuestCompleted()
    {
        
    }
}
