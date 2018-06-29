using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item {

    [Header("Weapon:")]
    public bool TwoHanded = false;

    [Header("Weapon ID:")]
    public int WeaponID = -1;

    [Header("Element Efects on Weapon:")]
    public bool SpawnElementEffect = false;
    public GameObject ElementEffectSpawnPoint;
    private GameObject ElementEffectInstance;

    public override void EquipItem(Character CharacterToEquipTo, int SlotID)
    {
        bool SuccesfullyEquipped = CharacterToEquipTo.EquipWeapon(this, TwoHanded, SlotID);

        if (SuccesfullyEquipped)
        {
            SetCurrentEquipSlot(SlotID);
            CurrentOwner = CharacterToEquipTo;
            EquippedSlotID = SlotID;
            ElementEffectSpawnOnEquip();
        }

        DestroyAllHitObjects();

        for (int i = 0; i < ItemSkills.Length; i++)
        {
            ItemSkills[i].SetCurrentCooldown(0.25f);
        }
    }

    public override void UnEquipItem()
    {
        ElementEffectDestroyOnUnEquip();
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

    public void ElementEffectSpawnOnEquip()
    {
        ElementItem EquippedElement = CurrentOwner.GetEquippedElement();

        if (!EquippedElement)
        {
            return;
        }

        ElementEffectDestroyInstance();

        ElementEffectInstance = Instantiate(EquippedElement.GetParticleEffectPrefab(), ElementEffectSpawnPoint.transform);

        if (EquippedElement.GetSpecialSkill())
        {
            EquipSpecialSkill(EquippedElement.GetSpecialSkill());
        }
    }

    public void ElementEffectDestroyOnUnEquip()
    {
        UnEquipSpecialSkill();
        ElementEffectDestroyInstance();
    }

    private void ElementEffectDestroyInstance()
    {
        if (ElementEffectInstance)
        {
            Destroy(ElementEffectInstance.gameObject);
            ElementEffectInstance = null;
        }
    }

    public int GetWeaponID()
    {
        return WeaponID;
    }
}
