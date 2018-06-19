using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_melee", menuName = "Combat/SkillArchetypes/SkillMelee", order = 2)]
public class SkillTypeMelee : SkillType {

    [Header("Skill Melee:")]
    public float HitTimeStart = 0.0f;
    public float HitTimeStop = 0.0f;

    public bool HitEachCharacterOnlyOnce = true;

    public int MaxHittableCharacters = -1;

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime >= HitTimeStart && CurrentActivationTime <= HitTimeStop)
        {
            if (!SourceItemSkill.CheckIfSkillIsUsingHitBox(SourceItemSkill))
            {
                SourceItemSkill.StartSkillCurrentlyUsingItemHitBox(HitEachCharacterOnlyOnce, MaxHittableCharacters);
            }
        }
        else if (SourceItemSkill.CheckIfSkillIsUsingHitBox(SourceItemSkill))
        {
            SourceItemSkill.EndSkillCurrentlyUsingItemHitBox();
        }

        if (CurrentActivationTime >= ActivationTime)
        {
            // Stop Skill Activation:
            if (Cooldown > 0)
            {
                SourceItemSkill.SetCurrentCooldown(Cooldown);
            }
            RemoveActivationMovementRateModifier(SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            SourceItemSkill.StoppedActivatingSkillWithHitObjects(this);
            SourceItemSkill.FinishedSkillActivation();
        }
    }
}
