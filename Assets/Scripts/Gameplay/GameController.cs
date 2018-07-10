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

    public Item PlayerBaseWeapon;
    public ElementItem PlayerBaseElement;

    public int PlayerStartHealth = 20000;
    public float SwarmlingHealthFactor = 1;

    [Header("Player Unlocks:")]
    public bool[] PlayerDataUnlockedWeapons = new bool[3];
    public bool[] PlayerDataUnlockedElements = new bool[3];

    private bool[] PlayerDataUnlockedWeaponsBase = new bool[3];
    private bool[] PlayerDataUnlockedElementsBase = new bool[3];

    [Header("Max Swarmling Spawn Number:")]
    public int MaxNumberOfSwarmlings = 100;

    [Header("Difficulty:")]
    public float DifficultyFactor = 1;
    public float DifficultyFactorCurrentLevel = 1;
    public float DifficultyFactorModPerArena = 0.1f;
    public float DifficultyFactorModPerLevel = 0.2f;

    public string LastPlayedBiome;

    // Use this for initialization
    protected override void Awake () {
		base.Awake();
        DontDestroyOnLoad(gameObject);


        for (int i = 0; i < PlayerDataUnlockedElements.Length; i++)
        {
            PlayerDataUnlockedElementsBase[i] = PlayerDataUnlockedElements[i];
        }

        for (int i = 0; i < PlayerDataUnlockedWeapons.Length; i++)
        {
            PlayerDataUnlockedWeaponsBase[i] = PlayerDataUnlockedWeapons[i];
        }
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

        // Update Difficulty for Playernumber:
        DifficultyCalculateStartDiffForPlayerNumber(ActivePlayers);
    }

    public void LoadTransitionArea()
    {
        // Reset Values:
        // Technically this is also called when going from the menu to the transitionArea. 
        // This should not be a problem, as the function should only be used to reset values.
        ReturenedToTransitionByReset(); 

        // Change Scene:
        GameState = GameStateEnum.Transition;
        SceneManager.LoadScene("TransitionArea");
    }

    public void FinalizeGameSession()
    {
        // Reset Values:
        DifficultyRestartedGame();

        for (int i = 0; i < PlayerDataUnlockedElements.Length; i++)
        {
            PlayerDataUnlockedElements[i] = PlayerDataUnlockedElementsBase[i];
        }

        for (int i = 0; i < PlayerDataUnlockedWeapons.Length; i++)
        {
            PlayerDataUnlockedWeapons[i] = PlayerDataUnlockedWeaponsBase[i];
        }

        for (int i = 0; i < PlayerDataSelectedWeapons.Length; i++)
        {
            PlayerDataSelectedWeapons[i] = null;
        }

        for (int i = 0; i < PlayerDataSelectedElements.Length; i++)
        {
            PlayerDataSelectedElements[i] = null;
        }

        // Change Scene:
        GameState = GameStateEnum.Menu;
        SceneManager.LoadScene("Menu2");
    }

    public void ReturnedToTransitionByPortal()
    {
        DifficultyFinishedLevel();
    }

    public void ReturenedToTransitionByReset()
    {
        DifficultyRestartedLevel();
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
        if (PlayerDataSelectedWeapons[id - 1])
        {
            return PlayerDataSelectedWeapons[id - 1];
        }

        if (GameState == GameStateEnum.Level)
        {
            return PlayerBaseWeapon;
        }
        return null;
    }

    public ElementItem GetPlayerElement(int id)
    {
        if (PlayerDataSelectedElements[id - 1])
        {
            return PlayerDataSelectedElements[id - 1];
        }

        if (GameState == GameStateEnum.Level)
        {
            return PlayerBaseElement;
        }
        return null;
    }

    public void UnlockWeapon(Weapon weaponToUnlock)
    {
        if (GameState != GameStateEnum.Level)
        {
            return;
        }

        int tempID = weaponToUnlock.GetWeaponID();
        if (tempID >= 0 && tempID < PlayerDataUnlockedWeapons.Length)
        {
            PlayerDataUnlockedWeapons[tempID] = true;
        }
    }

    public void UnlockElement(ElementItem elementToUnlock)
    {
        if (GameState != GameStateEnum.Level)
        {
            return;
        }

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

    public void SetPlayerStartHealth(int NewStartHealth)
    {
        PlayerStartHealth = NewStartHealth;
    }

    public int GetPlayerStartHealth()
    {
        return PlayerStartHealth;
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

    public void SetSwarmlingHealthFactor(float NewFactor)
    {
        SwarmlingHealthFactor = NewFactor;
    }

    public float GetSwarmlingHealthFactor()
    {
        return SwarmlingHealthFactor;
    }

    // ======================================/  Spawner Data  /=====================================


    // ========================================  Difficulty  ========================================

    public void DifficultyCalculateStartDiffForPlayerNumber(int PlayerNumber)
    {
        DifficultyFactor = 1 + PlayerNumber * 0.3f;
        DifficultyFactorCurrentLevel = DifficultyFactor;
    }

    public float GetCurrentDifficultyFactor()
    {
        return DifficultyFactorCurrentLevel;
    }

    public void DifficultyFinishedArena() // Difficulty increase after each arena.
    {
        DifficultyFactorCurrentLevel += DifficultyFactorModPerArena;
    }

    public void DifficultyFinishedLevel() // Difficulty increase after each beaten level.
    {
        DifficultyFactorCurrentLevel += DifficultyFactorModPerLevel;
        DifficultyFactor = DifficultyFactorCurrentLevel;
    }

    public void DifficultyRestartedLevel() // Reset difficulty to that at the start of current level if players restart level.
    {
        DifficultyFactorCurrentLevel = DifficultyFactor;
    }

    public void DifficultyRestartedGame() // Reset difficulty to base value if whole game is restarted.
    {
        DifficultyFactor = 1;
        DifficultyFactorCurrentLevel = DifficultyFactor;
    }

    // =======================================/  Difficulty  /=======================================


}
