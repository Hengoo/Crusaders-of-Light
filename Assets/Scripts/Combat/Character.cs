using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    public enum TeamAlignment // Used for Skills!
    {
        NONE = 0,
        PLAYERS = 1,
        ENEMIES = 2,
        ALL = 3
    }

    public enum Resistance
    {
        NONE = -1,          // When used as Damage Type: Damage Value is unaffected by any Resistance!
        PHYSICAL = 0,
        MAGICAL = 1,
        FIRE = 2,
        COLD = 3,
        SHOCK = 4,
        POISON = 5
    }

    [Header("Character Attributes:")]
    public int HealthCurrent = 100;
    public int HealthMax = 100;

    public int EnergyCurrent = 100;
    public int EnergyMax = 100;

    public float[] Resistances = new float[6]; // Resistances[Enum Resistance], Check for Resistance.NONE!

    [Header("Equipment:")]
    public int SkillsPerWeapon = 2;                 // Note: Number of Skills granted by each equipped weapon. The ItemSkillSlots[] has to take that number into account. Weapons with less Skills are allowed to exist!
    public Item[] WeaponSlots = new Item[2];        // Note: [0]: Left Hand,  [1]: Right Hand
    // public Item[] ItemSlots = new Item[0];       // Note: Currently Unused, define which slot equals which type of item if more item types that are equipable are implemented.

    // NOTE : When Equipping Weapons, Left Hand Writes Skills from Left, Right Hand from Right. 
    // CURRENT IDEA: 2 Weapon Skill Slots: Left Hand [0], Right Hand [1], but if other Slot empty: Write Second Skill (Primary Skill always, Secondary if possible(ie. no other Skill).
    // Two Handed: Write Both (unequip other weapon!).

    public ItemSkill[] ItemSkillSlots = new ItemSkill[4];       // Here all Skills that the Character has access to are saved. For Players, match the Controller Buttons to these Slots for skill activation. 
    public bool[] SkillActivationButtonsPressed = new bool[4];  // Whether the Button is currently pressed down!

    public int SkillCurrentlyActivating = -1; // Character is currently activating a Skill.
    public float SkillActivationTimer = 0.0f;

    [Header("Active Conditions:")]
    public List<ActiveCondition> ActiveConditions = new List<ActiveCondition>();

    protected virtual void Update()
    {
        UpdateAllConditions();
    }

    // ====================================== ATTRIBUTES ======================================

    // Health:
    // Health Current:
    public void SetHealthCurrent(int NewValue)
    {
        HealthCurrent = Mathf.Clamp(NewValue, 0, HealthMax);
        CheckIfCharacterDied(); 
    }

    public void ChangeHealthCurrent(int Value)
    {
        HealthCurrent = Mathf.Clamp(HealthCurrent + Value, 0, HealthMax);
        CheckIfCharacterDied();
    }

    private void CheckIfCharacterDied()
    {
        if (HealthCurrent <= 0)
        {
            // TODO : Character Died!
            Debug.Log("TODO: " + this + " died!");
        }
    }

    public int GetHealthCurrent()
    {
        return HealthCurrent;
    }

    public float GetHealthCurrentPercentage()
    {
        return (float)(HealthCurrent) / (float)(HealthMax);
    }

    // Health Max:
    // Note: MaxHealth Changes are mirrored to current Health!
    public void SetHealthMax(int NewValue)
    {
        int change = NewValue - HealthMax;
        ChangeHealthMax(change);
    }

    public void ChangeHealthMax(int Value)
    {
        HealthMax += Value;
        ChangeHealthCurrent(Value);
    }

    public int GetHealthMax()
    {
        return HealthMax;
    }

    // Energy:
    // Energy Current:
    public void SetEnergyCurrent(int NewValue)
    {
        EnergyCurrent = Mathf.Clamp(NewValue, 0, EnergyMax);
    }

    public void ChangeEnergyCurrent(int Value)
    {
        EnergyCurrent = Mathf.Clamp(EnergyCurrent + Value, 0, EnergyMax);
    }

    public int GetEnergyCurrent()
    {
        return EnergyCurrent;
    }

    // Energy Max:
    // Note: MaxEnergy Changes are mirrored to current Energy!
    public void SetEnergyMax(int NewValue)
    {
        int change = NewValue - EnergyMax;
        ChangeEnergyMax(change);
    }

    public void ChangeEnergyMax(int Value)
    {
        EnergyMax += Value;
        ChangeEnergyCurrent(Value);
    }

    public int GetEnergyMax()
    {
        return EnergyMax;
    }
    // ===================================== /ATTRIBUTES ======================================

    // ===================================  EQUIPMENT SLOTS ====================================
    // General Flow to Equip Weapon: Press key when in range of weapon -> call EquipItem(this) in weapon.

    public bool EquipWeapon(Item Weapon, bool IsTwoHanded, int SlotID)
    {
        if (!IsTwoHanded)               // Single Handed Weapon:
        {
            if (WeaponSlots[SlotID])    // Single Handed Weapon, Already something equipped in Slot:
            {
                // Drop Old Weapon:
                UnEquipWeapon(SlotID);
            }
                                        // Single Handed Weapon, nothing equipped in Slot (now):
            // Equip New Weapon:
            WeaponSlots[SlotID] = Weapon;

            EquipSkills(Weapon.GetItemSkills(), SlotID * SkillsPerWeapon, SkillsPerWeapon);
        }
        else                            // Two Handed Weapon:
        {
            // Drop any Weapon equipped:
            for (int i = 0; i < WeaponSlots.Length; i++)
            {
                if (WeaponSlots[i])
                {
                    UnEquipWeapon(i);
                }
            }

            // Equip Two Handed Weapon:
            WeaponSlots[0] = WeaponSlots[1] = Weapon;
            EquipSkills(Weapon.GetItemSkills(), 0, SkillsPerWeapon * 2);
        }
        return false;
    }

    private void EquipSkills(ItemSkill[] SkillsToEquip, int StartingSkillSlotID, int MaxNumberOfSkills)
    {
        for (int i = 0; i < Mathf.Min(SkillsToEquip.Length, MaxNumberOfSkills); i++)
        {
            ItemSkillSlots[i + StartingSkillSlotID] = SkillsToEquip[i];
        }
    }

    private void UnEquipSkills(int StartingSkillSlotID, int MaxNumberOfSkills)
    {
        for (int i = 0; i < MaxNumberOfSkills; i++)
        {
            ItemSkillSlots[i + StartingSkillSlotID] = null;
        }
    }

    private void UnEquipWeapon(int WeaponSlotID)
    {
        int MaxNumberOfSkills = SkillsPerWeapon;
        Weapon WeaponToUnequip = (Weapon)WeaponSlots[WeaponSlotID];

        if (WeaponToUnequip.IsTwoHanded())              // Weapon is Two Handed
        {
            MaxNumberOfSkills *= 2;
            WeaponSlots[0].UnEquipItem();
            WeaponSlots[0] = null;
            WeaponSlots[1] = null;
        }
        else                                            // Weapon is One Handed
        {
            WeaponSlots[WeaponSlotID].UnEquipItem();
            WeaponSlots[WeaponSlotID] = null;
        }

        UnEquipSkills(WeaponSlotID, MaxNumberOfSkills);
    }

    // ===================================  /EQUIPMENT SLOTS ===================================

    // ======================================  ALIGNMENT =======================================

    public virtual TeamAlignment GetAlignment()
    {
        return TeamAlignment.NONE;
    }

    public bool IsCharacterSameTeam(Character otherCharacter)
    {
        if (otherCharacter.GetAlignment() == this.GetAlignment())
        {
            return true;
        }
        return false;
    }

    // =====================================  /ALIGNMENT =======================================

    // =================================== SKILL ACTIVATION ====================================

    protected void StartSkillActivation(int WeaponSkillSlotID)
    {

        if (!ItemSkillSlots[WeaponSkillSlotID]) { return; }

        if (!ItemSkillSlots[WeaponSkillSlotID].StartSkillActivation()) { return; }

        SkillCurrentlyActivating = WeaponSkillSlotID;
        SkillActivationTimer = 0.0f;
    }

    protected void UpdateCurrentSkillActivation()
    {
        if (SkillCurrentlyActivating < 0) { return; }

        SkillActivationTimer += Time.deltaTime;

        ItemSkillSlots[SkillCurrentlyActivating].UpdateSkillActivation(SkillActivationTimer, SkillActivationButtonsPressed[SkillCurrentlyActivating]);
    }

    public void StopCurrentSkillActivation() // Unused!
    {
        if (SkillCurrentlyActivating >= 0)
        {
            SkillCurrentlyActivating = -1;
            SkillActivationTimer = 0.0f;
        }
    }

    public void FinishedCurrentSkillActivation()
    {
        if (SkillCurrentlyActivating >= 0)
        {
            SkillCurrentlyActivating = -1;
            SkillActivationTimer = 0.0f;
        }
    }


    // =================================== /SKILL ACTIVATION ====================================

    // =================================== EFFECT INTERACTION ===================================

    // Note: DamageAmount is assumed to be positive!
    public int InflictDamage(Resistance DamageType, int Amount)
    {
        int FinalAmount = DamageCalculationResistance(DamageType, Amount);

        ChangeHealthCurrent(-1 * FinalAmount);

        return FinalAmount; // Note: Currently returns the amount of Damage that would theoretically be inflicted, not the actual amount of health lost.
    }

    private int DamageCalculationResistance(Resistance DamageType, int Amount)
    {
        int DamageTypeID = (int)(DamageType);

        if (DamageTypeID < 0 || DamageTypeID >= Resistances.Length)
        {
            return Amount;
        }

        return Mathf.Max(0, Amount - Mathf.RoundToInt(Amount * Resistances[DamageTypeID]));
    }

    public void ChangeResistance(Resistance ResistanceType, float Amount)
    {
        Resistances[(int)ResistanceType] += Amount;
    }

    // =================================== /EFFECT INTERACTION ===================================

    // =================================== ACTIVE CONDITIONS ===================================

    [System.Serializable]
    public class ActiveCondition
    {
        [SerializeField]
        Character TargetCharacter;
        [SerializeField]
        Character SourceCharacter;
        [SerializeField]
        ItemSkill SourceItemSkill;

        [SerializeField]
        Condition Cond;
        [SerializeField]
        float TimeCounter;
        [SerializeField]
        float TickCounter;

        public ActiveCondition(Character _TargetCharacter, Character _SourceCharacter, ItemSkill _SourceItemSkill, Condition _Condition)
        {
            TargetCharacter = _TargetCharacter;
            SourceCharacter = _SourceCharacter;
            SourceItemSkill = _SourceItemSkill;
            Cond = _Condition;
            TimeCounter = 0f;
            TickCounter = 0f;

            ApplyCondition();
        }

        void ApplyCondition()
        {
            Cond.ApplyCondition(SourceCharacter, SourceItemSkill, TargetCharacter);
        }

        // Return : True: Condition Ended, False: Condition did not end.
        public bool UpdateCondition()
        {
            float UpdateTime = Time.deltaTime;
            TimeCounter += UpdateTime;
            TickCounter += UpdateTime;
            
            if (Cond.ReachedTick(TickCounter))
            {
                TickCounter -= Cond.GetTickTime();
                Cond.ApplyEffectsOnTick(SourceCharacter, SourceItemSkill, TargetCharacter);
            }

            if (Cond.ReachedEnd(TimeCounter))
            {
                Cond.EndCondition(SourceCharacter, SourceItemSkill, TargetCharacter);
                return true;
            }
            return false;
        }

        public bool RepresentsThisCondition(Condition ConditionToCheck)
        {
            if (Cond = ConditionToCheck)
            {
                return true;
            }
            return false;
        }
    }

    public void ApplyNewCondition(Condition NewCondition, Character SourceCharacter, ItemSkill SourceItemSkill)
    {
        // TODO : Check first if Condition already exists / Logic for Stacking Conditions!
        ActiveCondition NewActiveCondition = new ActiveCondition(this, SourceCharacter, SourceItemSkill, NewCondition);
        ActiveConditions.Add(NewActiveCondition);
    }

    private void UpdateAllConditions()
    {
       // ActiveCondition CurrentCondition;
        List<ActiveCondition> ConditionsEnded = new List<ActiveCondition>();

        for (int i = 0; i < ActiveConditions.Count; i++)
        {

            if (ActiveConditions[i].UpdateCondition())
            {
                ConditionsEnded.Add(ActiveConditions[i]);
            }
        }
/*
        foreach (ActiveCondition AC in ActiveConditions)
        {
            CurrentCondition = AC;
            Debug.Log("FOREACH");
            if (CurrentCondition.UpdateCondition())
            {
                ConditionsEnded.Add(CurrentCondition);
            }
        }
        */
        foreach (ActiveCondition AC in ConditionsEnded)
        {
            ActiveConditions.Remove(AC);
        }

        ConditionsEnded.Clear();
    }

    public bool CheckIfConditionExists(Condition ConditionToCheck)
    {
        for (int i = 0; i < ActiveConditions.Count; i++)
        {
            if (ActiveConditions[i].RepresentsThisCondition(ConditionToCheck))
            {
                return true;
            }
        }
        return false;
    }

    // =================================== /ACTIVE CONDITIONS ===================================
}
