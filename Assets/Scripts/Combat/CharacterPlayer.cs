using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class CharacterPlayer : Character {

    [Header("Character Player:")]
    public bool[] SkillActivationButtonsPressed = new bool[4];  // Whether the Button is currently pressed down!

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.PLAYERS;

    [Header("Close Items List:")]
    public List<Item> ItemsInRange = new List<Item>();

    [Header("Input:")]
    public int PlayerID = -1; // Set from 1 to 4!
    public int HoldDownButtonID = 1; // The button that is checked wether it is held down for skills. If there is time, replace with check depending on skill position in combo tree.

    [Header("Respawning:")]
    public CharacterDeathTimer DeathTimer;

    public static int CharacterLayerID = 8;
    public static int DeadCharacterLayerID = 12;

    public float RespawnMinRange = 2f;
    public float RespawnMaxRange = 5f;
    
    public float RespawnHealthCostPerc = 0.2f;
    public float RespawnHealthGainPerc = 0.15f;

    public string DeathAnimationResetTrigger = "Trigger_DeathReset";

    [Header("Navmesh Movement:")]
    public NavMeshAgent NavAgent;
    public float RotationSpeed = 5;

    Vector3 targetVel = Vector3.zero;
    Vector3 targetDir = Vector3.zero;

    [Header("Orb Input:")]
    public LightOrbEffects LightOrbEffects;
    public float OrbInputTimer = -1f;
    public float OrbInputReviveTime = 1f;
    public float OrbInputHealMaxTime = 0.3f;
    private bool OrbInputButtonPressed = false;


 //   public float DyingPhysicsDuration = 0.8f;
 //   public bool DyingPhysicsTimerRunning = false;
 //   public float DyingPhysicsTimer = 0f;

    protected override void Start()
    {
        base.Start();
        NavAgent.updateRotation = false;
        SpawnAndEquipStartingWeapons();
        
    }

    protected override void SpawnAndEquipStartingElement()
    {
        if (GameController.Instance)
        {
            StartingElement = GameController.Instance.GetPlayerElement(PlayerID);
        }

        base.SpawnAndEquipStartingElement();
    }

    public override void SpawnAndEquipStartingWeapons()
    {
        if (GameController.Instance)
        {
            StartingWeapons[0] = GameController.Instance.GetPlayerItem(PlayerID);
        }

        base.SpawnAndEquipStartingWeapons();
    }

    private UnityAction _itemPickupAction;

    protected override void Update()
    {
        PlayerInput();
        base.Update();
    }

    private void FixedUpdate()
    {
        float speedfaktor = 7* GetMovementRateModifier();
        
        if (!GetOverrideMovement())
        {
            //left stick
            targetVel = new Vector3(Input.GetAxisRaw("Horizontal_" + PlayerID), 0, Input.GetAxisRaw("Vertical_" + PlayerID));
        }
        else
        {
            targetVel = OverrideMovementVec;
        }

        if (!GetOverrideRotation())
        {
            //right stick
            targetDir = new Vector3(Input.GetAxisRaw("Horizontal2_" + PlayerID), 0, -Input.GetAxisRaw("Vertical2_" + PlayerID));
        }
        else
        {
            targetDir = OverrideRotationVec;
        }

        if (Vector3.Magnitude(targetDir) <= 0.3f)
        {
            targetDir = targetVel;
        }

        targetVel = targetVel * speedfaktor * Time.deltaTime;

        // Rotate towards Velocity Direction:
        if (targetDir.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * RotationSpeed);
        }

        //PhysCont.SetVelRot(targetVel, targetDir);
        NavAgent.Move(targetVel);

        if (!IsWalking
            && (GetOverrideMovement()
            || (Input.GetAxisRaw("Horizontal_" + PlayerID) >= 0.05f 
            || Input.GetAxisRaw("Vertical_" + PlayerID) >= 0.05f
            || Input.GetAxisRaw("Horizontal_" + PlayerID) <= -0.05f
            || Input.GetAxisRaw("Vertical_" + PlayerID) <= -0.05f)))
        {
            IsWalking = true;
            StartBodyAnimation(Anim_StartWalking);
        }
        else if (IsWalking)
        {
            if (!GetOverrideMovement() 
            && (Input.GetAxisRaw("Horizontal_" + PlayerID) < 0.05f
            && Input.GetAxisRaw("Vertical_" + PlayerID) < 0.05f
            && Input.GetAxisRaw("Horizontal_" + PlayerID) > -0.05f
            && Input.GetAxisRaw("Vertical_" + PlayerID) > -0.05f))
            {
                IsWalking = false;
                StartBodyAnimation(Anim_EndWalking);
            }
            else
            {
                StartBodyAnimation(speedfaktor / 10);
            }
        }
    }

    public override TeamAlignment GetAlignment()
    {
        return Alignment;
    }

    protected override void CharacterDied()
    {
        //base.CharacterDied();

        // End Conditions:
        EndAllConditions();

        // Character is Dead:
        CharacterIsDead = true;

        // Unequip Weapons (so they drop on the gound):
        for (int i = 0; i < WeaponSlots.Length; i++)
        {
            if (WeaponSlots[i])
            {
                UnEquipWeapon(i);
            }
        }

        for (int i = 0; i < SkillCurrentlyActivating.Length; i++)
        {
            SkillCurrentlyActivating[i] = -1;
        }

        // Update Heal:
        HealthHealingMax = HealthHealingMin;

        // Update Attention:
        AttentionThisCharacterDied();

        // Update LightOrb:
        LightOrbEffects.CharacterDied(this);

        //Stop Active Coroutines and Sound
        StopAllCoroutines();
        GetComponent<AudioSource>().Stop();

        // Invoke death actions (e.g. Quest System)
        if (_onCharacterDeathAction != null)
            _onCharacterDeathAction.Invoke();

        // Deactivate Character:
        SwitchActiveStateCharacterFollowGUI(false);
        //gameObject.SetActive(false);
        //DyingPhysicsTimer = 0f;
        //DyingPhysicsTimerRunning = true;
        //GetComponent<Rigidbody>().isKinematic = true;
        DeathTimer.StartDeathTimer();
        gameObject.layer = DeadCharacterLayerID;

        // Disable NavMeshAgent:
        NavAgent.enabled = false;

        CameraController.Instance.GetCameraPositioner().UpdateCameraTargetsOnPlayerDeath(this.gameObject);
        LevelController.Instance.CheckIfAllDead();

        this.enabled = false;
    }

    /*public void UpdateDyingPhysics()
    {
        if (!DyingPhysicsTimerRunning)
        {
            return;
        }

        DyingPhysicsTimer += Time.deltaTime;

        if (DyingPhysicsTimer >= DyingPhysicsDuration)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            DyingPhysicsTimerRunning = false;
        }
    }*/

    // =================================== SKILL ACTIVATION ====================================

    protected override void UpdateCurrentSkillActivation()
    {
        for (int i = 0; i < SkillCurrentlyActivating.Length; i++)
        {
            if (SkillCurrentlyActivating[i] >= 0)
            {
                //ItemSkillSlots[SkillCurrentlyActivating[i]].UpdateSkillActivation(SkillActivationButtonsPressed[SkillCurrentlyActivating[i]]);
                ItemSkillSlots[SkillCurrentlyActivating[i]].UpdateSkillActivation(SkillActivationButtonsPressed[HoldDownButtonID]); // Should be replaced with check based on current skill position in combo tree if there is time.
            }
        }
    }

    public override void FinishedCurrentSkillActivation(int WeaponSlotID, int Hindrance)
    {
        for (int i = 0; i < Hands.Length; i++)
        {
            if (Hands[i])
            {
                Hands[i].ResetTriggers();
            }
        }
        base.FinishedCurrentSkillActivation(WeaponSlotID, Hindrance);
    }

    // =================================== /SKILL ACTIVATION ====================================

    // =================================== RESPAWNING ====================================

    public void RespawnNearestCharacter()
    {
        List<Character> PlayersInAttentionRange = CharAttention.GetPlayersInAttentionRange();

        Character ClosestDeadPlayer = null;
        float ClosestDeadPlayerDistance = RespawnMaxRange + 10;
        float CurrentDistance = 0;

        for (int i = 0; i < PlayersInAttentionRange.Count; i++)
        {
            if (PlayersInAttentionRange[i] && PlayersInAttentionRange[i].gameObject.layer == DeadCharacterLayerID)
            {
                CurrentDistance = Vector3.Distance(transform.position, PlayersInAttentionRange[i].transform.position);

                if (CurrentDistance < ClosestDeadPlayerDistance 
                    && CurrentDistance >= RespawnMinRange 
                    && CurrentDistance <= RespawnMaxRange)
                {
                    ClosestDeadPlayerDistance = CurrentDistance;
                    ClosestDeadPlayer = PlayersInAttentionRange[i];
                }
                
            }
        }

        if (!ClosestDeadPlayer)
        {
            return;
        }

        ChangeHealthCurrent(Mathf.Max(- 1* (HealthCurrent - 1), -1 * Mathf.RoundToInt(GetHealthMax() * RespawnHealthCostPerc)));
        ((CharacterPlayer)(ClosestDeadPlayer)).RespawnThisCharacter(RespawnHealthGainPerc);
    }

    public void RespawnThisCharacter(float SpawnHealthPercentage)
    {
        transform.rotation = Quaternion.Euler(Vector3.forward);
       // DyingPhysicsTimerRunning = false;
        SetHealthCurrent(Mathf.RoundToInt(GetHealthMax() * SpawnHealthPercentage));
        gameObject.layer = CharacterLayerID;
        CharAttention.OwnerPlayerRespawned();
        SwitchActiveStateCharacterFollowGUI(true);
        if (GUIChar)
        {
            GUIChar.UpdateHealthHealingBar(GetHealthHealingPercentage());
            GUIChar.UpdateHealthBar(GetHealthCurrentPercentage());
        }
        MovementRateModfier = 1f;
        HindranceLevel = 0;
        CharacterIsDead = false;
        ResetAnimations();
        DeathTimer.enabled = false;
        IsWalking = false;
        CameraController.Instance.GetCameraPositioner().UpdateCameraTargetsOnPlayerRespawn(this.gameObject);
        //GetComponent<Rigidbody>().isKinematic = false;
        LightOrbEffects.CharacterRevived(this);
        NavAgent.enabled = true;
        this.enabled = true;
    }

    public void RespawnThisCharacter()
    {
        RespawnThisCharacter(RespawnHealthGainPerc);
    }

    public void RespawnThisCharacter(Vector3 AtPosition)
    {
        RespawnThisCharacter();
        NavAgent.Warp(AtPosition);
    }

    private void ResetAnimations()
    {
        for (int i = 0; i < HandAnimators.Length; i++)
        {
            for (int j = 0; j < HandAnimators[i].parameters.Length; j++)
            {
                HandAnimators[i].ResetTrigger(HandAnimators[i].parameters[j].name);
            }

            HandAnimators[i].SetTrigger(DeathAnimationResetTrigger);
        }

        for (int j = 0; j < BodyAnimator.parameters.Length; j++)
        {
            BodyAnimator.ResetTrigger(BodyAnimator.parameters[j].name);
        }

        BodyAnimator.SetTrigger(DeathAnimationResetTrigger);
    }

    public void SetLightOrbEffects(LightOrbEffects NewLightOrbEffects)
    {
        LightOrbEffects = NewLightOrbEffects;
    }

    // =================================== !RESPAWNING ====================================

    // =================================== ITEM PICKUP ====================================

    private bool PickUpClosestItem()
    {
        Collider[] EquipPoints = Physics.OverlapSphere(transform.position, 3);

        for (int i = 0; i < EquipPoints.Length; i++)
        {
            if (EquipPoints[i] && EquipPoints[i].tag == "EquipPoint")
            {
                EquipPoints[i].GetComponent<EquipPoint>().TriggerEquip(PlayerID);
            }
        }

        return false;
        /*if (ItemsInRange.Count == 0)
        {
            return false;
        }

        int EquipSlotID = 0;
        /*
        if (Input.GetButtonDown("W1Skill1_" + PlayerID)
            && SkillCurrentlyActivating[0] < 0)
        {
            EquipSlotID = 0;
        }
        else if (Input.GetButtonDown("W2Skill1_" + PlayerID)
            && SkillCurrentlyActivating[1] < 0)
        {
            EquipSlotID = 1;
        }
        else
        {
            return false;
        }
        *//*
        Item ClosestItem = ItemsInRange[0];
        float ClosestDistance = Vector3.Distance(this.transform.position, ClosestItem.transform.position);
        float CurrentDistance = ClosestDistance;
        List<Item> ItemsOfOtherPlayers = new List<Item>(); // Items that other players picked up.
        bool LegitItem = false;

        for (int i = 0; i < ItemsInRange.Count; i++)
        {
            if (ItemsInRange[i].GetOwner() != null)
            {
                ItemsOfOtherPlayers.Add(ItemsInRange[i]);
            }
            else
            {
                CurrentDistance = Vector3.Distance(this.transform.position, ItemsInRange[i].transform.position);
                if (CurrentDistance <= ClosestDistance)
                {
                    ClosestDistance = CurrentDistance;
                    ClosestItem = ItemsInRange[i];
                    if (!LegitItem)
                    {
                        LegitItem = true;
                    }
                }
            }
        }

        for (int i = 0; i < ItemsOfOtherPlayers.Count; i++)
        {
            ItemsInRange.Remove(ItemsOfOtherPlayers[i]);
        }

        if (!LegitItem)
        {
            return false;
        }

        ClosestItem.EquipItem(this, EquipSlotID);
        ItemsInRange.Remove(ClosestItem);


        if (_itemPickupAction != null)
            _itemPickupAction.Invoke();

        return true;*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Weapon"
            && other.gameObject.GetComponent<Item>().GetOwner() == null)
        {
            ItemsInRange.Add(other.gameObject.GetComponent<Item>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Weapon"
            && other.gameObject.GetComponent<Item>().GetOwner() == null)
        {
            ItemsInRange.Remove(other.gameObject.GetComponent<Item>());
        }
    }

    // =================================== /ITEM PICKUP ====================================

    // ========================================= INPUT =========================================

    public void PlayerInput()
    {
        // Skill Activation Buttons:
        if (Input.GetButtonDown("W1Skill1_" + PlayerID))
        {
            SkillActivationButtonsPressed[0] = true;
        }
        else if (Input.GetButtonUp("W1Skill1_" + PlayerID))
        {
            SkillActivationButtonsPressed[0] = false;
        }

        if (Input.GetAxis("W1Skill2_" + PlayerID) >= 0.3f)
        {
            SkillActivationButtonsPressed[1] = true;
        }
        else if (Input.GetAxis("W1Skill2_" + PlayerID) < 0.3f)
        {
            SkillActivationButtonsPressed[1] = false;
        }

        if (Input.GetButtonDown("W2Skill1_" + PlayerID))
        {
            SkillActivationButtonsPressed[2] = true;
        }
        else if (Input.GetButtonUp("W2Skill1_" + PlayerID))
        {
            SkillActivationButtonsPressed[2] = false;
        }

        if (Input.GetAxis("W2Skill2_" + PlayerID) >= 0.3f)
        {
            SkillActivationButtonsPressed[3] = true;
        }
        else if (Input.GetAxis("W2Skill2_" + PlayerID) < 0.3f)
        {
            SkillActivationButtonsPressed[3] = false;
        }

       /* // Respawning Players:
        if (Input.GetButtonDown("RevivePlayer_" + PlayerID))
        {
            RespawnNearestCharacter();
        }*/


        // Weapon PickUp:
        if (Input.GetButton("IPickUp_" + PlayerID))
        {
            if (PickUpClosestItem())
            {
                return;
            }
        }

        // Light Orb Interaction:
        UpdatePlayerOrbInput();

        // Skill Activation:
        PlayerInputStartSkillActivation();
    }

    private void UpdatePlayerOrbInput()
    {
        // Light Orb Interaction:
        if (Input.GetButton("RevivePlayer_" + PlayerID))
        {
            // Fresh button press (Timer not running):
            if (OrbInputTimer <= 0)
            {
                OrbInputButtonPressed = true;
                OrbInputTimer = 0;
            }
            // Press Button again after pressing it shortly before (Timer still running):
            else if (!OrbInputButtonPressed)
            {
                if (OrbInputTimer <= OrbInputHealMaxTime)
                {
                    LightOrbEffects.ActivateOrbHeal(this);
                }
                OrbInputTimer = -1;
            }    
        }
        else if (OrbInputButtonPressed)
        {
            OrbInputButtonPressed = false;
        }

        if (OrbInputTimer >= 0)
        {
            OrbInputTimer += Time.deltaTime;

            if (OrbInputTimer >= OrbInputReviveTime)
            {
                if (OrbInputButtonPressed)
                {
                    LightOrbEffects.ActivateOrbRevive(this);
                }
                OrbInputTimer = -1;
            }
        }
    }

    private void PlayerInputStartSkillActivation()
    {/*
        if (SkillCurrentlyActivating[0] < 0)
        {
            if (SkillActivationButtonsPressed[0])
            {
                // Try starting Activation of Skill 1 from Weapon 1
                StartSkillActivation(0);
            }
            else if (SkillActivationButtonsPressed[1])
            {
                // Try starting Activation of Skill 2 from Weapon 1
                StartSkillActivation(1);
            }
        }

        if (SkillCurrentlyActivating[1] < 0)
        {
            if (SkillActivationButtonsPressed[2])
            {
                // Try starting Activation of Skill 1 from Weapon 2
                StartSkillActivation(2);
            }
            else if (SkillActivationButtonsPressed[3])
            {
                // Try starting Activation of Skill 2 from Weapon 2
                StartSkillActivation(3);
            }
        }   */
        
        if (SkillCurrentlyActivating[0] >= 0)
        {
            // A Skill is currently Activating!
            return;
        }

        if (LastSkillActivated < 0)
        {
            for (int i = 0; i < SkillActivationButtonsPressed.Length; i++)
            {
                if (SkillActivationButtonsPressed[i] && WeaponSlots[0] && WeaponSlots[0].GetItemSkillComboStart(i) >= 0)
                {
                    StartSkillActivation(WeaponSlots[0].GetItemSkillComboStart(i));
                    return;
                }
            }
        }
        else
        {
            for (int i = 0; i < SkillActivationButtonsPressed.Length; i++)
            {
                if (SkillActivationButtonsPressed[i] && ItemSkillSlots[LastSkillActivated].GetItemSkillIDFromComboInput(i) >= 0)
                {
                    StartSkillActivation(ItemSkillSlots[LastSkillActivated].GetItemSkillIDFromComboInput(i));
                    return;
                }
            }
        }
    }

    public void SetPlayerID(int ID)
    {
        PlayerID = ID;
    }

    public int GetPlayerID()
    {
        return PlayerID;
    }

    public int GetCurrentItemSkillIDForInput()
    {
        if (SkillCurrentlyActivating[0] >= 0)
        {
            return SkillCurrentlyActivating[0];
        }

        return LastSkillActivated;
    }

    public override void UpdateLastSkillActivated()
    {
        if (LastSkillActivatedTimer > 0)
        {
            LastSkillActivatedTimer -= Time.deltaTime;

            if (LastSkillActivatedTimer <= 0)
            {
                HandAnimators[0].SetTrigger(Anim_BreakAnim);
                LastSkillActivated = -1;
            }
        }
    }

    // ======================================== /INPUT =========================================

    // ======================================== EVENT =========================================

    public void SubscribeItemPickupAction(UnityAction action)
    {
        _itemPickupAction += action;
    }

    public void ClearItemPickupActions()
    {
        _itemPickupAction = null;
    }

    // ======================================== /EVENT =========================================
}
