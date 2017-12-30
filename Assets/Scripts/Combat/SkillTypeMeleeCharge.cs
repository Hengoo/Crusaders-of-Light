using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_melee_charge", menuName = "Combat/SkillArchetypes/SkillMeleeCharge", order = 4)]
public class SkillTypeMeleeCharge : SkillType {

    [Header("Skill Charge Up Melee:")]
    public float ChargeUpTimeMax = -1; // If > 0 : Hard limit, after that time Skill automatically activates as if releasing the button.
    public float EffectiveChargeUpTimeMax = 1; // Effective Limit: After this time further charging has no effect. This is not calculated, it has to be set manually.
    public float AfterReleaseActivationTime = 1f;

    public bool HitEachCharacterOnlyOnce = true;

    [Header("Skill Charge Up Melee Animation: (Only set to something else if fully intended)")]
    public string ReleaseAnimation = "Charge_Released";
 
    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime < ActivationTime)
        {
            return;
        }

        if (!StillActivating || (ChargeUpTimeMax > 0 && CurrentActivationTime >= ChargeUpTimeMax))
        {
            if (!SourceItemSkill.GetEffectOnlyOnceBool())
            {
                SourceItemSkill.SetEffectFloat(CurrentActivationTime + AfterReleaseActivationTime);
                SourceItemSkill.GetCurrentOwner().StartAnimation(ReleaseAnimation, AfterReleaseActivationTime, SourceItemSkill.GetParentItemEquipmentSlot());
                SourceItemSkill.StartSkillCurrentlyUsingItemHitBox(HitEachCharacterOnlyOnce);
            }

            SourceItemSkill.SetEffectOnlyOnceBool(true);
        }

        if (SourceItemSkill.GetEffectOnlyOnceBool() && CurrentActivationTime >= SourceItemSkill.GetEffectFloat())
        {
            // Stop Skill Activation:
            SourceItemSkill.EndSkillCurrentlyUsingItemHitBox();
            SourceItemSkill.FinishedSkillActivation();
        }


        /*
        if (!SourceItemSkill.GetEffectOnlyOnceBool())
        {
            SourceItemSkill.SetEffectOnlyOnceBool(true);

            for (int i = 0; i < EffectsStart.Length; i++)
            {
                EffectsStart[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            }
        }
        // Maybe not:
        if (ActivationIntervallReached)
        {
            ApplyEffects(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
        }

        if (!StillActivating || (ActivationTimeMax > 0 && CurrentActivationTime >= ActivationTimeMax))
        {
            for (int i = 0; i < EffectsEnd.Length; i++)
            {
                EffectsEnd[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            }

            // Stop Skill Activation:
            SourceItemSkill.FinishedSkillActivation();
    SourceItemSkill.EndSkillCurrentlyUsingItemHitBox();
        }*/
    }

    public override float GetOverwriteAnimationSpeedScaling()
    {
        return EffectiveChargeUpTimeMax;
    }
}
