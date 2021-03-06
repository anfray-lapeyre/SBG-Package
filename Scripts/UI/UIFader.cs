using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaltButter.UI
{
    public class UIFader : MonoBehaviour
    {

        public CanvasGroup uiElement;

        public void FadeIn()
        {
            FadeIn(0.5f);
        }

        public void FadeIn(float duration = .5f)
        {
            StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 1, duration));
        }

        public void FadeOut()
        {
            FadeOut(0.5f);
        }


        public void FadeOut(float duration = .5f)
        {
            StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 0, duration));
        }

        public IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float lerpTime = 1)
        {
            float _timeStartedLerping = Time.time;
            float timeSinceStarted = Time.time - _timeStartedLerping;
            float percentageComplete = timeSinceStarted / lerpTime;

            while (true)
            {
                timeSinceStarted = Time.time - _timeStartedLerping;
                percentageComplete = timeSinceStarted / lerpTime;

                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                cg.alpha = currentValue;

                if (percentageComplete >= 1) break;

                yield return new WaitForFixedUpdate();
            }

        }
    }
}
