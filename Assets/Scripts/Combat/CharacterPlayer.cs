using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPlayer : Character {

    [Header("Character Player:")]
    public bool[] SkillActivationButtonsPressed = new bool[4];  // Whether the Button is currently pressed down!

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.PLAYERS;

    [Header("Close Items List:")]
    public List<Item> ItemsInRange = new List<Item>();

    protected override void Start()
    {
        base.Start();
        SpawnAndEquipStartingWeapons();
    }

    protected override void Update()
    {
        PlayerInput();
        base.Update();
    }

    private void FixedUpdate()
    {
        float speedfaktor = 10 * GetMovementRateModifier();
        //left stick
        Vector3 targetVel = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * speedfaktor;

        //right stick
        Vector3 targetDir = new Vector3(Input.GetAxisRaw("Horizontal2"), 0, -Input.GetAxisRaw("Vertical2"));

        PhysCont.SetVelRot(targetVel, targetDir);
    }

    public override TeamAlignment GetAlignment()
    {
        return Alignment;
    }

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

    // =================================== ITEM PICKUP ====================================

    private bool PickUpClosestItem()
    {
        if (ItemsInRange.Count == 0)
        {
            return false;
        }

        int EquipSlotID = -1;

        if (SkillActivationButtonsPressed[0] || SkillActivationButtonsPressed[1])
        {
            EquipSlotID = 0;
        }
        else if (SkillActivationButtonsPressed[2] || SkillActivationButtonsPressed[3])
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
        if (Input.GetButtonDown("W1Skill1"))
        {
            SkillActivationButtonsPressed[0] = true;
        }
        else if (Input.GetButtonUp("W1Skill1"))
        {
            SkillActivationButtonsPressed[0] = false;
        }

        if (Input.GetAxis("W1Skill2") >= 0.3f)
        {
            SkillActivationButtonsPressed[1] = true;
        }
        else if (Input.GetAxis("W1Skill2") < 0.3f)
        {
            SkillActivationButtonsPressed[1] = false;
        }

        if (Input.GetButtonDown("W2Skill1"))
        {
            SkillActivationButtonsPressed[2] = true;
        }
        else if (Input.GetButtonUp("W2Skill1"))
        {
            SkillActivationButtonsPressed[2] = false;
        }

        if (Input.GetAxis("W2Skill2") >= 0.3f)
        {
            SkillActivationButtonsPressed[3] = true;
        }
        else if (Input.GetAxis("W2Skill2") < 0.3f)
        {
            SkillActivationButtonsPressed[3] = false;
        }

        // Weapon PickUp:
        if (Input.GetButtonDown("IPickUp"))
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

    // ======================================== /INPUT =========================================
}
