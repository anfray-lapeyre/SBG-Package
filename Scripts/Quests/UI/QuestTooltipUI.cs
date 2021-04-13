using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaltButter.Quests.UI
{
    public class QuestTooltipUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] GameObject whereToSpawnPrefab;
        [SerializeField] GameObject objectivePrefab;
        [SerializeField] TextMeshProUGUI reward;
        // Start is called before the first frame update
        public void Setup(QuestStatus status)
        {
            foreach (Transform child in whereToSpawnPrefab.transform)
            {
                Destroy(child.gameObject);
            }

            if (status == null || status.GetQuest() ==null)
                return;

            Quest quest = status.GetQuest();
            title.text = quest.GetTitle();
            
            for (int i = 0; i < quest.GetObjectiveCount(); i++)
            {
                GameObject instance = Instantiate(objectivePrefab, whereToSpawnPrefab.transform);
                TextMeshProUGUI text = instance.GetComponentInChildren<TextMeshProUGUI>();
                text.text = quest.GetObjective(i).description;
                if (!status.isCompleted(quest.GetObjective(i).reference))
                {
                    instance.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                }
            }
            reward.text = quest.GetReward();
        }

    }
}
