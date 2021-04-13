using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SaltButter.Quests
{
    public class QuestGiver : MonoBehaviour
    {
        [SerializeField] string quest;

        public void GiveQuest()
        {
            QuestList list = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            list.AddQuest(Quest.GetByName(quest), quest);
        }

    }
}
