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

    public enum Defense
    {
        NONE = -1,          // When used as Damage Type: Damage Value is unaffected by any Defense!
        MELEE = 0,
        RANGED = 1,
        MAGIC = 2
    }

    [Header("Character Attributes:")]
    public int HealthCurrent = 100;
    public int HealthMax = 100;
    private bool CharacterIsDead = false;     // To Check if the Character already died, but was not removed yet. (Happened with GUI).

    public int EnergyCurrent = 100;
    public int EnergyMax = 100;

    public float[] Resistances = new float[6]; // Resistances[Enum Resistance], Check for Resistance.NONE!
    public float[] Defenses = new float[3];     // Defenses[Enum Defense], Check for Defense.NONE!

    public int SkillLevelModifier = 0;

    [Header("Equipment:")]
    public Transform[] CharacterHands = new Transform[2]; // Note: 0 : Left Hand, 1 : Right Hand
    public Item[] StartingWeapons = new Item[0];    // Note: Slot in Array corresponds to Hand it is holding. Up to 2 Starting Weapons!

    [Header("Equipment (for Testing):")]
    public int SkillsPerWeapon = 2;                 // Note: Number of Skills granted by each equipped weapon. The ItemSkillSlots[] has to take that number into account. Weapons with less Skills are allowed to exist!
    public Item[] WeaponSlots = new Item[2];        // Note: [0]: Left Hand,  [1]: Right Hand
    protected bool TwoHandedWeaponEquipped = false;
    // public Item[] ItemSlots = new Item[0];       // Note: Currently Unused, define which slot equals which type of item if more item types that are equipable are implemented.

    // NOTE : When Equipping Weapons, Left Hand Writes Skills from Left, Right Hand from Right. 
    // CURRENT IDEA: 2 Weapon Skill Slots: Left Hand [0], Right Hand [1], but if other Slot empty: Write Second Skill (Primary Skill always, Secondary if possible(ie. no other Skill).
    // Two Handed: Write Both (unequip other weapon!).

    public ItemSkill[] ItemSkillSlots = new ItemSkill[4];       // Here all Skills that the Character has access to are saved. For Players, match the Controller Buttons to these Slots for skill activation. 
    

    public int[] SkillCurrentlyActivating = { -1, -1 }; // Character is currently activating a Skill.
    //public float SkillActivationTimer = 0.0f;

    [Header("Active Conditions:")]
    public List<ActiveCondition> ActiveConditions = new List<ActiveCondition>();

    [Header("Physics Controller:")]
    protected PhysicsController PhysCont;

    [Header("Animation:")]
    public Animator[] HandAnimators = new Animator[2]; // Note: 0 : Left Hand, 1 : Right Hand

    //[Header("GUI (for Testing Purposes):")]
    private GUICharacterFollow GUIChar;

    protected void Start()
    {
        PhysCont = new PhysicsController(gameObject);
        CreateCharacterFollowGUI();     // Could be changed to when entering camera view or close to players, etc... as optimization.
        SpawnAndEquipStartingWeapons();
    }

    protected virtual void Update()
    {
        UpdateAllConditions();
        UpdateCurrentSkillActivation();
        UpdateAllCooldowns();
        
    }

    protected void LateUpdate()
    {
        UpdateCharacterFollowGUI();
    }

    // ====================================== ATTRIBUTES ======================================

    // Health:
    // Health Current:
    public void SetHealthCurrent(int NewValue)
    {
        HealthCurrent = Mathf.Clamp(NewValue, 0, HealthMax);
        if (!CheckIfCharacterDied())
        {
            GUIChar.UpdateHealthBar(GetHealthCurrentPercentage());
        }
    }

    public void ChangeHealthCurrent(int Value)
    {
        HealthCurrent = Mathf.Clamp(HealthCurrent + Value, 0, HealthMax);
        if (!CheckIfCharacterDied())
        {
            GUIChar.UpdateHealthBar(GetHealthCurrentPercentage());
        }         
    }

    protected bool CheckIfCharacterDied()
    {
        if (HealthCurrent <= 0)
        {
            if (!CharacterIsDead)
            {
                CharacterDied();
            }
            return true;
        }
        return false;
    }

    protected void CharacterDied()
    {
        CharacterIsDead = true; 

        // Unequip Weapons (so they drop on the gound):
        for (int i = 0; i < WeaponSlots.Length; i++)
        {
            if (WeaponSlots[i])
            {
                UnEquipWeapon(i);
            }
        }

        // Remove GUI:
        RemoveCharacterFollowGUI();

        // Destroy this Character:
        Destroy(this.gameObject);
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

    public int GetSkillLevelModifier()
    {
        return SkillLevelModifier;
    }

    public void ChangeSkillLevelModifier(int change)
    {
        SkillLevelModifier += change;
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
            EquipWeaponVisually(Weapon, SlotID);
            EquipSkills(Weapon.GetItemSkills(), SlotID * SkillsPerWeapon, SkillsPerWeapon);
            return true;
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
            EquipWeaponVisually(Weapon, 0);
            EquipSkills(Weapon.GetItemSkills(), 0, SkillsPerWeapon * 2);
            TwoHandedWeaponEquipped = true;
            return true;
        }
        //return false;
    }

    protected void EquipSkills(ItemSkill[] SkillsToEquip, int StartingSkillSlotID, int MaxNumberOfSkills)
    {
        for (int i = 0; i < Mathf.Min(SkillsToEquip.Length, MaxNumberOfSkills); i++)
        {
            ItemSkillSlots[i + StartingSkillSlotID] = SkillsToEquip[i];
            ItemSkillSlots[i + StartingSkillSlotID].UpdateCooldown(-1000);
        }
    }

    protected void UnEquipSkills(int StartingSkillSlotID, int MaxNumberOfSkills)
    {
        for (int i = 0; i < MaxNumberOfSkills; i++)
        {
            ItemSkillSlots[i + StartingSkillSlotID] = null;
        }
    }

    protected void UnEquipWeapon(int WeaponSlotID)
    {
        int MaxNumberOfSkills = SkillsPerWeapon;
        Weapon WeaponToUnequip = (Weapon)WeaponSlots[WeaponSlotID];

        InterruptCurrentSkillActivation(WeaponSlotID);

        if (WeaponToUnequip.IsTwoHanded())              // Weapon is Two Handed
        {
            UnEquipWeaponVisually(0);
            MaxNumberOfSkills *= 2;
            WeaponSlots[0].UnEquipItem();
            WeaponSlots[0] = null;
            WeaponSlots[1] = null;
            TwoHandedWeaponEquipped = false;
        }
        else                                            // Weapon is One Handed
        {
            UnEquipWeaponVisually(WeaponSlotID);
            WeaponSlots[WeaponSlotID].UnEquipItem();
            WeaponSlots[WeaponSlotID] = null;
        }

        UnEquipSkills(WeaponSlotID, MaxNumberOfSkills);
    }


    private void EquipWeaponVisually(Item Weapon, int HandSlotID)
    {
        Weapon.SwitchItemEquippedState(true);
        Weapon.transform.position = Weapon.GetEquippedPosition();
        Weapon.transform.localRotation = Quaternion.Euler(Weapon.GetEquippedRotation());

     //   int ScaleFlipMod = 1;
      //          if (HandSlotID == 1)
       //         {
       //             ScaleFlipMod = -1;
       //         }

               // Weapon.transform.localScale = new Vector3(Weapon.transform.localScale.x, Mathf.Abs(Weapon.transform.localScale.y) * ScaleFlipMod, Weapon.transform.localScale.z);
                
     //   BoxCollider test = Weapon.GetComponent<BoxCollider>();

        //test.size = new Vector3(test.size.x, Mathf.Abs(test.size.y) * ScaleFlipMod, test.size.z);


        Weapon.transform.SetParent(CharacterHands[HandSlotID], false);
    }

    private void UnEquipWeaponVisually(int HandSlotID)
    {
        //WeaponSlots[HandSlotID].gameObject.transform.SetParent(this.transform.parent);
        WeaponSlots[HandSlotID].SwitchItemEquippedState(false);
        WeaponSlots[HandSlotID].gameObject.transform.parent = null;
    }

    private void SpawnAndEquipStartingWeapons()
    {
        Item CurrentItem = null;
        for (int i = 0; i < StartingWeapons.Length; i++)
        {
            if (StartingWeapons[i])
            {
                CurrentItem = Instantiate(StartingWeapons[i]);
                CurrentItem.EquipItem(this, i);
            }
        }
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

    protected virtual void StartSkillActivation(int WeaponSkillSlotID)
    {
        if (!ItemSkillSlots[WeaponSkillSlotID]) { return; }

        if (!ItemSkillSlots[WeaponSkillSlotID].StartSkillActivation()) { return; }
        
        if (WeaponSkillSlotID < SkillsPerWeapon || TwoHandedWeaponEquipped)
        {
            SkillCurrentlyActivating[0] = WeaponSkillSlotID;
            WeaponSlots[0].SetSkillActivationTimer(0.0f);
        }
        else
        {
            SkillCurrentlyActivating[1] = WeaponSkillSlotID;
            WeaponSlots[1].SetSkillActivationTimer(0.0f);
        }
    }

    protected virtual void UpdateCurrentSkillActivation() { }

 /*   public void StopCurrentSkillActivation() // Unused!
    {
        if (SkillCurrentlyActivating >= 0)
        {
            SkillCurrentlyActivating = -1;
            SkillActivationTimer = 0.0f;
        }
    }
*/

    public virtual void FinishedCurrentSkillActivation(int WeaponSlotID)
    {
        /*if (SkillCurrentlyActivating < 0) // Shouldn't be needed?
        {
            return;
        }*/
        SkillCurrentlyActivating[WeaponSlotID] = -1;
       // SkillActivationTimer = 0.0f; // Now handled in ItemSkill/Item
    }

    private void UpdateAllCooldowns()
    {
        for (int i = 0; i < ItemSkillSlots.Length; i++)
        {
            if (ItemSkillSlots[i])
            {
                ItemSkillSlots[i].UpdateCooldown(Time.deltaTime);
            }
        }
    }

    private void InterruptCurrentSkillActivation(int WeaponSlotID)
    {
        if (SkillCurrentlyActivating[WeaponSlotID] < 0)
        {
            return;
        }

        ItemSkillSlots[SkillCurrentlyActivating[WeaponSlotID]].InterruptSkill(true);

        SkillCurrentlyActivating[WeaponSlotID] = -1;
        WeaponSlots[WeaponSlotID].SetSkillActivationTimer(0.0f);
    }


    // =================================== /SKILL ACTIVATION ====================================

    // =================================== EFFECT INTERACTION ===================================

    // Note: DamageAmount is assumed to be positive!
    public int InflictDamage(Defense DefenseType, Resistance DamageType, int Amount)
    {
        int FinalAmount = DamageCalculationDefense(DefenseType, Amount);

        FinalAmount = DamageCalculationResistance(DamageType, FinalAmount);


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

    private int DamageCalculationDefense(Defense DefenseType, int Amount)
    {
        int DamageTypeID = (int)(DefenseType);

        if (DamageTypeID < 0 || DamageTypeID >= Defenses.Length)
        {
            return Amount;
        }

        return Mathf.Max(0, Amount - Mathf.RoundToInt(Amount * Defenses[DamageTypeID]));
    }

    public void ChangeResistance(Resistance ResistanceType, float Amount)
    {
        Resistances[(int)ResistanceType] += Amount;
    }

    public void ChangeDefense(Defense DefenseType, float Amount)
    {
        Defenses[(int)DefenseType] += Amount;
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
        [SerializeField]
        int FixedLevel;

        public ActiveCondition(Character _TargetCharacter, Character _SourceCharacter, ItemSkill _SourceItemSkill, Condition _Condition)
        {
            TargetCharacter = _TargetCharacter;
            SourceCharacter = _SourceCharacter;
            SourceItemSkill = _SourceItemSkill;
            Cond = _Condition;
            TimeCounter = 0f;
            TickCounter = 0f;
            FixedLevel = SourceItemSkill.GetSkillLevel();

            ApplyCondition();
        }

        void ApplyCondition()
        {
            Cond.ApplyCondition(SourceCharacter, SourceItemSkill, TargetCharacter, FixedLevel);
        }

        // Return : True: Condition Ended, False: Condition did not end.
        public bool UpdateCondition()
        {
            float UpdateTime = Time.deltaTime;
                       
            if (Cond.HasTicks())
            {
                TickCounter += UpdateTime;

                if (Cond.ReachedTick(TickCounter))
                {
                    TickCounter -= Cond.GetTickTime();
                    Cond.ApplyEffectsOnTick(SourceCharacter, SourceItemSkill, TargetCharacter, FixedLevel);
                }
            }

            TimeCounter += UpdateTime;
            if (Cond.ReachedEnd(TimeCounter))
            {
                Cond.EndCondition(SourceCharacter, SourceItemSkill, TargetCharacter, FixedLevel);
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

        public void RemoveThisCondition()
        {
            Cond.EndCondition(SourceCharacter, SourceItemSkill, TargetCharacter, FixedLevel);
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

    public void RemoveCondition(Condition ConditionToRemove)
    {
        ActiveCondition ActCondToRemove = null;

        for (int i = 0; i < ActiveConditions.Count; i++)
        {
            if (ActiveConditions[i].RepresentsThisCondition(ConditionToRemove))
            {
                ActCondToRemove = ActiveConditions[i];
            }
        }

        if (ActCondToRemove != null)
        {
            ActCondToRemove.RemoveThisCondition();
            ActiveConditions.Remove(ActCondToRemove);
        }
    }

    // =================================== /ACTIVE CONDITIONS ===================================

    // =========================================== ANIMATION ==========================================

    public void StartAnimation(string AnimationName, float AnimationSpeed, int HandID)
    {
        HandAnimators[HandID].SetTrigger("Trigger_" + AnimationName);
        HandAnimators[HandID].speed = 1 / AnimationSpeed;
    }

    // ========================================== /ANIMATION ==========================================

    // =========================================== GUI ==========================================

    private void CreateCharacterFollowGUI()
    {
        if (!GUIChar)
        {
            GUIChar = GUIController.Instance.GenerateGUICharacterFollow();
            GUIChar.Initialize(this);
        }
    }

    private void UpdateCharacterFollowGUI()
    {
        GUIChar.UpdateGUIPosition();
    }

    private void RemoveCharacterFollowGUI()
    {
        GUIChar.DestroyGUICharacterFollow();
        GUIChar = null;
    }

    // ========================================== /GUI ==========================================
}
