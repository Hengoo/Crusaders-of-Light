using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;

public class LevelController : Singleton<LevelController>
{
    public List<AreaBase> AreaPuzzlePool; //Pool of quests for an entire area available from start
    public List<AreaBase> QuestBossPool; //Pool of quests for the last area, aka Boss Area

    public QuestController QuestController;

    public CharacterPlayer[] PlayerCharacters;

    public SceneryStructure SceneryStructure;
    public Terrain Terrain;
    public Canvas Instructions;

    public Camera MainCamera;

    private float _quitTimer = 0;
    private bool _loadingLevel = false;

    void Start()
    {
        //Deactivate inactive players
        for (int i = GameController.Instance.ActivePlayers; i < PlayerCharacters.Length; i++)
            Destroy(PlayerCharacters[i].gameObject);
    }

    void Update()
    {
        //Intructions show/hide
        if(Input.GetButtonDown("Back"))
            Instructions.enabled = true;
        if(Input.GetButtonUp("Back"))
            Instructions.enabled = false;

        //Quit button
        if (Input.GetKey(KeyCode.Q))
            _quitTimer += Time.deltaTime;
        if (Input.GetKeyUp(KeyCode.Q))
            _quitTimer = 0;
        if(_quitTimer > 2 && !_loadingLevel)
            FinalizeLevel();
    }
    
    public void InitializeLevel()
    {
        QuestController.ClearQuests();
    }

    public void FinalizeLevel()
    {
        _loadingLevel = true;
        GameController.Instance.FinalizeGameSession();
    }


    public void FinalizeLevelWithWait(float seconds)
    {
        StartCoroutine(WaitAndFinalize(seconds));
    }

    private IEnumerator WaitAndFinalize(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GameController.Instance.FinalizeGameSession();
    }

    public void CheckIfAllDead()
    {
        for(var i = 0; i < GameController.Instance.ActivePlayers; i++)
            if (!PlayerCharacters[i].GetCharacterIsDead())
                return;

        StartCoroutine(WaitAndFinalize(5));
    }

    public void StartGame(SceneryStructure sceneryStructure, Terrain terrain)
    {
        SceneryStructure = sceneryStructure;
        Terrain = terrain;
        
        var terrainStructure = SceneryStructure.TerrainStructure;
        var startPosition2D = terrainStructure.BiomeGraph.GetNodeData(terrainStructure.StartBiomeNode.Value).Center;
        for (var i = 0; i < PlayerCharacters.Length; i++)
        {
            Vector3 spawnPosition = new Vector3(startPosition2D.x, 0, startPosition2D.y);
            switch (i)
            {
                case 0:
                    spawnPosition += new Vector3(3,0,0);
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
            spawnPosition = new Vector3(spawnPosition.x, Terrain.SampleHeight(spawnPosition) + 0.05f, spawnPosition.z);
            PlayerCharacters[i].transform.position = spawnPosition;
        }
        var cameraPosition = new Vector3(startPosition2D.x, 0, startPosition2D.y);
        MainCamera.gameObject.transform.position = cameraPosition + new Vector3(0, Terrain.SampleHeight(cameraPosition) + 20, 0);
        QuestController.Instance.StartQuests();
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
