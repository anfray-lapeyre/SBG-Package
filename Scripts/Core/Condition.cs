using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Core
{
    [System.Serializable]
    public class Condition
    {
        [SerializeField] string predicate;
        [SerializeField] string[] parameters;

        /// <summary>
        /// A condition can be used in a good number of ways, it uses a complex predicate and parameters, that it will send to evaluators in order to return a boolean	
        /// </summary>
        /// <param name="_predicate">The predicate is a string that marks the logic that we want to apply </param>
        /// <param name="_parameters">The parameters are the values that will be the inputs of the predicate</param>
        public Condition(string _predicate, string[] _parameters)
        {
            predicate = _predicate;
            if(_parameters != null)
            {
                parameters = _parameters.Clone() as string[];
            }
        }

        /// <summary>
        /// Returns the predicate
        /// </summary>
        public string GetPredicate()
        {
            return predicate;
        }

        /// <summary>
        /// Returns the entire array of parameters
        /// </summary>
        /// <returns></returns>
        public string[] GetParameters()
        {
            return parameters;
        }



        /// <summary>
        /// This function will, for every PredicateEvaluator it knows, ask for the PredicateHelp to Evaluate the condition with the evaluator's system and will return a bool.
        /// </summary>
        /// <param name="evaluators"></param>
        /// <returns>Returns true if the condition is filled, or if no evaluator has been found that is capable of assessing this condition.
        /// Returns false if an evaluator is capable of assessing the condition and it was false.</returns>
        public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
        {
            foreach (var evaluator in evaluators)
            {
                bool? result = PredicateHelper.Evaluate(predicate, evaluator);
                if (result == null)
                {
                    continue;
                }

                if (result == false)
                {
                    return false;
                }
                if(result == true)
                {
                    return true;
                }
            }
            return true;
        }




    }

}