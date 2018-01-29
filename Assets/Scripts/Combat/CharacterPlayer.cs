using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterPlayer : Character {

    [Header("Character Player:")]
    public bool[] SkillActivationButtonsPressed = new bool[4];  // Whether the Button is currently pressed down!

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.PLAYERS;

    [Header("Close Items List:")]
    public List<Item> ItemsInRange = new List<Item>();

    [Header("Input:")]
    public int PlayerID = -1; // Set from 1 to 4!

    [Header("Respawning:")]
    public CharacterDeathTimer DeathTimer;

    public int CharacterLayerID = 8;
    public int DeadCharacterLayerID = 12;

    public float RespawnMinRange = 2f;
    public float RespawnMaxRange = 5f;
    
    public float RespawnHealthCostPerc = 0.2f;
    public float RespawnHealthGainPerc = 0.15f;

    public string DeathAnimationResetTrigger = "Trigger_DeathReset";

 //   public float DyingPhysicsDuration = 0.8f;
 //   public bool DyingPhysicsTimerRunning = false;
 //   public float DyingPhysicsTimer = 0f;

    protected override void Start()
    {
        base.Start();
        SpawnAndEquipStartingWeapons();
    }

    private UnityAction _itemPickupAction;

    protected override void Update()
    {
        PlayerInput();
        base.Update();
    }

    private void FixedUpdate()
    {
        float speedfaktor = 10 * GetMovementRateModifier();
        //left stick
        Vector3 targetVel = new Vector3(Input.GetAxisRaw("Horizontal_" + PlayerID), 0, Input.GetAxisRaw("Vertical_" + PlayerID));

        //right stick
        Vector3 targetDir = new Vector3(Input.GetAxisRaw("Horizontal2_" + PlayerID), 0, -Input.GetAxisRaw("Vertical2_" + PlayerID));

        if (Vector3.Magnitude(targetDir) <= 0.3f)
        {
            targetDir = targetVel;
        }

        targetVel *= speedfaktor;

        PhysCont.SetVelRot(targetVel, targetDir);

        if (!IsWalking && (Input.GetAxisRaw("Horizontal_" + PlayerID) >= 0.05f 
            || Input.GetAxisRaw("Vertical_" + PlayerID) >= 0.05f
            || Input.GetAxisRaw("Horizontal_" + PlayerID) <= -0.05f
            || Input.GetAxisRaw("Vertical_" + PlayerID) <= -0.05f))
        {
            IsWalking = true;
            StartBodyAnimation(Anim_StartWalking);
        }
        else if (IsWalking)
        {
            if (Input.GetAxisRaw("Horizontal_" + PlayerID) < 0.05f
            && Input.GetAxisRaw("Vertical_" + PlayerID) < 0.05f
            && Input.GetAxisRaw("Horizontal_" + PlayerID) > -0.05f
            && Input.GetAxisRaw("Vertical_" + PlayerID) > -0.05f)
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

        // Invoke death actions (e.g. Quest System)
        if (_onCharacterDeathAction != null)
            _onCharacterDeathAction.Invoke();

        // Deactivate Character:
        SwitchActiveStateCharacterFollowGUI(false);
        //gameObject.SetActive(false);
        //DyingPhysicsTimer = 0f;
        // DyingPhysicsTimerRunning = true;
        //GetComponent<Rigidbody>().isKinematic = true;
        DeathTimer.StartDeathTimer();
        gameObject.layer = DeadCharacterLayerID;

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
                ItemSkillSlots[SkillCurrentlyActivating[i]].UpdateSkillActivation(SkillActivationButtonsPressed[SkillCurrentlyActivating[i]]);
            }
        }
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
        GUIChar.UpdateHealthHealingBar(GetHealthHealingPercentage());
        GUIChar.UpdateHealthBar(GetHealthCurrentPercentage());
        MovementRateModfier = 1f;
        HindranceLevel = 0;
        CharacterIsDead = false;
        ResetAnimations();
        DeathTimer.enabled = false;
        IsWalking = false;
        CameraController.Instance.GetCameraPositioner().UpdateCameraTargetsOnPlayerRespawn(this.gameObject);
        GetComponent<Rigidbody>().isKinematic = false;
        this.enabled = true;
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

    // =================================== !RESPAWNING ====================================

    // =================================== ITEM PICKUP ====================================

    private bool PickUpClosestItem()
    {
        if (ItemsInRange.Count == 0)
        {
            return false;
        }

        int EquipSlotID = -1;

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

        Item ClosestItem = ItemsInRange[0];
        float ClosestDistance = Vector3.Distance(this.transform.position, ClosestItem.transform.position);
        float CurrentDistance = ClosestDistance;

        for (int i = 1; i < ItemsInRange.Count; i++)
        {
            CurrentDistance = Vector3.Distance(this.transform.position, ItemsInRange[i].transform.position);
            if (CurrentDistance < ClosestDistance)
            {
                ClosestDistance = CurrentDistance;
                ClosestItem = ItemsInRange[i];
            }
        }

        ClosestItem.EquipItem(this, EquipSlotID);
        ItemsInRange.Remove(ClosestItem);


        if (_itemPickupAction != null)
            _itemPickupAction.Invoke();

        return true;
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

        // Respawning Players:
        if (Input.GetButtonDown("RevivePlayer_" + PlayerID))
        {
            RespawnNearestCharacter();
        }


        // Weapon PickUp:
        if (Input.GetButton("IPickUp_" + PlayerID))
        {
            if (PickUpClosestItem())
            {
                return;
            }
        }

        // Skill Activation:
        PlayerInputStartSkillActivation();
    }

    private void PlayerInputStartSkillActivation()
    {
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
