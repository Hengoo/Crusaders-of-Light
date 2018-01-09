using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_push", menuName = "Combat/SkillEffects/Push", order = 7)]
public class SkillEffectPush : SkillEffect {

    public enum Direction
    {
        NONE = -1,
        OWNER_DIRECTION = 0,
        FROM_OWNER_TO_TARGET = 1,
    }

    [Header("Skill Effect Push:")]
    public Direction PushDirection = Direction.NONE;
    
    public float ForceMagnitude = 1.0f;

    [Header("Skill Effect Push Value Modifier:")]
    public SkillEffectValueModifier[] ForceValueModifiers = new SkillEffectValueModifier[0];

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        Vector3 ForceDirection = Vector3.zero;

        if (PushDirection == Direction.OWNER_DIRECTION)
        {
            ForceDirection = Owner.transform.rotation * Vector3.forward;
        }
        else if (PushDirection == Direction.FROM_OWNER_TO_TARGET)
        {
            ForceDirection = (Target.transform.position - Owner.transform.position).normalized;
        }

        float FinalMagnitude =  ForceMagnitude;

        for (int i = 0; i < ForceValueModifiers.Length; i++)
        {
            FinalMagnitude = ForceValueModifiers[i].ModifyValue(FinalMagnitude, Owner, SourceItemSkill, Target);
        }

        Target.GetComponent<Rigidbody>().AddForce(ForceDirection * FinalMagnitude, ForceMode.Impulse);
    }

}
