using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_remove_condition", menuName = "Combat/SkillEffects/RemoveCondition", order = 4)]
public class SkillEffectRemoveCondition : SkillEffect {

    [Header("Skill Effect Remove Condition:")]
    public Condition ConditionToRemove;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        Target.RemoveCondition(ConditionToRemove);
    }
}
