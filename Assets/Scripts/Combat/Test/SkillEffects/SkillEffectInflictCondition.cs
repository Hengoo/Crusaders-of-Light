using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_inflict_condition", menuName = "Combat/SkillEffects/InflictCondition", order = 3)]
public class SkillEffectInflictCondition : SkillEffect {

    [Header("Skill Effect Inflict Condition:")]
    public Condition ConditionToApply;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        Target.ApplyNewCondition(ConditionToApply, Owner, SourceItemSkill);
    }
}
