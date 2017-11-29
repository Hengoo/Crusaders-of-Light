using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPlayer : Character {

    [Header("Character Player:")]
    public bool[] SkillActivationButtonsPressed = new bool[4];  // Whether the Button is currently pressed down!

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.PLAYERS;

    protected override void Update()
    {
        PlayerInput();
        base.Update();
    }

    private void FixedUpdate()
    {
        float speedfaktor = 10;
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
        if (SkillCurrentlyActivating < 0) { return; }

        SkillActivationTimer += Time.deltaTime;

        ItemSkillSlots[SkillCurrentlyActivating].UpdateSkillActivation(SkillActivationTimer, SkillActivationButtonsPressed[SkillCurrentlyActivating]);
    }

    // =================================== /SKILL ACTIVATION ====================================

    // ========================================= INPUT =========================================

    public void PlayerInput()
    {
        // TODO : Match the SkillActivationButtonsPressed[] to Controller Shoulder Buttons depending on Player NodeCount/ID.
        if (Input.GetKeyDown(KeyCode.T))
        {
            SkillActivationButtonsPressed[0] = true;
        }
        else if (Input.GetKeyUp(KeyCode.T))
        {
            SkillActivationButtonsPressed[0] = false;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SkillActivationButtonsPressed[1] = true;
        }
        else if (Input.GetKeyUp(KeyCode.Z))
        {
            SkillActivationButtonsPressed[1] = false;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            SkillActivationButtonsPressed[2] = true;
        }
        else if (Input.GetKeyUp(KeyCode.U))
        {
            SkillActivationButtonsPressed[2] = false;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            SkillActivationButtonsPressed[3] = true;
        }
        else if (Input.GetKeyUp(KeyCode.I))
        {
            SkillActivationButtonsPressed[3] = false;
        }

        if (SkillCurrentlyActivating < 0)
        {
            PlayerInputStartSkillActivation();
        }
    }

    private void PlayerInputStartSkillActivation()
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
        else if (SkillActivationButtonsPressed[2])
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

    // ======================================== /INPUT =========================================
}
