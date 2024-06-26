﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipPoint : MonoBehaviour {

    public Weapon weapon;
    public ElementItem elementItem;

    public GameObject VisualRepresentation;

    private void Start()
    {
        if (GameController.Instance && VisualRepresentation)
        {
            if ((weapon && GameController.Instance.CheckIfWeaponUnlocked(weapon))
                || (elementItem && GameController.Instance.CheckIfElementUnlocked(elementItem)))
                
            {
                VisualRepresentation.SetActive(true);
            }
            else
            {
                VisualRepresentation.SetActive(false);
            }
        }
    }

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
