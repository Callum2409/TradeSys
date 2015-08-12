using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Goods {
	public string name;
	public int basePrice;
	public int minPrice;
	public int maxPrice;
	public float mass;
	public int average;
}

[System.Serializable]
public class Trade {
	public GameObject postA;
	public GameObject postB;
	public string type;
}

[System.Serializable]
public class Trading {
	public GameObject sellPost;
	public GameObject buyPost;
	public int number;
	public string type;
}

[System.Serializable]
public class Poss {
	public float distance;
	public int cNo;
	public int tNo;
}

[System.Serializable]
public class NeedMake {
	public int item;
	public int number;
}

[System.Serializable]
public class Items {
	public string name;
	public List<NeedMake> needing;
	public List<NeedMake> making;
}

public class SuperBool{
	public bool showMain;
	public bool showNeed;
	public bool showMake;
}

public class Controller : MonoBehaviour {
	#region initialize
	public List<Goods> goods = new List<Goods> ();
	public List<GameObject> posts;
	public List<GameObject> traders;
	List<Trade> buy = new List<Trade> ();
	List<Trade> sell = new List<Trade> ();
	List<Trade> compare = new List<Trade> ();
	public List<Trading> ongoing = new List<Trading> ();
	List<Poss> poss = new List<Poss> ();
	public List<Trade> moving = new List<Trade> ();
	public List<Items> manufacturing = new List<Items>();
	
	public bool showAllG;
	public List<bool> showSmallG;
	public bool showAllM;
	public List<bool> showSmallM;
	
	public List<string> allNames;
	#endregion
	
	void Awake () {		
		posts = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Trade Post"));
		traders = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Trader"));
		
		for (int x = 0; x <goods.Count; x++) {
			int total = 0;
			for (int y = 0; y<posts.Count; y++)
				total += posts [y].GetComponent<TradePost> ().stock [x].number;
			goods [x].average = total / posts.Count;
		}
	}
	
	void Update ()
	{		
		UpdateLists ();
		TradeCall ();
	}
	
	public void UpdateAverage (int productID) {
		int total = 0;
		for (int p = 0; p<posts.Count; p++)
			total += posts [p].GetComponent<TradePost> ().stock [productID].number;
		goods [productID].average = total / posts.Count;
		
		UpdateLists ();
		TradeCall ();
		for (int p = 0; p<posts.Count; p++)
			posts [p].GetComponent<TradePost> ().UpdatePrice ();
	}
	
	void UpdateLists () {
		for (int g = 0; g<goods.Count; g++) {
			#region add to lists
			for (int p = 0; p<posts.Count; p++) {
				TradePost post = posts [p].GetComponent<TradePost> ();
				
				if (post.stock [g].number > Mathf.RoundToInt (goods [g].average * 1.5f) && 
					!sell.Exists (x => x.postA == posts [p] && x.type == goods [g].name)) 
					sell.Add (new Trade{postA = posts [p], type = goods [g].name});
				if (Mathf.RoundToInt (post.stock [g].number * 1.5f) < goods [g].average && 
					!buy.Exists (x => x.postA == posts [p] && x.type == goods [g].name)) 
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
	
	bool CheckLocation (GameObject check, GameObject buyPost, string type) {
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
		for (int t = 0; t<traders.Count; t++) {
			Trader traderScript = traders [t].GetComponent<Trader> ();
			shortest = 0;
			toAdd = false;
			if (!traderScript.onCall) {
				if (compare.Exists (x => x.postA == traderScript.targetPost)) {
					#region post in compare
					int c = compare.FindIndex (x => x.postA == traderScript.targetPost);
					if (!ongoing.Exists (x => x.buyPost == compare [c].postB && x.type == compare [c].type)) {
						TradePost postA = compare [c].postA.GetComponent<TradePost> ();
						
						int goodsNo = goods.FindIndex (x => x.name == compare [c].type);
						
						int average = goods [goodsNo].average;
						int sellQuantity = postA.stock [goodsNo].number - Mathf.RoundToInt (average * 1.5f);
						int buyQuantity = average - Mathf.RoundToInt (compare [c].postB.GetComponent<TradePost> ().stock [goodsNo].number * 1.5f);				
						int quantity = (int)((sellQuantity + buyQuantity)/ 2f);
						
						if (quantity > 0) {
							postA.stock [goodsNo].number -= quantity;
							postA.UpdatePrice ();
							
							ongoing.Add (new Trading{sellPost = compare [c].postA, buyPost = compare [c].postB, number = quantity, type = compare [c].type});
						
							traderScript.onCall = true;
							traderScript.type = compare [c].type;
							traderScript.quantity = quantity;
							traderScript.targetPost = compare [c].postB;
							compare [c].postA.GetComponent<TradePost> ().UpdatePrice ();
							traderScript.PauseGo ();
							break;
						#endregion
						} //doesn't need an else because should be removed in the next Update()
					}//end if not in ongoing
				} else {//end if post not in compare => need to move to new post
					for (int c = 0; c<compare.Count; c++) {
						if (!ongoing.Exists (x => x.buyPost == compare [c].postB && x.type == compare [c].type)) {
							float distance = Vector3.Distance (traderScript.targetPost.transform.position, compare [c].postA.transform.position);
							if ((distance < shortest || shortest == 0)) {//check distances
								if (!poss.Exists (x => x.cNo == c)) {//check that compare has not previously been added
									shortest = distance;
									cNo = c;//need to add the distance to array, 
									tNo = t;//and keep the cNo and tNo because then can add to the poss list at the end of the compare check
									toAdd = true;
								} else {
									int index = poss.FindIndex (x => x.cNo == c);
									if (distance < poss [index].distance) {//current distance is less than the distance found in the poss list
										shortest = distance;
										tNo = t;
										
									} else {//BUT IF NOT LESS, THEN NONE OF THE PREVIOUS WILL BE EITHER. BUT THE NEXT COULD BE
										shortest = poss [index].distance;
										tNo = poss [index].tNo;
									}
									cNo = c;
									poss.RemoveAt (index);
									toAdd = true;
								}//need to compare against the previously added compare to find shortest distance
								//either way, a new entry needs to be added because it either disnt exist, or has been removed.
								//new entry to be added at the end of the compare for loop
							}//check that the distance is smaller than the shortest for the trader
						}//end if not in ongoing
					}//end for compare
				}//end if compare not in ongoing
				//need to add new entry into poss
				if (toAdd) {
//					print ("poss.Count = " + poss.Count);
					poss.Add (new Poss{distance = shortest, cNo = cNo, tNo = tNo});//this will then be checked etc in the next run through
					toAdd = false;
//					print ("new poss size is " + poss.Count);
				}//if still exists after this, needs to be added to a list containing all of the traders that are moving, 
				//and then add that to this, but will just be a check not exist
			}//end if not on call
		}//end for traders
		//here, the poss list needs to be executed and added to a moving list
		ExecuteMove ();
	}
	
	void ExecuteMove () {
		for (int p = 0; p<poss.Count; p++) {
			Trader tradeScript = traders [poss [p].tNo].GetComponent<Trader> ();
			GameObject targetPost = compare [poss [p].cNo].postA;
				
			moving.Add (new Trade{postA = tradeScript.targetPost, postB = targetPost, type = tradeScript.name});
			tradeScript.targetPost = targetPost;
			tradeScript.onCall = true;
			tradeScript.quantity = 0;
			poss.RemoveAt (p);
			tradeScript.PauseGo ();
		}
	}
}