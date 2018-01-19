using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item {

    [Header("Weapon:")]
    public bool TwoHanded = false;

    public override void EquipItem(Character CharacterToEquipTo, int SlotID)
    {
        bool SuccesfullyEquipped = CharacterToEquipTo.EquipWeapon(this, TwoHanded, SlotID);

        if (SuccesfullyEquipped)
        {
            SetCurrentEquipSlot(SlotID);
            CurrentOwner = CharacterToEquipTo;
            EquippedSlotID = SlotID;
        }

        DestroyAllHitObjects();

        for (int i = 0; i < ItemSkills.Length; i++)
        {
            ItemSkills[i].SetCurrentCooldown(0.25f);
        }
    }

    public override void UnEquipItem()
    {
        CurrentEquipSlot = -1;
        CurrentOwner = null;
        EquippedSlotID = -1;
        GetComponent<AudioSource>().Stop();
        DestroyAllHitObjects();
    }

    public bool IsTwoHanded()
    {
        return TwoHanded;
    }

    public int GetPowerLevel()
    {
        return 0;
    }
}
