using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_damage", menuName = "Combat/SkillEffects/Damage", order = 1)]
public class SkillEffectDamage : SkillEffect
{
    [Header("Skill Effect Damage:")]
    public int DamageValueBase = 0;
    public int DamageValuePerLevel = 0;
    public Character.Resistance DamageType = Character.Resistance.NONE;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        int FinalDamageValue = DamageValueBase;

        FinalDamageValue += DamageValuePerLevel * SourceItemSkill.GetSkillLevel();

        Target.InflictDamage(DamageType, FinalDamageValue);
    }

}
