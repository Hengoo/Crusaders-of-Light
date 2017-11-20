using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillType : ScriptableObject {

    [Header("Skill Base:")]
    public int Cost;
    public float ActivationTime;
    public float Cooldown = -1; // Negative: No Cooldown

    [Header("Skill Targeting:")]
    public bool AllowTargetFriendly;
    public bool AllowTargetEnemy;

    [Header("Skill Effects:")]
    public SkillEffect[] Effects;


    public bool GetAllowTargetFriendly()
    {
        return AllowTargetFriendly;
    }

    public bool GetAllowTargetEnemy()
    {
        return AllowTargetEnemy;
    }

    public bool StartSkillActivation(ItemSkill SourceWeaponSkill, Character Owner)
    {
        Debug.Log("BREAK 3");
        // Pay Activation Cost:
        if (Owner.GetEnergyCurrent() < Cost)
        {
            Debug.Log(Owner + " can not activate Skill " + this + "! Reason: Not enough Energy!");
            return false;
        }
        Owner.ChangeEnergyCurrent(-1 * Cost);
        Debug.Log("BREAK 4");
        // Skill succesfully activated if this point is reached:

        // Start Cooldown:      (Note: The current Cooldown is saved in the SourceWeapon)
        if (Cooldown > 0)
        {
            SourceWeaponSkill.SetCurrentCooldown(Cooldown);
        }
        Debug.Log("BREAK 5");
        return true;
    }

    public virtual void UpdateSkillActivation(ItemSkill SourceWeaponSkill, float CurrentActivationTime, bool StillActivating)
    {

    }

    public virtual void ApplyEffects(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        for (int i = 0; i < Effects.Length; i++)
        {
            Effects[i].ApplyEffect(Owner, SourceItemSkill, Target);
        }
    }
}
