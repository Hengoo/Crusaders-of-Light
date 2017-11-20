using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_conditional_specific_condition", menuName = "Combat/SkillEffectsConditionals/SpecificCondition", order = 2)]
public class SkillEffectConSpecificCondition : SkillEffect {

    [Header("Skill Effect Conditional Specific Condition:")]
    public Condition RequiredCondition;
    public SkillEffect[] SkillEffectsIfHasCondition = new SkillEffect[0];
    public SkillEffect[] SkillEffectsIfNoCondition = new SkillEffect[0];

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        if (Target.CheckIfConditionExists(RequiredCondition))
        {
            for (int i = 0; i < SkillEffectsIfHasCondition.Length; i++)
            {
                SkillEffectsIfHasCondition[i].ApplyEffect(Owner, SourceItemSkill, Target);
            }
        }
        else
        {
            for (int i = 0; i < SkillEffectsIfNoCondition.Length; i++)
            {
                SkillEffectsIfNoCondition[i].ApplyEffect(Owner, SourceItemSkill, Target);
            }
        }
    }
}
