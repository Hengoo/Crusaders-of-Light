using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightOrbEffects : MonoBehaviour {

    public List<CharacterPlayer> PlayerCharacters = new List<CharacterPlayer>();

    private List<CharacterPlayer> DeadPlayerCharacters = new List<CharacterPlayer>();

    [Header("Revive:")]
    public float ReviveMaxDistance = 5f;

    public float ReviveCooldown = 5f;
    private float ReviveCooldownCounter = -1f;

    [Header("Heal:")]
    public float HealMaxActivationDistance = 5f;
    public float HealEffectRange = 6f;
    public float HealPercentage = 0.25f;

    public float HealCooldown = 5f;
    private float HealCooldownCounter = -1f;

    private void Update()
    {
        UpdateReviveCooldown();
        UpdateHealCooldown();
    }

    public void AddPlayerCharacter(CharacterPlayer NewChar)
    {
        PlayerCharacters.Add(NewChar);
    }

    public void CharacterDied(CharacterPlayer DeadChar)
    {
        DeadPlayerCharacters.Add(DeadChar);
    }

    public void CharacterRevived(CharacterPlayer RevivedChar)
    {
        DeadPlayerCharacters.Remove(RevivedChar);
    }

    public void InitializeLightOrb(CharacterPlayer[] ActivePlayers, int NumberActivePlayers)
    {
        for (int i = 0; i < NumberActivePlayers; i++)
        {
            if (ActivePlayers[i])
            {
                AddPlayerCharacter(ActivePlayers[i]);
            }
        }
    }

    // =============================================== EFFECT: REVIVE ===============================================

    private void OrbEffectRevive()
    {
        CharacterPlayer ClosestDeadPlayer = null;
        float ClosestDeadPlayerDistance = -1;
        float CurrentDistance = -2;

        for (int i = 0; i < DeadPlayerCharacters.Count; i++)
        {
            if (DeadPlayerCharacters[i].gameObject.layer == CharacterPlayer.DeadCharacterLayerID)
            {
                CurrentDistance = Vector3.Distance(transform.position, DeadPlayerCharacters[i].transform.position);

                if (CurrentDistance < ClosestDeadPlayerDistance || ClosestDeadPlayerDistance < 0)
                {
                    ClosestDeadPlayerDistance = CurrentDistance;
                    ClosestDeadPlayer = DeadPlayerCharacters[i];
                }
            }
        }

        if (!ClosestDeadPlayer)
        {
            return;
        }

        //ClosestDeadPlayer.ChangeHealthCurrent(Mathf.Max(-1 * (HealthCurrent - 1), -1 * Mathf.RoundToInt(GetHealthMax() * RespawnHealthCostPerc)));
        ClosestDeadPlayer.RespawnThisCharacter(transform.position);
    }
	
    public void ActivateOrbRevive(CharacterPlayer ActivatingChar)
    {
        if (IsReviveOnCooldown())
        {
            Debug.Log("Activate Orb Revive is on Cooldown!");
            return;
        }
        Debug.Log("Called: Activate Orb Revive!");

        if (Vector3.SqrMagnitude(transform.position - ActivatingChar.transform.position) > Mathf.Pow(ReviveMaxDistance, 2))
        {
            return;
        }

        StartReviveCooldown();
        OrbEffectRevive();
    }

    private void UpdateReviveCooldown()
    {
        if (ReviveCooldownCounter > 0)
        {
            ReviveCooldownCounter -= Time.deltaTime;
        }
    }

    private void StartReviveCooldown()
    {
        ReviveCooldownCounter = ReviveCooldown;
    }

    private bool IsReviveOnCooldown()
    {
        if (ReviveCooldownCounter > 0)
        {
            return true;
        }
        return false;
    }

    // ==============================================/ EFFECT: REVIVE /==============================================



    // ================================================= EFFECT: HEAL ================================================

    private void OrbEffectHeal()
    {
        for (int i = 0; i < PlayerCharacters.Count; i++)
        {
            if (PlayerCharacters[i].gameObject.layer == CharacterPlayer.CharacterLayerID
                && Vector3.SqrMagnitude(transform.position - PlayerCharacters[i].transform.position) > Mathf.Pow(HealEffectRange, 2))
            {
                PlayerCharacters[i].Heal(PlayerCharacters[i].GetHealthPercentageAbsoluteValue(HealPercentage));
            }
        }
    }

    public void ActivateOrbHeal(CharacterPlayer ActivatingChar)
    {
        if (IsHealOnCooldown())
        {
            Debug.Log("Activate Orb Heal is on Cooldown!");
            return;
        }
        Debug.Log("Called: Activate Orb Heal!");

        if (Vector3.SqrMagnitude(transform.position - ActivatingChar.transform.position) > Mathf.Pow(HealMaxActivationDistance, 2))
        {
            return;
        }

        StartHealCooldown();
        OrbEffectHeal();
    }

    private void UpdateHealCooldown()
    {
        if (HealCooldownCounter > 0)
        {
            HealCooldownCounter -= Time.deltaTime;
        }
    }

    private void StartHealCooldown()
    {
        HealCooldownCounter = HealCooldown;
    }

    private bool IsHealOnCooldown()
    {
        if (HealCooldownCounter > 0)
        {
            return true;
        }
        return false;
    }

    // ================================================/ EFFECT: HEAL /===============================================
}
