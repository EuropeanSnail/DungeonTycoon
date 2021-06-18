﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;

public class TrainPanel : UIObject {

    public GameObject informationPanel;
    public GameObject myAdvSelectionPanel;
    public GameObject myAdvInfoPanel;
    public GameObject itemEquipPanel;
    public GameObject bossPanel;
    public GameObject otherSpAdvSelectionPanel;
    public GameObject otherSpAdvInfoPanel;
    public GameObject spAdvPreviewPanel;

    private GameObject currentShowingPanel;

    //Information Panel's
    public GameObject storyPanel;
    public GameObject statusPanel;
    public Image statusButtonImage;
    public Image storyButtonImage;

    //Item Panel's
    public GameObject itemWeaponsView;
    public GameObject itemArmorsView;
    public GameObject itemAccessoriesView;

    public GameObject itemWeaponsContainer;
    public GameObject itemArmorsContainer;
    public GameObject itemAccessoriesContainer;

    public List<GameObject> itemWeapons;
    public List<GameObject> itemArmors;
    public List<GameObject> itemAccessories1;
    public List<GameObject> itemAccessories2;

    public Text myAdventurerHealth;
    public Text myAdventurerDefense;
    public Text myAdventurerMoveSpeed;
    public Text myAdventurerRange;
    public Text myAdventurerDPS;
    public Text myAdventurerAttack;
    public Text myAdventurerAttackSpeed;
    public Text myAdventurerCritical;
    public Text myAdventurerCriticalAttack;
    public Text myAdventurerPenetrateFixed;
    public Text myAdventurerPenetratePercent;
    public GameObject myAdventurerAbilityList;
    public List<GameObject> myAdventurerAbilities;

    public Text itemsName;
    public Text itemsCategory;
    public Text itemsAbility;
    public Text itemsContent;
    public Text itemsValue;
    //button의   Text

    public Button itemEquipButton;

    // 일선 모험가 선택 관련
    //private bool isSelected = false;


    public override void Show()
    {
        GameObject spAdvPanel;

		if (GameManager.Instance.playerSpAdvIndex != -1)
			spAdvPanel = myAdvInfoPanel;
		else
			spAdvPanel = myAdvSelectionPanel;

        spAdvPanel.SetActive(true);
        itemEquipPanel.SetActive(false);
        bossPanel.SetActive(false);
        otherSpAdvSelectionPanel.SetActive(false);
        otherSpAdvInfoPanel.SetActive(false);
        spAdvPreviewPanel.SetActive(false);

		RefreshEquipButton();
        
        base.Show();
        currentShowingPanel = spAdvPanel;
		//Dialog
		ProgressManager.Instance.OpenTrainMenu();
	}

    public override void Hide()
    {
        //하위 Object 초기상태로 세팅 후 base.hide 해야함.
        currentShowingPanel = null;
        //SetInitialState();
        base.Hide();
    }


    public void OpenPanel(GameObject panel)
    {
        currentShowingPanel.SetActive(false);
        currentShowingPanel = panel;
        currentShowingPanel.SetActive(true);
    }

    public void OpenSpAdvPanel()
    {
        GameObject spAdvPanel;

		if (GameManager.Instance.playerSpAdvIndex != -1)
			spAdvPanel = myAdvInfoPanel;
		else
			spAdvPanel = myAdvSelectionPanel;

        currentShowingPanel.SetActive(false);
        currentShowingPanel = spAdvPanel;
        currentShowingPanel.SetActive(true);
    }

    public void OpenOtherSpAdvInfoPanel(string nameKey)
    {
        otherSpAdvSelectionPanel.GetComponent<OtherSpAdvSelectionUI>().SelectSpAdv(nameKey);
        OpenPanel(otherSpAdvInfoPanel);
    }

    public void OpenSpAdvPreviewPanel()
    {
        OpenPanel(spAdvPreviewPanel);
    }

    public void SwitchStoryStatusPanel(int cat)
    {
        if(cat == 0) // story -> status
        {
            storyButtonImage.material = UIManager.Instance.grayScaleMaterial;
            storyPanel.SetActive(false);
            statusButtonImage.material = null;
            statusPanel.SetActive(true);
        }
        else if(cat == 1) //status -> story
        {
            statusButtonImage.material = UIManager.Instance.grayScaleMaterial;
            statusPanel.SetActive(false);
            storyButtonImage.material = null;
            storyPanel.SetActive(true);
        }
    }

    public void RefreshEquipButton()
    {
        itemEquipButton.interactable = GameManager.Instance.HadSpAdvChosen;
    }

    //
    //public void SetInitialState()
    //{
    //    statusButtonImage.material = UIManager.Instance.grayScaleMaterial;
    //    statusPanel.SetActive(false);
    //    storyButtonImage.material = null;
    //    storyPanel.SetActive(true);
    //    //item  초기상태

    //}

    //public void SpAdvSelected()
    //{
    //    isSelected = true;
    //}
}
