using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_channel_self", menuName = "Combat/SkillArchetypes/SkillChannelSelf", order = 3)]
public class SkillTypeChannelSelf : SkillType
{
    [Header("Skill Channel Self:")]
    public float ActivationTimeMax = -1;

    public SkillEffect[] EffectsStart;
    public SkillEffect[] EffectsEnd;

    [Header("Skill Channel Self Animation: (Only set to something else if fully intended)")]
    public string IdleAnimation = "Channel_Idle";
    public string ReleaseAnimation = "Channel_Released";

    public AudioClip ChanellingSound;

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime < ActivationTime)
        {
            return;
        }

        if (!SourceItemSkill.GetEffectOnlyOnceBool(0))
        {
            SourceItemSkill.SetEffectOnlyOnceBool(0, true);
            
            for (int i = 0; i < EffectsStart.Length; i++)
            {
                EffectsStart[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            }

            SourceItemSkill.GetCurrentOwner().StartAnimation(IdleAnimation, 1, SourceItemSkill.GetParentItemEquipmentSlot());
        }

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

            SourceItemSkill.GetCurrentOwner().StartAnimation(ReleaseAnimation, 1, SourceItemSkill.GetParentItemEquipmentSlot());

            // Stop Skill Activation:
            if (Cooldown > 0)
            {
                SourceItemSkill.SetCurrentCooldown(Cooldown);
            }
            RemoveActivationMovementRateModifier(SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            SourceItemSkill.FinishedSkillActivation();
        }
    }
}
