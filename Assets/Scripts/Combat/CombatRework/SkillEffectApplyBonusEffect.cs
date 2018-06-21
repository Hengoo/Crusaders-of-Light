using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_element_bonus_effects", menuName = "Combat/SkillEffects/ElementBonusEffects", order = 50)]
public class SkillEffectApplyBonusEffect : SkillEffect {

    [Header("Bonus Effect Type:")]
    public ElementItem.EffectType BonusEffectType = ElementItem.EffectType.BASIC_1;

    [Header("Effects: OnApply(true) or OnActivation(false):")]
    public bool OnApply = true;
    

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        SkillEffect[] BonusEffects = Owner.GetEquippedElement().GetBonusEffectOfType(BonusEffectType).GetAdditionalEffects(OnApply);        

        for (int i = 0; i < BonusEffects.Length; i++)
        {
            BonusEffects[i].ApplyEffect(Owner, SourceItemSkill, Target);
        }
    }

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target, int FixedLevel)
    {
        SkillEffect[] BonusEffects = Owner.GetEquippedElement().GetBonusEffectOfType(BonusEffectType).GetAdditionalEffects(OnApply);

        for (int i = 0; i < BonusEffects.Length; i++)
        {
            BonusEffects[i].ApplyEffect(Owner, SourceItemSkill, Target, FixedLevel);
        }
    }

}
