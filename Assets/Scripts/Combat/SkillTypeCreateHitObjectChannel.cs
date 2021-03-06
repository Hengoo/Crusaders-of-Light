﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_channel_create_hit_object", menuName = "Combat/SkillArchetypes/SkillChannelCreateHitObject", order = 5)]
public class SkillTypeCreateHitObjectChannel : SkillType {

    [Header("Skill Channel Create Hit Object:")]
    public float ActivationTimeMax = -1;

    public SkillEffect[] EffectsStart;
    public SkillEffect[] EffectsEnd;

    public SkillHitObject[] AtStartHitObjectPrefabs;
    public SkillHitObject[] IntervallHitObjectPrefabs;
    public SkillHitObject[] AtEndHitObjectPrefabs;

    public bool UseSkillLevelAtActivationMoment = true;

    [Header("Skill Channel Create Hit Object Animation: (Only set to something else if fully intended)")]
    public string IdleAnimation = "Channel_Idle";
    public string ReleaseAnimation = "Channel_Released";

    public AudioClip ChanellingLoop;
    public AudioClip ChanellingRelease;

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime < ActivationTime)
        {
            return;
        }

        if (!SourceItemSkill.GetEffectOnlyOnceBool(0))
        {
            SourceItemSkill.SetEffectOnlyOnceBool(0, true);

            SourceItemSkill.StopAllCoroutines();
            if (ChanellingLoop)
            {
                var audioSource = SourceItemSkill.GetComponent<AudioSource>();
                audioSource.clip = ChanellingLoop;
                audioSource.loop = true;
                audioSource.Play();
            }

            SkillHitObject SpawnedHitObjectStart;

            for (int i = 0; i < EffectsStart.Length; i++)
            {
                EffectsStart[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            }

            for (int i = 0; i < AtStartHitObjectPrefabs.Length; i++)
            {
                // Spawn and Initialize Projectile:
                SpawnedHitObjectStart = Instantiate(AtStartHitObjectPrefabs[i], SourceItemSkill.transform.position, SourceItemSkill.GetCurrentOwner().transform.rotation);
                SpawnedHitObjectStart.InitializeHitObject(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, this, UseSkillLevelAtActivationMoment);
            }
            
            SourceItemSkill.GetCurrentOwner().StartAnimation(IdleAnimation, 1, SourceItemSkill.GetParentItemEquipmentSlot());
        }

        if (ActivationIntervallReached)
        {
            ApplyEffects(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());

            SkillHitObject SpawnedHitObjectIntervall;

            for (int i = 0; i < IntervallHitObjectPrefabs.Length; i++)
            {
                // Spawn and Initialize Projectile:
                SpawnedHitObjectIntervall = Instantiate(IntervallHitObjectPrefabs[i], SourceItemSkill.transform.position, SourceItemSkill.GetCurrentOwner().transform.rotation);
                SpawnedHitObjectIntervall.InitializeHitObject(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, this, UseSkillLevelAtActivationMoment);
            }
        }

        if (!StillActivating || (ActivationTimeMax > 0 && CurrentActivationTime >= ActivationTimeMax))
        {
            for (int i = 0; i < EffectsEnd.Length; i++)
            {
                EffectsEnd[i].ApplyEffect(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            }


            var audioSource = SourceItemSkill.GetComponent<AudioSource>();
            SourceItemSkill.StopAllCoroutines();
            if (ChanellingRelease)
            {
                audioSource.clip = ChanellingRelease;
                audioSource.loop = false;
                audioSource.Play();
            }
            else
            {
                SourceItemSkill.StartCoroutine(FadeSoundOutAndStop(audioSource, .3f));
            }

            SkillHitObject SpawnedHitObjectEnd;

            for (int i = 0; i < AtEndHitObjectPrefabs.Length; i++)
            {
                // Spawn and Initialize Projectile:
                SpawnedHitObjectEnd = Instantiate(AtEndHitObjectPrefabs[i], SourceItemSkill.transform.position, SourceItemSkill.GetCurrentOwner().transform.rotation);
                SpawnedHitObjectEnd.InitializeHitObject(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, this, UseSkillLevelAtActivationMoment);
            }

            SourceItemSkill.GetCurrentOwner().StartAnimation(ReleaseAnimation, 1, SourceItemSkill.GetParentItemEquipmentSlot());

            // Stop Skill Activation:
            if (Cooldown > 0)
            {
                SourceItemSkill.SetCurrentCooldown(Cooldown);
            }
            RemoveActivationMovementRateModifier(SourceItemSkill, SourceItemSkill.GetCurrentOwner());
            SourceItemSkill.StoppedActivatingSkillWithHitObjects(this);
            SourceItemSkill.FinishedSkillActivation();
        }
    }

    private IEnumerator FadeSoundIn(AudioSource source, float seconds)
    {
        source.volume = 0;
        var stepPerSecond = 1.0f / seconds;
        while (source.volume < 1.0f)
        {
            yield return new WaitForEndOfFrame();
            source.volume += stepPerSecond * Time.deltaTime;
        }
        source.volume = 1;
    }

    private IEnumerator FadeSoundOutAndStop(AudioSource source, float seconds)
    {
        var stepPerSecond = 1.0f / seconds;
        while (source.volume > 0)
        {
            yield return new WaitForEndOfFrame();
            source.volume -= stepPerSecond * Time.deltaTime;
        }
        source.volume = 0;
        source.Stop();
    }
}

