using UnityEngine;
using System.Collections;

public class TSTraderAI : MonoBehaviour
{
		/*this script is example code for the AI for traders. It has been added so that it is clear what you should edit when you are making your own trader AI.
		the whole of this script is example code. Any code which is important for use is marked IMPORTANT and is required to be called to make TradeSys work
		this script makes a lot of references to variables set in the trader script, but this is because this script is only a small additional element
		by having the AI in a separate script, it makes it possible to have a different AI for different traders*/
			
		public float radarDistance = 5;//this is how far away items can be for the trader to see them to collect
		public float droneTime = 2;//this is how long the trader pauses for when it collects an item. is only required when the trader is allowed to collect items
		//you may not want a specific pause time, but to have some different code instead. for example, a salvage drone going and collecting the item
			
		TradeSys.Trader tS;//this is the trader script on the gameobject that this is attached to
			
		GameObject target;//this is the trade post that the trader is heading for
		
		Collider[] itemsInRadar;//this is the colliders of the gameobjects that the trader is close to. this is used for collecting dropped items
		
		bool collect;//this is used to know if collection has been enabled in the controller

		TradeSys.Controller controller;//the controller script

		void Awake ()
		{
				tS = gameObject.GetComponent<TradeSys.Trader> ();//need to get the Trader script
				
				collect = GameObject.FindGameObjectWithTag (TradeSys.Tags.C).GetComponent<TradeSys.Controller> ().pickUp;//get the pickUP option in the controller
		
				controller = GameObject.FindGameObjectWithTag (TradeSys.Tags.C).GetComponent<TradeSys.Controller> ();//get the controller script
		}//end Awake
	
		void Update ()
		{
				target = tS.target;//set the target. this saves referencing it in the trader script each time
				
				if (target == null){
				if(tS.finalPost != null)
						target = tS.target = tS.finalPost.gameObject;//if not heading for anything, then go to the final post
				else
				target = tS.target = tS.startPost.gameObject;//if final post gone, go to start post
						}//end if target null
				//the line above is in case the item being collected has been collected by another trader
				
				if (tS.onCall && tS.allowGo) {//if these are both true, then the trader is allowed to move
						//replace the next two lines with your movement AI with the destination set to be the target
						transform.LookAt (target.transform.position);//look at where we want to go
						transform.Translate (Vector3.forward * Time.timeScale * .05f);//move towards the target
				
						if (collect && tS.allowCollect) {//only needs to check for dropped items if is set in the controller and on the trader
				
								itemsInRadar = Physics.OverlapSphere (transform.position, radarDistance);//get the objects within the radar distance
					
								GameObject cItem = null;//the closest gameobject of the collider
								float cDist = Mathf.Infinity;//the distance that the closest object is away
					
								for (int c = 0; c<itemsInRadar.Length; c++) {//go through all items in radar
					
										GameObject item = itemsInRadar [c].gameObject;//the gameobject that is being checked
					
										if (item.tag == TradeSys.Tags.I) {//if the tag of the gameobject is item
												TradeSys.Item iS = item.GetComponent<TradeSys.Item> ();//get the item script
					
												if (iS.traderCollect && tS.spaceRemaining >= (iS.number * controller.goods [iS.groupID].goods [iS.itemID].mass)) {
														//check that the trader can collect the item and has enough cargo space for this item
					
														float dist = (transform.position - item.transform.position).sqrMagnitude;//get the magnitude of the distance away. uses sqrmagnitude as the actual distance is not required
					
														if (dist < cDist) {//if is the closest item
																cDist = dist;//set the closest distance
																cItem = item;//set the item
														}//end is the closest item
												}//end space check
										}//end if is item
								}//end for all items
								//now have the closest item to the trader, so need to tell the trader to go to it
								if (cItem != null)
										target = tS.target = cItem;//set the target to be the closest item
						}//end if collection allowed
						
						
						if (target != null && Vector3.Distance (transform.position, target.transform.position) <= tS.closeDistance) { //is close enough
								if (target.gameObject.tag == TradeSys.Tags.TP)//if the target is a trade post
										StartCoroutine (tS.AtPost ());//IMPORTANT - call the AtPost method, so will unload the cargo
					else//else the target is an item, so needs to be picked up
										StartCoroutine (CollectItem ());//collect the item
						}//end if close enough
				}//end if able to go
		}//end Update
		
		IEnumerator CollectItem ()
		{//collect the item
				tS.allowGo = false;//stop the trader moving
				yield return new WaitForSeconds (droneTime);//pause for the set time
				tS.allowGo = true;//trader can now continue
		
				if (target != null) {
		
						TradeSys.Item iS = target.GetComponent<TradeSys.Item> ();//get the item script
						tS.items [iS.groupID].items [iS.itemID].number += iS.number;//increase the number being carreid
						tS.spaceRemaining -= iS.number * controller.goods [iS.groupID].goods [iS.itemID].mass;//decrease the cargo space remaining
						
						iS.Collected ();//IMPORTANT - need to say that the item has been collected so the spawner count can be updated
				}
		}//end CollectItem
}//end TSTraderAI