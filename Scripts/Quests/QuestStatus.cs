using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Quests { 
    public class QuestStatus
    {
        Quest quest;
        List<string> completedObjectives = new List<string>();
        string localizationTerm;

        [Serializable]
        class QuestStatusRecord
        {
            string name;
            List<string> completedObjectives;

            public QuestStatusRecord(string _name, List<string> _completedObjectives)
            {
                this.name = _name;
                this.completedObjectives = new List<string>(_completedObjectives);
            }

            public string GetName()
            {
                return this.name;
            }

            public List<string> getCompletedObjectives()
            {
                return this.completedObjectives;
            }
        }

        public QuestStatus(Quest _quest, string _term)
        {
            quest = _quest;
            localizationTerm = _term;
        }

        public QuestStatus(object obj)
        {
            QuestStatusRecord record = obj as QuestStatusRecord;
            if (record == null)
                return;
            quest = Quest.GetByName(record.GetName());
            localizationTerm = record.GetName();
            completedObjectives = new List<string>(record.getCompletedObjectives());
            
        }

        public bool IsComplete()
        {
            return GetCompletedObjectivesCount() == quest.GetObjectiveCount();
        }

        public object CaptureState()
        {
            return new QuestStatusRecord(localizationTerm, completedObjectives) as object;
        }

        public void CompleteObjective(string objective)
        {
            if(objective != string.Empty && !completedObjectives.Contains(objective))
            {
                completedObjectives.Add(objective);
            }
        }

        public Quest GetQuest()
        {
            return quest;
        }

        public int GetCompletedObjectivesCount()
        {
            return completedObjectives.Count;
        }

        public bool isCompleted(string objective)
        {
            return completedObjectives.Contains(objective);
        }
    }
}
