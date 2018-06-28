using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStateEnum
{
    Level,
    Transition,
    Menu
}

public class GameController : Singleton<GameController>
{
    public int Seed { get; private set; }
    public float Brightness { get; private set; }

    public GameStateEnum GameState = GameStateEnum.Menu;
    public int ActivePlayers = 4;

    [Header("Player Data:")]
    public Item[] PlayerDataSelectedWeapons = new Item[4];
    public ElementItem[] PlayerDataSelectedElements = new ElementItem[4];

    [Header("Player Unlocks:")]
    public bool[] PlayerDataUnlockedWeapons = new bool[3];
    public bool[] PlayerDataUnlockedElements = new bool[3];

    [Header("Max Swarmling Spawn Number:")]
    public int MaxNumberOfSwarmlings = 100;

    public string LastPlayedBiome;

    // Use this for initialization
    protected override void Awake () {
		base.Awake();
        DontDestroyOnLoad(gameObject);
	}
    
    public void SetSeed(int value)
    {
        Seed = value;
    }

    public void SetBrightness(float value)
    {
        Brightness = value;
    }

    public void SetActivePlayers(int value)
    {
        ActivePlayers = Mathf.Clamp(value, 1, 4);
    }

    public void InitializeGameSession()
    {
        GameState = GameStateEnum.Transition;
        SceneManager.LoadScene("TransitionArea");
    }

    public void FinalizeGameSession()
    {
        GameState = GameStateEnum.Menu;
        SceneManager.LoadScene("Menu");
    }


    // =======================================  Player Data  =======================================

    // id-1, because Player IDs are from 1-4, not 0-3:
    public void SetPlayerItem(int id, Item item)
    {
        PlayerDataSelectedWeapons[id-1] = item;
    }

    public void SetPlayerElement(int id, ElementItem element)
    {
        PlayerDataSelectedElements[id-1] = element;
    }

    public Item GetPlayerItem(int id)
    {
        return PlayerDataSelectedWeapons[id-1];
    }

    public ElementItem GetPlayerElement(int id)
    {
        return PlayerDataSelectedElements[id-1];
    }

    public void UnlockWeapon(Weapon weaponToUnlock)
    {
        int tempID = weaponToUnlock.GetWeaponID();
        if (tempID >= 0 && tempID < PlayerDataUnlockedWeapons.Length)
        {
            PlayerDataUnlockedWeapons[tempID] = true;
        }
    }

    public void UnlockElement(ElementItem elementToUnlock)
    {
        int tempID = elementToUnlock.GetElementID();
        if (tempID >= 0 && tempID < PlayerDataUnlockedElements.Length)
        {
            PlayerDataUnlockedElements[tempID] = true;
        }
    }

    public bool CheckIfWeaponUnlocked(Weapon weaponToCheck)
    {
        int tempID = weaponToCheck.GetWeaponID();
        if (tempID >= 0 && tempID < PlayerDataUnlockedWeapons.Length)
        {
            return PlayerDataUnlockedWeapons[tempID];
        }
        return false;
    }

    public bool CheckIfElementUnlocked(ElementItem elementToCheck)
    {
        int tempID = elementToCheck.GetElementID();
        if (tempID >= 0 && tempID < PlayerDataUnlockedElements.Length)
        {
            return PlayerDataUnlockedElements[tempID];
        }
        return false;
    }

    public void UnlockEverything()
    {
        for (int i = 0; i < PlayerDataUnlockedWeapons.Length; i++)
        {
            PlayerDataUnlockedWeapons[i] = true;
        }

        for (int i = 0; i < PlayerDataUnlockedElements.Length; i++)
        {
            PlayerDataUnlockedElements[i] = true;
        }
    }

    // ======================================/  Player Data  /======================================


    // =======================================  Spawner Data  ======================================

    public int GetMaxNumberSwarmlings()
    {
        return MaxNumberOfSwarmlings;
    }

    public void SetMaxNumberSwarmlings(int NewNumber)
    {
        MaxNumberOfSwarmlings = NewNumber;
    }

    // ======================================/  Spawner Data  /=====================================
}
