﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traveler : Actor {
	//acting 구성
	//useStructure ~ 구현
	

	Tile destination = null;
	protected int pathFindCount = 0;
	Coroutine act;
	protected void Awake()
	{
		base.Awake();
	}
	// Use this for initialization
	void Start () {
		//_stat = GameManager.Instance.GetNewStats(Type Traveler);
		pathFinder.SetValidateTile(ValidateNextTile);
		SetPathFindEvent();
	}
	public void OnEnable()
	{
		act = StartCoroutine(Act());
	}
	public void OnDisable()
	{
		StopCoroutine(act);
		//골드, 능력치 초기화...  // current , origin 따로둬야할까?
	}
	public Stat stat
	{
		get
		{
			return _stat;
		}
	}

	private Stat _stat;

	IEnumerator Act()
	{
		Structure[] structureListByPref;
		while(true)
		{
			yield return null;
			switch(state)
			{
				case State.Idle:
					structureListByPref = StructureManager.Instance.FindStructureByDesire(stat.GetHighestDesire(), stat); // 1위 욕구에 따라 타입 결정하고 정렬된 건물 List 받아옴
					
					destination = pathFindCount > structureListByPref.Length ? structureListByPref[pathFindCount].GetEntrance() : GameManager.Instance.GetRandomEntrance(); 
					// list 순회 다했는데도 맞는 건물이 없다면 퇴장..
					yield return StartCoroutine(pathFinder.Moves(curTile, destination));
					//길찾기 후 State = Moving으로 변경.
					//길 못찾음 Event 처리...(다음 건물로 건너뛰어야함.
					//delegate call됨()
					break;
				case State.Moving:
					//찾은 경로를 통해 1칸씩 이동? 혹은 한번에(코루틴 통해) 이동.
					break;
				case State.Indoor:
					//건물 들어가서 계산을 마치고 invisible로 건물
					break;
				case State.Exit:
					break;
				default:
					break;
			}
		}
	}
	
	
	public override void SetPathFindEvent()
	{
		pathFinder.SetNotifyEvent(PathFindSuccess, PathFindFail);
	}

	public void PathFindSuccess()
	{
		pathFindCount = 0;
		state = State.Moving;
	}
	public void PathFindFail()
	{
		pathFindCount++;
		state = State.Idle;
	}

	public override bool ValidateNextTile(Tile tile) // Pathfinder delegate
	{
		if (tile.GetPassable())
			return true;
		return false;
	}

	

	

	
	
}
