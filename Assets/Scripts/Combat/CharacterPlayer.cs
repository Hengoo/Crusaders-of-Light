using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPlayer : Character {

    [Header("Team Alignment:")]
    public TeamAlignment Alignment = TeamAlignment.PLAYERS;

    void Update()
    {
        PlayerInput();
        UpdateCurrentSkillActivation();
    }

    public override TeamAlignment GetAlignment()
    {
        return Alignment;
    }

    // ========================================= INPUT =========================================

    public void PlayerInput()
    {
        // TODO : Match the SkillActivationButtonsPressed[] to Controller Shoulder Buttons depending on Player Count/ID.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SkillActivationButtonsPressed[0] = true;
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            SkillActivationButtonsPressed[0] = false;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SkillActivationButtonsPressed[1] = true;
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            SkillActivationButtonsPressed[1] = false;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SkillActivationButtonsPressed[2] = true;
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            SkillActivationButtonsPressed[2] = false;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SkillActivationButtonsPressed[3] = true;
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            SkillActivationButtonsPressed[3] = false;
        }

        if (SkillCurrentlyActivating < 0)
        {
            PlayerInputStartSkillActivation();
        }

        // Only for quick testing, remove later:
        if (Input.GetKeyDown(KeyCode.U))
        {
            this.gameObject.transform.Translate(Vector3.forward);
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
