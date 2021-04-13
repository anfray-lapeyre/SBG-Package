using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SaltButter.Quests.UI
{
    public class QuestItemUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] TextMeshProUGUI progress;
        private QuestStatus currentQuest = null;
        public void Setup(QuestStatus status)
        {
            Debug.Log(status.GetQuest());
            text.text = status.GetQuest().GetTitle();
            progress.text = status.GetCompletedObjectivesCount()+"/" + status.GetQuest().GetObjectiveCount();
            currentQuest = status;
        }

        public QuestStatus GetQuestStatus()
        {
            return currentQuest;
        }
    }
}
