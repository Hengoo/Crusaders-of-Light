using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_change_movement_mod", menuName = "Combat/SkillEffects/ChangeMovementMod", order = 9)]
public class SkillEffectChangeMovementModifier : SkillEffect {

    [Header("Skill Effect Change Movement Mod:")]
    public float MoveValueBase = 0;
    public float MoveValuePerLevel = 0;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        float FinalMoveValue = MoveValueBase;

        FinalMoveValue += MoveValuePerLevel * SourceItemSkill.GetSkillLevel();

        Target.ChangeMovementRateModifier(FinalMoveValue);
    }

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        float FinalMoveValue = MoveValueBase;

        FinalMoveValue += MoveValuePerLevel * FixedLevel;

        Target.ChangeMovementRateModifier(FinalMoveValue);
    }
}
