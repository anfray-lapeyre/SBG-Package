using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Quests
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest", order = 0)]
    public class Quest : ScriptableObject
    {
        [SerializeField] List<Objective> objectives = new List<Objective>();
        [SerializeField] string reward;
        
        [System.Serializable]
        public class Objective
        {
            public string reference;
            public string description;
        }

        public string GetTitle()
        {
            return name;
        }

        public bool hasObjective(string obj)
        {
           foreach(Objective objRef in objectives)
            {
                if(obj == objRef.reference)
                {
                    return true;
                }
            }
            return false;
        }
        public int GetObjectiveCount()
        {
            return objectives.Count;
        }

        public Objective GetObjective(int i)
        {
            return objectives[i];
        }

        public string GetReward()
        {
            return reward;
        }

        public static bool HasQuestByName(string questName)
        {
            return questName != "" && LocalizationManager.GetTermsList("Quest").Contains("Quest/" + questName);
            ;
        }

        public static Quest GetByName(string questName)
        {
            if(HasQuestByName(questName)){
                return LocalizationManager.GetTranslatedObjectByTermName<Quest>("Quest/" + questName) ;
            }
            return null;
        }
    }
}
