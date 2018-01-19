using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_melee_charge", menuName = "Combat/SkillArchetypes/SkillMeleeCharge", order = 4)]
public class SkillTypeMeleeCharge : SkillType
{

    [Header("Skill Charge Up Melee:")]
    public float ChargeUpTimeMax = -1; // If > 0 : Hard limit, after that time Skill automatically activates as if releasing the button.
    public float EffectiveChargeUpTimeMax = 1; // Effective Limit: After this time further charging has no effect. This is not calculated, it has to be set manually.
    public bool HitEachCharacterOnlyOnce = true;

    [Header("Skill Charge Up Melee After Release Time:")]
    public float AfterReleaseActivationTime = 1f;
    public bool ScaleAfterReleaseTimeWithChannelTime = false;
    public float MinTime = 0.0f;
    public float MaxTime = 1.0f;
    public float MinModifier = 1.0f;
    public float MaxModifier = 1.0f;



    [Header("Skill Charge Up Melee Additional Effects:")]
    public SkillEffect[] EffectsSelfOnMinimumActivationTimeEnd = new SkillEffect[0];
    public SkillEffect[] EffectsSelfOnRelease = new SkillEffect[0];
    public SkillEffect[] EffectsSelfOnEnd = new SkillEffect[0];

    [Header("Skill Charge Up Melee Animation: (Only set to something else if fully intended)")]
    public string ReleaseAnimation = "Charge_Released";

    public AudioClip ChargeLoop;
    public AudioClip ChargeRelease;
    

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime < ActivationTime)
        {
            return;
        }

        if (!SourceItemSkill.GetEffectOnlyOnceBool(0))
        {
            SourceItemSkill.SetEffectOnlyOnceBool(0, true);
            for (int i = 0; i < EffectsSelfOnMinimumActivationTimeEnd.Length; i++)
            {
                EffectsSelfOnMinimumActivationTimeEnd[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            }
        }

        if (!StillActivating || (ChargeUpTimeMax > 0 && CurrentActivationTime >= ChargeUpTimeMax))
        {
            if (!SourceItemSkill.GetEffectOnlyOnceBool(1))
            {
                float TimeAfterRelease = AfterReleaseActivationTime;

                if (ScaleAfterReleaseTimeWithChannelTime)
                {
                    float TimePerc = Mathf.Clamp01((CurrentActivationTime - MinTime) / (MaxTime - MinTime));
                    TimeAfterRelease = TimeAfterRelease * Mathf.Lerp(MinModifier, MaxModifier, TimePerc);
                }

                SourceItemSkill.SetEffectFloat(CurrentActivationTime + TimeAfterRelease);
                SourceItemSkill.GetCurrentOwner().StartAnimation(ReleaseAnimation, TimeAfterRelease, SourceItemSkill.GetParentItemEquipmentSlot());
                SourceItemSkill.StartSkillCurrentlyUsingItemHitBox(HitEachCharacterOnlyOnce);

                for (int i = 0; i < EffectsSelfOnRelease.Length; i++)
                {
                    EffectsSelfOnRelease[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
                }

                if (ChargeRelease)
                {
                    var weaponAudioSource = SourceItemSkill.GetComponent<AudioSource>();
                    weaponAudioSource.Stop();
                    weaponAudioSource.clip = ChargeRelease;
                    weaponAudioSource.loop = false;
                    weaponAudioSource.Play();
                }
            }
            SourceItemSkill.SetEffectOnlyOnceBool(1, true);
        }

        if (SourceItemSkill.GetEffectOnlyOnceBool(1) && CurrentActivationTime >= SourceItemSkill.GetEffectFloat())
        {
            for (int i = 0; i < EffectsSelfOnEnd.Length; i++)
            {
                EffectsSelfOnEnd[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            }

            // Stop Skill Activation:
            if (Cooldown > 0)
            {
                SourceItemSkill.SetCurrentCooldown(Cooldown);
            }
            SourceItemSkill.EndSkillCurrentlyUsingItemHitBox();
            RemoveActivationMovementRateModifier(SourceItemSkill, SourceItemSkill.GetCurrentOwner());
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

    public override bool StartSkillActivation(ItemSkill SourceItemSkill, Character Owner)
    {
        var result = base.StartSkillActivation(SourceItemSkill, Owner);
        if (result && ChargeLoop)
        {
            var weaponAudioSource = SourceItemSkill.GetComponent<AudioSource>();
            weaponAudioSource.clip = ChargeLoop;
            weaponAudioSource.loop = true;
            weaponAudioSource.Play();
        }
        return result;
    }
}
