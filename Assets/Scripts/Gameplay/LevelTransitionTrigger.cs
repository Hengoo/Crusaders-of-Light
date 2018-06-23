using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionTrigger : MonoBehaviour
{
    private readonly HashSet<CharacterPlayer> _players = new HashSet<CharacterPlayer>();
    private int _count = 0;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            _players.Add(col.GetComponent<CharacterPlayer>());
            _count = _players.Count;
            if (_players.Count >= GameController.Instance.ActivePlayers)
            {
                SceneManager.LoadScene(GameController.Instance.GameState == GameStateEnum.Transition
                    ? "TerrainGeneration"
                    : "TransitionArea");
                GameController.Instance.GameState = GameController.Instance.GameState == GameStateEnum.Transition
                    ? GameStateEnum.Level
                    : GameStateEnum.Transition;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            _count = _players.Count;
            _players.Remove(col.GetComponent<CharacterPlayer>());
        }
    }
}
