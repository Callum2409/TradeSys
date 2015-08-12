using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys
{//use namespace to stop any name conflicts
	[AddComponentMenu("TradeSys/Make Post")]//add to component menu
	public class TradePost : MonoBehaviour
	{
		#region options
		Controller controller;//the controller
		public bool customPricing;//option whether the prices are set manually
		#endregion
	
		#region variables
		public List<StockGroup> stock = new List<StockGroup> ();//the stock list
		public List<MnfctrGroup> manufacture = new List<MnfctrGroup> ();//manufacturing lists
		public int cash = 10000;//the cash that the trade post has, so can buy and sell items
		public ShowTGF tags = new ShowTGF ();//the tags that can be selected for the trade post
		public ShowTGF groups = new ShowTGF ();//the groups that the trade post belongs to
		public ShowTGF factions = new ShowTGF ();//the factions that the trade post belongs to
		public bool stopProcesses;//if selected, and the number of an item is more or less than the max / min, then any process requiring the item will stop
	
		internal bool updated;//true if the prices have been updated in the current TradeCall
		
		public bool allowTrades = true, allowManufacture = true;//whether a trade post is allowed to trade or manufacture items. Does not show in editor so everything keeps previous values
		#endregion
	
//System.Diagnostics.Stopwatch stoppy = new System.Diagnostics.Stopwatch();
	
		void Awake ()
		{
			controller = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
			controller.SortTradePost (this);//make sure that the trade post has all of the correct information
			for (int m1 = 0; m1<manufacture.Count; m1++) {//for all manufacture groups
				for (int m2 = 0; m2<manufacture[m1].manufacture.Count; m2++) {//for all manufacture processes
					if (manufacture [m1].manufacture [m2].enabled)
						controller.manufacture [m1].manufacture [m2].postCount++;
				}//end for manufacture processes
			}//end for manufacture groups
		}//end Awake
	
		public void UpdatePrices ()
		{//set the prices of goods
			if (!customPricing) {//if custom pricing, then the prices are not set automatically
				for (int g = 0; g<stock.Count; g++) {//go through all groups
					for (int i = 0; i<stock[g].stock.Count; i++) {//go through all items
						if (!stock [g].stock [i].hidden)//only needs to update the price if it is not hidden
							UpdateSinglePrice (g, i);
					}//end for all items		
				}//end for all groups
			}//end ig not custom pricing
		}//end UpdatePrices
	
		public void UpdateSinglePrice (int g, int i)
		{//update the price of a single item
			Stock currentS = stock [g].stock [i];
			Goods currentG = controller.goods [g].goods [i];
			if (currentS.number == 0)//if zero, then set as the max price
				currentS.price = currentG.maxPrice;
			else
				//The price is found by dividing the average by the number of that item available at the post.
				//This is then multiplied by the base price, and is clamped between the min and max.
				//It is then rounded with 0.5 rounding to 1
				currentS.price = (int)System.Math.Round (Mathf.Clamp (currentG.basePrice * (((float)currentG.average) / currentS.number),
						currentG.minPrice, currentG.maxPrice), System.MidpointRounding.AwayFromZero);
		}//end UpdateSinglePrice
	
		public void ManufactureCheck ()
		{//go through the manufacturing processes, and if not being made, check that can
			if (allowManufacture) {//check is allowed to manufacture items
				for (int m1 = 0; m1<manufacture.Count; m1++) {//go through manufacture groups
					for (int m2 = 0; m2<manufacture[m1].manufacture.Count; m2++) {//go through manufacture processes
						PostMnfctr cMan = manufacture [m1].manufacture [m2];
						if (cMan.enabled && !cMan.running) {//check that the process is allowed and not currently running
							if (ResourceCheck (m1, m2)) {//check that has enough resources and check the stock numbers
								//now needs to follow the process
								StartCoroutine (Create (m1, m2));//follow the process
							}//end if enough resources
						}//end if running
					}//end for manufacture processes
				}//end for manufacture groups
			}//end manufacture allow check
		}//end Manfuacture Check
	
		bool ResourceCheck (int groupID, int processID)
		{//check that the manufacturing process has enough resources to work, and numbers are not above or below min values if option selected
			Mnfctr cProcess = controller.manufacture [groupID].manufacture [processID];
			List<NeedMake> check = cProcess.needing;//check the needing list
			for (int n = 0; n<check.Count; n++) {//go through all needing
				NeedMake cCheck = check [n];
				Stock cStock = stock [cCheck.groupID].stock [cCheck.itemID];
				if (cStock.number < cCheck.number || (cStock.minMaxE && stopProcesses && cStock.number <= cStock.min))//if not enough, or has below minimum with stop processes enabled
					return false;//then return false as it cannot be made
			}//end for needing
	
			check = cProcess.making;//check the making list
			for (int m = 0; m<check.Count; m++) {//go through all making
				NeedMake cCheck = check [m];
				Stock cStock = stock [cCheck.groupID].stock [cCheck.itemID];
				if (cStock.minMaxE && stopProcesses && cStock.number >= cStock.max)//if already has too many items, and has stop processes enabled
					return false;//then return false as it cannot be made
			}//end for making
			//if has managed to pass all of the checks
			return true;//return true as the process can be done
		}//end ResourceCheck
	
		IEnumerator Create (int groupID, int processID)
		{//follow the manufacturing process
			Mnfctr process = controller.manufacture [groupID].manufacture [processID];//the manufacturing process in the controller
			PostMnfctr postMan = manufacture [groupID].manufacture [processID];//the manufacturing process at the trade post
		
			postMan.running = true;//set to true so cannot be called again until done
			AddRemove (process.needing, true);//remove the items needed
			yield return new WaitForSeconds(postMan.create);//pause for the creation time
			AddRemove (process.making, false);//add the items made
			yield return new WaitForSeconds(postMan.cooldown);//pause for the cooldown time
			postMan.running = false;//now set to false because has finished the process
		}//end Create
	
		void AddRemove (List<NeedMake> items, bool needing)
		{//go through all the items in the list, adding or removing them
			for (int i = 0; i<items.Count; i++) {//go through all items
				NeedMake cNM = items [i];
				int number = 0;
				number = cNM.number * (needing ? -1 : 1);//the number of items multiply by -1 if needing so they are removed
				stock [cNM.groupID].stock [cNM.itemID].number += number;//add or remove from post stock
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
		public void EditProcess (int manufactureGroup, int processNumber, bool enabled, int createTime, int cooldownTime)
		{
			//need to check that has received valid changes
			if (manufactureGroup > manufacture.Count || manufactureGroup < 0)//check that group is valid
				Debug.LogError ("Invalid manufacture group number");
			if (processNumber > manufacture [manufactureGroup].manufacture.Count || processNumber < 0)//check that process is valid
				Debug.LogError ("Invalid manufacture process number");
			if (createTime < 1)//check create time
				Debug.LogError ("Create time should be greater than 1");
			if (cooldownTime < 0)//check cooldown time
				Debug.LogError ("Cooldown time should be greater than 0");
			
			PostMnfctr editing = manufacture [manufactureGroup].manufacture [processNumber];//get the process to edit
			editing.enabled = enabled;//set if enabled or not
			editing.create = createTime;//set the create time
			editing.cooldown = cooldownTime;//set the cooldown time
			manufacture [manufactureGroup].manufacture [processNumber] = editing;//apply the changes
		}//end EditProcess
		
		public void EnableDisable (bool enableTrades, bool enableManufacture)
		{
			//Needs to enable a trade post that has been disabled or disable an enabled trade post
			if (enableTrades != allowTrades) {//check that not already the same
				//need to go through all items updating averages
				int change = enableTrades ? 1 : -1;
				for (int g = 0; g<stock.Count; g++) {//go through all groups
					for (int i = 0; i<stock[g].stock.Count; i++) {//go through all items
						controller.UpdateAverage (g, i, stock [g].stock [i].number * change, change);//update the average for the item
					}//end for items
				}//end for groups
				allowTrades = enableTrades;//set to new value
			}//end check changing trades

			allowManufacture = enableManufacture;//set to the value

		}//end EnableDisable
		
	}//end TradePost
}//end namespace