using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionTrigger : MonoBehaviour
{
    private readonly HashSet<int> _players = new HashSet<int>();

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            var player = col.GetComponent<CharacterPlayer>();
            if (!player)
                return;

            _players.Add(col.GetComponent<CharacterPlayer>().PlayerID);
            if (_players.Count >= GameController.Instance.ActivePlayers)
            {
                if (GameController.Instance.GameState == GameStateEnum.Transition)
                {
                    StartCoroutine(LevelAsync());
                }
                else
                {
                    StartCoroutine(TransitionAsync());
                }
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            _players.Remove(col.GetComponent<CharacterPlayer>().PlayerID);
        }
    }

    IEnumerator LevelAsync()
    {
        GameController.Instance.GameState = GameStateEnum.Level;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TerrainGeneration");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    IEnumerator TransitionAsync()
    {
        GameController.Instance.GameState = GameStateEnum.Transition;
        GameController.Instance.SetSeed(Random.Range(int.MinValue, int.MaxValue));
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TransitionArea");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
