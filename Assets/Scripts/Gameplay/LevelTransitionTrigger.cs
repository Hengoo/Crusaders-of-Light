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
                if(GameController.Instance.GameState == GameStateEnum.Transition)
                {
                    GameController.Instance.GameState = GameStateEnum.Level;
                    SceneManager.LoadScene("TerrainGeneration");
                }
                else
                {
                    GameController.Instance.GameState = GameStateEnum.Transition;
                    GameController.Instance.SetSeed(Random.Range(int.MinValue, int.MaxValue));
                    SceneManager.LoadScene("TransitionArea");
                }
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
