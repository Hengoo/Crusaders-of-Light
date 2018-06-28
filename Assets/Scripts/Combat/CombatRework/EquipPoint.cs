using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipPoint : MonoBehaviour {

    public Weapon weapon;
    public ElementItem elementItem;

    public void TriggerEquipToPlayer(CharacterPlayer PlayerToEquipTo)
    {
        if (!GameController.Instance)
        {
            return;
        }

        if (weapon && GameController.Instance.CheckIfWeaponUnlocked(weapon))
        {
            GameController.Instance.SetPlayerItem(PlayerToEquipTo.GetPlayerID(), weapon);
            PlayerToEquipTo.PlayerPickUpWeapon();
        }

        if (elementItem && GameController.Instance.CheckIfElementUnlocked(elementItem))
        {
            GameController.Instance.SetPlayerElement(PlayerToEquipTo.GetPlayerID(), elementItem);
            PlayerToEquipTo.PlayerPickUpElement();
        }
    }
}
