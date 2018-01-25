using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestController : Singleton<QuestController>
{
    public readonly Queue<QuestBase> QuestsQueue = new Queue<QuestBase>();
    public QuestBase CurrentQuest { get; private set; }

    public Image QuestImage;
    public Text QuestTitleHUDText;
    public Text QuestDescriptionHUDText;

    public Camera MainCamera;
    public AudioClip VictoryAudioClip;
    public Light Sun;

    private AudioSource _cameraNextQuestAudioSource;
    private AudioSource _cameraAmbienceAudioSource;
    private AudioSource _cameraSpecialAudioSource;

    private Coroutine _currentBlink;

    protected override void Awake()
    {
        base.Awake();
        _cameraAmbienceAudioSource = MainCamera.GetComponents<AudioSource>()[0];
        _cameraSpecialAudioSource = MainCamera.GetComponents<AudioSource>()[1];
        _cameraNextQuestAudioSource = MainCamera.GetComponents<AudioSource>()[2];
    }

    public void AddQuest(QuestBase quest)
    {
        QuestsQueue.Enqueue(quest);
    }

    public void StartQuests()
    {
        if (CurrentQuest == null)
            NextQuest();
    }

    //Starts next quest in the queue. If there is none, end the game
    private void NextQuest()
    {
        if (_currentBlink != null)
            StopCoroutine(_currentBlink);
        _cameraNextQuestAudioSource.Play();
        _currentBlink = StartCoroutine(BlinkQuest());

        if (QuestsQueue.Count <= 0)
        {
            QuestTitleHUDText.text = "YOU WIN!";
            QuestDescriptionHUDText.text = "Congratulations";
            FadeAudioToSpecial(2f, VictoryAudioClip);
            StartCoroutine(VictoryEnding());
        }
        else
        {
            CurrentQuest = QuestsQueue.Dequeue();
            QuestTitleHUDText.text = CurrentQuest.Title;
            QuestDescriptionHUDText.text = CurrentQuest.Description;
            CurrentQuest.QuestEndAction += NextQuest;


            CurrentQuest.OnQuestStarted(); //Start quest
        }
    }

    public void ClearQuests()
    {
        CurrentQuest = null;
        QuestsQueue.Clear();
        QuestTitleHUDText.text = "TITLE";
        QuestDescriptionHUDText.text = "DESCRIPTION";
    }

    private IEnumerator VictoryEnding()
    {
        const float step = 1 / 3f;
        while (Sun.intensity < 20)
        {
            Sun.intensity += step * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        LevelController.Instance.FinalizeLevelWithWait(10);
    }

    public void FadeAudioToSpecial(float seconds, AudioClip audioClip)
    {
        if (audioClip)
            StartCoroutine(FadeAudioToSpecialCoroutine(seconds, audioClip));
    }

    private IEnumerator FadeAudioToSpecialCoroutine(float seconds, AudioClip audioClip)
    {
        var step = 1 / seconds;
        _cameraSpecialAudioSource.clip = audioClip;
        _cameraSpecialAudioSource.volume = 0;
        _cameraSpecialAudioSource.Play();

        while (_cameraSpecialAudioSource.volume < 1f)
        {
            var amount = step * Time.deltaTime;
            _cameraSpecialAudioSource.volume += amount;
            _cameraAmbienceAudioSource.volume -= amount;
            yield return new WaitForEndOfFrame();
        }

        _cameraSpecialAudioSource.volume = 1;
        _cameraAmbienceAudioSource.volume = 0;
        _cameraAmbienceAudioSource.Stop();
    }

    public void FadeAudioToAmbience(float seconds)
    {
        StartCoroutine(FadeAudioToAmbienceCoroutine(seconds));
    }

    private IEnumerator FadeAudioToAmbienceCoroutine(float seconds)
    {
        var step = 1 / seconds;

        _cameraAmbienceAudioSource.volume = 0;
        _cameraAmbienceAudioSource.Play();
        while (_cameraAmbienceAudioSource.volume < 1f)
        {
            var amount = step * Time.deltaTime;
            _cameraAmbienceAudioSource.volume += amount;
            _cameraSpecialAudioSource.volume -= amount;
            yield return new WaitForEndOfFrame();
        }

        _cameraAmbienceAudioSource.volume = 1;
        _cameraSpecialAudioSource.volume = 0;
        _cameraSpecialAudioSource.Stop();
    }

    private IEnumerator BlinkQuest()
    {
        var baseColor = QuestImage.color;
        var newColor = new Color(197/255f, 164/255f, 114/255f, 90/255f);
        for (var i = 0; i < 4; i++)
        {
            var lerpValue = 0f;
            var frequency = 1.5f;
            while (lerpValue < 1)
            {
                QuestImage.color = Color.Lerp(baseColor, newColor, lerpValue);
                lerpValue += Time.deltaTime * frequency;
                yield return new WaitForEndOfFrame();
            }
            lerpValue = 1;
            while (lerpValue > 0)
            {
                QuestImage.color = Color.Lerp(baseColor, newColor, lerpValue);
                lerpValue -= Time.deltaTime * frequency;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
