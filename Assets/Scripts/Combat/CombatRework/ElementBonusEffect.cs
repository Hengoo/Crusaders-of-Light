using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "element_bonus_effect", menuName = "Combat/Element/BonusEffect", order = 1)]
public class ElementBonusEffect : ScriptableObject {

    [Header("Element Bonus Effect:")]
    public ElementItem.EffectType EType = ElementItem.EffectType.BASIC_1;

    [Header("Additional Effects:")]
    public SkillEffect[] AdditionalEffectsOnApply = new SkillEffect[0];
    public SkillEffect[] AdditionalEffectsOnActivation = new SkillEffect[0];

    public SkillEffect[] GetAdditionalEffects(bool OnApply)
    {
        if (OnApply)
        {
            return AdditionalEffectsOnApply;
        }
        else
        {
            return AdditionalEffectsOnActivation;
        }
    }
}
