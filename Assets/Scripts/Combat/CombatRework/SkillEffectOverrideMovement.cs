using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_override_movement", menuName = "Combat/SkillEffects/OverrideMovement", order = 40)]
public class SkillEffectOverrideMovement : SkillEffect {

    [Header("Override Movement:")]
    public bool OverrideMovement = false;
    public bool OverrideMovementRelative = true;
    public Vector3 OverrideMovementVec = Vector3.zero;

    [Header("Override Rotation:")]
    public bool OverrideRotation = false;
    public bool OverrideRotationRelative = true;
    public Vector3 OverrideRotationVec = Vector3.zero;



    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        if (OverrideRotation)
        {
            Vector3 NewRotation = OverrideRotationVec;

            if (OverrideRotationRelative)
            {
                NewRotation = Owner.transform.rotation * NewRotation;
            }

            Owner.SwitchOverrideRotation(OverrideRotation, NewRotation);
        }
        else
        {
            Owner.SwitchOverrideRotation(OverrideRotation, Vector3.zero);
        }

        if (OverrideMovement)
        {
            Vector3 NewMovement = OverrideMovementVec;

            if (OverrideMovementRelative)
            {
                NewMovement = Owner.transform.rotation * NewMovement;
            }

            Owner.SwitchOverrideMovement(OverrideMovement, NewMovement);
        }
        else
        {
            Owner.SwitchOverrideMovement(OverrideMovement, Vector3.zero);
        }
    }
}
