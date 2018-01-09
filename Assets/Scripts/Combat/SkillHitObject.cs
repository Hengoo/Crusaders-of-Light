﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHitObject : MonoBehaviour {

    [Header("Hit Object Attributes:")]
    public float MaxTimeAlive = 0;
    private float TimeAliveCounter = 0;

    public float TickTime = 0;
    public float CurrentTickTime = 0.0f;
    public bool TickTimeReached = false;

    public bool CanHitSameTargetMultipleTime = false;
    public int MaxNumberOfTargets = -1;
    public bool HitObjectIsChildOfOwner = false;

    public bool AlwaysHitOwner = false;

    [Header("Hit Object - Does not need to be set in Editor!:")]
    public Character Owner;
    public SkillType SourceSkill;
    public ItemSkill SourceItemSkill;
    public int FixedLevel;

    public float[] Threat = new float[0];

    public Character.TeamAlignment HitObjectAlignment = Character.TeamAlignment.NONE;
    public List<Character> HitCharacters = new List<Character>();

    public List<Character> AlreadyHitCharacters = new List<Character>();

    public List<CharacterAttention> InCharactersAttentions = new List<CharacterAttention>();

    public void InitializeHitObject(Character _Owner, ItemSkill _SourceItemSkill, SkillType _SourceSkill, bool UseLevelAtActivationMoment)
    {
        // Link Skill User and Skill:
        Owner = _Owner;
        SourceSkill = _SourceSkill;
        SourceItemSkill = _SourceItemSkill;
        
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

        if (SourceSkill.GetAllowTargetFriendly())
        {
            counter += (int)(Owner.GetAlignment());
        }

        if (SourceSkill.GetAllowTargetEnemy())
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
        Threat = SourceSkill.GetThreat();
    }

    public void Update()
    {
        if (MaxTimeAlive > 0)
        {
            TimeAliveCounter += Time.deltaTime;

            if (TimeAliveCounter >= MaxTimeAlive)
            {
                HitObjectTimeOut();
                return;
            }
        }    

        if (TickTime > 0)
        {
            CurrentTickTime -= Time.deltaTime;

            if (CurrentTickTime <= 0)
            {
                TickTimeReached = true;

                if (AlwaysHitOwner)
                {
                    HitTarget(Owner);
                }

                List<Character> CleanUpMissingCharacters = new List<Character>();

                for (int i = 0; i < HitCharacters.Count; i++)
                {
                    if (HitCharacters[i] == null)
                    {
                        CleanUpMissingCharacters.Add(HitCharacters[i]);
                    }
                    else
                    {
                        HitTarget(HitCharacters[i]);
                    }
                }

                for (int i = 0; i < CleanUpMissingCharacters.Count; i++)
                {
                    HitCharacters.Remove(CleanUpMissingCharacters[i]);
                }

                CurrentTickTime += TickTime;
            }
            else if (TickTimeReached)
            {
                TickTimeReached = false;
            }
        }
    }

    public void HitObjectTimeOut()
    {
        RemoveFromAllAttentionsWhenDestroyed();
        Destroy(this.gameObject);
    }

    public void HitObjectSkillActivationEnd()
    {
        RemoveFromAllAttentionsWhenDestroyed();
        Destroy(this.gameObject);
    }

    protected virtual void HitTarget(Character TargetCharacter)
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

        AlreadyHitCharacters.Add(TargetCharacter);

        if (FixedLevel >= 0)
        {
            SourceSkill.ApplyEffects(Owner, SourceItemSkill, TargetCharacter, FixedLevel);
        }
        else
        {
            SourceSkill.ApplyEffects(Owner, SourceItemSkill, TargetCharacter);
        }

        if (MaxNumberOfTargets > 0
            && MaxNumberOfTargets >= AlreadyHitCharacters.Count)
        {
            ReachedMaxNumberOfTargets();
        }
    }
    
    protected virtual void ReachedMaxNumberOfTargets()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            HitCharacters.Add(other.GetComponent<Character>());

            if (TickTime > 0 && !TickTimeReached)
            {
                return;
            }
            HitTarget(other.gameObject.GetComponent<Character>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            HitCharacters.Remove(other.GetComponent<Character>());
        }
    }

    public bool IsParentSkill(SkillType Skill)
    {
        if (Skill == SourceSkill)
        {
            return true;
        }
        return false;
    }

    public Character.TeamAlignment GetAlignment()
    {
        return HitObjectAlignment;
    }

    public Character.TeamAlignment GetOwnerAlignment()
    {
        return Owner.GetAlignment();
    }

    public void AddInCharactersAttention(CharacterAttention InAttention)
    {
        InCharactersAttentions.Add(InAttention);
    }

    public void RemoveInCharactersAttention(CharacterAttention InAttention)
    {
        InCharactersAttentions.Remove(InAttention);
    }

    public void RemoveFromAllAttentionsWhenDestroyed()
    {
        for (int i = 0; i < InCharactersAttentions.Count; i++)
        {
            InCharactersAttentions[i].RemoveHitObjectAfterDestroy(this);
        }
    }

    public float GetCurrentThreat() // Includes Melee, not Far currently!
    {
        return Threat[0] + Threat[1];
    }
}