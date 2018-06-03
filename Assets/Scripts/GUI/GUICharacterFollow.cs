using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUICharacterFollow : MonoBehaviour {

    public RectTransform RectTrans;

    Character Character;

    public Image HealthBarCurrent;
    public Image HealthHealingBarCurrent;

    public Sprite HealthBarPlayerSprite;

    public void Initialize(Character Char)
    {
        Character = Char;

        if (Character.GetAlignment() == Character.TeamAlignment.PLAYERS)
        {
            HealthBarCurrent.sprite = HealthBarPlayerSprite;
        }
        else
        {
            HealthHealingBarCurrent.enabled = false;
            gameObject.SetActive(false);
        }
    }

    public void UpdateHealthBar(float NormalizedHealthValue)
    {
        HealthBarCurrent.fillAmount = NormalizedHealthValue;
    }

    public void UpdateHealthHealingBar(float NormalizedHealthHealingValue)
    {
        HealthHealingBarCurrent.fillAmount = NormalizedHealthHealingValue;
    }

    public void UpdateGUIPosition()
    {
        // Scale:
        float DistanceTest = (Vector3.Distance(CameraController.Instance.gameObject.transform.position, Character.transform.position))/40f;

        float DistanceValue = Mathf.Pow((DistanceTest -1.25f), 4) * (0.75f) + 0.25f;
        RectTrans.localScale = new Vector3(DistanceValue, DistanceValue, DistanceValue);
     
        // Position:
        Vector3 ViewPortPos = CameraController.Instance.GetCamera().WorldToViewportPoint(Character.transform.position);

        RectTransform CanvasRectTrans = GUIController.Instance.GetCanvasRectTrans();

        Vector3 WorldObjectScreenPos = new Vector3((ViewPortPos.x * CanvasRectTrans.sizeDelta.x) - (CanvasRectTrans.sizeDelta.x * 0.5f),
                                                    (ViewPortPos.y * CanvasRectTrans.sizeDelta.y) - (CanvasRectTrans.sizeDelta.y * 0.45f),
                                                    (ViewPortPos.z));
        RectTrans.localPosition = WorldObjectScreenPos;
    }

    public void SwitchGUIActive(bool state)
    {
        gameObject.SetActive(state);
    }

    // Only Destroys the GUI, Character might still have a reference:
    public void DestroyGUICharacterFollow()
    {
        Destroy(this.gameObject);
    }
}
