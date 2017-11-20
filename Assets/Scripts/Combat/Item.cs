using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    [Header("Item:")]
    public Character CurrentOwner;

    [Header("Item Skills:")]
    public ItemSkill[] ItemSkills = new ItemSkill[1];

    public virtual void EquipItem(Character CharacterToEquipTo, int SlotID)
    {

    }

    public virtual void UnEquipItem()
    {

    }

    public ItemSkill[] GetItemSkills()
    {
        return ItemSkills;
    }
}
