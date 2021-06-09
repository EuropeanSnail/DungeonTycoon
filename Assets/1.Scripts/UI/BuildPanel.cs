using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

enum Category
{
	drink,
	food,
	lodge,
	equipment,
	tour,
	convenience,
	fun,
	santuary
};
enum Genre
{
	accomodation,
	alcohol,
	armor,
	celebrate,
	convenience,
	equipment,
	game,
	juice,
	meal,
	necessaries,
	potion,
	rest,
	santuary,
	show,
	sightseeing,
	snack,
	weapon
}
public class BuildPanel : UIObject {
	private string upText = "▲";
	private string downText = "▼";
	private string middleText = "〓";


	public GameObject drinkPanel;
    public GameObject foodPanel;
    public GameObject lodgePanel;
    public GameObject equipmentPanel;
    public GameObject tourPanel;
    public GameObject conveniencePanel;
    public GameObject funPanel;
    public GameObject santuaryPanel;
    public GameObject rescuePanel;

	public GameObject[] categoryScrolls;

    GameObject currentShowingPanel;

	bool isInstantiated = false;
	JSONNode structuresInfo;
	JSONNode structuresMaxInfo;

	public GameObject structureUIEntityOrigin; // for dup
	public Sprite constructableSprite;
	public Sprite notConstructableSprite;
	public Dictionary<string, Sprite> genreImages;
	Category cat;
    public override void Awake()
    { 
        base.Awake();
		
    }

	public void Start()
	{
		
		StartCoroutine(LateStart());
	}
	IEnumerator LateStart()
	{
		yield return null;
		if (isInstantiated == true)
			yield break;
		else
		{
			//structure ui Instantiate
			structuresInfo = StructureManager.Instance.GetStructuresJSON();
			structuresMaxInfo = GameManager.Instance.GetStructureMaxInfo();
			/*
			 * drink
			 * food
			 * lodge
			 * equipment
			 * tour
			 * convenience
			 * fun
			 * santuary
			 */
			StructureUIEntity entity;
			JSONNode tempStructureJSON;
			for(int i = 0; i<System.Enum.GetValues(typeof(Category)).Length; i++)
			{
				cat = (Category)i;
				for(int j = 0; j<structuresMaxInfo[System.Enum.GetName(typeof(Category), i)].AsInt; j++)
				{
					//j_max 만큼 생성
					tempStructureJSON = structuresInfo[System.Enum.GetName(typeof(Category), i)][j];
					entity = Instantiate(structureUIEntityOrigin, categoryScrolls[i].transform).GetComponent<StructureUIEntity>();
					entity.structureName.text = tempStructureJSON["name"];

					//entity.structureAreaImage.SetNativeSize();
				}
			}
			isInstantiated = true;
		}
	}
	public override void Show()
    {
        drinkPanel.SetActive(true);
        foodPanel.SetActive(false);
        lodgePanel.SetActive(false);
        equipmentPanel.SetActive(false);
        tourPanel.SetActive(false);
        conveniencePanel.SetActive(false);
        funPanel.SetActive(false);
        santuaryPanel.SetActive(false);
        rescuePanel.SetActive(false);

        base.Show();
        currentShowingPanel = drinkPanel;
        OpenPanel(drinkPanel);
    }
    public override void Hide()
    {
        currentShowingPanel = null;
        base.Hide();
    }
    public void OpenPanel(GameObject panel)
    {
        SetInitialPosition(FindChildScroll(currentShowingPanel));
        currentShowingPanel.SetActive(false);

        currentShowingPanel = panel;
        SetInitialPosition(FindChildScroll(currentShowingPanel));
        currentShowingPanel.SetActive(true);
    }

    public void SetInitialPosition(GameObject scroll)
    {
        RectTransform rt = scroll.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2((rt.sizeDelta.x / 2) - rt.GetChild(0).GetComponent<RectTransform>().sizeDelta.x, rt.anchoredPosition.y);
    }
    public GameObject FindChildScroll(GameObject panel)
    {
        for (int i = 0; i < currentShowingPanel.transform.childCount; i++)
        {
            if (currentShowingPanel.transform.GetChild(i).tag == "HorizontalScroll")
            {
                GameObject scroll = panel.transform.GetChild(i).gameObject;
                return scroll;
            }
        }
        Debug.Log("BuilPanel.FindChildScroll() returns null!!\n GameObject name == " + panel.name);
        return null;
    }
	
	public string GetPreferenceVariance(int pref)
	{
		if (pref < 40)
			return downText;
		else if (pref >= 40 && pref < 60)
			return middleText;
		else
			return upText;
	}
}
