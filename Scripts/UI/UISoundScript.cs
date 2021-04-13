using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaltButter.Core;
using SaltButter.Inputs;
namespace SaltButter.UI
{
    public class UISoundScript : Observer
    {
        private AudioSource audioSource;
        public AudioClip soundClick;
        public AudioClip soundMoveDown;
        public AudioClip soundMoveUp;

        private void Awake()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("InputHandler");
            InputHandler handler;
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].TryGetComponent(out handler))
                {
                    handler.addObserver(this);
                    break;
                }
            }
            audioSource = this.GetComponent<AudioSource>();
        }

        override public void OnNotify(GameObject entity, object notifiedEvent)
        {
            switch (notifiedEvent.GetType().ToString())
            {
                case "MoveCommand":
                    OnDirection(((Inputs.Command.MoveCommand)notifiedEvent).getMove());
                    break;
                case "SpellCommand":
                    OnConfirm(((Inputs.Command.SpellCommand)notifiedEvent).isPressed());
                    break;
                case "RestartCommand":
                    OnReturn(((Inputs.Command.RestartCommand)notifiedEvent).isPressed());
                    break;
                case "PauseCommand":
                    OnReturn(((Inputs.Command.PauseCommand)notifiedEvent).isPressed());
                    break;
                case "EagleViewCommand":
                    //OnPause(((EagleViewCommand)notifiedEvent).isPressed());
                    break;
                case "TopViewCommand":
                    //OnPause(((TopViewCommand)notifiedEvent).isPressed());
                    break;
                default:
                    break;
            }
        }

        public void OnDirection(Vector2 value)
        {
            if (value.y > 0)
            {
                audioSource.clip = soundMoveUp;
                audioSource.Play();
            }
            else if (value.y < 0)
            {
                audioSource.clip = soundMoveDown;
                audioSource.Play();
            }
            else if (value.x > 0)
            {
                audioSource.clip = soundMoveUp;
                audioSource.Play();
            }
            else if (value.x < 0)
            {
                audioSource.clip = soundMoveDown;
                audioSource.Play();
            }
        }

        public void OnConfirm(bool value)
        {
            if (value && audioSource.enabled)
            {
                audioSource.clip = soundClick;
                audioSource.Play();
            }
        }

        public void OnReturn(bool value)
        {
            if (value && audioSource.enabled)
            {
                audioSource.clip = soundClick;
                audioSource.Play();
            }
        }
    }
}
