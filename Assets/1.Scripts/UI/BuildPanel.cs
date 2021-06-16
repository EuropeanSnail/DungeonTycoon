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
	accommodation,
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
	private string upText = "<color=#93FF00>▲</color>";
	private string downText = "<color=#FFB923>▼</color>";
	private string middleText = "<color=#323232>〓</color>";

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
	public Dictionary<string, Sprite> genreImages = new Dictionary<string, Sprite>();
	

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
		//load genre icon
		for(int i = 0; i<System.Enum.GetNames(typeof(Genre)).Length; i++)
		{
			genreImages.Add(System.Enum.GetName(typeof(Genre), i), Resources.Load<Sprite>("GenreIcon/" + System.Enum.GetName(typeof(Genre), i)));
		}
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
				for(int j = 0; j<structuresMaxInfo[System.Enum.GetName(typeof(Category), i)].AsInt; j++)
				{
					//j_max 만큼 생성
					
					tempStructureJSON = structuresInfo[System.Enum.GetName(typeof(Category), i)][j];
					entity = Instantiate(structureUIEntityOrigin, categoryScrolls[i].transform).GetComponent<StructureUIEntity>();
					entity.structureName.text = tempStructureJSON["name"];
					entity.genreImage.sprite = genreImages[tempStructureJSON["genre"]];
					entity.structureGenre.text = tempStructureJSON["genreDisplay"];
					//entity.structureImage .....
					entity.structureAreaImage.sprite = Resources.Load<Sprite>("StructureAreaImage/" + System.Enum.GetName(typeof(Category), i) + "/" + j);
					entity.structureAreaImage.SetNativeSize();
					entity.structureCharge.text = tempStructureJSON["charge"];
					entity.structureCapacity.text = tempStructureJSON["capacity"];
					entity.structureDuration.text = tempStructureJSON["duration"];

					entity.prefAdventurer.text = GetPreferenceVariance(tempStructureJSON["preference"]["adventurer"].AsInt);
					entity.prefTraveler.text = GetPreferenceVariance(tempStructureJSON["preference"]["tourist"].AsInt);
					entity.prefLower.text = GetPreferenceVariance(tempStructureJSON["preference"]["lowerclass"].AsInt);
					entity.prefMiddle.text = GetPreferenceVariance(tempStructureJSON["preference"]["middleclass"].AsInt);
					entity.prefUpper.text = GetPreferenceVariance(tempStructureJSON["preference"]["upperclass"].AsInt);
					entity.prefHuman.text = GetPreferenceVariance(tempStructureJSON["preference"]["human"].AsInt);
					entity.prefElf.text = GetPreferenceVariance(tempStructureJSON["preference"]["elf"].AsInt);
					entity.prefDwarf.text = GetPreferenceVariance(tempStructureJSON["preference"]["dwarf"].AsInt);
					entity.prefOrc.text = GetPreferenceVariance(tempStructureJSON["preference"]["orc"].AsInt);
					entity.prefDog.text = GetPreferenceVariance(tempStructureJSON["preference"]["dog"].AsInt);
					entity.prefCat.text = GetPreferenceVariance(tempStructureJSON["preference"]["cat"].AsInt);

					entity.structureExplanation.text = tempStructureJSON["explanation"];
					entity.structureConstructCharge.text = tempStructureJSON["expenses"];

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
        //SetInitialPosition(FindChildScroll(currentShowingPanel));
        currentShowingPanel.SetActive(false);

        currentShowingPanel = panel;
        //SetInitialPosition(FindChildScroll(currentShowingPanel));
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
		if (pref < 35)
			return downText;
		else if (pref >= 35 && pref <= 50)
			return middleText;
		else
			return upText;
	}
	
}
