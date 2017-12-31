using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_push", menuName = "Combat/SkillEffects/Push", order = 7)]
public class SkillEffectPush : SkillEffect {

    public enum Direction
    {
        NONE = -1,
        OWNER_DIRECTION = 0
    }

    [Header("Skill Effect Push:")]
    public Direction PushDirection = Direction.NONE;
    public float ForceMagnitude = 1.0f;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        Vector3 ForceDirection = Vector3.zero;

        if (PushDirection == Direction.OWNER_DIRECTION)
        {
            ForceDirection = Owner.transform.rotation * Vector3.forward;
        }

        Target.GetComponent<Rigidbody>().AddForce(ForceDirection * ForceMagnitude, ForceMode.Impulse);
    }

}
