using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

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
	public GameObject itemCrate;
}

[System.Serializable]
public class Trade : IEquatable<Trade>
{
	public int postA;
	public int postB;
	public int typeID;
	
	public bool Equals (Trade other)
	{
		if (System.Object.ReferenceEquals (other, null))
			return false;
		if (System.Object.ReferenceEquals (this, other))
			return true;
		return 	postA.Equals (other.postA) &&
				postB.Equals (other.postB) &&
				typeID.Equals (other.typeID);
	}
	
	public override int GetHashCode ()
	{
		int hashPostA = postA == -1 ? 0 : postA.GetHashCode ();
		int hashPostB = postB == -1 ? 0 : postB.GetHashCode ();
		int hashTypeID = typeID == -1 ? 0 : typeID.GetHashCode ();
		
		return hashPostA ^ hashPostB ^ hashTypeID;
	}
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

[System.Serializable]
public class Spawned
{
	public GameObject spawner;
	public GameObject item;
	public int goodID;
}

[System.Serializable]
public class Faction
{
	public string name;
	public Color color;
}

public class Controller : MonoBehaviour
{
	#region initialize
	public bool pauseBeforeStart, expendable, allowPickup, allowGroups, allowFactions;//USED IN GAME
	public int maxNoTraders = 100;
	GameObject allTraders;
	public List<GameObject> expendableT = new List<GameObject> ();
	public List<Goods> goods = new List<Goods> ();
	internal Goods[] goodsArray;
	public List<GameObject> posts = new List<GameObject> ();
	public List<TradePost> postScripts = new List<TradePost> ();
	public List<GameObject> traders = new List<GameObject> ();
	public List<Trader> traderScripts = new List<Trader> ();
	List<Trade> buy = new List<Trade> ();
	List<Trade> sell = new List<Trade> ();
	List<Trade> compare = new List<Trade> ();
	internal List<Trading> ongoing = new List<Trading> ();
	List<Poss> poss = new List<Poss> ();
	internal List<Trade> moving = new List<Trade> ();
	public List<Items> manufacturing = new List<Items> ();
	public List<bool> showSmallG = new List<bool> ();
	public List<bool> showSmallM = new List<bool> ();
	public List<string> allNames;
	public List<Unit> units = new List<Unit>();
	public bool showE, showP, showAU, showHoriz, showG, showF, showR, showRP, showPG, showPF, showPE, showTF;//USED FOR EDITOR
	public int  selC, selP;//USED IN EDITOR
	public List<Spawned> spawned = new List<Spawned> ();
	public float updateInterval = 0.5f;
	public List<String> groups = new List<String> ();
	public List<Faction> factions = new List<Faction> ();
	#endregion
	
	void Awake ()
	{		
		posts = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Trade Post"));
		for (int p = 0; p<posts.Count; p++)  
			postScripts.Add (posts [p].GetComponent<TradePost> ());
		
		if (!expendable) { 
			traders = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Trader"));
			for (int t = 0; t<traders.Count; t++) {
				traderScripts.Add (traders [t].GetComponent<Trader> ());
				if (traderScripts [t] == null)
					Debug.LogError ("One of the traders does not have a Trader component.\nPlease add the Trader script to " + traders[t].name);
			}
		}
		
		goodsArray = goods.ToArray ();
		for (int x = 0; x <goodsArray.Length; x++) 
			Average (x);
		if (expendable)
			allTraders = new GameObject ("Traders");
		
		InvokeRepeating ("UpdateMethods", 0, updateInterval);
	}
	
	void UpdateMethods ()
	{
		for (int p = 0; p<postScripts.Count; p++)
			postScripts [p].UpdatePrice ();
		
		UpdateLists ();
		TradeCall ();
	}
	
	public void UpdateAverage (int productID, int changeI, int changeP)
	{
		goodsArray [productID].average = ((goodsArray [productID].average * goodsArray [productID].postCount + changeI) / (goodsArray [productID].postCount + changeP));
		goodsArray [productID].postCount += changeP;
	}
	
	void Average (int productID)
	{//can only be called once at the start because does not include what is being carried
		int total = 0;
		int stocked = 0;
		for (int p = 0; p<posts.Count; p++) {
			if (postScripts [p].stock [productID].allow) {
				total += postScripts [p].stock [productID].number;
				stocked++;
			}
		}
		goodsArray [productID].postCount = stocked;
		if (total > 0)
			goodsArray [productID].average = total / (stocked * 1f);
	}
	
	void UpdateLists ()
	{
		for (int g = 0; g<goodsArray.Length; g++) {
			#region add to lists
			for (int p = 0; p<postScripts.Count; p++) {
				Stock checking = postScripts [p].stock [g];
				if (checking.allow) {
					if (Mathf.RoundToInt (checking.number * 1.5f) < goodsArray [g].average) 
						buy.Add (new Trade{postB = p, typeID = g});
					else if (checking.number > Mathf.RoundToInt (goodsArray [g].average * 1.5f)) 
						sell.Add (new Trade{postA = p, typeID = g});
				}
			}
			#endregion
			#region remove from lists
			for (int s = 0; s < sell.Count; s++) {
				if (sell [s].typeID == g && 
				!(postScripts [sell [s].postA].stock [g].number > Mathf.RoundToInt (goodsArray [g].average * 1.5f))) {
					sell.RemoveAt (s);
					break;
				}
			}
		
			for (int b = 0; b < buy.Count; b++) {
				if (buy [b].typeID == g && 
					!(Mathf.RoundToInt (postScripts [buy [b].postB].stock [g].number * 1.5f) < goodsArray [g].average)) {
					buy.RemoveAt (b);
					break;
				}
			}
			#endregion
			#region remove compare
			for (int c = 0; c< compare.Count; c++) {
				if (compare [c].typeID == g &&
					(!(postScripts [compare [c].postA].stock [g].number > Mathf.RoundToInt (goodsArray [g].average * 1.5f)) ||
					 !(Mathf.RoundToInt (postScripts [compare [c].postB].stock [g].number * 1.5f) < goodsArray [g].average))) {
					compare.RemoveAt (c);
					break;
				}
			}
			#endregion
		}
		buy = buy.Distinct ().ToList ();
		sell = sell.Distinct ().ToList ();
		#region add to compare
		for (int s=0; s<sell.Count; s++) {
			for (int b = 0; b<buy.Count; b++) {
				if (sell [s].typeID == buy [b].typeID &&
					CheckGroupsFactions (postScripts[buy [b].postB], postScripts[sell [s].postA]) &&
					CheckLocation (sell [s].postA, buy [b].postB, sell [s].typeID)) {
					compare.Add (new Trade{postA = sell [s].postA, postB = buy [b].postB, typeID = sell [s].typeID});
				}
			}
		}
		compare = compare.Distinct ().ToList ();
		#endregion
	}
	
	public bool CheckGroupsFactions (TradePost postA, TradePost postB)
	{
		if (CheckGroups (postA, postB) && CheckFactions (postA, postB))
			return true;
		else
			return false;
	}
	
	bool CheckGroups (TradePost postA, TradePost postB)
	{
		if (!allowGroups)
			return true;
		else {
			for (int g = 0; g<groups.Count; g++) {
				if (postA.groups [g] && postB.groups [g])
					return true;
			}
			return false;
		}
	}
	
	bool CheckFactions (TradePost postA, TradePost postB)
	{
		if (!allowFactions)
			return true;
		else {
			for (int f = 0; f<factions.Count; f++) {
				if (postA.factions [f] && postB.factions [f])
					return true;
			}
			return false;
		}
	}
	
	bool CheckLocation (int check, int buyPost, int typeID)
	{
		for (int x = 0; x< compare.Count; x++) {
			if (compare [x].typeID == typeID && compare [x].postB == buyPost) {
				if (Vector3.Distance (posts [check].transform.position, posts [buyPost].transform.position) >= 
					Vector3.Distance (posts [compare [x].postA].transform.position, posts [buyPost].transform.position))
					return false;
				else {
					compare.RemoveAt (x);
				}
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
			if (expendableT.Count == 0)
				Debug.LogError ("No expendable traders have been set up. No traders will be created.\nAdd trader types in the controller inspector.");
			else {
				for (int c = 0; c<compare.Count; c++) {
					List<Trade> sameLocations = compare.FindAll (x => x.postA == compare [c].postA && x.postB == compare [c].postB);
					if (!ongoing.Exists (x => x.buyPost == posts [compare [c].postB] && x.typeID == compare [c].typeID))
						TraderSet (null, sameLocations, c, 0);
				}
			}
		} else {
			for (int t = 0; t<traders.Count; t++) {
				Trader traderScript = traderScripts [t];
				shortest = 0;
				toAdd = false;
				if (!traderScript.onCall) {
					TradePost targetScript = traderScript.target.GetComponent<TradePost> ();
					if (compare.Exists (x => posts [x.postA] == traderScript.target)) {
					#region post in compare
						int c = compare.FindIndex (x => posts [x.postA] == traderScript.target);
						if (TraderFaction (traderScript, postScripts[compare [c].postB])) {
							List<Trade> sameLocations = compare.FindAll (x => posts [x.postA] == traderScript.target && x.postB == compare [c].postB);

							TraderSet (traders [t], sameLocations, c, t);
						}//check trader in same faction
					#endregion
					} else {//end if post not in compare => need to move to new post
						for (int c = 0; c<compare.Count; c++) {
							TradePost comparePostScript = postScripts [compare [c].postA];
							if (!ongoing.Exists (x => x.buyPost == posts [compare [c].postB] && x.typeID == compare [c].typeID) && !moving.Exists (x => x.postB == compare [c].postA) &&
								CheckGroupsFactions (targetScript, comparePostScript) && TraderFaction(traderScript, comparePostScript)) {
								float distance = Vector3.Distance (traderScript.target.transform.position, posts [compare [c].postA].transform.position);
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
	
	void TraderSet (GameObject trader, List<Trade> sameLocations, int c, int traderID)
	{
		int random = 0;
		if (expendable) {
			if (maxNoTraders == 0 || GameObject.FindGameObjectsWithTag ("Trader").Length < maxNoTraders) {
				random = UnityEngine.Random.Range (0, expendableT.Count);
				if (expendableT [random] == null) {
					Debug.LogError ("One of your trader types has not been set\nPlease make sure that all trader types have been set");
					return;
				}
				trader = (GameObject)Instantiate (expendableT [random], posts [sameLocations [0].postA].transform.position, Quaternion.identity);
				trader.transform.parent = allTraders.transform;
			} else
				return;
			traderID = random;
		}
		
		Trader traderScript;
		if (expendable){ 
			traderScript = trader.GetComponent<Trader>( );
			traderScripts.Add(traderScript);
		}else
			traderScript = traderScripts [traderID];
		
		for (int a = 0; a < sameLocations.Count; a++) {
			if (!ongoing.Exists (x => x.buyPost == posts [sameLocations [a].postB] && x.typeID == sameLocations [a].typeID)) {
				
				TradePost postA = postScripts [sameLocations [a].postA];
						
				int goodsNo = sameLocations [a].typeID;
						
				float average = goodsArray [goodsNo].average;
				int sellQuantity = postA.stock [goodsNo].number - Mathf.RoundToInt (average * 1.5f);
				int buyQuantity = Mathf.RoundToInt (average - (postScripts [compare [c].postB].stock [goodsNo].number * 1.5f));				
				int quantity = (int)Mathf.Min ((int)((sellQuantity + buyQuantity) / 1.5f), Mathf.Floor (traderScript.spaceRemaining / goodsArray [goodsNo].mass));

				if (quantity > 0) {
					postA.stock [goodsNo].number -= quantity;
							
					ongoing.Add (new Trading{sellPost = posts [sameLocations [a].postA], buyPost = posts [sameLocations [a].postB], number = quantity, typeID = sameLocations [a].typeID});
						
					traderScript.onCall = true;
					traderScript.trading.Add (new NoType{number = quantity, goodID = sameLocations [a].typeID});
					traderScript.spaceRemaining -= quantity * (float)goodsArray [goodsNo].mass;
					traderScript.finalPost = traderScript.target = posts [sameLocations [a].postB];
					postScripts [sameLocations [a].postA].UpdatePrice ();
					if (traderScript.spaceRemaining == 0)
						break;
				}
			}//end if not in ongoing
		}//end for all compares to same location	
		if (expendable && traderScript.target == null){
			traderScripts.Remove(traderScript);
			Destroy (trader);}
	}
	
	void ExecuteMove ()
	{
		for (int p = 0; p<poss.Count; p++) {
			if (poss [p].cNo < compare.Count) {
				Trader traderScript = traderScripts [poss [p].tNo];
				GameObject target = posts [compare [poss [p].cNo].postA];
				
				moving.Add (new Trade{postA = posts.FindIndex (x => x == traderScript.target), postB = posts.FindIndex (x => x == target), typeID = traders.FindIndex (x => x == traders [poss [p].tNo])});
				traderScript.finalPost = traderScript.target = target;
				traderScript.onCall = true;
			}
		}
		poss.Clear ();
	}
	
	bool TraderFaction (Trader traderScript, TradePost targetPost)
	{
		if(!allowFactions)
			return true;
		else{
		for (int f = 0; f<factions.Count; f++)
			if (traderScript.factions [f] && targetPost.factions [f])return true;
		return false;
		}
	}
}