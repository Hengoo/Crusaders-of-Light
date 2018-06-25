using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipPoint : MonoBehaviour {

    public Item item;
    public ElementItem elementItem;

    public void TriggerEquip(int PlayerID)
    {
        if (GameController.Instance)
        {
            if (item)
            {
                GameController.Instance.SetPlayerItem(PlayerID, item);

            }

            if (elementItem)
            {
                GameController.Instance.SetPlayerElement(PlayerID, elementItem);
            }
        }
    }
}
