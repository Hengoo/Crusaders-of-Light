using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item {

    [Header("Weapon:")]
    public bool TwoHanded = false;

    public override void EquipItem(Character CharacterToEquipTo, int SlotID)
    {
        // TODO : Graphical Atachment!

        bool SuccesfullyEquipped = CharacterToEquipTo.EquipWeapon(this, TwoHanded, SlotID);

        if (SuccesfullyEquipped)
        {
            CurrentOwner = CharacterToEquipTo;
        }
    }

    public override void UnEquipItem()
    {
        CurrentOwner = null;

        // TODO : Graphical Detachment!
    }

    public bool IsTwoHanded()
    {
        return TwoHanded;
    }
}
