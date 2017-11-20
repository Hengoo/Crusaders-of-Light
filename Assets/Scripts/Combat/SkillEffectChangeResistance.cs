using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_change_resistance", menuName = "Combat/SkillEffects/ChangeResistance", order = 4)]
public class SkillEffectChangeResistance : SkillEffect {
    [Header("Skill Effect Change Resistance:")]
    public float ResistanceValueBase = 0;
    public float ResistanceValuePerLevel = 0;
    public Character.Resistance ResistanceType = Character.Resistance.NONE;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        float FinalResistanceValue = ResistanceValueBase;

        FinalResistanceValue += ResistanceValuePerLevel * SourceItemSkill.GetSkillLevel();

        Target.ChangeResistance(ResistanceType, FinalResistanceValue);
    }


}
