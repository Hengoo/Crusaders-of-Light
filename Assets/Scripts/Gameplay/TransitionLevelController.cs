using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransitionLevelController : Singleton<TransitionLevelController> {

    public CharacterPlayer[] PlayerCharacters;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        //Deactivate all controllers
        foreach (var player in PlayerCharacters)
            player.gameObject.SetActive(false);

        for (int i = 0; i < GameController.Instance.ActivePlayers; i++)
        {
            PlayerCharacters[i].gameObject.SetActive(true);
        }
    }

    public GameObject[] GetActivePlayers()
    {
        return PlayerCharacters.Take(GameController.Instance.ActivePlayers).Select(e => e.gameObject).ToArray();
    }
}
