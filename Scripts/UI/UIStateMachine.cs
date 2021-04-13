using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SaltButter.Core;
using SaltButter.Inputs;
using SaltButter.Inputs.Command;


namespace SaltButter.UI
{
    public class UIStateMachine : Observer
    {
        public UIButton firstSelected;
        private InputHandler persistantHandler;

        public UIButton onReturn;

        private void Start()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("InputHandler");
            InputHandler handler;
            for (int i = 0; i < objects.Length; i++)
            {
                handler = objects[i].GetComponent<InputHandler>();
                if (handler != null)
                {
                    persistantHandler = handler;
                    handler.addObserver(this);
                    break;
                }
            }

            firstSelected.changeState(UIButton.SELECTED);

        }



        public void testButtonAction(BaseEventData data)
        {
            Debug.Log("Event received from : " + data.selectedObject.name);
        }

        public void changeActiveObject(UIButton newActiveObject)
        {
            if (newActiveObject != null)
            {
                UIButton value = newActiveObject.changeState(UIButton.SELECTED);
                if (value != null)
                {
                    firstSelected.changeState(UIButton.UNSELECTED);
                    firstSelected = newActiveObject;
                }

            }
        }

        override public void OnNotify(GameObject entity, object notifiedEvent)
        {
            switch (notifiedEvent.GetType().ToString())
            {
                case "SaltButter.Inputs.Command.MoveCommand":
                    if (((MoveCommand)notifiedEvent).isMenuController)
                    {
                        OnMove(((MoveCommand)notifiedEvent).getMove());
                    }
                    break;
                case "SaltButter.Inputs.Command.SpellCommand":
                    Debug.Log("Spell is pressed : " + (( SpellCommand)notifiedEvent).isPressed());
                    if ((( SpellCommand)notifiedEvent).isPressed())// && !waitingforButtontoUnpress)
                    {
                        firstSelected.ExecuteFunction();
                    }
                    else
                    {
                        firstSelected.unPress();

                    }
                    break;
                case "SaltButter.Inputs.Command.RestartCommand":
                    if (onReturn && (( RestartCommand)notifiedEvent).isPressed())
                    {
                        switch (onReturn.GetType().ToString())
                        {
                            case "UIButton":
                                onReturn.ExecuteFunction();
                                break;
                            case "UIDropDown":
                                (onReturn as UIDropDown).getBack();
                                break;
                        }
                    }
                    break;
                case "SaltButter.Inputs.Command.PauseCommand":
                    //OnPause(((RestartCommand)notifiedEvent).isPressed());
                    break;
                case "SaltButter.Inputs.Command.EagleViewCommand":
                    //OnPause(((EagleViewCommand)notifiedEvent).isPressed());
                    break;
                case "SaltButter.Inputs.Command.TopViewCommand":
                    //OnPause(((TopViewCommand)notifiedEvent).isPressed());
                    break;
                default:
                    break;
            }
        }

        public void OnMove(Vector2 value)
        {
            if (value.y > 0)
            {
                moveInDirection(UIButton.GOUP);
            }
            else if (value.y < 0)
            {
                moveInDirection(UIButton.GODOWN);
            }
            else if (value.x > 0)
            {
                moveInDirection(UIButton.GORIGHT);
            }
            else if (value.x < 0)
            {
                moveInDirection(UIButton.GOLEFT);
            }
        }

        private void moveInDirection(int direction)
        {
            UIButton button = firstSelected.changeState(direction);
            if (button != null)
            {
                firstSelected = button;
            }
        }


        public void StartObserving()
        {
            persistantHandler.addObserver(this);

        }

        public void StopObserving()
        {
            if(subject != null)
                subject.removeObserver(this);

        }


    }
}
