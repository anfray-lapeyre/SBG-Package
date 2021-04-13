using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaltButter.Dialogue.UI
{
    public class DialogueUIScript : MonoBehaviour
    {
        Runtime.PlayerConversant playerConversant;

        [SerializeField]TextMeshProUGUI currentSpeaker;
        [SerializeField]TextMeshProUGUI dialogueText;
        [SerializeField] Button choice1Button;
        [SerializeField] Button choice2Button;
        [SerializeField] Button choice3Button;
        [SerializeField] Button closeButton;
        // Start is called before the first frame update
        void Start()
        {
            playerConversant= GameObject.FindGameObjectWithTag("Player").GetComponent<Runtime.PlayerConversant>();
            playerConversant.OnConversationUpdated += UpdateUI;
            choice1Button.onClick.AddListener(() => playerConversant.Next(choice1Button.GetComponentInChildren<TextMeshProUGUI>().text));
            choice2Button.onClick.AddListener(() => playerConversant.Next(choice2Button.GetComponentInChildren<TextMeshProUGUI>().text));
            choice3Button.onClick.AddListener(() => playerConversant.Next(choice3Button.GetComponentInChildren<TextMeshProUGUI>().text));
            closeButton.onClick.AddListener(() => playerConversant.QuitDialogue());
            UpdateUI();
        }


        /// <summary>
        /// Will update the values of each component and the visibility of the three buttons in function of the number of choices available
        /// </summary>
        void UpdateUI()
        {
            this.gameObject.SetActive(playerConversant.isActive());
            if (!playerConversant.isActive())
            {
                return;
            }
            currentSpeaker.text = playerConversant.GetCurrentConversant();
            dialogueText.text = playerConversant.GetText();
            List<string> choices = playerConversant.GetChoices();
            if (choices != null && choices.Count>0)
            {
                if (choices.Count >= 1)
                {
                    choice1Button.gameObject.SetActive(true);
                    choice1Button.GetComponentInChildren<TextMeshProUGUI>().text = choices[0];
                }
                else
                {
                    choice1Button.gameObject.SetActive(false);
                    choice2Button.gameObject.SetActive(false);
                    choice3Button.gameObject.SetActive(false);
                    return;
                }
                if (choices.Count >= 2)
                {
                    choice2Button.gameObject.SetActive(true);
                    choice2Button.GetComponentInChildren<TextMeshProUGUI>().text = choices[1];
                }
                else
                {
                    choice2Button.gameObject.SetActive(false);
                    choice3Button.gameObject.SetActive(false);
                    return;
                }
                if (choices.Count >= 3)
                {
                    choice3Button.gameObject.SetActive(true);
                    choice3Button.GetComponentInChildren<TextMeshProUGUI>().text = choices[2];
                }
                else
                {
                    choice3Button.gameObject.SetActive(false);
                    return;
                }
            }
            else
            {
                choice1Button.gameObject.SetActive(false);
                choice2Button.gameObject.SetActive(false);
                choice3Button.gameObject.SetActive(false);
            }


        }
    }
}
