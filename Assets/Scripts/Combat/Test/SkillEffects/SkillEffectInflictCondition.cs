using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_inflict_condition", menuName = "Combat/SkillEffects/InflictCondition", order = 3)]
public class SkillEffectInflictCondition : SkillEffect {

    [Header("Skill Effect Inflict Condition:")]
    public Condition ConditionToApply;

    public int DurationBase = 0;
    public int DurationPerLevel = 0;

    [Header("Skill Effect Condition Duration Modifier:")]
    public SkillEffectValueModifier[] DurationModifiers = new SkillEffectValueModifier[0];

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        int FinalDuration = DurationBase;

        FinalDuration += DurationPerLevel * SourceItemSkill.GetSkillLevel();

        for (int i = 0; i < DurationModifiers.Length; i++)
        {
            FinalDuration = Mathf.RoundToInt(DurationModifiers[i].ModifyValue(FinalDuration, Owner, SourceItemSkill, Target));
        }

        Target.ApplyNewCondition(ConditionToApply, Owner, SourceItemSkill, FinalDuration);
    }

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        int FinalDuration = DurationBase;

        FinalDuration += DurationPerLevel * FixedLevel;

        for (int i = 0; i < DurationModifiers.Length; i++)
        {
            FinalDuration = Mathf.RoundToInt(DurationModifiers[i].ModifyValue(FinalDuration, Owner, SourceItemSkill, Target));
        }

        Target.ApplyNewCondition(ConditionToApply, Owner, SourceItemSkill, FinalDuration);
    }
}
