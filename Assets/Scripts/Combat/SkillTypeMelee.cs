using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_melee", menuName = "Combat/SkillArchetypes/SkillMelee", order = 2)]
public class SkillTypeMelee : SkillType {

    [Header("Skill Melee:")]
    public float HitTimeStart = 0.0f;
    public float HitTimeStop = 0.0f;

    public bool HitEachCharacterOnlyOnce = true;

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating)
    {
        if (CurrentActivationTime >= HitTimeStart && CurrentActivationTime <= HitTimeStop)
        {
            if (!SourceItemSkill.CheckIfSkillIsUsingHitBox(SourceItemSkill))
            {
                SourceItemSkill.StartSkillCurrentlyUsingItemHitBox(HitEachCharacterOnlyOnce);
            }
        }
        else if (SourceItemSkill.CheckIfSkillIsUsingHitBox(SourceItemSkill))
        {
            SourceItemSkill.EndSkillCurrentlyUsingItemHitBox();
        }

        if (CurrentActivationTime >= ActivationTime)
        {
            // Stop Skill Activation:
            SourceItemSkill.FinishedSkillActivation();
        }
    }
}
