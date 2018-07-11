using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_create_hit_object", menuName = "Combat/SkillArchetypes/SkillCreateHitObject", order = 5)]
public class SkillTypeCreateHitObject : SkillType
{
    [Header("Skill Create Hit Object:")] public SkillHitObject HitObjectPrefab;
    public bool UseSkillLevelAtActivationMoment = true;

    public AudioClip SpawnSound;
    public bool LoopSound;

    public override void UpdateSkillActivation(ItemSkill SourceItemSkill, float CurrentActivationTime, bool StillActivating, bool ActivationIntervallReached)
    {
        if (CurrentActivationTime < ActivationTime)
        {
            return;
        }

        // Spawn and Initialize Projectile:
        SkillHitObject SpawnedHitObject = Instantiate(HitObjectPrefab, SourceItemSkill.transform.position, SourceItemSkill.GetCurrentOwner().transform.rotation);

        if (SpawnSound)
        {
            var audioSource = SpawnedHitObject.gameObject.GetComponent<AudioSource>();
            if (!audioSource)
                audioSource = SpawnedHitObject.gameObject.AddComponent<AudioSource>();
            audioSource.clip = SpawnSound;
            if (LoopSound)
            {
                audioSource.loop = LoopSound;
                SpawnedHitObject.FadeSound = true;
                SpawnedHitObject.StartCoroutine(FadeAudioIn(audioSource, .5f));
            }
            else
            {
                audioSource.Play();
            }
        }

        SpawnedHitObject.InitializeHitObject(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, this, UseSkillLevelAtActivationMoment);

        // Stop Skill Activation:
        if (Cooldown > 0)
        {
            SourceItemSkill.SetCurrentCooldown(Cooldown);
        }
        RemoveActivationMovementRateModifier(SourceItemSkill, SourceItemSkill.GetCurrentOwner());
        SourceItemSkill.StoppedActivatingSkillWithHitObjects(this);
        SourceItemSkill.FinishedSkillActivation();
    }

    private IEnumerator FadeAudioIn(AudioSource source, float time)
    {
        source.volume = 0;
        source.Play();
        var step = 1 / time;
        while (source.volume < 1)
        {
            source.volume += step * Time.deltaTime;
            yield return null;
        }
        source.volume = 1;
    }
}