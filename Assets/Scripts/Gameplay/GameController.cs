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

    // This is a quick hack for the presentation, should be done better afterwards!!!!!:
    public Item[] PlayerStartWeapons = new Item[4];
    public ElementItem[] PlayerStartElements = new ElementItem[4];

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

    // This is a quick hack for the presentation, should be done better afterwards!!!!!:
    public void SetPlayerItem(int id, Item item)
    {
        PlayerStartWeapons[id-1] = item;
    }

    public void SetPlayerElement(int id, ElementItem element)
    {
        PlayerStartElements[id-1] = element;
    }

    public Item GetPlayerItem(int id)
    {
        return PlayerStartWeapons[id-1];
    }

    public ElementItem GetPlayerElement(int id)
    {
        return PlayerStartElements[id-1];
    }
}
