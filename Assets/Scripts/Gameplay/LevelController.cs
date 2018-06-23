using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class LevelController : Singleton<LevelController>
{
    public LevelCreator LevelCreator;

    public CharacterPlayer[] PlayerCharacters;
    public GameObject LightWisp;
    public SwarmSpawner SwarmlingSpawner;

    public Canvas Instructions;

    public Camera MainCamera;

    public bool SkipIntro = true;

    private float _quitTimer = 0;
    private bool _loadingLevel = false;

    void Start()
    {
        InitializeLevel();
        LevelCreator.CreateGameLevel();
        StartLevel();
    }

    void Update()
    {
        //Intructions show/hide
        if (Input.GetButtonDown("Back"))
            Instructions.enabled = true;
        if (Input.GetButtonUp("Back"))
            Instructions.enabled = false;

        //Quit button
        if (Input.GetKey(KeyCode.Q))
            _quitTimer += Time.deltaTime;
        if (Input.GetKeyUp(KeyCode.Q))
            _quitTimer = 0;
        if (_quitTimer > 2 && !_loadingLevel)
            FinalizeLevel();
    }

    public void InitializeLevel()
    {
        //Deactivate all controllers
        foreach (var player in PlayerCharacters)
            player.gameObject.SetActive(false);

        //Initialize Light Wisp
        if (LightWisp && LightWisp.GetComponent<LightOrbEffects>())
        {
            LightWisp.GetComponent<LightOrbEffects>().InitializeLightOrb(PlayerCharacters, GameController.Instance.ActivePlayers);
        }

        //Set reference to terrain;
        SwarmlingSpawner.SetTerrain(LevelCreator.Terrain);

        //Initialize SwarmSpawner
        if (SwarmlingSpawner)
            SwarmlingSpawner.InitializeSwarmSpawner(PlayerCharacters, GameController.Instance.ActivePlayers);
    }

    public void FinalizeLevel()
    {
        _loadingLevel = true;
        GameController.Instance.FinalizeGameSession();
    }

    public bool CheckIfAllDead()
    {
        for (var i = 0; i < GameController.Instance.ActivePlayers; i++)
            if (!PlayerCharacters[i].GetCharacterIsDead())
                return false;
        
        return true;
    }

    public void StartLevel()
    {
        var terrain = LevelCreator.Terrain;
        var startPosition2D = LevelCreator.StartPostion;
        for (var i = 0; i < GameController.Instance.ActivePlayers; i++)
        {
            Vector3 spawnPosition = new Vector3(startPosition2D.x, 0, startPosition2D.y);
            switch (i)
            {
                case 0:
                    spawnPosition += new Vector3(3, 0, 0);
                    break;
                case 1:
                    spawnPosition += new Vector3(0, 0, 3);
                    break;
                case 2:
                    spawnPosition += new Vector3(-3, 0, 0);
                    break;
                case 3:
                    spawnPosition += new Vector3(0, 0, -3);
                    break;
            }
            spawnPosition = new Vector3(spawnPosition.x, terrain.SampleHeight(spawnPosition) + 0.05f, spawnPosition.z);
            PlayerCharacters[i].transform.position = spawnPosition;
        }
        Vector3 wispPosition = new Vector3(startPosition2D.x, 0, startPosition2D.y);
        wispPosition += new Vector3(0, terrain.SampleHeight(wispPosition) + 0.05f, 0);
        LightWisp.GetComponent<NavMeshAgent>().Warp(wispPosition);

        var cameraPosition = new Vector3(startPosition2D.x, 0, startPosition2D.y);
        MainCamera.gameObject.transform.position = cameraPosition + new Vector3(0, terrain.SampleHeight(cameraPosition) + 20, 0);

        //Reactivate all controllers
        for (var i = 0; i < GameController.Instance.ActivePlayers; i++)
            PlayerCharacters[i].gameObject.SetActive(true);
    }

    public GameObject[] GetActivePlayers()
    {
        var result = new GameObject[GameController.Instance.ActivePlayers];
        //var result = new GameObject[PlayerCharacters.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = PlayerCharacters[i].gameObject;

        return result;
    }
}
