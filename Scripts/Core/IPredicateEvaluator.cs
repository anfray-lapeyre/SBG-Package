using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Core
{
    public interface IPredicateEvaluator
    {
        /// <summary>
        /// This interface enables Conditions to be treated by any object that inherits this interface, by overriding HandleSinglePredicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        bool? HandleSinglePredicate(Condition predicate);

    }
}
