using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_projectile", menuName = "Combat/SkillArchetypes/SkillProjectile", order = 1)]
public class SkillTypeProjectile : SkillType
{

    [Header("Skill Projectile:")]
    public SkillProjectile ProjectilePrefab;

    public AudioClip ThrowSound;

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime < ActivationTime)
        {
            return;
        }

        // Spawn and Initialize Projectile:
        SkillProjectile SpawnedProjectile = Instantiate(ProjectilePrefab, SourceItemSkill.transform.position, SourceItemSkill.GetCurrentOwner().transform.rotation);
        SpawnedProjectile.InitializeProjectile(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, this);

        if (ThrowSound)
        {
            var audioSource = SourceItemSkill.GetComponent<AudioSource>();
            audioSource.clip = ThrowSound;
            audioSource.volume = 1;
            audioSource.loop = false;
            audioSource.Play();
        }

        // Stop Skill Activation:
        if (Cooldown > 0)
        {
            SourceItemSkill.SetCurrentCooldown(Cooldown);
        }
        RemoveActivationMovementRateModifier(SourceItemSkill, SourceItemSkill.GetCurrentOwner());
        SourceItemSkill.FinishedSkillActivation();
    }
}
