using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "element_bonus_effect", menuName = "Combat/Element/BonusEffect", order = 1)]
public class ElementBonusEffect : ScriptableObject {

    [Header("Element Bonus Effect:")]
    public ElementItem.EffectType EType = ElementItem.EffectType.LIGHT;

    [Header("Additional Effects:")]
    public SkillEffect[] AdditionalEffects = new SkillEffect[0];

    public SkillEffect[] GetAdditionalEffects()
    {
        return AdditionalEffects;
    }
}
