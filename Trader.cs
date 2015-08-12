using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys {//use namespace to stop any name conflicts
	[AddComponentMenu("TradeSys/Make Trader")]//add to component menu
	public class Trader : MonoBehaviour {
		Controller controller;
		public GameObject target;//the current target of the trader
		public TradePost finalPost;//the final post where the trader will end up
		public int postID;//the ID of the current post
		public bool onCall;//true if the trader has been given a destination post
		public bool allowGo = false;//whether the trader is allowed to move or not
		public int cash = 1000;//the amount of money that the trader has to buy items with
		public double cargoSpace = 10, spaceRemaining;//the cargo space of the trader, and how much is left
		public List<NeedMake> cargo = new List<NeedMake> ();//the cargo being carried. Uses NeedMake because has item info, and the number, which is what is needed
		public float closeDistance = 1.5f;//how far away the trader needs to be from the trade post before it registers as being there
		public float stopTime;//how long the trader needs to stop for
		public float radarDistance;//how far away can the trader see dropped items
//	public float droneTime;	
		public List<AllowGroup> allowItems = new List<AllowGroup> ();//a list where it is possible to select what a trader can and can't carry
		public List<bool> factions = new List<bool> ();//select which factions the trader belongs to

		void Awake () {
			controller = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
			controller.SortTrader (this);
			spaceRemaining = cargoSpace;//set the space remaining to be the same as the cargo space, because have no cargo
		}
	
		void Start () {
	
		}
	
		void Update () {			
			if (onCall && allowGo) {//check is able to go
				this.transform.LookAt (target.transform.position);
				this.transform.Translate (Vector3.forward * Time.timeScale * .05f);
			
				if (Vector3.Distance (this.transform.position, target.transform.position) <= closeDistance) //if close enough
					StartCoroutine (AtPost ());//call the AtPost method, so will unload the cargo
			}//end if able to go
		}//end Update
	
		IEnumerator AtPost () {//called when the trader has reached the post, so needs to sell items
			allowGo = false; 
			float totalTime = 0;//the number of items that were transferred
			for (int c = 0; c<cargo.Count; c++) {//go through each item, and remove
				Stock stock = finalPost.stock [cargo [c].groupID].stock [cargo [c].itemID];
				Goods cGood = controller.goods [cargo [c].groupID].goods [cargo [c].itemID];
				int number = cargo [c].number;//the number of items that were transferred
				if (stock.buy) {//needs to check that the post is allowed to buy the item
					//this is done to make sure that the post is able to afford all of the items
					while (cargo[c].number > 0) {//go through each item, checking that the trade post can afford to buy it
						int cost = Mathf.RoundToInt (stock.price * controller.purchasePercent);
						if (finalPost.cash > stock.price * controller.purchasePercent) {//if has enough money to buy the item
							cargo [c].number --;//remove stock from hold
							spaceRemaining += cGood.mass;//increase space remaining								
							stock.number++;//add to trade post
							finalPost.cash -= cost;//pay for item
							cash += cost;//receive money
							if (controller.priceUpdates)//if update after each trade
								finalPost.UpdateSinglePrice (cargo [c].groupID, cargo [c].itemID);//update the price
						} else//end if enough cash
							break;//if no more cash, then break from this while loop for loop will continue to make sure that as many items 
						//as possible are sold
					}//end while more than 0
					number -= cargo [c].number;//reduce the number for those still being carried
					totalTime += number * cGood.pausePerUnit;
					
					if (cargo [c].number == 0) {//if no more of the item, remove cargo list
						finalPost.UpdateSinglePrice (cargo [c].groupID, cargo [c].itemID);//update the price
						cargo.RemoveAt (c);
						c--;
					}//end if no more
				}//end if post buy check
			}//end for cargo
		
			if (controller.pauseEnter)//if entry pause
				yield return StartCoroutine (Pause (controller.pauseOption < 2?stopTime:totalTime));//pause of stop time, else the total time
		
			onCall = false;//set onCall to false so can be told the next trade
		}//end AtPost
	
		public IEnumerator PauseExit (float time) {//start the pause which will allow the trader to go when the trader is leaving a trade post
			if (controller.pauseExit) //only needs to pause if the option to pause on exit is enabled
				yield return StartCoroutine(Pause(controller.pauseOption < 2 ? stopTime : time));//pause for stop time if trader, else the time sent in
			allowGo = true;//is now allowed to go
		}//end Pause
	
		IEnumerator Pause (float time) {//pause for the required time
			yield return new WaitForSeconds(time);
		}//end Pause
	}//end Trader
}//end namespace