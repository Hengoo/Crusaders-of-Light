using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementItem : MonoBehaviour {

    public enum EffectType
    {
        BASIC_1 = 0,
        BASIC_2 = 1,
        SPECIAL_1 = 2,
        SPECIAL_2 = 3,
        SPECIAL_3 = 4,
        SPELL_1 = 5
    }

    [Header("Bonus Effects:")]
    public ElementBonusEffect[] BonusEffects = new ElementBonusEffect[6];   // Needs to be exactly as big as there are effectTypes!

    [Header("Weapon Particle Effects:")]
    public GameObject ParticleEffectPrefab;

    public ElementBonusEffect GetBonusEffectOfType(EffectType EType)
    {
        return BonusEffects[(int)EType];
    }

    public GameObject GetParticleEffectPrefab()
    {
        return ParticleEffectPrefab;
    }
}
