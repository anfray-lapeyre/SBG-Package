using SaltButter.Saving;
using SaltButter.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Quests
{
    public class QuestList : MonoBehaviour, ISaveable, IPredicateEvaluator
    {
        List<QuestStatus> statuses = new List<QuestStatus>();
        public event Action onUpdate;
        public event Action onComplete;
        QuestStatus lastFinishedQuest = null;
        public void AddQuest(Quest quest, string term)
        {
            if (quest == null || HasQuest(quest))
                return;
            
            statuses.Add( new QuestStatus(quest,term));
            if(onUpdate != null) onUpdate();
            
        }

        internal void CompleteObjective(Quest quest,string objective)
        {
            if(quest == null || objective == string.Empty || !quest.hasObjective(objective))
            {
                Debug.LogError("Quest null, objective null or quest does not have that objective.");

                return;
            }

            QuestStatus status = GetQuestStatus(quest);
            if(status == null)
            {
                Debug.LogError("Did not find appropriate quest");
                return;
            }

            status.CompleteObjective(objective);
            if (status.IsComplete())
            {
                lastFinishedQuest = status;
                if (onComplete != null) onComplete();
            }
            if (onUpdate != null) onUpdate();
        }

        public bool HasQuest(Quest quest)
        {
            return GetQuestStatus(quest)!=null;
        }

        private QuestStatus GetQuestStatus(Quest quest)
        {
            foreach (QuestStatus status in statuses)
            {
                if (status.GetQuest() == quest)
                {
                    return status;
                }
            }
            return null;
        }

        public IEnumerable<QuestStatus> GetStatuses()
        {
            return statuses;
        }

        public object CaptureState()
        {
            List<object> state = new List<object>();
            foreach(QuestStatus status in statuses)
            {
                state.Add(status.CaptureState());
            }
            return state;
        }

        public void RestoreState(object state)
        {
            List<object> statelist = state as List<object>;
            if(statelist == null)
            {
                return;
            }
            statuses.Clear();
            foreach(object obj in statelist)
            {
                statuses.Add(new QuestStatus(obj));
            }
        }


        public QuestStatus GetLastFinishedStatus()
        {
            return lastFinishedQuest;
        }

      

        public bool? HandleSinglePredicate(Condition condition)
        {
            bool? value = null;
            if (condition == null || condition.GetPredicate() == null)
                return null;
            string predicate = condition.GetPredicate();
            string[] parameters = condition.GetParameters();
            if (predicate == PredicateHelper.predicateType.HasQuest.ToString())
            {
                if (parameters == null || parameters.Length < 1)
                {
                    return value;
                }
                //We check only the first parameter
                value = false;

                return HasQuest(Quest.GetByName(parameters[0]));

            }
            else if (predicate == PredicateHelper.predicateType.HasNotQuest.ToString())
            {
                if (parameters == null || parameters.Length < 1)
                {
                    return value;
                }
                //We check only the first parameter
                value = false;
                return !HasQuest(Quest.GetByName(parameters[0]));

            }
            else if (predicate == PredicateHelper.predicateType.HasFinishedQuest.ToString())
            {

                if (parameters == null || parameters.Length < 1)
                {
                    return value;
                }
                //We check only the first parameter
                value = false;
                return GetQuestStatus(Quest.GetByName(parameters[0])).IsComplete();
            }
            else if (predicate == PredicateHelper.predicateType.HasNotFinishedQuest.ToString())
            {

                if (parameters == null || parameters.Length < 1)
                {
                    return value;
                }
                //We check only the first parameter
                value = false;
                return !GetQuestStatus(Quest.GetByName(parameters[0])).IsComplete();
            }
            else if (predicate == PredicateHelper.predicateType.IsObjectiveComplete.ToString())
            {
                if (parameters == null || parameters.Length < 2)
                {
                    return value;
                }
                //We check only the first parameter
                value = false;
                if (GetQuestStatus(Quest.GetByName(parameters[0])) == null)
                    return false;
                return GetQuestStatus(Quest.GetByName(parameters[0])).isCompleted(parameters[1]);
            }
            else if (predicate == PredicateHelper.predicateType.IsNotObjectiveComplete.ToString())
            {
                if (parameters == null || parameters.Length < 2)
                {
                    return value;
                }
                //We check only the first parameter
                value = false;
                if (GetQuestStatus(Quest.GetByName(parameters[0])) == null)
                    return true;
                return !GetQuestStatus(Quest.GetByName(parameters[0])).isCompleted(parameters[1]);
            }
            Debug.Log("We arrive here");

            return value;
        }
    }
}
