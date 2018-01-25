using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class LevelController : Singleton<LevelController>
{
    public QuestController QuestController;

    public CharacterPlayer[] PlayerCharacters;

    public SceneryStructure SceneryStructure;
    public Terrain Terrain;

    public Canvas Instructions;

    public Canvas Intro;
    public Image IntroBlackImage;
    public Image IntroImage;

    public Canvas Outro;
    public Image OutroDeadImage;
    public Image OutroVictoryImage;

    public Camera MainCamera;

    public bool SkipIntro = true;

    private float _quitTimer = 0;
    private bool _loadingLevel = false;
    private bool _allDead = false;

    void Start()
    {
        //Deactivate inactive players
        for (int i = GameController.Instance.ActivePlayers; i < PlayerCharacters.Length; i++)
            Destroy(PlayerCharacters[i].gameObject);

#if UNITY_EDITOR
        if (SkipIntro) return;
#endif

        StartCoroutine(DisplayIntro(10));
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
        QuestController.ClearQuests();
    }

    public void FinalizeLevel()
    {
        _loadingLevel = true;
        GameController.Instance.FinalizeGameSession();
    }

    private IEnumerator DisplayIntro(float seconds)
    {
        Intro.enabled = true;
        IntroImage.color = new Color(IntroImage.color.r, IntroImage.color.g, IntroImage.color.b, 0);
        var text = IntroImage.GetComponentInChildren<Text>();
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);

        yield return new WaitForSeconds(3);

        const float step = 1 / 5f;
        while (IntroImage.color.a < 1)
        {
            var currentColour = IntroImage.color;
            var currentTextColour = text.color;
            var value = step * Time.deltaTime;
            currentColour += new Color(0, 0, 0, value);
            currentTextColour += new Color(0, 0, 0, value);
            IntroImage.color = currentColour;
            text.color = currentTextColour;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(seconds);

        while (IntroImage.color.a > 0)
        {
            var currentColour = IntroImage.color;
            var currentTextColour = text.color;
            var value = step * Time.deltaTime;
            currentColour -= new Color(0, 0, 0, value);
            currentTextColour -= new Color(0, 0, 0, value);
            IntroImage.color = currentColour;
            text.color = currentTextColour;
            yield return new WaitForEndOfFrame();
        }

        while (IntroBlackImage.color.a > 0)
        {
            var currentColour = IntroBlackImage.color;
            currentColour -= new Color(0, 0, 0, step * Time.deltaTime);
            IntroBlackImage.color = currentColour;
            yield return new WaitForEndOfFrame();
        }

        Intro.enabled = false;
    }


    public void FinalizeLevelWithWait(float seconds)
    {
        StartCoroutine(WaitAndFinalize(seconds));
    }

    private IEnumerator WaitAndFinalize(float seconds)
    {
        Outro.enabled = true;
        OutroDeadImage.color = new Color(OutroDeadImage.color.r, OutroDeadImage.color.g, OutroDeadImage.color.b, 0);
        OutroVictoryImage.color = new Color(OutroDeadImage.color.r, OutroDeadImage.color.g, OutroDeadImage.color.b, 0);
        OutroDeadImage.GetComponentInChildren<Text>().color -= new Color(0, 0, 0, 1);
        OutroVictoryImage.GetComponentInChildren<Text>().color -= new Color(0, 0, 0, 1);

        QuestController.Instance.FadeAudioToAmbience(3);

        const float step = 1 / 5f;
        if (_allDead)
        {
            var text = OutroDeadImage.GetComponentInChildren<Text>();
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
            while (OutroDeadImage.color.a < 1f)
            {
                var currentColour = OutroDeadImage.color;
                var currentTextColour = text.color;
                var value = step * Time.deltaTime;
                currentColour += new Color(0, 0, 0, value);
                currentTextColour += new Color(0, 0, 0, value);
                OutroDeadImage.color = currentColour;
                text.color = currentTextColour;
                yield return new WaitForEndOfFrame();
            }

        }
        else
        {
            var text = OutroVictoryImage.GetComponentInChildren<Text>();
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
            while (OutroVictoryImage.color.a < 1f)
            {
                var currentColour = OutroVictoryImage.color;
                var currentTextColour = text.color;
                var value = step * Time.deltaTime;
                currentColour += new Color(0, 0, 0, value);
                currentTextColour += new Color(0, 0, 0, value);
                OutroVictoryImage.color = currentColour;
                text.color = currentTextColour;
                yield return new WaitForEndOfFrame();
            }

        }
        yield return new WaitForSeconds(seconds);
        GameController.Instance.FinalizeGameSession();
    }

    public void CheckIfAllDead()
    {
        for (var i = 0; i < GameController.Instance.ActivePlayers; i++)
            if (!PlayerCharacters[i].GetCharacterIsDead())
                return;

        _allDead = true;
        StartCoroutine(WaitAndFinalize(10));
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
