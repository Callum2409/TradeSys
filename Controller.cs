using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Goods
{
	public string name;
	public int basePrice;
	public int minPrice;
	public int maxPrice;
	public float mass;
	public float average;
	public int unit;
	public int postCount;
}

[System.Serializable]
public class Trade
{
	public GameObject postA;
	public GameObject postB;
	public int typeID;
}

[System.Serializable]
public class Trading
{
	public GameObject sellPost;
	public GameObject buyPost;
	public int number;
	public int typeID;
}

[System.Serializable]
public class Poss
{
	public float distance;
	public int cNo;
	public int tNo;
}

[System.Serializable]
public class NeedMake
{
	public int item;
	public int number;
}

[System.Serializable]
public class Items
{
	public string name;
	public List<NeedMake> needing;
	public List<NeedMake> making;
}

[System.Serializable]
public class Unit
{
	public string suffix;
	public float min;
	public float max;
}

public class Controller : MonoBehaviour
{
	#region initialize
	public bool settings, pauseBeforeStart, expendable, showE;
	public int maxNoTraders = 100;
	GameObject allTraders;
	public List<GameObject> expendableT = new List<GameObject> ();
	public List<Goods> goods = new List<Goods> ();
	public List<GameObject> posts = new List<GameObject> ();
	public List<GameObject> traders = new List<GameObject> ();
	List<Trade> buy = new List<Trade> ();
	List<Trade> sell = new List<Trade> ();
	List<Trade> compare = new List<Trade> ();
	internal List<Trading> ongoing = new List<Trading> ();
	List<Poss> poss = new List<Poss> ();
	internal List<Trade> moving = new List<Trade> ();
	public List<Items> manufacturing = new List<Items> ();
	public bool showAllG, showAllM, showS, showM, showP, showAG, showAllU, showHoriz;
	public List<bool> showSmallG = new List<bool> ();
	public List<bool> showSmallM = new List<bool> ();
	public List<string> allNames;
	public List<Unit> units;
	#endregion
	
	void Awake ()
	{		
		posts = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Trade Post"));
		if (!expendable)
			traders = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Trader"));
		
		for (int x = 0; x <goods.Count; x++) 
			Average(x);
		if (expendable)
			allTraders = new GameObject ("Traders");
	}
	
	void Update ()
	{		
		UpdateLists ();
		TradeCall ();
	}
	
	public void UpdateAverage (int productID, int changeI, int changeP)
	{
		goods [productID].average = ((goods [productID].average * goods [productID].postCount + changeI) / (goods [productID].postCount+ changeP));
		goods[productID].postCount += changeP;
		UpdateLists ();
		for (int p = 0; p<posts.Count; p++)
			posts [p].GetComponent<TradePost> ().UpdatePrice ();
	}
	
	void Average (int productID)
	{//can only be called once at the start because does not include what is being carried
		int total = 0;
		int stocked = 0;
		for (int p = 0; p<posts.Count; p++) {
			TradePost postScript = posts [p].GetComponent<TradePost> ();
			if (postScript.stock [productID].allow) {
				total += posts [p].GetComponent<TradePost> ().stock [productID].number;
				stocked++;
			}
		}
		goods [productID].postCount = stocked;
		if (total > 0)
			goods [productID].average = total / (stocked*1f);
	}
	
	void UpdateLists ()
	{
		for (int g = 0; g<goods.Count; g++) {
			#region add to lists
			for (int p = 0; p<posts.Count; p++) {
				TradePost post = posts [p].GetComponent<TradePost> ();
				
				if (post.stock [g].number > Mathf.RoundToInt (goods [g].average * 1.5f) && 
					!sell.Exists (x => x.postA == posts [p] && x.typeID == g) && post.stock[g].allow) 
					sell.Add (new Trade{postA = posts [p], typeID = g});
				if (Mathf.RoundToInt (post.stock [g].number * 1.5f) < goods [g].average && 
					!buy.Exists (x => x.postA == posts [p] && x.typeID == g) && post.stock[g].allow) 
					buy.Add (new Trade{postA = posts [p], typeID = g});
			}
			#endregion
			#region remove from lists
			for (int s = 0; s < sell.Count; s++) {
				if (sell [s].typeID == g && 
				!(sell [s].postA.GetComponent<TradePost> ().stock [g].number > Mathf.RoundToInt (goods [g].average * 1.5f))) {
					sell.RemoveAt (s);
					break;
				}
			}
		
			for (int b = 0; b < buy.Count; b++) {
				if (buy [b].typeID == g && 
					!(Mathf.RoundToInt (buy [b].postA.GetComponent<TradePost> ().stock [g].number * 1.5f) < goods [g].average)) {
					buy.RemoveAt (b);
					break;
				}
			}
			#endregion
			#region remove compare
			for (int c = 0; c< compare.Count; c++) {
				if (compare [c].typeID == g &&
					(!(compare [c].postA.GetComponent<TradePost> ().stock [g].number > Mathf.RoundToInt (goods [g].average * 1.5f)) ||
					 !(Mathf.RoundToInt (compare [c].postB.GetComponent<TradePost> ().stock [g].number * 1.5f) < goods [g].average))) {
					compare.RemoveAt (c);
					break;
				}
			}
			#endregion
		}
		#region add to compare
		for (int s=0; s<sell.Count; s++) {
			for (int b = 0; b<buy.Count; b++) {
				if (sell [s].typeID == buy [b].typeID &&
					!compare.Exists (x => x.postA == sell [s].postA && x.postB == buy [b].postA && x.typeID == sell [s].typeID) &&
					CheckLocation (sell [s].postA, buy [b].postA, sell [s].typeID)) {
					compare.Add (new Trade{postA = sell [s].postA, postB = buy [b].postA, typeID = sell [s].typeID});
					
				}
			}
		}
		#endregion
	}
	
	bool CheckLocation (GameObject check, GameObject buyPost, int typeID)
	{
		for (int x = 0; x< compare.Count; x++) {
			if (compare [x].typeID == typeID && compare [x].postB == buyPost) {
				if (Vector3.Distance (check.transform.position, buyPost.transform.position) >= 
					Vector3.Distance (compare [x].postA.transform.position, buyPost.transform.position))
					return false;
				else
					compare.RemoveAt (x);
			}
		}
		return true;
	}
	
	public void TradeCall ()
	{
		float shortest;
		int cNo = -1;
		int tNo = -1;
		bool toAdd;
		if (expendable) {
			for (int c = 0; c<compare.Count; c++) {
				List<Trade> sameLocations = compare.FindAll (x => x.postA == compare [c].postA && x.postB == compare [c].postB);
				if (!ongoing.Exists (x => x.buyPost == compare [c].postB && x.typeID == compare [c].typeID))
					TraderSet (null, sameLocations, c);
			}
		} else {
			for (int t = 0; t<traders.Count; t++) {
				Trader traderScript = traders [t].GetComponent<Trader> ();
				shortest = 0;
				toAdd = false;
				if (!traderScript.onCall) {
					if (compare.Exists (x => x.postA == traderScript.targetPost)) {
					#region post in compare
						int c = compare.FindIndex (x => x.postA == traderScript.targetPost);
						List<Trade> sameLocations = compare.FindAll (x => x.postA == traderScript.targetPost && x.postB == compare [c].postB);

						TraderSet (traders [t], sameLocations, c);
						#endregion
					} else {//end if post not in compare => need to move to new post
						for (int c = 0; c<compare.Count; c++) {
							if (!ongoing.Exists (x => x.buyPost == compare [c].postB && x.typeID == compare [c].typeID) && !moving.Exists (x => x.postB == compare [c].postA)) {
								float distance = Vector3.Distance (traderScript.targetPost.transform.position, compare [c].postA.transform.position);
								if ((distance < shortest || shortest == 0)) {//check distances
									if (!poss.Exists (x => x.cNo == c)) {//check that compare has not previously been added
										shortest = distance;
										cNo = c;
										tNo = t;
										toAdd = true;
									} else {
										int index = poss.FindIndex (x => x.cNo == c);
										if (distance < poss [index].distance) {//current distance is less
											shortest = distance;
											tNo = t;
										
										} else {
											shortest = poss [index].distance;
											tNo = poss [index].tNo;
										}
										cNo = c;
										poss.RemoveAt (index);
										toAdd = true;
									}//end else compare added
								}//end check distance
							}//end if not in ongoing
						}//end for compare
					}//end if compare not in ongoing
					if (toAdd) {
						poss.Add (new Poss{distance = shortest, cNo = cNo, tNo = tNo});
						toAdd = false;
					}
				}//end if not on call
			}//end for traders
			ExecuteMove ();
		}//end else not expendable
	}
	
	void TraderSet (GameObject trader, List<Trade> sameLocations, int c)
	{
		if (expendable) {
			if (maxNoTraders == 0 || GameObject.FindGameObjectsWithTag ("Trader").Length < maxNoTraders) {
				int random = UnityEngine.Random.Range (0, expendableT.Count);
				if (expendableT [random] == null) {
					Debug.LogError ("One of your trader types has not been set\nPlease make sure that all trader types have been set");
					return;
				}
				trader = (GameObject)Instantiate (expendableT [random], sameLocations [0].postA.transform.position, Quaternion.identity);
				trader.transform.parent = allTraders.transform;
			} else
				return;
		}
		Trader traderScript = trader.GetComponent<Trader> ();
		
		for (int a = 0; a < sameLocations.Count; a++) {
			if (!ongoing.Exists (x => x.buyPost == sameLocations [a].postB && x.typeID == sameLocations [a].typeID)) {
				
				TradePost postA = sameLocations [a].postA.GetComponent<TradePost> ();
						
				int goodsNo = sameLocations[a].typeID;
						
				float average = goods [goodsNo].average;
				int sellQuantity = postA.stock [goodsNo].number - Mathf.RoundToInt (average * 1.5f);
				int buyQuantity = Mathf.RoundToInt(average - (compare [c].postB.GetComponent<TradePost> ().stock [goodsNo].number * 1.5f));				
				int quantity = (int)Mathf.Min ((int)((sellQuantity + buyQuantity) / 1.5f), Mathf.Floor (traderScript.spaceRemaining / goods [goodsNo].mass));

				if (quantity > 0) {
					postA.stock [goodsNo].number -= quantity;
							
					ongoing.Add (new Trading{sellPost = sameLocations [a].postA, buyPost = sameLocations [a].postB, number = quantity, typeID = sameLocations [a].typeID});
						
					traderScript.onCall = true;
					traderScript.trading.Add (new NoType{number = quantity, goodID = sameLocations [a].typeID});
					traderScript.spaceRemaining -= quantity * (float)goods [goodsNo].mass;
					traderScript.targetPost = sameLocations [a].postB;
					sameLocations [a].postA.GetComponent<TradePost> ().UpdatePrice ();
					if (traderScript.spaceRemaining == 0)
						break;
				}
			}//end if not in ongoing
		}//end for all compares to same location	
		if (expendable && traderScript.targetPost == null)
			Destroy (trader);
	}
	
	void ExecuteMove ()
	{
		for (int p = 0; p<poss.Count; p++) {
			if (poss [p].cNo < compare.Count) {
				Trader traderScript = traders [poss [p].tNo].GetComponent<Trader> ();
				GameObject targetPost = compare [poss [p].cNo].postA;
				
				moving.Add (new Trade{postA = traderScript.targetPost, postB = targetPost});
				traderScript.targetPost = targetPost;
				traderScript.onCall = true;
			}
		}
		poss.Clear();
	}
}