using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Quests.UI
{
    public class QuestListUI : MonoBehaviour
    {
        [SerializeField] QuestItemUI questPrefab;
        QuestList list= null;
        private void Start()
        {
            list = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            list.onUpdate += Redraw;
            Redraw();
        }

        private void Redraw()
        {
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            foreach (QuestStatus status in list.GetStatuses())
            {
                QuestItemUI uiInstance = Instantiate<QuestItemUI>(questPrefab, transform);
                uiInstance.Setup(status);
            }
        }
    }
}
