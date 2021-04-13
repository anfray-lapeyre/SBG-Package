﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SaltButter.UI
{
    public class UIAutoScrollDropDown : MonoBehaviour
    {
        public UIStateMachine eventSystem;
        private UISlider bar;
        private RectTransform scrollRect;
        public UIDropDown dropdown;
        private int waitforframes = 1;
        private int iteration = 0;
        private int oldSelected = 0;

        public void Awake()
        {
            bar = GetComponentInChildren<UISlider>();
            scrollRect = GetComponent<RectTransform>();
        }


        public void resetScrollBar()
        {

            if (eventSystem.firstSelected.GetType().ToString() == "UIDropDownItem")
            {
                UIDropDownItem firstSelect = eventSystem.firstSelected as UIDropDownItem;
                if (bar)
                    bar.value = Mathf.Max(0, Mathf.FloorToInt((((float)firstSelect.value) / dropdown.options.Count) * bar.nbSteps - 0.2f));

            }
            oldSelected = dropdown.value;
            if (bar)
                bar.Refresh();
        }

        // Update is called once per frame
        void Update()
        {
            if (iteration < waitforframes)
            {
                iteration++;
            }
            else if (iteration == waitforframes)
            {
                //We wait just the right number of iterations  
                resetScrollBar();
                iteration++;
            }
            else
            {
                //We are here after the first few frames
                if (eventSystem.firstSelected.GetType().ToString() == "UIDropDownItem")
                {
                    UIDropDownItem firstSelect = eventSystem.firstSelected as UIDropDownItem;
                    int currentSelected = firstSelect.value;
                    if (currentSelected != oldSelected)
                    {


                        oldSelected = currentSelected;

                        if (bar)
                        {
                            bar.value = Mathf.Max(0, Mathf.FloorToInt((((float)firstSelect.value) / dropdown.options.Count) * bar.nbSteps - 0.2f));
                            bar.Refresh();
                        }
                    }
                }

            }
        }
    }
}
