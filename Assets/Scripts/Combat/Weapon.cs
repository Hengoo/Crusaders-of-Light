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
            CurrentOwner = CharacterToEquipTo;
        }
    }

    public override void UnEquipItem()
    {
        CurrentOwner = null;
    }

    public bool IsTwoHanded()
    {
        return TwoHanded;
    }
}
