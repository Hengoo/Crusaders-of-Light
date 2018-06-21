using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHitObjectForEffect : SkillHitObject {

    [Header("Hit Object for Skill Effects:")]
    public SkillEffect[] HitObjectSkillEffects = new SkillEffect[0];

    [Header("Particle Effects:")]
    public GameObject ParticlesPrefab;
    public GameObject ParticlesSpawnParent;
    private GameObject ParticlesInstance;

    public void InitializeHitObject(Character _Owner, ItemSkill _SourceItemSkill, SkillEffect[] _SkillEffects, bool[] AllowTargets, float[] SkillThreat, bool UseLevelAtActivationMoment)
    {
        // Link Skill User and Skill:
        Owner = _Owner;
        SourceItemSkill = _SourceItemSkill;
        HitObjectSkillEffects = _SkillEffects;
        _audioSource = GetComponent<AudioSource>();


        if (UseLevelAtActivationMoment)
        {
            FixedLevel = SourceItemSkill.GetSkillLevel();
        }
        else
        {
            FixedLevel = -1;
        }

        // Calculate which Team(s) the HitObject can hit:
        int counter = 0;

        if (AllowTargets[0])
        {
            counter += (int)(Owner.GetAlignment());
        }

        if (AllowTargets[1])
        {
            counter += ((int)(Owner.GetAlignment()) % 2) + 1;
        }

        HitObjectAlignment = (Character.TeamAlignment)(counter);

        // Living Time:
        if (MaxTimeAlive <= 0)
        {
            SourceItemSkill.AddEffectSkillHitObject(this);
        }

        if (TickTime > 0)
        {
            CurrentTickTime = TickTime;
        }

        // Parenting:
        if (HitObjectIsChildOfOwner)
        {
            transform.SetParent(Owner.transform);
        }

        if (AlwaysHitOwner)
        {
            HitTarget(Owner);
        }

        // Threat:
        Threat = SkillThreat;

        // Force Impulse:
        if (UseForceImpulse)
        {
            ApplyForceImpulse();
        }

        // Spawn Particle System:
        if (ParticlesPrefab)
        {
            ParticlesInstance = Instantiate(ParticlesPrefab, ParticlesSpawnParent.transform);
        }
    }

    protected override void HitTarget(Character TargetCharacter)
    {
        if (!CanHitSameTargetMultipleTime && AlreadyHitCharacters.Contains(TargetCharacter))
        {
            return;
        }

        if (HitObjectAlignment != TargetCharacter.GetAlignment()
            && HitObjectAlignment != Character.TeamAlignment.ALL)
        {
            return;
        }

        if (!AlreadyHitCharacters.Contains(TargetCharacter))
            AlreadyHitCharacters.Add(TargetCharacter);

        ApplyAllEffects(TargetCharacter);

        if (MaxNumberOfTargets > 0
            && MaxNumberOfTargets >= AlreadyHitCharacters.Count)
        {
            ReachedMaxNumberOfTargets();
        }
    }

    private void ApplyAllEffects(Character TargetCharacter)
    {
        if (FixedLevel >= 0)
        {
            for (int i = 0; i < HitObjectSkillEffects.Length; i++)
            {
                HitObjectSkillEffects[i].ApplyEffect(Owner, SourceItemSkill, TargetCharacter, FixedLevel);
            }

        }
        else
        {
            for (int i = 0; i < HitObjectSkillEffects.Length; i++)
            {
                HitObjectSkillEffects[i].ApplyEffect(Owner, SourceItemSkill, TargetCharacter);
            }
        }
    }

}
