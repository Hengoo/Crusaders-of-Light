using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : Singleton<GameController>
{

    public int Seed { get; private set; }
    public float Brightness { get; private set; }
    public GameStateEnum GameState = GameStateEnum.Menu;
    public int ActivePlayers = 4;

	// Use this for initialization
    protected override void Awake () {
		base.Awake();
        DontDestroyOnLoad(gameObject);
	}
    
    public void SetSeed(int value)
    {
        if (GameState != GameStateEnum.Menu) return;

        Seed = value;
        Random.InitState(value);
    }

    public void SetBrightness(float value)
    {
        Brightness = value;
    }

    public void InitializeGameSession()
    {
        //TODO: create real game map
        SceneManager.LoadScene("TerrainGenerationTest");
        GameState = GameStateEnum.Play;
    }

    public void FinalizeGameSession()
    {
        GameState = GameStateEnum.Menu;
    }
}


public enum GameStateEnum
{
    Play,
    Pause,
    Menu
}
