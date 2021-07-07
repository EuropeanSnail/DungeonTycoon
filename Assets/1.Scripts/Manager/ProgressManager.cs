using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public enum SpAdvNames
{
	Hana, Iris, Maxi, Murat, OldMan, Yeonhwa, Nyang, Wal
}
public enum ExtNames
{
	Build, Equip, Train
}
public class ProgressManager : MonoBehaviour {


	//씬 스토리 진행 관련 매니저
	//현재 선택한 캐릭터 스토리 JSON..
	//어느 시점에서 대화 띄울지 정보
	//-> 씬 번호, 클리어한 방 기준? JSON
	//

	
	JSONNode dialogBindingJson;
	public Dictionary<string, int> characterDialogProgressData;
	public Dictionary<string, int> extDialogProgressData;
	public static ProgressManager Instance
	{
		get
		{
			if (_instance != null)
				return _instance;
			else
			{
				return null;
			}
		}
	}

	private static ProgressManager _instance;

	private void Awake()
	{
		if (ProgressManager.Instance != null) // 이미 씬에 있다면(로드했거나 다음씬 진행했을때?)
		{
			Destroy(gameObject);
			return;
		}
		_instance = this;
		LoadProgressData();
		
	}
	private void Start()
	{
		GameObject.DontDestroyOnLoad(gameObject);
		if (SaveLoadManager.Instance.isLoadedGame == false)
		{
			characterDialogProgressData = new Dictionary<string, int>();
			extDialogProgressData = new Dictionary<string, int>();
			for (int i = 0; i < System.Enum.GetNames(typeof(SpAdvNames)).Length; i++)
			{
				characterDialogProgressData.Add(System.Enum.GetName(typeof(SpAdvNames), i), 0);
			}
			for(int i = 0; i< System.Enum.GetNames(typeof(ExtNames)).Length; i++)
			{
				extDialogProgressData.Add(System.Enum.GetName(typeof(ExtNames), i), 0);
			}
		}
	}
	// Use this for initialization
	public void LoadProgressData()
	{
		dialogBindingJson = SimpleJSON.JSON.Parse(Resources.Load<TextAsset>("Dialogs/DialogsBinding").text);
	}
	//DialogBinding
	//dialogBindingJson["stage"][int]는 스테이지 번호 = 인덱스

	//dialogBindingJson[캐릭터이름][int]는 캐릭터 별 진행상황(PlayerPrefs에 저장) = 인덱스

	#region Check
	//Stage
	public void SceneStarted(int sceneNum) //GameManager에서 LateStarted로 isloaded == false 일때만 // progressData 신경 안써도됨
	{
		if (dialogBindingJson["stage"][sceneNum]["scenestart"] != null)
			DialogManager.Instance.StartDialog(dialogBindingJson["stage"][sceneNum]["scenestart"]);
	}
	public void SceneEnded(int sceneNum)
	{
		if (dialogBindingJson["stage"][sceneNum]["sceneend"] != null)
			DialogManager.Instance.StartDialog(dialogBindingJson["stage"][sceneNum]["sceneend"]);
		characterDialogProgressData[GetCurSpAdvName()]++;
		if(string.Equals(GetCurSpAdvName(), System.Enum.GetName(typeof(SpAdvNames), 6)))// 6 == nyang
		{
			characterDialogProgressData[System.Enum.GetName(typeof(SpAdvNames), 7)]++;
		}
		else if (string.Equals(GetCurSpAdvName(), System.Enum.GetName(typeof(SpAdvNames), 7)))// 7 == wal
		{
			characterDialogProgressData[System.Enum.GetName(typeof(SpAdvNames), 6)]++;
		}
	}
	public void ConquerStarted(int areaNum) // 로드 시 이미 conquer 된것으로 나오기때문에 -- 다시 나오진 않을듯?
	{
		if (GetCurSpAdvName() == string.Empty)
			return;
		if(dialogBindingJson[GetCurSpAdvName().ToLower()][characterDialogProgressData[GetCurSpAdvName()]]["conquerstart"][areaNum.ToString()] != null)
			DialogManager.Instance.StartDialog(dialogBindingJson[GetCurSpAdvName().ToLower()][characterDialogProgressData[GetCurSpAdvName()]]["conquerstart"][areaNum.ToString()]);
		Debug.Log("ConquerStarted() Called - " + areaNum);
	}
	public void ConquerEnded(int areaNum)
	{
		if (GetCurSpAdvName() == string.Empty)
			return;
		if (dialogBindingJson[GetCurSpAdvName().ToLower()][characterDialogProgressData[GetCurSpAdvName()]]["conquerend"][areaNum.ToString()] != null)
			DialogManager.Instance.StartDialog(dialogBindingJson[GetCurSpAdvName().ToLower()][characterDialogProgressData[GetCurSpAdvName()]]["conquerend"][areaNum.ToString()]);
		Debug.Log("ConquerEndted() Called - " + areaNum);
	}
	public void CheckCharacterSelected()
	{

	}
	//EndStage
	//Character
	public void BuildStructure()
	{
		//sceneNum 1일때만 한번 실행
		if (extDialogProgressData["Build"] <= 0)
		{
			
			DialogManager.Instance.StartDialog(dialogBindingJson["stage"][1]["buildstructure"]);
			extDialogProgressData["Build"]++;
		}
	}
	public void EquipItem()
	{
		//sceneNum 1일때만 한번 실행
		if (extDialogProgressData["Equip"] <= 0)
		{
			DialogManager.Instance.StartDialog(dialogBindingJson["stage"][1]["equipitem"]);
			extDialogProgressData["Equip"]++;
		}
	}
	public void OpenTrainMenu()
	{
		//육성 버튼 눌렀을때 설명
		if (extDialogProgressData["Train"] <= 0)
		{
			DialogManager.Instance.StartDialog(dialogBindingJson["stage"][1]["opentrain"]);
			extDialogProgressData["Train"]++;
		}
		Debug.Log("OpenTrain....");
	}
	public void SelectSpAdv(int charNum)
	{
		if (charNum < 0)
			return;
		//캐릭터 ProgressIndex == 0 일때 딱 한번만 실행.
		if(characterDialogProgressData[System.Enum.GetName(typeof(SpAdvNames), charNum)] <= 0)
		{
			Debug.Log("SelectSpAdv - charNum = " + charNum + ", bindingJsonFirst[] =  " + System.Enum.GetName(typeof(SpAdvNames), charNum) + " ToLower - " + System.Enum.GetName(typeof(SpAdvNames), charNum).ToLower());
			DialogManager.Instance.StartDialog(dialogBindingJson[System.Enum.GetName(typeof(SpAdvNames), charNum).ToLower()][0]["introduce"]);
			if (charNum == 6)// 6 == nyang
			{
				characterDialogProgressData[System.Enum.GetName(typeof(SpAdvNames), 7)]++;
			}
			else if (charNum == 7)// 7 == wal
			{
				characterDialogProgressData[System.Enum.GetName(typeof(SpAdvNames), 6)]++;
			}
			else
			{
				characterDialogProgressData[System.Enum.GetName(typeof(SpAdvNames), charNum)]++;
			}

		}
	}
	#endregion
	public string GetCurSpAdvName()
	{
		return GameManager.Instance.GetPlayerSpAdvName();
	}
	public void LoadCharacterDialogProgressData(GameSavedata save)
	{
		characterDialogProgressData = save.characterDialogProgressData;
	}
	public void LoadExtDialogProgressData(GameSavedata save)
	{
		extDialogProgressData = save.extDialogProgressData;
	}
}
