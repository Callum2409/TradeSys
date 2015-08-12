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
	public int average;
}

[System.Serializable]
public class Trade
{
	public GameObject postA;
	public GameObject postB;
	public string type;
}

[System.Serializable]
public class Trading
{
	public GameObject sellPost;
	public GameObject buyPost;
	public int number;
	public string type;
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
	public bool showAllG, showAllM, showS, showM, showP, showAG;
	public List<bool> showSmallG = new List<bool> ();
	public List<bool> showSmallM = new List<bool> ();
	public List<string> allNames;
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
	
	public void UpdateAverage (int productID)
	{	
		Average (productID);
		UpdateLists ();
		for (int p = 0; p<posts.Count; p++)
			posts [p].GetComponent<TradePost> ().UpdatePrice ();
	}
	
	void Average (int productID)
	{
		int total = 0;
		int stocked = 0;
		for (int p = 0; p<posts.Count; p++) {
			TradePost postScript = posts [p].GetComponent<TradePost> ();
			if (postScript.allowGoods [productID]) {
				total += posts [p].GetComponent<TradePost> ().stock [productID].number;
				stocked++;
			}
		}
		goods [productID].average = total / stocked;
	}
	
	void UpdateLists ()
	{
		for (int g = 0; g<goods.Count; g++) {
			#region add to lists
			for (int p = 0; p<posts.Count; p++) {
				TradePost post = posts [p].GetComponent<TradePost> ();
				
				if (post.stock [g].number > Mathf.RoundToInt (goods [g].average * 1.5f) && 
					!sell.Exists (x => x.postA == posts [p] && x.type == goods [g].name) && post.allowGoods[g]) 
					sell.Add (new Trade{postA = posts [p], type = goods [g].name});
				if (Mathf.RoundToInt (post.stock [g].number * 1.5f) < goods [g].average && 
					!buy.Exists (x => x.postA == posts [p] && x.type == goods [g].name) && post.allowGoods[g]) 
					buy.Add (new Trade{postA = posts [p], type = goods [g].name});
			}
			#endregion
			#region remove from lists
			for (int s = 0; s < sell.Count; s++) {
				if (sell [s].type == goods [g].name && 
				!(sell [s].postA.GetComponent<TradePost> ().stock [g].number > Mathf.RoundToInt (goods [g].average * 1.5f))) {
					sell.RemoveAt (s);
					break;
				}
			}
		
			for (int b = 0; b < buy.Count; b++) {
				if (buy [b].type == goods [g].name && 
					!(Mathf.RoundToInt (buy [b].postA.GetComponent<TradePost> ().stock [g].number * 1.5f) < goods [g].average)) {
					buy.RemoveAt (b);
					break;
				}
			}
			#endregion
			#region remove compare
			for (int c = 0; c< compare.Count; c++) {
				if (compare [c].type == goods [g].name &&
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
				if (sell [s].type == buy [b].type &&
					!compare.Exists (x => x.postA == sell [s].postA && x.postB == buy [b].postA && x.type == sell [s].type) &&
					CheckLocation (sell [s].postA, buy [b].postA, sell [s].type)) {
					compare.Add (new Trade{postA = sell [s].postA, postB = buy [b].postA, type = sell [s].type});
					
				}
			}
		}
		#endregion
	}
	
	bool CheckLocation (GameObject check, GameObject buyPost, string type)
	{
		for (int x = 0; x< compare.Count; x++) {
			if (compare [x].type == type && compare [x].postB == buyPost) {
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
				if (!ongoing.Exists (x => x.buyPost == compare [c].postB && x.type == compare [c].type))
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
							if (!ongoing.Exists (x => x.buyPost == compare [c].postB && x.type == compare [c].type) && !moving.Exists (x => x.postB == compare [c].postA)) {
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
			if (!ongoing.Exists (x => x.buyPost == sameLocations [a].postB && x.type == sameLocations [a].type)) {
				
				TradePost postA = sameLocations [a].postA.GetComponent<TradePost> ();
						
				int goodsNo = goods.FindIndex (x => x.name == sameLocations [a].type);
						
				int average = goods [goodsNo].average;
				int sellQuantity = postA.stock [goodsNo].number - Mathf.RoundToInt (average * 1.5f);
				int buyQuantity = average - Mathf.RoundToInt (compare [c].postB.GetComponent<TradePost> ().stock [goodsNo].number * 1.5f);				
				int quantity = (int)Mathf.Min ((int)((sellQuantity + buyQuantity) / 1.5f), Mathf.Floor (traderScript.spaceRemaining / goods [goodsNo].mass));

				if (quantity > 0) {
					postA.stock [goodsNo].number -= quantity;
					postA.UpdatePrice ();
							
					ongoing.Add (new Trading{sellPost = sameLocations [a].postA, buyPost = sameLocations [a].postB, number = quantity, type = sameLocations [a].type});
						
					traderScript.onCall = true;
					traderScript.trading.Add (new NoType{number = quantity, type = sameLocations [a].type});
					traderScript.spaceRemaining = traderScript.cargoSpace - quantity * goods [goodsNo].mass;
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
				
				moving.Add (new Trade{postA = traderScript.targetPost, postB = targetPost, type = traderScript.name});
				traderScript.targetPost = targetPost;
				traderScript.onCall = true;
			}
		}
		poss.Clear();
	}
}