using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_healing", menuName = "Combat/SkillEffects/Healing", order = 8)]
public class SkillEffectHealing : SkillEffect {

    [Header("Skill Effect Healing:")]
    public int HealingValueBase = 0;
    public int HealingValuePerLevel = 0;
    public float HealingValueBasePercMax = 0;
    public float HealingValuePerLevelPercMax = 0;

    [Header("Skill Effect Healing Value Modifier:")]
    public SkillEffectValueModifier[] HealingValueModifiers = new SkillEffectValueModifier[0];
    public SkillEffectValueModifier[] HealingValuePercMaxModifiers = new SkillEffectValueModifier[0];

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        int FinalHealingValue = HealingValueBase;

        FinalHealingValue += HealingValuePerLevel * SourceItemSkill.GetSkillLevel();

        for (int i = 0; i < HealingValueModifiers.Length; i++)
        {
            FinalHealingValue = Mathf.RoundToInt(HealingValueModifiers[i].ModifyValue(FinalHealingValue, Owner, SourceItemSkill, Target));
        }

        float FinalHealingPercentage = HealingValueBasePercMax;

        FinalHealingPercentage += HealingValuePerLevelPercMax * SourceItemSkill.GetSkillLevel();

        for (int i = 0; i < HealingValuePercMaxModifiers.Length; i++)
        {
            FinalHealingPercentage = Mathf.RoundToInt(HealingValuePercMaxModifiers[i].ModifyValue(FinalHealingPercentage, Owner, SourceItemSkill, Target));
        }

        FinalHealingValue += Target.GetHealthPercentageAbsoluteValue(FinalHealingPercentage);

        Target.Heal(FinalHealingValue);
    }

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        int FinalHealingValue = HealingValueBase;

        FinalHealingValue += HealingValuePerLevel * FixedLevel;

        for (int i = 0; i < HealingValueModifiers.Length; i++)
        {
            FinalHealingValue = Mathf.RoundToInt(HealingValueModifiers[i].ModifyValue(FinalHealingValue, Owner, SourceItemSkill, Target));
        }

        float FinalHealingPercentage = HealingValueBasePercMax;

        FinalHealingPercentage += HealingValuePerLevelPercMax * FixedLevel;

        for (int i = 0; i < HealingValuePercMaxModifiers.Length; i++)
        {
            FinalHealingPercentage = Mathf.RoundToInt(HealingValuePercMaxModifiers[i].ModifyValue(FinalHealingPercentage, Owner, SourceItemSkill, Target));
        }

        FinalHealingValue += Target.GetHealthPercentageAbsoluteValue(FinalHealingPercentage);

        Target.Heal(FinalHealingValue);
    }
}
