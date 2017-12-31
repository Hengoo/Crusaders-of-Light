using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MenuController : MonoBehaviour
{
    //Menus
    public Canvas MainMenu;


    public InputField Seed;
    public Slider Brightness;

    public void OnStartButton()
    {
        if (Seed.textComponent.text.Length > 0)
            GameController.Instance.SetSeed(int.Parse(Seed.textComponent.text));
        else
        {
            Random.InitState(System.DateTime.Now.GetHashCode());
            GameController.Instance.SetSeed(Random.Range(0, int.MaxValue));
        }

        GameController.Instance.SetBrightness(Brightness.normalizedValue);
        GameController.Instance.InitializeGameSession();
    }

    public void OnExitButton()
    {
        Application.Quit();
    }
}
