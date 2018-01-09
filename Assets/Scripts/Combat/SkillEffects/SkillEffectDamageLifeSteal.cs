using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_damage_life_steal", menuName = "Combat/SkillEffects/DamageLifeSteal", order = 2)]
public class SkillEffectDamageLifeSteal : SkillEffect {

    [Header("Skill Effect Damage Life Steal:")]
    public int DamageValueBase = 0;
    public int DamageValuePerLevel = 0;
    public Character.Defense DefenseType = Character.Defense.NONE;
    public Character.Resistance DamageType = Character.Resistance.NONE;

    public float LifeStealPercentage = 1;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        int FinalDamageValue = DamageValueBase;

        FinalDamageValue += DamageValuePerLevel * SourceItemSkill.GetSkillLevel();

        int LifeStealValue = Target.InflictDamage(DefenseType, DamageType, FinalDamageValue, 0, 0);

        LifeStealValue = Mathf.RoundToInt(LifeStealValue * LifeStealPercentage);

        Owner.ChangeHealthCurrent(LifeStealValue);
    }
}
