using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MenuController : MonoBehaviour
{
    //Menus
    public Canvas MainMenu;
    public Canvas Instructions;

    public InputField ActivePlayers;
    public InputField Seed;
    public Slider Brightness;

    public AudioSource Music;

    public void OnStartButton()
    {
        if (Seed.textComponent.text.Length > 0)
            GameController.Instance.SetSeed(int.Parse(Seed.textComponent.text));
        else
        {
            Random.InitState(System.DateTime.Now.GetHashCode());
            GameController.Instance.SetSeed(Random.Range(0, int.MaxValue));
        }

        MainMenu.enabled = false;
        Instructions.enabled = true;
        StartCoroutine(FadeMusicOut());

        GameController.Instance.SetActivePlayers(int.Parse(ActivePlayers.textComponent.text));
        GameController.Instance.SetBrightness(Brightness.normalizedValue);
        GameController.Instance.InitializeGameSession();
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    private IEnumerator FadeMusicOut()
    {
        while (Music.volume > 0)
        {
            Music.volume -= 1/3f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
