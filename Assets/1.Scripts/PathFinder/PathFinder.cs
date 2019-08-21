﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class PathFinder : MonoBehaviour
{
	public struct openListVisited
	{
		public bool isVisited;
		public bool isClosed;
		public int F;
		public openListVisited(bool _isVisited, bool _isClosed, int _F)
		{
			isVisited = _isVisited;
			isClosed = _isClosed;
			F = _F;
		}
	}
	public bool isNoPath = false;
	enum Direction {left, right, up, down};
	static Vector2[] DirectionVectors = { new Vector2(-1.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, -1.0f) };
	Dictionary<int, PathVertex> closeList = new Dictionary<int, PathVertex>();
	PriorityQueue<PathVertex> openList = new PriorityQueue<PathVertex>();
	List<PathVertex> path = new List<PathVertex>();
	public openListVisited[,] visited;

	int bestKey = 0;
	PathVertex latest = null;
	PathVertex best = null;
	PathVertex latest_simulate = null;

	public Tile myCurPos;
	public TileForMove myCurPosForMove;
	public Tile destination;
	public Tile nextMovePos;
	Tile next;

	public TileLayer tileLayer; //for Test
	bool isCal = false;
	public bool found = false;
	public bool isAdventurer = false;
	System.Random rd = new System.Random();
	public int id;
	int index = 0;
	int parentIndex = 0;
	WaitForSeconds wait;
	ThreadStart ts;
	Thread t;
	PathVertex tempVertex;

	void Start()
	{
		tileLayer = GameManager.Instance.GetMap().GetLayer(0).GetComponent<TileLayer>();
		id = gameObject.GetInstanceID();
		wait = new WaitForSeconds(0.5f);
		visited = new openListVisited[GameManager.Instance.GetMap().GetLayer(0).GetComponent<TileLayer>().GetLayerHeight(), GameManager.Instance.GetMap().GetLayer(0).GetComponent<TileLayer>().GetLayerWidth()];
		for (int i = 0; i < GameManager.Instance.GetMap().GetLayer(0).GetComponent<TileLayer>().GetLayerHeight(); i++)
		{
			for (int j = 0; j < GameManager.Instance.GetMap().GetLayer(0).GetComponent<TileLayer>().GetLayerWidth(); j++)
			{
				visited[i, j] = new openListVisited(false, false, 0);
			}
		}	
	}

	public IEnumerator Moves()
	{
		yield return null;

		for (int i = 0; i < path.Count; i++)
		{
			path[i].ClearReference();
		}
		path.Clear();
		closeList.Clear();
		openList.HalfClear();
		ClearVisited();
		found = false;
		ThreadPool.UnsafeQueueUserWorkItem(this.Simulate, null);
		while (found == false)
		{
			//Debug.Log("found == false While looop");
			yield return wait;
		}

		PathVertex trace = latest;
		while (trace != null)
		{
			path.Add(trace);
			trace = trace.Parent;
			yield return null;
		}
		path.Reverse();
		
		closeList.Clear();
		for (int i = 0; i < openList.Count; i++)
		{
			openList[i].ClearReference();
		}
		for (int i = 0; i < closeList.Count; i++)
		{
			closeList[i].ClearReference();
		}
		
	}

	public IEnumerator MoveinNoPath()
	{
		yield return null;

		path.Clear();
		closeList.Clear();
		openList.Clear();
		//closeDic.Clear();
		//openDic.Clear();
		yield return StartCoroutine(SimulateinNoPath());
		//ThreadStart ts = new ThreadStart(SimulateinNoPath);
		//Thread t = new Thread(ts);
		//t.Start();
		found = false;
		while (found == false)
		{
			yield return null;
		}
		PathVertex trace = latest;
		while (trace != null)
		{
			yield return null;
			path.Add(trace);
			trace = trace.Parent;

		}
		path.Reverse();

	}

	IEnumerator SimulateinNoPath()
	{
		yield return null;
		Debug.Log("NoPath!");
		/*latest_simulate = new PathVertex(null, myCurPos, destination);
		
		closeList.Add(latest_simulate);
		//closeDic.Add(latest_simulate.F, new List<PathVertex>());
		//closeDic[latest_simulate.F].Add(latest_simulate);
		
        int t = 0;
        
        while (!latest_simulate.curTile.Equals(destination))
        {
			yield return null;
            t++;
            
            for (int i = 0; i < 4; i++)
            {
                if ((next = t1.GetTileForMove((int)(latest_simulate.curTile.GetX() + DirectionVectors[i].x), (int)(latest_simulate.curTile.GetY() + DirectionVectors[i].y))) != null && next.GetPassableParent()) //+Exception
                {
                    //PathVertex nextVertex = new PathVertex(latest_simulate, next, destination);
                    AddOpenList(new PathVertex(latest_simulate, next, destination));
                }
                else if((next = t1.GetTileForMove((int)(latest_simulate.curTile.GetX() + DirectionVectors[i].x), (int)(latest_simulate.curTile.GetY() + DirectionVectors[i].y))) != null && next.GetPassableParent() == false && 
                    t1.GetTileForMove((int)(latest_simulate.curTile.GetX() + DirectionVectors[i].x), (int)(latest_simulate.curTile.GetY() + DirectionVectors[i].y)).GetParent().GetNonTile() == false)//gameObject.tag != "non_Tile")
                {
                    //PathVertex nextVertex = new PathVertex(latest_simulate, next, destination, 1);
                    AddOpenList(new PathVertex(latest_simulate, next, destination, 1));
                }
            }
			
            best = null;
			if(openDic.Count != 0)
				best = 
            if (openList.Count != 0)
                best = openList[0];
            else
            {
                //길 못찾음.
                //Debug.Log("Cannot Found Path");
                found = true;
            }

            foreach (PathVertex vertex in openList)
            {
                if (best.F > vertex.F)
                    best = vertex;
                else if (best.F == vertex.F)
                {
                    //System.Random rd = new System.Random();

                    if ((rd.Next() % 3) == 1)//Random.RandomRange(1, 3) == 1)
                    {
                        best = vertex;
                    }
                }

            }

            openList.Remove(best);
            closeList.Add(best);
            latest_simulate = best;
        }
        latest = latest_simulate;
        found = true;*/
	}

	//IEnumerator Simulate()
	public void Simulate(System.Object threadContext)
	{
		latest_simulate = new PathVertex(null, myCurPos, destination);
		bestKey = latest_simulate.X * 1000 + latest_simulate.Y;
		closeList.Add(latest_simulate.X * 1000 + latest_simulate.Y, latest_simulate);

		visited[latest_simulate.Y, latest_simulate.X].isClosed = true;
		Debug.Log("in");
		while (!latest_simulate.curTile.Equals(destination))
		{
			for (int i = 0; i < 4; i++)
			{
				if ((next = tileLayer.GetTileAsComponent((int)(latest_simulate.myTilePos.GetX() + DirectionVectors[i].x), (int)(latest_simulate.myTilePos.GetY() + DirectionVectors[i].y))) != null && next.GetPassable())
				{
					if (visited[next.GetY(), next.GetX()].isClosed == false)
					{
						if (visited[next.GetY(), next.GetX()].isVisited == false)
						{
							if (openList.Count <= openList.useCount)
							{
								openList.Add(tempVertex = new PathVertex(latest_simulate, next, destination));
								visited[next.GetY(), next.GetX()].isVisited = true;
								visited[next.GetY(), next.GetX()].F = tempVertex.F;
							}
							else
							{
								openList[openList.useCount].ReUse(latest_simulate, next, destination);
								tempVertex = openList[openList.useCount];
								index = openList.useCount;
								openList.useCount++;
								bool keepGoing = true;

								while (keepGoing)
								{
									if (index == 0) break;
									parentIndex = (index - 1) / 2;
									keepGoing = openList.CompareAndSwap(index, parentIndex);
									if (keepGoing) index = parentIndex;
								}
								visited[next.GetY(), next.GetX()].isVisited = true;
								visited[next.GetY(), next.GetX()].F = tempVertex.F;
							}
						}
						else // visited 있는경우 - 최적의 값 비교.
						{
							// openList안에서 찾는것이 우선 . . .  F (F = G + H / 현재까지 온 거리 +남은 거리 최소값) 기준으로 정렬됨.

							if (visited[next.GetY(), next.GetX()].F > (latest_simulate.G + 1 + (Mathf.Abs(destination.GetX() - next.GetX()) + Mathf.Abs(destination.GetY() - next.GetY()))))
							{
								bool find = false;
								for (int u = 0; u < openList.useCount; u++)
								{
									if (openList[u].X == next.GetX() && openList[u].Y == next.GetY())
									{
										find = true;
										index = u;
										break;
									}
								}

								if (find == true)
								{
									openList.RemoveAt(index);

									if (openList.Count <= openList.useCount)
									{
										openList.Add(tempVertex = new PathVertex(latest_simulate, next, destination));
										visited[next.GetY(), next.GetX()].isVisited = true;
										visited[next.GetY(), next.GetX()].F = tempVertex.F;
									}
									else
									{
										//items.Add(item); 
										//reuse 할때도 priority queue ~~~
										openList[openList.useCount].ReUse(latest_simulate, next, destination);
										tempVertex = openList[openList.useCount];
										index = openList.useCount;
										openList.useCount++;
										bool keepGoing = true;

										while (keepGoing)
										{
											if (index == 0) break;
											parentIndex = (index - 1) / 2;
											keepGoing = openList.CompareAndSwap(index, parentIndex);
											if (keepGoing) index = parentIndex;
										}
										visited[next.GetY(), next.GetX()].isVisited = true;
										visited[next.GetY(), next.GetX()].F = tempVertex.F;
									}
								}

							}
						}
					}
				}
			}
			if (openList.useCount != 0)
			{
				best = openList.Pop();
				bestKey = best.X * 1000 + best.Y;
			}
			else
			{
				isNoPath = true;
				found = true;
				break;
			}
			if (!closeList.ContainsKey(bestKey))
			{
				closeList.Add(bestKey, best);
				best.curTile.AddedCloseList();
			}
			latest_simulate = best;
		}
		latest = latest_simulate;
		found = true;
	}

	void AddOpenList(PathVertex newVertex)
	{
		if (closeList.ContainsKey(newVertex.X * 1000 + newVertex.Y))
		{

			return;
		}
		openList.Add(newVertex);
		newVertex.curTile.AddedOpenList();
	}

	public Tile GetCurTile()
	{
		return myCurPos;
	}
	public void SetCurTile(Tile t)
	{
		myCurPos = t;
	}
	public TileForMove GetCurTileForMove()
	{
		return myCurPosForMove;
	}
	public void SetCurTileForMove(TileForMove t)
	{
		myCurPosForMove = t;
	}
	public Tile GetDestination()
	{
		return destination;
	}
	public void SetDestination(Tile t)
	{
		destination = t;
	}
	public List<PathVertex> GetPath()
	{
		return path;
	}
	public void ClearVisited()
	{
		for (int i = 0; i < visited.GetLength(0); i++)
		{
			for (int j = 0; j < visited.GetLength(1); j++)
			{
				visited[i, j].isClosed = false;
				visited[i, j].isVisited = false;
			}
		}
	}
}

