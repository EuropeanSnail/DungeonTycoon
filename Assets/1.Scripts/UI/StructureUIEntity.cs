using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureUIEntity : MonoBehaviour {

	public Text structureName;
	public Image genreImage;
	public Text structureGenre;
	public Image structureImage;
	public Image structureAreaImage;
	public Text structureCharge;
	public Text structureCapacity;
	public Text structureDuration;

	public Text prefAdventurer;
	public Text prefTraveler;
	public Text prefLower;
	public Text prefMiddle;
	public Text prefUpper;

	public Text prefHuman;
	public Text prefElf;
	public Text prefDwarf;
	public Text prefOrc;
	public Text prefDog;
	public Text prefCat;

	public Text structureExplanation;

	public Text structureConstructCharge;
	public Image structureConstructImage;

	public BuildPanel buildPanel;
	private WaitForSeconds refreshInterval = new WaitForSeconds(3.0f);
	private Coroutine autoRefresh;
	
	public void OnEnable()
	{
		autoRefresh = StartCoroutine(AutoRefreshConstructImage());
	}
	public void OnDisable()
	{
		StopCoroutine(autoRefresh);
	}

	IEnumerator AutoRefreshConstructImage()
	{
		GameManager gameManager = GameManager.Instance;
		int constructGold = int.Parse(structureConstructCharge.text);
		while (true)
		{
			if (gameManager.GetPlayerGold() > constructGold)
			{
				//초록색
				structureConstructImage.sprite = buildPanel.constructableSprite;
			}
			else
			{
				structureConstructImage.sprite = buildPanel.notConstructableSprite;
			}
			yield return refreshInterval;
		}

	}
}
