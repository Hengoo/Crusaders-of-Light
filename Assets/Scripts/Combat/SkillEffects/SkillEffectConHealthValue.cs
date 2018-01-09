using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_conditional_health_value", menuName = "Combat/SkillEffectsConditionals/HealthValue", order = 1)]
public class SkillEffectConHealthValue : SkillEffect {

    public enum EffectTrigger
    {
        HEALTH_BELOW_PERCENTAGE = 0,
        HEALTH_ABOVE_PERCENTAGE = 1,
    }

    [Header("Skill Effect Conditional Health Value:")]
    public float HealthPercentage = 0;
    public EffectTrigger TriggerEffectsWhen = EffectTrigger.HEALTH_BELOW_PERCENTAGE;

    public SkillEffect[] SkillEffectsIfTrue = new SkillEffect[0];
    public SkillEffect[] SkillEffectsIfFalse = new SkillEffect[0];

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        if ((TriggerEffectsWhen == EffectTrigger.HEALTH_ABOVE_PERCENTAGE && Target.GetHealthCurrentPercentage() >= HealthPercentage)
            || (TriggerEffectsWhen == EffectTrigger.HEALTH_BELOW_PERCENTAGE && Target.GetHealthCurrentPercentage() <= HealthPercentage))
        {
            for (int i = 0; i < SkillEffectsIfTrue.Length; i++)
            {
                SkillEffectsIfTrue[i].ApplyEffect(Owner, SourceItemSkill, Target);
            }
        }
        else
        {
            for (int i = 0; i < SkillEffectsIfFalse.Length; i++)
            {
                SkillEffectsIfFalse[i].ApplyEffect(Owner, SourceItemSkill, Target);
            }
        }
    }


}
