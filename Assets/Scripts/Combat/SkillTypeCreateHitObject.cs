using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_create_hit_object", menuName = "Combat/SkillArchetypes/SkillCreateHitObject", order = 5)]
public class SkillTypeCreateHitObject : SkillType {

    [Header("Skill Create Hit Object:")]
    public SkillHitObject HitObjectPrefab;
    public bool UseSkillLevelAtActivationMoment = true;

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime < ActivationTime)
        {
            return;
        }

        // Spawn and Initialize Projectile:
        SkillHitObject SpawnedHitObject = Instantiate(HitObjectPrefab, SourceItemSkill.transform.position, SourceItemSkill.GetCurrentOwner().transform.rotation);
        SpawnedHitObject.InitializeHitObject(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, this, UseSkillLevelAtActivationMoment);

        // Stop Skill Activation:
        SourceItemSkill.FinishedSkillActivation();
    }
}
