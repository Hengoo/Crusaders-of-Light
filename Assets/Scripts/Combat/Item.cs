using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    [Header("Item (Do not set - for Testing only):")]
    public Character CurrentOwner;
    public int CurrentEquipSlot = -1;

    [Header("Item Skills:")]
    public ItemSkill[] ItemSkills = new ItemSkill[1];
    private List<ItemSkill> ItemSkillsOnCooldown = new List<ItemSkill>();

    [Header("Item Skill Activation (Do not set - for Testing only):")]
    public float SkillActivationTimer;

    [Header("Item Hit Box:")]
    public Rigidbody ItemRidgidBody;
    public Collider ItemCollider;
    public bool IgnoreCurrentOwnerForCollisionChecks = true;

    [Header("Item Equipped Position and Rotation:")]
    public Vector3 EquippedPosition = new Vector3(0, 0, 0);
    public Vector3 EquippedRotation = new Vector3(0, 0, 0);

    [Header("Item Hit Box (Do not set - for Testing only):")]
    public List<Character> CurrentlyCollidingCharacters = new List<Character>();

    public bool CanHitCharactersOnlyOnce = false;
    public SkillType SkillCurrentlyUsingItemHitBox;
    public ItemSkill ItemSkillCurrentlyUsingItemHitBox;
    public List<Character> AlreadyHitCharacters = new List<Character>();
    public Character.TeamAlignment CurrentItemHitBoxAlignment = Character.TeamAlignment.NONE;

    public int MaxHittableCharacters = -1;
    private int MaxHittableCharactersCounter = -2;

    public int EquippedSlotID = -1;

    [Header("Item AI:")]
    public int ItemPowerLevel = 0; // Only for the Item itself, the Skills it grants is already calculated through it's skills.
    public MovePattern[] ItemMovePatterns = new MovePattern[0];

    public virtual void EquipItem(Character CharacterToEquipTo, int SlotID)
    {
    }

    public virtual void UnEquipItem()
    {
        var audioSource = GetComponent<AudioSource>();
        if(audioSource)
            audioSource.Stop();
    }

    // Currently Unused, but might be useful later.
    public void UpdateCooldowns()
    {
        for (int i = 0; i < ItemSkills.Length; i++)
        {
            ItemSkills[i].UpdateCooldown(Time.deltaTime);
        }
    }

    public ItemSkill[] GetItemSkills()
    {
        return ItemSkills;
    }

    public Character GetOwner()
    {
        return CurrentOwner;
    }

    public List<Character> GetAllCurrentlyCollidingCharacters()
    {
        return CurrentlyCollidingCharacters;
    }

    public int GetEquippedSlotID()
    {
        return EquippedSlotID;
    }

    public Vector3 GetEquippedPosition()
    {
        return EquippedPosition;
    }

    public Vector3 GetEquippedRotation()
    {
        return EquippedRotation;
    }

    public void SwitchItemEquippedState(bool IsEquipped)
    {
        if (IsEquipped)
        {
            ItemRidgidBody.isKinematic = true;
            ItemCollider.isTrigger = true;
        }
        else
        {
            ItemRidgidBody.isKinematic = false;
            ItemCollider.isTrigger = false;
        }
    }

    public void SetCurrentEquipSlot(int SlotID)
    {
        CurrentEquipSlot = SlotID;
    }

    public int GetCurrentEquipSlot()
    {
        return CurrentEquipSlot;
    }

    public void SetAllItemSkillsLevel(int Value)
    {
        for (int i = 0; i < ItemSkills.Length; i++)
        {
            ItemSkills[i].SetSkillLevel(Value);
        }
    }

    public void SetSkillActivationTimer(float value)
    {
        SkillActivationTimer = value;
    }

    public float GetSkillActivationTimer()
    {
        return SkillActivationTimer;
    }

    public void UpdateSkillActivationTimer()
    {
        SkillActivationTimer += Time.deltaTime;
    }

    public void StartSkillCurrentlyUsingItemHitBox(ItemSkill SourceItemSkill, SkillType SourceSkill, bool HitEachCharacterOnce)
    {
        if (SkillCurrentlyUsingItemHitBox)
        {
            Debug.Log("WARNING: " + SourceSkill + " overwrites existing " + SkillCurrentlyUsingItemHitBox + " Skill for use of Item Hit Box!");
        }

        CanHitCharactersOnlyOnce = HitEachCharacterOnce;
        SkillCurrentlyUsingItemHitBox = SourceSkill;
        ItemSkillCurrentlyUsingItemHitBox = SourceItemSkill;
        AlreadyHitCharacters = new List<Character>();

        // Calculate which Team(s) the Item can hit:
        int counter = 0;

        if (SourceSkill.GetAllowTargetFriendly())
        {
            counter += (int)(CurrentOwner.GetAlignment());
        }

        if (SourceSkill.GetAllowTargetEnemy())
        {
            counter += ((int)(CurrentOwner.GetAlignment()) % 2) + 1;
        }

        CurrentItemHitBoxAlignment = (Character.TeamAlignment)(counter);

        // Try to hit all Characters already in the hit box:

        List<Character> CleanUpMissingCharacters = new List<Character>();

        for (int i = 0; i < CurrentlyCollidingCharacters.Count; i++)
        {
            if (CurrentlyCollidingCharacters[i] == null)
            {
                CleanUpMissingCharacters.Add(CurrentlyCollidingCharacters[i]);
            }
            else if (CheckIfEnterCharacterLegit(CurrentlyCollidingCharacters[i]))
            {
                ApplyCurrentSkillEffectsToCharacter(CurrentlyCollidingCharacters[i]);
            }
        }

        for (int i = 0; i < CleanUpMissingCharacters.Count; i++)
        {
            CurrentlyCollidingCharacters.Remove(CleanUpMissingCharacters[i]);
        }
    }

    public void StartSkillCurrentlyUsingItemHitBox(ItemSkill SourceItemSkill, SkillType SourceSkill, bool HitEachCharacterOnce, int MaxNumberOfCharactersHittable)
    {
        MaxHittableCharacters = MaxNumberOfCharactersHittable;
        MaxHittableCharactersCounter = 0;
        StartSkillCurrentlyUsingItemHitBox(SourceItemSkill, SourceSkill, HitEachCharacterOnce);
    }

    public void EndSkillCurrentlyUsingItemHitBox()
    {
        SkillCurrentlyUsingItemHitBox = null;
        ItemSkillCurrentlyUsingItemHitBox = null;
        AlreadyHitCharacters = new List<Character>();
        MaxHittableCharacters = -1;
        MaxHittableCharactersCounter = -2;
    }

    public bool CheckIfSkillIsUsingHitBox(ItemSkill SkillToCheck)
    {
        if (ItemSkillCurrentlyUsingItemHitBox && ItemSkillCurrentlyUsingItemHitBox == SkillToCheck)
        {
            return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            Character OtherCharacter = other.gameObject.GetComponent<Character>();

            if (!CheckIfEnterCharacterLegit(OtherCharacter))
            {
                return;
            }

            CurrentlyCollidingCharacters.Add(OtherCharacter);

            if (SkillCurrentlyUsingItemHitBox)
            {
                ApplyCurrentSkillEffectsToCharacter(OtherCharacter);
            }
        }
    }

    private void ApplyCurrentSkillEffectsToCharacter(Character HitCharacter)
    {
        if (MaxHittableCharactersCounter >= MaxHittableCharacters)
        {
            return;
        }

        if (CurrentItemHitBoxAlignment == Character.TeamAlignment.ALL
                    || CurrentItemHitBoxAlignment == HitCharacter.GetAlignment())
        {
            MaxHittableCharactersCounter++;
            SkillCurrentlyUsingItemHitBox.ApplyEffects(CurrentOwner, ItemSkillCurrentlyUsingItemHitBox, HitCharacter);
        }
    }

    private bool CheckIfEnterCharacterLegit(Character CharacterToCheck)
    {
        if (IgnoreCurrentOwnerForCollisionChecks && CharacterToCheck == CurrentOwner)
        {
            return false;
        }

        if (CanHitCharactersOnlyOnce && AlreadyHitCharacters.Contains(CharacterToCheck))
        {
            return false;
        }

        // If this point is reached, the Character is legit:

        if (CanHitCharactersOnlyOnce)
        {
            AlreadyHitCharacters.Add(CharacterToCheck);
        }

        return true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            Character OtherCharacter = other.gameObject.GetComponent<Character>();

            CurrentlyCollidingCharacters.Remove(OtherCharacter);
        }
    }

    public int GetTotalPowerLevel()
    {
        int TempPowerLevel = ItemPowerLevel;

        for (int i = 0; i < ItemSkills.Length; i++)
        {
            TempPowerLevel += ItemSkills[i].GetBasePowerLevel();
        }

        return TempPowerLevel;
    }

    public MovePattern[] GetMovePatterns()
    {
        return ItemMovePatterns;
    }

    public void DestroyAllHitObjects()
    {
        for (int i = 0; i < ItemSkills.Length; i++)
        {
            ItemSkills[i].DestroyAllHitObjects();
        }
    }
}
