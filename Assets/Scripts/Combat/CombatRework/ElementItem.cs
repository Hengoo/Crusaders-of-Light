using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementItem : MonoBehaviour {

    public enum EffectType
    {
        LIGHT = 0,
        HEAVY = 1,
        MOVEMENT = 2,
        SPECIAL = 3
    }

    [Header("Bonus Effects:")]
    public ElementBonusEffect[] BonusEffects = new ElementBonusEffect[4];   // Needs to be exactly as big as there are effectTypes!

    public ElementBonusEffect GetBonusEffectOfType(EffectType EType)
    {
        return BonusEffects[(int)EType];
    }
}
