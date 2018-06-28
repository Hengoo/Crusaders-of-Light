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
    public MenuLivePreview MenuPreview;

    public InputField MaxNumberSwarmlings;

    void Start()
    {
        var players = GameController.Instance.ActivePlayers;
        var brightness = GameController.Instance.Brightness;

        Brightness.value = brightness;
        ActivePlayers.text = players.ToString();
        MenuPreview.UpdateBrightness(Brightness.value);
        MenuPreview.UpdatePlayerCount(players.ToString());
    }

    public void OnStartButton()
    {
        if (Seed.textComponent.text.Length > 0)
            GameController.Instance.SetSeed(int.Parse(Seed.textComponent.text));
        else
        {
            Random.InitState(System.DateTime.Now.GetHashCode());
            GameController.Instance.SetSeed(Random.Range(0, int.MaxValue));
        }

        if (MaxNumberSwarmlings.text.Length > 0)
        {
            GameController.Instance.SetMaxNumberSwarmlings(int.Parse(MaxNumberSwarmlings.text));
        }

        MainMenu.enabled = false;
        Instructions.enabled = true;

        GameController.Instance.SetActivePlayers(int.Parse(ActivePlayers.textComponent.text));
        GameController.Instance.SetBrightness(Brightness.normalizedValue);

        StartCoroutine(FadeMusicOutAndLoad(5, 5));
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    private IEnumerator FadeMusicOutAndLoad(float musicFadeTime, float additionalTime)
    {
        while (Music.volume > 0)
        {
            Music.volume -= 1/musicFadeTime * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(additionalTime);
        GameController.Instance.InitializeGameSession();
    }
}
