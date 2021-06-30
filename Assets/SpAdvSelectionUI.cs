﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpAdvSelectionUI : MonoBehaviour
{
    public int curSelected = -1;
    public Button determineBtn;
    public Button moreInfoBtn;
    public TrainPanel trainPanel;
    public SpAdvPreviewUI spAdvPreviewUI;

    public void DetermineSpAdv()
    {
        //Debug.Log("Determined");
        if (curSelected != -1)
        {
            GameManager.Instance.ChooseSpAdv(curSelected);
            trainPanel.OpenSpAdvPanel();
            trainPanel.RefreshEquipButton();
        }
    }

    public void SetCurSelected(string nameKeyIn)
    {
        List<GameObject> spAdvs = GameManager.Instance.specialAdventurers;
        for (int i = 0; i<spAdvs.Count; i++)
        {
			//Debug.Log(nameKeyIn + ", "+i);
			if (spAdvs[i].GetComponent<SpecialAdventurer>().nameKey == nameKeyIn)
			{
				curSelected = i;
				break;
			}
        }

        if (curSelected != -1)
        {
            spAdvPreviewUI.SelectSpAdv(curSelected);
            determineBtn.interactable = true;
            moreInfoBtn.interactable = true;
        }
    }
}
