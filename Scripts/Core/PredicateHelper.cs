using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaltButter.Core {
    public static class PredicateHelper
    {
        /// <summary>
        /// Predicate Types are all the predicate that can be used in the Dialogue Conditions
        /// If you want to handle another situation, add a predicate to the Enum and the string[]. Make sure to keep the value in enum and string[] in order.
        /// </summary>
        public enum predicateType { HasQuest, HasFinishedQuest, IsObjectiveComplete, HasNotFinishedQuest, IsNotObjectiveComplete, HasNotQuest, HasUnlockedCharacter, HasPower, HowMuchCollectibles, HasFinishedLevel };
        public static readonly string[] predicateTypeString = { "HasQuest", "HasFinishedQuest", "IsObjectiveComplete", "HasNotFinishedQuest","IsNotObjectiveComplete","HasNotQuest", "HasUnlockedCharacter", "HasPower", "HowMuchCollectibles", "HasFinishedLevel" };





        /// <summary>
        /// Evaluate will parse the predicate to dissect it in simple conditions and handle any boolean operator.
        /// </summary>
        /// <param name="startPredicate">The complete predicate string. It might contain predicates, parameters for those predicates and boolean operators. Its format is as follow "HasQuest(FetchBread) || IsObjectiveComplete(FindObjects,Bread)"</param>
        /// <param name="evaluator">The evaluator is the script that will handle the single predicates. It is the one that will return true if the players has the quest FetchBread. </param>
        /// <returns>Returns null if anything in the startPredicate wasn't standard, true if all the conditions are matched for the predicate, false if the conditions aren't fulfilled (the player does not avec bread or the FetchBread quest)</returns>
        public static bool? Evaluate(string startPredicate, IPredicateEvaluator evaluator)
        {
            string predicate = startPredicate;
            //We handle if there is not predicate first
            if (String.IsNullOrEmpty(predicate))
                return null;
            bool? result = null;
            bool mustReverse = false;
            //First we need to remove spaces
            //That way we only have characters that matter
            predicate = predicate.Replace(" ", string.Empty);
            //We check then if it is a simple single parameter
            if (PredicateHelper.IsSinglePredicate(predicate))
            {
                //If so, we Evaluate after extracting everything
                return evaluator.HandleSinglePredicate(PredicateHelper.ExtractSinglePredicate(predicate));
            }
            //Most complex example we can get is
            //Predicate(param)&&!(!(Predicate()&&Predicate(param,param))||(Predicate(param)&&Predicate()))&&(Predicate(param)&&!Predicate(param,param,param))
            //If we manage to interpret something like this right, we can do anything
            //First, we need to define the order in which we treat the operators
            //Once all operators are treated, we will just have to sequence them right and handle them one by one


            //It is not a single predicate, so we first check if it is a predicate with a negation
            int i = PredicateHelper.GetNextOperatorIndex(predicate);
            char c = PredicateHelper.GetNextOperator(predicate);
            if (c == '!')
            {
                //There are three possible case !... && ... or !(...) or !...
                //!... && ... and !(...) are the same because it means there is another operator

                if (i != 0)
                    return null;

                predicate = predicate.Substring(1);

                //Now we need to check if we have a single predicate
                if (PredicateHelper.IsSinglePredicate(predicate))
                {
                    //If so we evaluate it and reverse the value
                    result = Evaluate(predicate, evaluator);
                    if (result != null)
                        return !(bool)result;
                    return null;
                }
                //If not
                //We need to check the nextOperator again
                i = PredicateHelper.GetNextOperatorIndex(predicate);
                c = PredicateHelper.GetNextOperator(predicate);
                //And set mustReverse as true
                //If so, the first element interpreted will be reversed
                mustReverse = true;
            }

            //If we reach here, we have something that is not a single predicate or a negation. We then have to check if it is a bracket
            if (c == '(')
            {
                //GetNextOperator makes sur that the bracket does not come from an operator
                //If we get a parenthesis as next operator, it then must be the first character
                if (i != 0)
                    return null;

                //We have now 2 possibilities
                //Either we simply have something like that (...) or we have something like that (...) && ...
                //Let's first treat the first case
                //For that we need to find the end bracket's index
                int j = PredicateHelper.FindEndBracket(predicate, i);

                if (j == -1)
                {
                    return null;
                }
                else if (j == predicate.Length - 1)
                {

                    //If we reach here, we know the brackets encapsulate all the predicate
                    //We can just remove them and call Evaluate again

                    return HandleReverse(Evaluate(predicate.Substring(1, predicate.Length - 2), evaluator), mustReverse);

                }

                //If we haven't returned, then we are in this situation (...) && ...
                //So we evaluate what's inside the brackets, and continue on to evaluate the rest
                result = HandleReverse(Evaluate(predicate.Substring(1, j - 1), evaluator), mustReverse);
                mustReverse = false;

                //We also must modify the string we are comparing
                predicate = predicate.Substring(j + 1);
                i = PredicateHelper.GetNextOperatorIndex(predicate);
                c = PredicateHelper.GetNextOperator(predicate);
            }

            if (c == '&')
            {
                //We have 2 use cases
                //Either we have a predicate that looks like that ... && ... or we have a predicate that looks like that && ...
                //The second one means that the first part of the predicate is already stored in result
                //We will handle the first case and reduce it down to a && ... case
                //For that, it is simple, we just have to evaluate everything that is before the '&'
                //And how do we know we are in a ... && ... case ? The '&' is not in place 0
                if (i != 0)
                {
                    result = HandleReverse(Evaluate(predicate.Substring(0, i), evaluator), mustReverse);
                    mustReverse = false;
                }

                //Case && ...
                bool? secondResult = HandleReverse(Evaluate(predicate.Substring(i + 2), evaluator), mustReverse);

                if (result == null || secondResult == null)
                    return null;
                //If they are not null, then we treat them as booleans
                return (bool)result && (bool)secondResult;

            }
            else if (c == '|')
            {
                //We have 2 use cases
                //Either we have a predicate that looks like that ... || ... or we have a predicate that looks like that && ...
                //The second one means that the first part of the predicate is already stored in result
                //We will handle the first case and reduce it down to a || ... case
                //For that, it is simple, we just have to evaluate everything that is before the '|'
                //And how do we know we are in a ... || ... case ? The '|' is not in place 0
                if (i != 0)
                {
                    result = HandleReverse(Evaluate(predicate.Substring(0, i), evaluator), mustReverse);
                    mustReverse = false;
                }

                //Case || ...
                bool? secondResult = HandleReverse(Evaluate(predicate.Substring(i + 2), evaluator), mustReverse);

                if (result == null || secondResult == null)
                    return null;
                //If they are not null, then we treat them as booleans
                return (bool)result || (bool)secondResult;
            }


            //If we reached here, it wasn't any case that we knew how to handle, so we return null
            return null;
        }



        /// <summary>
        /// This will handle the '!' operator
        /// </summary>
        /// <param name="v">Start boolean</param>
        /// <param name="mustReverse">If true, v will be reversed (if not null)</param>
        /// <returns>return null if v was null, if v isn't null, v will be reversed or not depending of mustReverse</returns>
        private static bool? HandleReverse(bool? v, bool mustReverse)
        {
            if (v == null || !mustReverse)
                return v;
            return !(bool)v;
        }




        /// <summary>
        /// Will check if the string is a single predicate. Ex: HasQuest(FetchBread)
        /// If any other character or operator is contained, it will returned false. Ex: (HasQuest(FetchBread))
        /// </summary>
        /// <param name="predicate">The string to analyze.</param>
        /// <returns></returns>
        private static bool IsSinglePredicate(string predicate)
        {
            if (predicate == null)
                return false;
            if (predicate.IndexOf('(') == 0)
                return false;
            int nbOfBrackets = 0;
            foreach (char c in predicate)
            {
                if (c == '(' || c == ')')
                {
                    nbOfBrackets++;
                }
            }

            if (nbOfBrackets > 2)
                return false;
            //We check if predicate is of type Predicate(param)
            return predicate.Contains("(") && predicate.Contains(")") && !predicate.Contains("&&") && !predicate.Contains("||") && !predicate.Contains("!");
        }


        /// <summary>
        /// It the string is a single predicate (Ex: HasQuest(FetchBread)), it will extract the predicate name (HasQuest) and the parameters (FetchBread)
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>Returns a Condition that has the predicate name and the parameters organized</returns>
        private static Condition ExtractSinglePredicate(string predicate)
        {
            if (!IsSinglePredicate(predicate))
                return null;
            //We get all there is before 
            string result = predicate.Split('(')[1].Replace(" ", string.Empty);
            result = result.Split(')')[0];
           return new Condition( predicate.Replace(" ", string.Empty).Split('(')[0], result.Split(',')) ;
        }

        /// <summary>
        /// This will act exactly as the Evaluate function, but only checking if everything is valid.
        /// </summary>
        /// <param name="startPredicate">The predicate to analyse</param>
        /// <returns>Returns true if the predicate is entirely parseable by the Evaluate methode.</returns>
        public static bool IsWholePredicateValid(string startPredicate)
        {
            string predicate = startPredicate;
            //We handle if there is not predicate first
            if (String.IsNullOrEmpty(predicate))
                return true;
            bool result = true;
            //First we need to remove spaces
            //That way we only have characters that matter
            predicate = predicate.Replace(" ", string.Empty);
            //We check then if it is a simple single parameter
            if (PredicateHelper.IsSinglePredicate(predicate))
            {
                //If so, we Evaluate after extracting everything
                return IsSinglePredicateValid(predicate);
            }
            //Most complex example we can get is
            //Predicate(param)&&!(!(Predicate()&&Predicate(param,param))||(Predicate(param)&&Predicate()))&&(Predicate(param)&&!Predicate(param,param,param))
            //If we manage to interpret something like this right, we can do anything
            //First, we need to define the order in which we treat the operators
            //Once all operators are treated, we will just have to sequence them right and handle them one by one


            //It is not a single predicate, so we first check if it is a predicate with a negation
            int i = PredicateHelper.GetNextOperatorIndex(predicate);
            char c = PredicateHelper.GetNextOperator(predicate);
            if (c == '!')
            {
                //There are three possible case !... && ... or !(...) or !...
                //!... && ... and !(...) are the same because it means there is another operator
                if (i != 0)
                {
                    return false;
                }

                predicate = predicate.Substring(1);

                //Now we need to check if we have a single predicate
                if (PredicateHelper.IsSinglePredicate(predicate))
                {
                    //If so we evaluate it and reverse the value
                    result = IsWholePredicateValid(predicate);

                    //We do not inverse it, we just want to know if it is valid
                    return result;
                }
                //If not
                //We need to check the nextOperator again
                i = PredicateHelper.GetNextOperatorIndex(predicate);
                c = PredicateHelper.GetNextOperator(predicate);


                //And set mustReverse as true
                //If so, the first element interpreted will be reversed
            }

            //If we reach here, we have something that is not a single predicate or a negation. We then have to check if it is a bracket
            if (c == '(')
            {
                //GetNextOperator makes sur that the bracket does not come from an operator
                //If we get a parenthesis as next operator, it then must be the first character

                if (i != 0)
                {
                    return false;
                }

                //We have now 2 possibilities
                //Either we simply have something like that (...) or we have something like that (...) && ...
                //Let's first treat the first case
                //For that we need to find the end bracket's index
                int j = PredicateHelper.FindEndBracket(predicate, i);

                if (j == -1)
                {

                    return false;
                }
                else if (j == predicate.Length - 1)
                {

                    //If we reach here, we know the brackets encapsulate all the predicate
                    //We can just remove them and call Evaluate again

                    return IsWholePredicateValid(predicate.Substring(1, predicate.Length - 2));

                }

                //If we haven't returned, then we are in this situation (...) && ...
                //So we evaluate what's inside the brackets, and continue on to evaluate the rest
                result = IsWholePredicateValid(predicate.Substring(1, j - 1));

                //We also must modify the string we are comparing
                predicate = predicate.Substring(j + 1);
                i = PredicateHelper.GetNextOperatorIndex(predicate);
                c = PredicateHelper.GetNextOperator(predicate);
            }

            if (c == '&')
            {
                if (predicate.Length <= i + 1 || predicate[i + 1] != '&')
                {
                    return false;
                }
                //We have 2 use cases
                //Either we have a predicate that looks like that ... && ... or we have a predicate that looks like that && ...
                //The second one means that the first part of the predicate is already stored in result
                //We will handle the first case and reduce it down to a && ... case
                //For that, it is simple, we just have to evaluate everything that is before the '&'
                //And how do we know we are in a ... && ... case ? The '&' is not in place 0
                if (i != 0)
                {
                    result = IsWholePredicateValid(predicate.Substring(0, i));
                }

                //Case && ...
                bool secondResult = IsWholePredicateValid(predicate.Substring(i + 2));
                //If they are not null, then we treat them as booleans
                return result && secondResult;

            }
            else if (c == '|')
            {
                if (predicate.Length <= i + 1 || predicate[i + 1] != '|')
                {

                    return false;
                }
                //We have 2 use cases
                //Either we have a predicate that looks like that ... || ... or we have a predicate that looks like that && ...
                //The second one means that the first part of the predicate is already stored in result
                //We will handle the first case and reduce it down to a || ... case
                //For that, it is simple, we just have to evaluate everything that is before the '|'
                //And how do we know we are in a ... || ... case ? The '|' is not in place 0
                if (i != 0)
                {
                    result = IsWholePredicateValid(predicate.Substring(0, i));
                }

                //Case || ...
                bool secondResult = IsWholePredicateValid(predicate.Substring(i + 2));

                //We make sure everything is valid
                return result && secondResult;
            }


            //If we reached here, it wasn't any case that we knew how to handle, so we return false
            return false;
        }

        /// <summary>
        /// Checks if the predicate is one of the PredicateTypes.
        /// This will avoid users entering wrong values.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static bool IsSinglePredicateValid(string predicate)
        {
            if (IsSinglePredicate(predicate))
            {
                Condition condition = ExtractSinglePredicate(predicate);

                if (!string.IsNullOrEmpty(condition.GetPredicate()) && !predicateTypeString.Contains<string>(condition.GetPredicate()))
                {
                    return false;
                }
            }
            //Will have to be changed
            return true;
        }

        /// <summary>
        /// Returns all the starting brackets '(' index
        /// This is particularly useful to understand indentation during evaluation
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>

        private static int[] GetStartParenthesesIndex(string predicate)
        {
            return GetParenthesesIndex(predicate, true);
        }

        /// <summary>
        /// Returns all the closing brackets ')' index        
        /// This is particularly useful to understand indentation during evaluation
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static int[] GetEndParenthesesIndex(string predicate)
        {
            return GetParenthesesIndex(predicate, false);
        }

        /// <summary>
        /// Returns index of any brackets '(' or ')'.
        /// Useful to know if it is a single predicate (2 brackets only)
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="startParentheses"></param>
        /// <returns></returns>
        private static int[] GetParenthesesIndex(string predicate, bool startParentheses=true)
        {
            List<int> indexes = new List<int>();
            for(int i=0;i< predicate.Length;i++)
            {
                if ((startParentheses && predicate[i] == '(') || (!startParentheses && predicate[i] == ')'))
                {
                   indexes.Add(i);
                }
            }

            return indexes.ToArray();
        }

        /// <summary>
        /// Will analyse if all the brackets it gathers are from a predicate or an encapsulation.
        /// If it is a predicate, it will go back to the first letter of the predicate, and will store its index.
        /// That way the evaluator has visibility of the start of all the predicates and can easily extract them
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static int[] GetPredicateIndexes(string predicate)
        {
            int[] parentheses = GetStartParenthesesIndex(predicate);
            //We have all parentheses
            List<int> conditionIndexes = new List<int>();
            foreach(int i in parentheses)
            {
                if(i>0 && char.IsLetterOrDigit(predicate[i - 1]))
                {
                    int result = GetFirstLetterOfPredicate(predicate, i);
                    if (result > -1)
                    {
                        conditionIndexes.Add(result);
                    }
                }
            }
            return conditionIndexes.ToArray();
        }

        /// <summary>
        /// Will return the index of the first letter of a predicate of the given bracket index comes indeed from a predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="bracketPosition"></param>
        /// <returns></returns>
        private static int GetFirstLetterOfPredicate(string predicate, int bracketPosition)
        {
            if (predicate == null || !char.IsLetterOrDigit(predicate[bracketPosition - 1]))
            {
                return -1;
            }

            int i = bracketPosition-1;
            while(i>=0 && char.IsLetterOrDigit(predicate[i]))
            {
                i--;
            }
            return i+1;
        }


        /// <summary>
        /// Checks if a brackets belongs to a predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="bracketPosition"></param>
        /// <returns></returns>
        private static bool IsBracketFromPredicate(string predicate, int bracketPosition)
        {
            if (predicate == null || (bracketPosition == 0 && predicate[bracketPosition] == '('))
                return false;
            if (predicate[bracketPosition] != '(' && predicate[bracketPosition] != ')')
                return false;

            if(predicate[bracketPosition] == ')')
            {
                //If the end bracket is at the first position
                if (bracketPosition <= 0)
                {
                    return false;
                }
                int i = bracketPosition - 1;
                
                while(i>=0 && char.IsLetterOrDigit(predicate[i]))
                {
                    i--;
                }
                //We stop when i is a special character
                //If it is a ',' then it's a Predicate
                if (predicate[i] == ',')
                    return true;
                //If it is a & or | or ! then it is not
                else if (predicate[i] == '&' || predicate[i] == '|' || predicate[i] == '!')
                    return false;
                //If it is a '(', then we launch back the method with the index of the start bracket
                return IsBracketFromPredicate(predicate, i);
            }

            return char.IsLetterOrDigit(predicate[bracketPosition - 1]);
        }

        /// <summary>
        /// Finds the index of the next closing bracket after the given index
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static int FindEndBracket(string predicate, int i)
        {
            if (predicate == null || i < 0 || i >= predicate.Length)
                return -1;
            int j = i+1;
            int startBracketCount = 0;
            int endBracketCount = 0;
            while (j < predicate.Length)
            {
                if (predicate[j] == '(')
                    startBracketCount++;
                else if(predicate[j] == ')')
                {
                    if (endBracketCount <startBracketCount)
                    {
                        endBracketCount++;
                    }else
                    {
                        return j;
                    }
                }


                j++;
            }
            //If we reached here, we have not found enough brackets and j is not <predicate.length 
            //There's an error
            return -1;
        }

        /// <summary>
        /// Returns the value of the next operator in the predicate.
        /// This will be used to crop the predicate the right way, by understanding what operator need to be treated.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static char GetNextOperator(string predicate)
        {
            int i = 0;
            while(isLetterOperator(predicate,i)){
                i++;
            }
            if(predicate.Length <= i)
            {
                return ' ';
            }
            return predicate[i];
        }

        /// <summary>
        /// We check if the letter we get is an operator we can handle
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static bool isLetterOperator(string predicate, int i)
        {
            bool value = i < predicate.Length && (char.IsLetterOrDigit(predicate[i]) || ((predicate[i] == '(' || predicate[i] == ')') && IsBracketFromPredicate(predicate, i)) || predicate[i] == ',');
            return value;
        }

        /// <summary>
        ///  Returns index of the next operator in the predicate.
        /// This will be used to crop the predicate the right way
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static int GetNextOperatorIndex(string predicate)
        {
            int i = 0;
            while (isLetterOperator(predicate, i))
            {
                i++;
            }
            return i;
        }


    }
}
