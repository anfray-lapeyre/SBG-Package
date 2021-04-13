using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Quests
{
    public class QuestCompletion : MonoBehaviour
    {
        [SerializeField] string quest;
        [SerializeField] string objective;

        public void CompleteObjective()
        {
            QuestList list = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            list.CompleteObjective(Quest.GetByName(quest), objective);
        }
    }
}
