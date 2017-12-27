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

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime < ActivationTime)
        {
            return;
        }

        if (!SourceItemSkill.GetEffectOnlyOnceBool())
        {
            SourceItemSkill.SetEffectOnlyOnceBool(true);
            
            for (int i = 0; i < EffectsStart.Length; i++)
            {
                EffectsStart[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            }
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

            // Stop Skill Activation:
            SourceItemSkill.FinishedSkillActivation();
        }
    }
}
