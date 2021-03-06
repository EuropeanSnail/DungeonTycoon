#define DEBUG_ITEM_INFO_UI

using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum ItemCondition
{
    None, Purchased, Equipped, Blank // Blank는 인덱스 -1번
}

public class ItemEquipUI : MonoBehaviour
{
    JSONNode itemJSON = null;

    private string selectedSlot = null;
    private int selectedIndex = -1;
    private string selectedCategory = null;

    public ItemListPanel listPanel;
    public ItemInfoPanel infoPanel;
    public ItemComparisonPanel comparisonPanel;
    public GameObject itemSlotsParent;
    public Text goldValue;
    
    private Dictionary<string, Dictionary<int, ItemCondition>> itemStorage; // 아이템 보유 및 장착 현황
    private Dictionary<string, int> curEquipped; // 장착중인 아이템 인덱스. -1은 빈칸
    private Dictionary<string, GameObject> itemSlots;
    private Dictionary<string, Dictionary<int, GameObject>> slotIcons; // 첫 번째 키 슬롯, 두 번째 키 인덱스
    
    private void Awake()
    {
        curEquipped = new Dictionary<string, int>();

        curEquipped.Add("Weapon", -1);
        curEquipped.Add("Armor", -1);
        curEquipped.Add("Accessory1", -1);
        curEquipped.Add("Accessory2", -1);

        itemSlots = new Dictionary<string, GameObject>();
        itemSlots.Add("Weapon", itemSlotsParent.transform.GetChild(0).gameObject);
        itemSlots.Add("Armor", itemSlotsParent.transform.GetChild(1).gameObject);
        itemSlots.Add("Accessory1", itemSlotsParent.transform.GetChild(2).gameObject);
        itemSlots.Add("Accessory2", itemSlotsParent.transform.GetChild(3).gameObject);
        //Debug.Log(itemSlots["Accessory1"].name);

        slotIcons = new Dictionary<string, Dictionary<int, GameObject>>();
        slotIcons.Add("Weapon", new Dictionary<int, GameObject>());
        slotIcons["Weapon"].Add(-1, itemSlotsParent.transform.GetChild(0).GetChild(0).gameObject);
        slotIcons.Add("Armor", new Dictionary<int, GameObject>());
        slotIcons["Armor"].Add(-1, itemSlotsParent.transform.GetChild(1).GetChild(0).gameObject);
        slotIcons.Add("Accessory1", new Dictionary<int, GameObject>());
        slotIcons["Accessory1"].Add(-1, itemSlotsParent.transform.GetChild(2).GetChild(0).gameObject);
        slotIcons.Add("Accessory2", new Dictionary<int, GameObject>());
        slotIcons["Accessory2"].Add(-1, itemSlotsParent.transform.GetChild(3).GetChild(0).gameObject);
        //slotIcons.Add("Weapon", new Dictionary<int, GameObject>());
    }

    private void Start()
    {
        UIManager.Instance.itemEquipUI = this;
        GetItemData();
        CreateItemStorage();

        InitSlotIcons();
		ProgressManager.Instance.EquipItem();
    }

    private void Update()
    {
        RefreshItemInfo();
        RefreshGold();
    }

    private void CreateItemStorage() // 아이템 보유 및 장착 현황 초기화
    {
        //로드 된 스토리지가 있을 때.
        if(itemStorage != null)
            return;
        
        JSONNode jsonNode = ItemManager.Instance.GetItemJSONNode();
        //GameObject itemIcon = (GameObject)Resources.Load("UIPrefabs/TrainUI/ItemIcon_8");
        itemStorage = new Dictionary<string, Dictionary<int, ItemCondition>>();

        //List<ItemCondition> newList = new List<ItemCondition>(jsonNode["Weapon"].Count);
        Dictionary<int, ItemCondition> newDict = new Dictionary<int, ItemCondition>();

        newDict.Add(-1, ItemCondition.Blank);
        for (int i = 0; i < jsonNode["Weapon"].Count; i++)
            newDict.Add(i, ItemCondition.None);
        itemStorage.Add("Weapon", newDict);

        newDict = new Dictionary<int, ItemCondition>();

        newDict.Add(-1, ItemCondition.Blank);
        for (int i = 0; i < jsonNode["Armor"].Count; i++)
            newDict.Add(i, ItemCondition.None);
        itemStorage.Add("Armor", newDict);

        newDict = new Dictionary<int, ItemCondition>();

        newDict.Add(-1, ItemCondition.Blank);
        for (int i = 0; i < jsonNode["Accessory"].Count; i++)
            newDict.Add(i, ItemCondition.None);
        itemStorage.Add("Accessory", newDict);

        //newList = new List<ItemCondition>(jsonNode["Armor"].Count);
        //for (int i = 0; i < jsonNode["Armor"].Count; i++)
        //    newList.Add(ItemCondition.None);
        //itemStorage.Add("Armor", newList);

        //newList = new List<ItemCondition>(jsonNode["Accessory"].Count);
        //for (int i = 0; i < jsonNode["Accessory"].Count; i++)
        //    newList.Add(ItemCondition.None);
        //itemStorage.Add("Accessory", newList);
    }

    private void OnEnable()
    {
        
    }

    public int GetCurEquipped(string slot)
    {
        return curEquipped[slot];
    }

    public void SetCurEquipped(string slot, int inputIndex)
    {
        curEquipped[slot] = inputIndex;
    }

    public void SelectSlot(string inputSlot) // 모험가 아이템슬롯 선택 메서드
    {
        if (selectedSlot != inputSlot)
        {
            selectedSlot = inputSlot;

            if (selectedSlot == "Accessory1" || selectedSlot == "Accessory2")
                selectedCategory = "Accessory";
            else
                selectedCategory = selectedSlot;

            listPanel.SelectCategory(selectedCategory);
        }

        for(int i = 0; i<itemSlots.Count; i++)
            itemSlots[itemSlots.Keys.ToArray()[i]].GetComponent<ItemSlotButton>().HighlightOff();

        itemSlots[selectedSlot].GetComponent<ItemSlotButton>().HighlightOn();

        if (curEquipped[selectedSlot] != -1)
            SelectItem(curEquipped[selectedSlot]);
        else
            infoPanel.HideContent();
    }

    public void GetItemData() // JSONNode Get
    {
        itemJSON = ItemManager.Instance.GetItemJSONNode();
    }

    public void SelectItem(int inputIndex) // 아이템 선택 메서드(카테고리는 SelectSlot에서 먼저 선택해야 함)
    {
        selectedIndex = inputIndex;

        //if (itemJSON[selectedCategory][selectedIndex]["PenetrationMult"].AsFloat == 0)
        //    Debug.Log(itemJSON[selectedCategory][selectedIndex]["PenetrationMult"].AsFloat);

        GameManager.Instance.RefreshDummies();
        GameManager.Instance.ChangeDummyItem(selectedSlot, selectedIndex);

        RefreshItemInfo();
        comparisonPanel.RefreshComparisonPanel();
        infoPanel.RevealContent();
    }

    // 선택된 카테고리, 인덱스에 해당하는 아이템 정보 infoPanel에 Set
    private void RefreshItemInfo()
    {
        if(selectedIndex == -1) //빈칸 선택 시.
        {
            infoPanel.SetName("장착 해제");
            infoPanel.SetDemandedLevel("0");

            infoPanel.SetOnlyExpl("");
            infoPanel.SetStat("이 슬롯을 비워둡니다.");

            infoPanel.SetPrice(0);
            //infoPanel.SetSelectedItemCondition(ItemCondition.Purchased);
            infoPanel.CheckPurchaseConditions(itemStorage[selectedCategory][selectedIndex], (curEquipped[selectedSlot] == selectedIndex));

            // 이 부분 수정 필요

            return;
        }

        infoPanel.SetName(itemJSON[selectedCategory][selectedIndex]["Name"]);
        //infoPanel.SetExplanation(itemJSON[selectedItemCategory][selectedItemIndex]["Explanation"]);

        if (itemJSON[selectedCategory][selectedIndex]["ItemSkill"] == null)
        {
            infoPanel.SetOnlyExpl(itemJSON[selectedCategory][selectedIndex]["Explanation"]);
            //Debug.Log("Skill X");
        }
        else
        {
            string skillName, skillEffect;
            SkillFactory.GetNameAndExplanation(itemJSON[selectedCategory][selectedIndex]["ItemSkill"], out skillName, out skillEffect);
            infoPanel.SetSkillAndExpl(skillName, skillEffect, itemJSON[selectedCategory][selectedIndex]["Explanation"]);
            //Debug.Log("Skill O");
        }

        infoPanel.SetStat(MakeStatString());

        if(Mathf.Abs(itemJSON[selectedCategory][selectedIndex]["DemendedLevel"].AsFloat) <= 0.0f + Mathf.Epsilon)
        {
            infoPanel.SetDemandedLevel("1");
        }
        else
        {
            infoPanel.SetDemandedLevel(itemJSON[selectedCategory][selectedIndex]["DemendedLevel"]);
        }

        //CheckPurchaseConditions();
        infoPanel.SetPrice(itemJSON[selectedCategory][selectedIndex]["Price"].AsInt);
        //infoPanel.SetSelectedItemCondition(itemStorage[selectedCategory][selectedIndex]);

        // 로드 제대로 안됐음
        //for (int i = -1; i < itemStorage["Weapon"].Count - 1; i++)
        //{
        //    //if (loadedStorage["Weapon"][i] == ItemCondition.Purchased)
        //    //    Debug.Log(i + "Purchased");
        //    //else if (loadedStorage["Weapon"][i] == ItemCondition.Equipped)
        //    //    Debug.Log(i + "Equipped");
        //    Debug.Log(i + " : " + itemStorage["Weapon"][i]);
        //}
        infoPanel.CheckPurchaseConditions(itemStorage[selectedCategory][selectedIndex], (curEquipped[selectedSlot] == selectedIndex));
    }

    private string MakeStatString()
    {
        string resultStr = "";
        int statCnt = 0;

        switch(selectedCategory)
        {
            case "Weapon":
                if (Mathf.Abs(itemJSON[selectedCategory][selectedIndex]["Attack"].AsFloat) > 0.0f + Mathf.Epsilon)
                {
                    resultStr += "공격력+" + itemJSON[selectedCategory][selectedIndex]["Attack"];
                    statCnt++;
                }
                if(Mathf.Abs(itemJSON[selectedCategory][selectedIndex]["CriticalChance"].AsFloat) > 0.0f + Mathf.Epsilon)
                {
                    if (statCnt != 0)
                        resultStr += ", ";
                    resultStr += "치명타 확률+" + itemJSON[selectedCategory][selectedIndex]["CriticalChance"].AsFloat * 100 + "%";
                    statCnt++;
                }
                if(Mathf.Abs(itemJSON[selectedCategory][selectedIndex]["AttackSpeed"].AsFloat) > 0.0f + Mathf.Epsilon)
                {
                    if (statCnt != 0)
                        resultStr += ", ";
                    resultStr += "공격속도+" + itemJSON[selectedCategory][selectedIndex]["AttackSpeed"].AsFloat * 100 + "%";
                    statCnt++;
                }
                if (Mathf.Abs(itemJSON[selectedCategory][selectedIndex]["PenetrationMult"].AsFloat) > 0.0f + Mathf.Epsilon)
                {
                    if (statCnt != 0)
                        resultStr += ", ";
                    resultStr += "방어구 관통력+" + itemJSON[selectedCategory][selectedIndex]["PenetrationMult"].AsFloat * 100 + "%";
                    statCnt++;
                }
                break;
            case "Armor":
                break;
            case "Accessory":
                break;
            default:
                resultStr = null;
                break;
        }

        return resultStr;
    }

    //private void CheckPurchaseConditions()
    //{
    //    // 장착중인지 어떻게 Check? 이거는 이름 비교하면 됨.
    //    // 구매했는지 어떻게 Check? 컬렉션을 가지고 있어야겠다.
        
    //}


    public void EquipItem()
    {
        if(infoPanel.GetIsPurchase())
            GameManager.Instance.AddGold(-itemJSON[selectedCategory][selectedIndex]["Price"].AsInt);

        if(curEquipped[selectedSlot] != -1)
            itemStorage[selectedCategory][curEquipped[selectedSlot]] = ItemCondition.Purchased;

        if (selectedIndex != -1)
            itemStorage[selectedCategory][selectedIndex] = ItemCondition.Equipped;

        curEquipped[selectedSlot] = selectedIndex;

        GameManager.Instance.EquipPlayerSpAdvItem(selectedSlot, selectedIndex);
        GameManager.Instance.RefreshDummies();

        RefreshItemInfo();
        comparisonPanel.RefreshComparisonPanel();

        CurSlotIconChange();
    }

    public void CurSlotIconChange()
    {
        for(int i = 0; i<slotIcons[selectedSlot].Count; i++)
            slotIcons[selectedSlot].Values.ToArray()[i].SetActive(false);

        if (slotIcons[selectedSlot].ContainsKey(selectedIndex) == false)
            GenSlotIcon();

        slotIcons[selectedSlot][selectedIndex].SetActive(true);
    }

    public void InitSlotIcons()
    {
        //curEquipped["Weapon"] = GameManager.Instance.GetPlayerSpAdvItemIndex("Weapon");
        //curEquipped["Armor"] = GameManager.Instance.GetPlayerSpAdvItemIndex("Armor");
        //curEquipped["Accessory1"] = GameManager.Instance.GetPlayerSpAdvItemIndex("Accessory1");
        //curEquipped["Accessory2"] = GameManager.Instance.GetPlayerSpAdvItemIndex("Accessory2");
        string origSlot = selectedSlot;

        //Debug.Log("Weapon : " + GameManager.Instance.GetPlayerSpAdvItemIndex("Weapon") + ", " + itemStorage["Weapon"][GameManager.Instance.GetPlayerSpAdvItemIndex("Weapon")]);
        //if (itemStorage == null)
        //    Debug.Log("NULL");
        //for (int i = -1; i<itemStorage["Weapon"].Count-1; i++)
        //{
        //    if (itemStorage["Weapon"][i] == ItemCondition.Purchased)
        //        Debug.Log(i + "Purchased");
        //    else if (itemStorage["Weapon"][i] == ItemCondition.Equipped)
        //        Debug.Log(i + "Equipped");
        //}

        SelectSlot("Weapon");
        SelectItem(GameManager.Instance.GetPlayerSpAdvItemIndex("Weapon"));
        EquipItem();
        SelectSlot("Armor");
        SelectItem(GameManager.Instance.GetPlayerSpAdvItemIndex("Armor"));
        EquipItem();
        SelectSlot("Accessory1");
        SelectItem(GameManager.Instance.GetPlayerSpAdvItemIndex("Accessory1"));
        EquipItem();
        SelectSlot("Accessory2");
        SelectItem(GameManager.Instance.GetPlayerSpAdvItemIndex("Accessory2"));
        EquipItem();

        if (origSlot != null)
            SelectSlot(origSlot);
        else
            SelectSlot("Weapon");
        //SelectItem(curEquipped[origSlot]);
    }

    private void GenSlotIcon()
    {
        GameObject newIcon = Instantiate<GameObject>(listPanel.GetComponent<ItemListPanel>().GetItemIconByIndex(selectedIndex));
        slotIcons[selectedSlot].Add(selectedIndex, newIcon);

        newIcon.transform.SetParent(itemSlots[selectedSlot].transform);
        
        newIcon.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        newIcon.transform.localPosition = new Vector3(0, 0, 0);

        newIcon.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        //Debug.Log(newIcon.GetComponent<RectTransform>().anchorMin.x + ", " + newIcon.GetComponent<RectTransform>().anchorMin.y);
        newIcon.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        //Debug.Log(newIcon.GetComponent<RectTransform>().anchorMax.x + ", " + newIcon.GetComponent<RectTransform>().anchorMax.y);

        //newIcon.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        //newIcon.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
        //newIcon.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        //newIcon.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);

        newIcon.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        newIcon.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        newIcon.transform.SetAsFirstSibling();
        //newIcon.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
    }

    public GameObject GetItemIcon(string category, int index)
    {
        return listPanel.GetItemIconByCategoryAndIndex(category, index);
    }

    public Dictionary<string, Dictionary<int, ItemCondition>> GetItemStorage()
    {
        return itemStorage;
    }

    public void LoadItemStorage(Dictionary<string, Dictionary<int, ItemCondition>> loadedStorage)
    {
        //for (int i = -1; i < loadedStorage["Weapon"].Count - 1; i++)
        //{
        //    Debug.Log(i + " : " + loadedStorage["Weapon"][i]);
        //}
        itemStorage = loadedStorage;
    }

    public void RefreshGold()
    {
        goldValue.text = GameManager.Instance.playerGold + "";
    }
}