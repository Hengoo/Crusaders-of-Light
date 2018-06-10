using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{

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

    [Header("Static:")]
    public static float HealthHealingMinimumPerc = 1.0f;
    public static float HealthHealingLostPerCountMaxPerc = 0.01f;

    [Header("Character Attributes:")]
    public int HealthCurrent = 100;
    public int HealthMax = 100;
    protected bool CharacterIsDead = false;     // To Check if the Character already died, but was not removed yet. (Happened with GUI).

    public int HealthHealingMin = 0;
    public int HealthHealingMax = 100;
    public int HealthHealingLostPerCount = 10;
    public float HealthHealingCounter = 0f;
    protected float HealthHealingCounterTimer = 1f;

    public int EnergyCurrent = 100;
    public int EnergyMax = 100;

    public float[] Resistances = new float[6]; // Resistances[Enum Resistance], Check for Resistance.NONE!
    public float[] Defenses = new float[3];     // Defenses[Enum Defense], Check for Defense.NONE!

    public int SkillLevelModifier = 0;

    [Header("Equipment:")]
    public Transform[] CharacterHands = new Transform[2]; // Note: 0 : Left Hand, 1 : Right Hand
    public Item[] StartingWeapons = new Item[0];    // Note: Slot in Array corresponds to Hand it is holding. Up to 2 Starting Weapons!

    [Header("Equipment (for Testing):")]
    public int SkillsPerWeapon = 8;                 // Note: Number of Skills granted by each equipped weapon. The ItemSkillSlots[] has to take that number into account. Weapons with less Skills are allowed to exist!
    public Item[] WeaponSlots = new Item[2];        // Note: [0]: Left Hand,  [1]: Right Hand
    protected bool TwoHandedWeaponEquipped = false;
    // public Item[] ItemSlots = new Item[0];       // Note: Currently Unused, define which slot equals which type of item if more item types that are equipable are implemented.

    // NOTE : When Equipping Weapons, Left Hand Writes Skills from Left, Right Hand from Right. 
    // CURRENT IDEA: 2 Weapon Skill Slots: Left Hand [0], Right Hand [1], but if other Slot empty: Write Second Skill (Primary Skill always, Secondary if possible(ie. no other Skill).
    // Two Handed: Write Both (unequip other weapon!).

    public ItemSkill[] ItemSkillSlots = new ItemSkill[8];       // Here all Skills that the Character has access to are saved. For Players, match the Controller Buttons to these Slots for skill activation. 

    [Header("Element:")]
    public ElementItem EquippedElement;
    public ElementItem StartingElement;

    [Header("Skill Activation:")]
    protected int[] SkillCurrentlyActivating = { -1, -1 }; // Character is currently activating a Skill.
    protected int LastSkillActivated = -1;
    protected float LastSkillActivatedTimer = 0.0f;
    public float LastSkillActivatedStartTime = 1f;

    //public float SkillActivationTimer = 0.0f;

    protected int HindranceLevel = 0;

    [Header("Active Conditions:")]
    public List<ActiveCondition> ActiveConditions = new List<ActiveCondition>();

    [Header("Physics Controller:")]
    protected PhysicsController PhysCont;
    public float MovementRateModfier = 1.0f;

    [Header("Animation:")]
    public Animator[] HandAnimators = new Animator[2]; // Note: 0 : Left Hand, 1 : Right Hand
    public Animator BodyAnimator;

    protected bool IsWalking = false;
    public string Anim_StartWalking = "StartWalking";
    public string Anim_EndWalking = "EndWalking";

    //[Header("GUI (for Testing Purposes):")]
    [Header("GUI HealthBars:")]
    protected GUICharacterFollow GUIChar;

    // Attention:
    [Header("Attention:")]
    public CharacterAttention CharAttention;
    
    protected UnityAction _onCharacterDeathAction; // Event system for character death

    

    protected virtual void Start()
    {
        HealthHealingMax = HealthCurrent;
        CalculateHealthHealingMin();
        //PhysCont = new PhysicsController(gameObject);
        CreateCharacterFollowGUI();     // Could be changed to when entering camera view or close to players, etc... as optimization.
        // SpawnAndEquipStartingWeapons();
        SpawnAndEquipStartingElement();
    }

    protected virtual void Update()
    {
        UpdateAllConditions();
        UpdateCurrentSkillActivation();
        UpdateAllCooldowns();
        UpdateHealthHealingMax();
        UpdateLastSkillActivated();
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

    protected virtual void CharacterDied()
    {
        CharacterIsDead = true;

        /*      ### Project 2: Characters should no longer drop Weapons on death! ###
        // Unequip Weapons (so they drop on the gound):
        for (int i = 0; i < WeaponSlots.Length; i++)
        {
            if (WeaponSlots[i])
            {
                UnEquipWeapon(i);
            }
        }
        */

        // Update Attention:
        AttentionThisCharacterDied();

        // Invoke death actions (e.g. Quest System)
        if(_onCharacterDeathAction != null)
            _onCharacterDeathAction.Invoke();

        // Destroy this Character:
        Destroy(this.gameObject);
    }

    public bool GetCharacterIsDead()
    {
        return CharacterIsDead;
    }

    void OnDestroy()
    {
        //Remove GUI
        RemoveCharacterFollowGUI();
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
        CalculateHealthHealingMin();
    }

    public void ChangeHealthMax(int Value)
    {
        HealthMax += Value;
        ChangeHealthCurrent(Value);
        CalculateHealthHealingMin();
    }

    public int GetHealthMax()
    {
        return HealthMax;
    }

    public void CalculateHealthHealingMin()
    {
        HealthHealingMin = Mathf.RoundToInt(HealthHealingMinimumPerc * GetHealthMax());
        HealthHealingLostPerCount = Mathf.RoundToInt(HealthHealingLostPerCountMaxPerc * GetHealthMax());
    }

    public float GetHealthHealingPercentage()
    {
        return (float)HealthHealingMax / (float)GetHealthMax();
    }

    public void UpdateHealthHealingMax()
    {
        if (HealthHealingMax <= HealthCurrent || HealthHealingMax <= HealthHealingMin || CharacterIsDead)
        {
            return;
        }

        HealthHealingCounter += Time.deltaTime;

        if (HealthHealingCounter >= HealthHealingCounterTimer)
        {
            HealthHealingCounter -= HealthHealingCounterTimer;

            HealthHealingMax = Mathf.Max(HealthHealingMin, HealthHealingMax - HealthHealingLostPerCount, HealthCurrent);
            GUIChar.UpdateHealthHealingBar(GetHealthHealingPercentage());
        }
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

    public float GetMovementRateModifier()
    {
        return Mathf.Max(0, MovementRateModfier);
    }

    public void ChangeMovementRateModifier(float Change)
    {
        MovementRateModfier += Change;
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

        UnEquipSkills(WeaponSlotID * SkillsPerWeapon, MaxNumberOfSkills);
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

    //Need this for quest fire wizard spawning - Jean
    public void SpawnAndEquipStartingWeapons()
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


    // =======================================  ELEMENT  =======================================

    public void EquipElement(ElementItem ElementToEquip)
    {
        EquippedElement = ElementToEquip;
        EquipElementVisually();
    }

    private void EquipElementVisually()
    {
        EquippedElement.transform.SetParent(CharacterHands[0], false);
        // TODO : How exactly should the Element be displayed?
        // TODO : Add Element Effect to Weapon!
    }

    public ElementItem UnequipElement()
    {
        ElementItem tempElement = EquippedElement;
        EquippedElement = null;
        return tempElement;
    }

    public void UnequipElementAndDestroy()
    {
        Destroy(UnequipElement().gameObject);
    }

    public ElementItem GetEquippedElement()
    {
        if(EquippedElement)
        {
            return EquippedElement;
        }
        return null;
    }

    private void SpawnAndEquipStartingElement()
    {
        if (!StartingElement)
        {
            return;
        }

        ElementItem tempEle = Instantiate(StartingElement);
        EquipElement(tempEle);
    }

    // ======================================= /ELEMENT  =======================================


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
            ResetLastSkillActivated();
        }
        else
        {
            SkillCurrentlyActivating[1] = WeaponSkillSlotID;
            WeaponSlots[1].SetSkillActivationTimer(0.0f);
            ResetLastSkillActivated();
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

    public virtual void FinishedCurrentSkillActivation(int WeaponSlotID, int Hindrance)
    {
        /*if (SkillCurrentlyActivating < 0) // Shouldn't be needed?
        {
            return;
        }*/
        ChangeHindranceLevel(Hindrance);

        SetLastSkillActivated(SkillCurrentlyActivating[WeaponSlotID]);

        SkillCurrentlyActivating[WeaponSlotID] = -1;
        
        // SkillActivationTimer = 0.0f; // Now handled in ItemSkill/Item
    }

    private void UpdateAllCooldowns()
    {
        for (int i = 0; i < ItemSkillSlots.Length; i++)
        {
            if (ItemSkillSlots[i] && SkillCurrentlyActivating[0] != i && SkillCurrentlyActivating[1] != i)
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

    public void ChangeHindranceLevel(int Change)
    {
        HindranceLevel += Change;
    }

    public void ChangeHindranceLevel(SkillType.Hindrance Change)
    {
        HindranceLevel += (int)(Change);
    }

    public bool CheckHindrance(SkillType.Hindrance Hindrance)
    {
        if (Hindrance == SkillType.Hindrance.NO_OTHER_SKILLS)
        {
            if (HindranceLevel > 0)
            {
                return false;
            }
            return true;
        }

        if ((int)(Hindrance) + HindranceLevel > 3)
        {
            return false;
        }
        return true;
    }

    public void UpdateLastSkillActivated()
    {
        if (LastSkillActivatedTimer > 0)
        {
            LastSkillActivatedTimer -= Time.deltaTime;

            if (LastSkillActivatedTimer <= 0)
            {
                LastSkillActivated = -1;
            }
        }
    }

    public void SetLastSkillActivated(int SkillSlotID)
    {
        LastSkillActivated = SkillSlotID;
        LastSkillActivatedTimer = LastSkillActivatedStartTime;
    }

    public void ResetLastSkillActivated()
    {
        LastSkillActivated = -1;
        LastSkillActivatedTimer = -1;
    }

    // =================================== /SKILL ACTIVATION ====================================

    // =================================== EFFECT INTERACTION ===================================

    public void Heal(int Amount)
    {
        if (Amount + GetHealthCurrent() > HealthHealingMax)
        {
            SetHealthCurrent(HealthHealingMax);
        }
        else
        {
            ChangeHealthCurrent(Amount);
        }
    }

    public int GetHealthPercentageAbsoluteValue(float HealthPercentage)
    {
        return Mathf.RoundToInt(HealthPercentage * GetHealthMax());
    }

    // Note: DamageAmount is assumed to be positive!
    public int InflictDamage(Defense DefenseType, Resistance DamageType, int Amount, int DefenseIgnore, int ResistanceIgnore)
    {
        int FinalAmount = DamageCalculationDefense(DefenseType, Amount, DefenseIgnore);

        FinalAmount = DamageCalculationResistance(DamageType, FinalAmount, ResistanceIgnore);


        ChangeHealthCurrent(-1 * FinalAmount);

        return FinalAmount; // Note: Currently returns the amount of Damage that would theoretically be inflicted, not the actual amount of health lost.
    }

    private int DamageCalculationResistance(Resistance DamageType, int Amount, int ResistanceIgnore)
    {
        int DamageTypeID = (int)(DamageType);

        if (DamageTypeID < 0 || DamageTypeID >= Resistances.Length)
        {
            return Amount;
        }

        // return Mathf.Max(0, Amount - Mathf.RoundToInt(Amount * Resistances[DamageTypeID])); // Resistance as Percentage Reduction.

        // Damage: At 0: Damage Value / At 10: 0.5 Value / At 20: 0.25 Value / At 30: 0.125 Value / ... At -10: 2 Value / At -20: 4 Value / ...
        return Mathf.RoundToInt(Amount * Mathf.Pow(2, (-1f * (Mathf.Max(0, Resistances[DamageTypeID] - ResistanceIgnore)) / 10.0f)));
    }

    private int DamageCalculationDefense(Defense DefenseType, int Amount, int DefenseIgnore)
    {
        int DamageTypeID = (int)(DefenseType);

        if (DamageTypeID < 0 || DamageTypeID >= Defenses.Length)
        {
            return Amount;
        }

        // return Mathf.Max(0, Amount - Mathf.RoundToInt(Amount * Defenses[DamageTypeID])); // Defense as Percentage Reduction.

        // Damage: At 0: Damage Value / At 10: 0.5 Value / At 20: 0.25 Value / At 30: 0.125 Value / ... At -10: 2 Value / At -20: 4 Value / ...
        return Mathf.RoundToInt(Amount * Mathf.Pow(2, (-1f * (Mathf.Max(0, Defenses[DamageTypeID] - DefenseIgnore)) / 10.0f)));
    }

    public void ChangeResistance(Resistance ResistanceType, float Amount)
    {
        Resistances[(int)ResistanceType] += Amount;
    }

    public float GetResistance(Resistance ResistanceType)
    {
        int ResistanceTypeID = (int)(ResistanceType);

        if (ResistanceTypeID < 0)
        {
            return 0;
        }

        return Resistances[ResistanceTypeID];
    }

    public void ChangeDefense(Defense DefenseType, float Amount)
    {
        Defenses[(int)DefenseType] += Amount;
    }

    public float GetDefense(Defense DefenseType)
    {
        int DefenseTypeID = (int)(DefenseType);

        if (DefenseTypeID < 0)
        {
            return 0;
        }

        return Defenses[DefenseTypeID];
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
        [SerializeField]
        float MaxDuration;

        [SerializeField]
        GameObject VisualEffectObject;

        public ActiveCondition(Character _TargetCharacter, Character _SourceCharacter, ItemSkill _SourceItemSkill, Condition _Condition, float Duration)
        {
            TargetCharacter = _TargetCharacter;
            SourceCharacter = _SourceCharacter;
            SourceItemSkill = _SourceItemSkill;
            Cond = _Condition;
            TimeCounter = 0f;
            TickCounter = 0f;
            FixedLevel = SourceItemSkill.GetSkillLevel();

            MaxDuration = Duration;

            if (Cond.GetVisualEffectObject())
            {
                VisualEffectObject = Instantiate(Cond.GetVisualEffectObject(), TargetCharacter.transform.position, TargetCharacter.transform.rotation);
                VisualEffectObject.transform.SetParent(TargetCharacter.transform, true);
            }

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
            if (ReachedEnd(TimeCounter))
            {
                EndCondition();
                return true;
            }
            return false;
        }

        public bool RepresentsThisCondition(Condition ConditionToCheck)
        {
            if (Cond == ConditionToCheck)
            {
                return true;
            }
            return false;
        }

        public void RemoveThisCondition()
        {
            EndCondition();
        }

        private void EndCondition()
        {
            if (VisualEffectObject)
            {
                Destroy(VisualEffectObject.gameObject);
            }
            Cond.EndCondition(SourceCharacter, SourceItemSkill, TargetCharacter, FixedLevel);
        }

        public bool ReachedEnd(float TimeCounter)
        {
            if (MaxDuration >= 0 && TimeCounter >= MaxDuration)
            {
                return true;
            }
            return false;
        }
    }

    public void ApplyNewCondition(Condition NewCondition, Character SourceCharacter, ItemSkill SourceItemSkill, float Duration)
    {
        // If the maximum Instances of this Condition is reached, one is removed and the new one applied:
        if (CheckIfConditionExists(NewCondition) >= NewCondition.GetInstanceMaximum())
        {
            RemoveCondition(NewCondition);
        }

        ActiveCondition NewActiveCondition = new ActiveCondition(this, SourceCharacter, SourceItemSkill, NewCondition, Duration);
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

    protected void EndAllConditions()
    {
        for (int i = 0; i < ActiveConditions.Count; i++)
        {
            ActiveConditions[i].RemoveThisCondition();    
        }

        ActiveConditions = new List<ActiveCondition>();
    }

    public int CheckIfConditionExists(Condition ConditionToCheck)
    {
        int NumberConditions = 0;

        for (int i = 0; i < ActiveConditions.Count; i++)
        {
            if (ActiveConditions[i].RepresentsThisCondition(ConditionToCheck))
            {
                NumberConditions++;
                if (ConditionToCheck.GetInstanceMaximum() >= NumberConditions)
                {
                    return NumberConditions;
                }
            }
        }
        return NumberConditions;
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

    // =========================================== ATTENTION ==========================================

    public CharacterAttention GetAttention()
    {
        return CharAttention;
    }

    public void AttentionThisCharacterDied()
    {
        CharAttention.OwnerDied();
    }

    public void AttentionCharacterDied(Character CharDied)
    {
        CharAttention.CharacterDied(CharDied);
    }

    public void AttentionPlayerRespawned(Character CharRespawned)
    {
        CharAttention.PlayerEntersAttentionRange(CharRespawned);
    }

    // ========================================== /ATTENTION ==========================================

    // =========================================== ANIMATION ==========================================

    public void StartAnimation(string AnimationName, float AnimationSpeed, int HandID)
    {
        HandAnimators[HandID].SetTrigger("Trigger_" + AnimationName);
        HandAnimators[HandID].speed = 1 / AnimationSpeed;
    }

    public void StartBodyAnimation(string AnimationName, float AnimationSpeed)
    {
        BodyAnimator.SetTrigger("Trigger_" + AnimationName);
        BodyAnimator.speed = 1 / AnimationSpeed;
    }

    public void StartBodyAnimation(string AnimationName)
    {
        BodyAnimator.SetTrigger("Trigger_" + AnimationName);
    }

    public void StartBodyAnimation(float AnimationSpeed)
    {
        BodyAnimator.speed = AnimationSpeed;
    }

    public void SetIsWalking(bool state)
    {
        IsWalking = state;
    }

    public bool GetIsWalking()
    {
        return IsWalking;
    }

    public void SwitchWalkingAnimation(bool state)
    {
        if (state)
        {
            if (!IsWalking)
            {
                IsWalking = true;
                StartBodyAnimation(Anim_StartWalking);
            }
        }
        else
        {
            if (IsWalking)
            {
                IsWalking = false;
                StartBodyAnimation(Anim_EndWalking);
            }
        }
    }

    // ========================================== /ANIMATION ==========================================

    // ========================================= AI =========================================

    public float GetCurrentThreatLevel(bool IncludeMeleeRange, bool IncludeFarRange)
    {
        float TotalThreat = 0.0f;
        float[] CurrentThreat = new float[3];

        for (int i = 0; i < SkillCurrentlyActivating.Length; i++)
        {
            if(SkillCurrentlyActivating[i] >= 0)
            {
                CurrentThreat = ItemSkillSlots[SkillCurrentlyActivating[i]].AIGetThreat();

                TotalThreat += CurrentThreat[0];

                if (IncludeMeleeRange)
                {
                    TotalThreat += CurrentThreat[1];
                }

                if (IncludeFarRange)
                {
                    TotalThreat += CurrentThreat[2];
                }
            }
        }

        return TotalThreat;
    }

    // ========================================= /AI =========================================

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
        if(GUIChar)
            GUIChar.DestroyGUICharacterFollow();
        GUIChar = null;
    }

    protected void SwitchActiveStateCharacterFollowGUI(bool state)
    {
        if(GUIChar)
        {
            GUIChar.SwitchGUIActive(state);
        }
    }

    // ========================================== /GUI ==========================================


    // =========================================== EVENTS ==========================================

    public void SubscribeDeathAction(UnityAction action)
    {
        if(action != null)
            _onCharacterDeathAction += action;
    }

    // ========================================== /EVENTS ==========================================
}
