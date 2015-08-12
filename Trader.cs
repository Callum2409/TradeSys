using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CallumP.TradeSys
{//use namespace to stop any name conflicts
		[AddComponentMenu("CallumP/TradeSys/Make Trader")]
		//add to component menu
	public class Trader : MonoBehaviour
		{
				Controller controller;
				public GameObject target;//the current target of the trader
				internal TradePost startPost, finalPost;//the final post where the trader will end up
				internal int postID;//the ID of the current post
				internal int homeID;//the ID of the starting post of the trader
				internal bool onCall;//true if the trader has been given a destination post
				internal bool allowGo = false;//whether the trader is allowed to move or not
				public bool allowCollect;//whether the trader is allowed to collect dropped items
				public int cash = 1000;//the amount of money that the trader has to buy items with
				public double cargoSpace = 10, spaceRemaining;//the cargo space of the trader, and how much is left
				public float closeDistance = 1.5f;//how far away the trader needs to be from the trade post before it registers as being there
				public float stopTime;//how long the trader needs to stop for
				public float radarDistance;//how far away can the trader see dropped items	
				public List<ItemGroup> items = new List<ItemGroup> ();//a list where it is possible to select what a trader can and can't carry
				public List<MnfctrGroup> manufacture = new List<MnfctrGroup> ();//manufacturing lists
				public int tradeType = 0;//0 - standard, 1 - depot backhaul, 2 - depot no backhaul
				public bool dropCargo;//whether cargo can be dropped or not
				public bool dropSingle;//whether a single item is dropped at a time or all of one item in a crate
				internal bool empty = true;//if the trader was not carrying anything
				
				bool expendable;//if expendable traders has been enabled in the controller

				void Awake ()
				{
						controller = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
						tag = Tags.T;
						spaceRemaining = cargoSpace;//set the space remaining to be the same as the cargo space, because have no cargo
						
						expendable = controller.expTraders.enabled;
						
						controller.SortController();
						controller.SortTags(gameObject, true);//make sure factions there
			
						
						InvokeRepeating ("ManufactureCheck", 0, controller.updateInterval);//check for manufacture changes periodically. do this here so expendables can do it too
				}//end Awake
	
				public IEnumerator AtPost ()
				{//called when the trader has reached the post, so needs to sell items
						allowGo = false; 
						float totalTime = 0;//the number of items that were transferred
						for(int g = 0; g<items.Count; g++){//go through group
							for(int i = 0; i<items[g].items.Count; i++){//go through items
								Stock stock = finalPost.stock[g].stock[i];
								Goods cGood = controller.goods[g].goods[i];
								int number = items[g].items[i].number;
					
								if(stock.buy){//make sure post allowed to buy the item
						//next is done to make sure that the post is able to afford all of the items
						while(number > 0){//if has more than 0 
							int cost = Mathf.RoundToInt (stock.price * controller.purchasePercent);
							if (finalPost.cash >= stock.price * controller.purchasePercent || expendable) {//if has enough money to buy the item
								number --;//remove stock from hold
								spaceRemaining += cGood.mass;//increase space remaining								
								stock.number++;//add to trade post
								
								if(!expendable){//only need to sort prices if not expendable
								finalPost.cash -= cost;//pay for item
								cash += cost;//receive money
								if (controller.priceUpdates)//if update after each trade
									finalPost.UpdateSinglePrice (g, i);//update the price
									}//end not expendable so sort prices
							} else//end if enough cash
								break;//if no more cash, then break from this while loop for loop will continue to make sure that as many items
							//as possible are sold
						}//end while still has items
						int transferred = items[g].items[i].number-number;//work out how many items were transferred
						items[g].items[i].number = number;
						totalTime += transferred * cGood.pausePerUnit;
								}//end post buy check
							}//end for items
						}//end for group
						
			if(expendable)//if expendable
						DestroyTrader();//destroy the trader
	
						if (controller.pauseEnter)//if entry pause
								yield return StartCoroutine (Pause (controller.pauseOption < 2 ? stopTime : totalTime));//pause of stop time, else the total time
		
						onCall = false;//set onCall to false so can be told the next trade
				}//end AtPost
	
				public IEnumerator PauseExit (float time)
				{//start the pause which will allow the trader to go when the trader is leaving a trade post
						if (controller.pauseExit) //only needs to pause if the option to pause on exit is enabled
								yield return StartCoroutine (Pause (controller.pauseOption < 2 ? stopTime : time));//pause for stop time if trader, else the time sent in
						allowGo = true;//is now allowed to go
				}//end Pause
	
				IEnumerator Pause (float time)
				{//pause for the required time
						yield return new WaitForSeconds (time);
				}//end Pause
				
		public void ManufactureCheck ()
		{//go through the manufacturing processes, and if not being made, check that can
			if(!expendable){//if expendable, dont manufacture anything
				for (int m1 = 0; m1<manufacture.Count; m1++) {//go through manufacture groups
					for (int m2 = 0; m2<manufacture[m1].manufacture.Count; m2++) {//go through manufacture processes
						RunMnfctr cMan = manufacture [m1].manufacture [m2];
						if (cMan.enabled && !cMan.running  && cash - cMan.price > 0) {
						//check that the process is allowed and not currently running and has enough cash
							if (ResourceCheck (m1, m2)) {//check that has enough resources and check the stock numbers
								//now needs to follow the process
								StartCoroutine (Create (m1, m2));//follow the process
							}//end if enough resources
						}//end if running
					}//end for manufacture processes
				}//end for manufacture groups
				}//end if not expendable
		}//end Manfuacture Check
		
		bool ResourceCheck (int groupID, int processID)
		{//check that the manufacturing process has enough resources to work, and numbers are not above or below min values if option selected
			Mnfctr cMan = controller.manufacture [groupID].manufacture[processID];
			
			for (int n = 0; n<cMan.needing.Count; n++) {//go through all needing
				NeedMake cCheck = cMan.needing [n];
				int quant = items [cCheck.groupID].items [cCheck.itemID].number;
			
				if (quant < cCheck.number)//if not enough
					return false;//then return false as it cannot be made
			}//end for needing
			
			if((spaceRemaining+cMan.needingMass-cMan.makingMass)<0)//check that has enough space to be able to manufacture
				return false;
			
			//if has managed to pass all of the checks
			return true;//return true as the process can be done
		}//end ResourceCheck
		
		IEnumerator Create (int groupID, int processID)
		{//follow the manufacturing process
			Mnfctr process = controller.manufacture [groupID].manufacture [processID];//the manufacturing process in the controller
			RunMnfctr traderMan = manufacture [groupID].manufacture [processID];//the manufacturing process at the trade post
			
			cash -= traderMan.price;//remove the amount of money required to run the process
			
			//needs to pause before removing the items here so that they dont appear later on, potentially after the trader has been to a trade post
			
			traderMan.running = true;//set to true so cannot be called again until done
			yield return new WaitForSeconds (traderMan.create);//pause for the creation time
			
			if(onCall && allowGo){//check the trader is moving as it may have arrived at a trade post within the pause time
			AddRemove (process.needing, true);//remove the items needed
			AddRemove (process.making, false);//add the items made
			spaceRemaining += process.needingMass - process.makingMass;
			yield return new WaitForSeconds (traderMan.cooldown);//pause for the cooldown time
			}//end if trader travelling
			
			traderMan.running = false;//now set to false because has finished the process
		}//end Create
		
		void AddRemove (List<NeedMake> nm, bool needing)
		{//go through all the items in the list, adding or removing them
			for (int i = 0; i<nm.Count; i++) {//go through all items
				NeedMake cNM = nm [i];
				int number = 0;
				number = cNM.number * (needing ? -1 : 1);//the number of items multiply by -1 if needing so they are removed
				items[cNM.groupID].items[cNM.itemID].number += number;//add or remove from post stock
				controller.UpdateAverage (cNM.groupID, cNM.itemID, number, 0);//need to update the average number of this item
			}//end for items
		}//end AddRemove
		
		/// <summary>
		/// Edit the manufacturing process, making it enabled or disabled or changing the create and cooldown times.
		/// </summary>
		/// <param name='manufactureGroup'>
		/// The manufacture group the process belongs to
		/// </param>
		/// <param name='processNumber'>
		/// The number of the process in the manufacture group
		/// </param>
		/// <param name='enabled'>
		/// Set if the process is enabled or not
		/// </param>
		/// <param name='createTime'>
		/// How long it takes for the process to create everything in the making list
		/// </param>
		/// <param name='cooldownTime'>
		/// How long before the process is allowed to be run again
		/// </param>	
		/// <param name='price'>
		/// How much the process costs to run. Make negative to receive money
		/// </param>			
		public void EditProcess (int manufactureGroup, int processNumber, bool enabled, int createTime, int cooldownTime, int price)
		{
			controller.EditProcess (manufacture, manufactureGroup, processNumber, enabled, createTime, cooldownTime, price);
		}//end EditProcess
		
		public void ChangeTraderHome(GameObject post){//used to change which trade post appears as the home post. Only useful for depots
			homeID = controller.GetPostID(post);
		}//end ChangeTradeHome
		
		public void ChangeTraderHome(TradePost post){//used to change which trade post appears as the home post. Only useful for depots
			homeID = controller.GetPostID(post.gameObject);
		}//end ChangeTradeHome
		
		public void DestroyTrader(){//destroy the trader
		//this will get called by expendable trader methods but can be used to remove the trader from the game
			if(expendable)//if expendable
				controller.traderCount--;//reduce the number of traders by one
			else
				controller.GetTraderScripts();//else needs to update the list of trader scripts
		
			DropAllCargo();//drop all carried cargo
		
			Destroy(gameObject);//destroy the game object. replace this with pooling etc for efficiency
		}//end DestroyTrader
		
		public void DropAllCargo(){//drop all of the cargo. is used here so can be called when destroyed, but could be used for jetissoing all cargo
			for(int g = 0; g<items.Count; g++){//for all groups
				for(int i = 0; i<items[g].items.Count; i++){//for all items
					int number = items[g].items[i].number;//the number of the item
					
					if(number > 0){//if there is cargo to drop
					if(dropCargo){//check can drop cargo
						if(dropSingle)//if needing to drop a single item
							for(int n = 0; n<number; n++)
								DropCargo(1, g, i);//drop the cargo
						else
							DropCargo(number, g, i);//else drop all at once
							}else//end check drop cargo
							controller.UpdateAverage(g, i, number, 0);//needs to update the average						
					}//end if dropping cargo
				}//end for all items
			}//end for all groups
		}//end DropAllCargo
		
		public void DropCargo(int number, int groupID, int itemID){//drop the number of cargo
			if(dropCargo){//if allowed to drop cargo
			Item droppedItem = (Item)Instantiate(controller.goods[groupID].goods[itemID].itemCrate, transform.position, Quaternion.identity);//create the item
			//set the details of the dropped item
			droppedItem.groupID = groupID;
			droppedItem.itemID = itemID;
			droppedItem.number = Mathf.Min(number, items[groupID].items[itemID].number);//drop the min number
			droppedItem.traderCollect = true;
			droppedItem.dropped = true;
				}//end if can drop cargo
		}//end DropCargo
	}//end Trader
}//end namespace