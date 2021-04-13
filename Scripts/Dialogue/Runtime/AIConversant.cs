using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.Dialogue.Runtime {

    /// <summary>
    /// This will have to change to get rid of the IRaycastable
    /// </summary>
    public class AIConversant : MonoBehaviour
    {

        [SerializeField] string key = "";
        [SerializeField] public string CharacterName = "";

        public bool HasDialogue()
        {
            return key != "" && LocalizationManager.GetTermsList("Dialogue").Contains("Dialogue/"+key);
        }

        public DialogueContainer GetDialogue()
        {

            return LocalizationManager.GetTranslatedObjectByTermName<DialogueContainer>("Dialogue/" + key);
        }


        public bool OpenDialogue(GameObject conversant)
        {
            //You can change language like the comment below
            //I2.Loc.LocalizationManager.CurrentLanguage = "French";
            DialogueContainer dialogue = GetDialogue();
            PlayerConversant playerConversant = conversant.GetComponent<PlayerConversant>();
            if (playerConversant == null)
                return false;
            playerConversant.StartDialogue(this,GetDialogue());
            return true;
        }

    }
}