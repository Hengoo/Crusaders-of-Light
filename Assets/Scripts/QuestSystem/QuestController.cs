using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestController : Singleton<QuestController>
{
    public readonly Queue<QuestBase> QuestsQueue = new Queue<QuestBase>();
    public QuestBase CurrentQuest { get; private set; }

    public Text QuestTitleHUDText;
    public Text QuestDescriptionHUDText;

    public Camera MainCamera;

    private AudioSource _cameraNextQuestAudioSource;

    public void AddQuest(QuestBase quest)
    {
        QuestsQueue.Enqueue(quest);
        if(CurrentQuest == null)
            NextQuest();

        _cameraNextQuestAudioSource = MainCamera.GetComponents<AudioSource>()[1];
    }

    //Starts next quest in the queue. If there is none, end the game
    private void NextQuest()
    {
        _cameraNextQuestAudioSource.Play();
        if (QuestsQueue.Count <= 0)
        {
            QuestTitleHUDText.text = "YOU WIN!";
            QuestDescriptionHUDText.text = "Congratulations";
            LevelController.Instance.FinalizeLevelWithWait(10);
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

}
