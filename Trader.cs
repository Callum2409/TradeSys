using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NoType
{
	public int number;
	public int goodID;
}

public class Trader : MonoBehaviour
{
	public GameObject target;//this is what the trader is heading for, trade post or dropped item
	public GameObject finalPost;//need to temporarily move the target post here while collects dropped items
	internal List<NoType> trading = new List<NoType> ();//the manifest of the cargo carried
	internal bool onCall = false;//this is used to tell if the trader has been told to go somewhere
	public float stopTime = 2.0f;//the time required for the trader to stop at a trade post
	public float cargoSpace = 1;//the cargo space of the trader
	internal float spaceRemaining;//the space remaining
	internal bool allowGo, pausing;//this is used if the trader is allowed to go / unload cargo as it has waited long enough
	public float radarDistance = 10f;//this is used so that any items within this distance may be collected if it has been enabled
	Controller controller;
	internal Collider[] itemsInRadar;
	public float droneTime = 1.0f;
	public bool allowTraderPickup = false;
	public List<bool> factions = new List<bool> ();
	public bool expendable = false;
	public float closeDistance = 1.5f;
	
	void Awake ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();	
		cargoSpace = Mathf.Clamp (cargoSpace, 0, Mathf.Infinity);
		spaceRemaining = cargoSpace;
	}
	
	public void NewTrader (GameObject post, float space, float stop, bool[] factionsAllow)
	{//only called if the trader is created in-game
		this.gameObject.tag = "Trader";
		target = post;
		Awake ();
		controller.traders.Add (gameObject);
		controller.traderScripts.Add (this);
		cargoSpace = space;
		stopTime = stop;
		if (factionsAllow.Length != controller.factions.Count)
			Debug.LogError ("Please ensure that the factions bool array sent to the new trade post is the correct length." +
				"\nIt needs to have a value for each faction");
		else
			for (int f = 0; f<factionsAllow.Length; f++)
				factions.Add (factionsAllow [f]);
	}
	
	void FixedUpdate ()
	{
		if (allowGo && onCall) {
			if (target == null)
				target = finalPost;
			CheckNearest ();//This is used so that any dropped items are found and if the required settings in controller are
			//enabled, the trader will go there before continuing
			
			this.transform.Translate (Vector3.forward * Time.timeScale * .5f);
			this.transform.LookAt (target.transform);//The line above and this line are demo code used to mkae the traders
			//move. Replace this with your AI and movement code, with the target as the point the trader needs to get to
			
			if (Vector3.Distance (this.transform.position, target.transform.position) <= closeDistance) 
				StartCoroutine (AtLocation ());
		}
	}
	
	public void CheckNearest ()
	{
		if(controller.allowPickup)
		if (controller.allowPickup && allowTraderPickup && onCall && allowGo) {
			itemsInRadar = Physics.OverlapSphere (this.transform.position, radarDistance);//get all items in radar
			GameObject cItem = null;//keep the nearest item
			float cDist = 0;//keep the distance to the nearest item;
			for (int i = 0; i<itemsInRadar.Length; i++) {//go through all of the radar items and check
				float nDist = Vector3.Distance (this.transform.position, itemsInRadar [i].transform.position);
				//get the distance to the item to be checked
				if (itemsInRadar [i].tag == "Item" && controller.spawned.FindIndex (x => x.item == itemsInRadar [i].gameObject) != -1 &&
				(cDist == 0 || nDist < cDist) &&
				spaceRemaining >= controller.goodsArray [controller.spawned.Find (x => x.item == itemsInRadar [i].gameObject).goodID].mass) {
					//make sure it is an item, the distance is closer and there is enough cargo space to pick it up
					cDist = nDist;//set the distances to the new shoretest
					cItem = itemsInRadar [i].gameObject;//set the new closest item
				}//end if
			}//end for
			if (cItem != null) {//make sure that there was an item in radar
				target = cItem;//make the closest item the target
			} else {
				target = finalPost;//set the targetPost back if there is nothing nearby
			}
		}
	}//end CheckNearest
	
	IEnumerator Pause ()
	{
		yield return new WaitForSeconds(stopTime);
		allowGo = true;
	}
	
	public void ExitPause ()
	{
		if (trading.Count == 0 && controller.moving.FindIndex (x => controller.posts[x.postB] == target) == -1)
			Debug.LogError(this.name+" is moving but the controller has not been told");
		if (!controller.pauseOnExit || ((controller.pauseOption == 2 || controller.pauseOption == 3) && trading.Count == 0))
			allowGo = true;
		else
			StartCoroutine (Pause ());
	}
	
	IEnumerator AtLocation ()
	{
		#region Trade Post
		if (target.tag == "Trade Post") {//needs to make sure that the target is a post, as it could be an item
			allowGo = false;//trader not allowed to leave until waited
			if (trading.Count > 0) {//check that trader is carrying items
				if ((controller.pauseOption == 0 || controller.pauseOption == 1) && controller.pauseOnEnter)
					yield return new WaitForSeconds(stopTime);
				TradePost tP = target.GetComponent<TradePost> ();//get the TradePost script of the trade post
				for (int t = 0; t<trading.Count; t++) {//go through all of the items
					if ((controller.pauseOption == 2 || controller.pauseOption == 3) && controller.pauseOnEnter)
						yield return new WaitForSeconds(controller.goodsArray[trading[t].goodID].pausePerUnit*trading[t].number);
					tP.stock [trading [t].goodID].number += trading [t].number;//add each item to the trade post
					tP.UpdatePrice ();//update the price of the item
					int index = controller.ongoing.FindIndex (x => x.number == trading [t].number && x.typeID == trading [t].goodID && x.buyPost == target);
					controller.ongoing.RemoveAt (index);//remove the trader from the ongoing list
				}
				spaceRemaining = cargoSpace;//there should now not be anything in the cargo hold, so space remaining is full
				trading.Clear ();//clear all items carried
				if (controller.expendable) {//check for expendable option
					controller.traderScripts.Remove (this);
					Destroy (this.gameObject);//if expendable, the trader is destroyed
				}
			} else //else if not carrying anything
				controller.moving.RemoveAt (controller.moving.FindIndex (x => controller.posts [x.postB] == target));//remove trader from moving list
			onCall = false;//no longer on call, this allows a new trade to be assaigned
		#endregion
		} else { 
		#region Item
			if (target.tag == "Item") {//if the target is an item, needs to pick it up
				allowGo = false; //set this to false so that the trader has to wait as the item is being picked up
				yield return new WaitForSeconds(droneTime);//stop the trader for the time
				allowGo = true; //item has been picked up, so can now continue
				int itemNo = controller.spawned.FindIndex (x => x.item == target);
				if (itemNo >= 0) {
					int tradNo = trading.FindIndex (x => x.goodID == controller.spawned [itemNo].goodID);
					//need to add the item picked up to ships manifest and to the ongoing trades list
					if (tradNo == -1) {
						if (trading.Count == 0)
							controller.moving.RemoveAt (controller.moving.FindIndex (x => controller.posts [x.postB] == finalPost));
						controller.ongoing.Add (new Trading{sellPost = controller.spawned [itemNo].spawner, buyPost = finalPost, number = 1, typeID = controller.spawned [itemNo].goodID});
						trading.Add (new NoType{number = 1, goodID = controller.spawned [itemNo].goodID});
					} else {
						controller.ongoing [controller.ongoing.FindIndex (x => x.buyPost == finalPost && x.typeID == controller.spawned [itemNo].goodID && x.number == trading [tradNo].number)].number += 1;
						trading [tradNo].number += 1;
					}
					spaceRemaining -= controller.goodsArray [controller.spawned [itemNo].goodID].mass;
					controller.spawned.RemoveAt (itemNo);
					Destroy (target);
					CheckAllNearest ();/*call checknearest for any more items, or set the target back to the post
			needs to do it for all of the traders to make sure that all are continuing to places as one trader may have
			picked up the cargo that another was heading for*/
				}//end if item is still there
			}//end if item
		#endregion
		}
	}
	
	void CheckAllNearest ()
	{
		for (int t = 0; t < controller.traderScripts.Count; t++)
			controller.traderScripts [t].CheckNearest ();
	}
	
	public void DropItems (int itemID)
	{
		int cargoNo = trading.FindIndex (x => x.goodID == itemID);
		if (cargoNo != -1) {
			Debug.Log ("Item dropped!");
			GameObject dropped = (GameObject)Object.Instantiate (controller.goodsArray [itemID].itemCrate, this.transform.position, this.transform.rotation);
			controller.spawned.Add (new Spawned{goodID = itemID, item = dropped, spawner = null});
			dropped.tag = "Item";
			int ongoingNo = controller.ongoing.FindIndex (x => x.buyPost == finalPost && x.number == trading [cargoNo].number && x.typeID == itemID);
			if (trading [cargoNo].number > 1) {
				controller.ongoing [ongoingNo].number--;
				trading [cargoNo].number--;
			} else {
				controller.ongoing.RemoveAt (ongoingNo);
				trading.RemoveAt (cargoNo);
				if (trading.Count == 0)
					controller.moving.Add (new Trade{postB = controller.posts.FindIndex (x => x == finalPost)});
			}
		} 
	}
	
	void OnDrawGizmosSelected ()
	{
		if (allowTraderPickup) {
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere (transform.position, radarDistance);
		}
	}
}