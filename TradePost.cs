using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CallumP.TradeSys
{//use namespace to stop any name conflicts
    [AddComponentMenu("CallumP/TradeSys/Make Post")]
    //add to component menu
    public class TradePost : MonoBehaviour
    {
        #region options
        Controller controller;//the controller
        public bool customPricing;//option whether the prices are set manually
        #endregion

        #region variables
        public List<StockGroup> stock = new List<StockGroup>();//the stock list
        public List<MnfctrGroup> manufacture = new List<MnfctrGroup>();//manufacturing lists
        public List<float> currencies = new List<float>();//currency lists
        public List<bool> exchanges = new List<bool>();//exchange lists
        public float cash = 10000;//the cash that the trade post has, so can buy and sell items
        public bool convertCurrency;//whether trade post can make currency conversions
        public bool stopProcesses;//if selected, and the number of an item is more or less than the max / min, then any process requiring the item will stop

        internal bool updated;//true if the prices have been updated in the current TradeCall

        public bool allowTrades = true, allowManufacture = true;//whether a trade post is allowed to trade or manufacture items. Does not show in editor so everything keeps previous values

        public int postID;//this is the location within the post array in the controller

        public TagManagement.ObjectTags factions, groups;
        #endregion

        //System.Diagnostics.Stopwatch stoppy = new System.Diagnostics.Stopwatch();

        void Awake()
        {
            controller = GameObject.FindGameObjectWithTag(Tags.C).GetComponent<Controller>();
            tag = Tags.TP;
            //make sure that the tags are sorted
            controller.SortController();
            controller.SortTradePost(this);
        }//end Awake

        public void UpdatePrices()
        {//set the prices of goods
            if (!customPricing && !controller.expTraders.enabled)
            {//if custom pricing, then the prices are not set automatically, and if expendable then not required at all
                for (int g = 0; g < stock.Count; g++)
                {//go through all groups
                    for (int i = 0; i < stock[g].stock.Count; i++)
                    {//go through all items
                        if (!stock[g].stock[i].hidden)//only needs to update the price if it is not hidden
                            UpdateSinglePrice(g, i);
                    }//end for all items		
                }//end for all groups
            }//end if not custom pricing
        }//end UpdatePrices

        public void UpdateSinglePrice(int g, int i)
        {//update the price of a single item
            Stock currentS = stock[g].stock[i];
            Goods currentG = controller.goods[g].goods[i];
            if (currentS.number == 0)//if zero, then set as the max price
                currentS.price = currentG.maxPrice;
            else
                //The price is found by dividing the average by the number of that item available at the post.
                //This is then multiplied by the base price, and is clamped between the min and max.
                currentS.price = (float)System.Math.Round(Mathf.Clamp(currentG.basePrice * (((float)currentG.average) / currentS.number), currentG.minPrice, currentG.maxPrice), controller.currencies[controller.goods[g].goods[i].currencyID].decimals, System.MidpointRounding.AwayFromZero);
        }//end UpdateSinglePrice

        public void ManufactureCheck()
        {//go through the manufacturing processes, and if not being made, check that can
            if (allowManufacture)
            {//check is allowed to manufacture items
                for (int m1 = 0; m1 < manufacture.Count; m1++)
                {//go through manufacture groups
                    for (int m2 = 0; m2 < manufacture[m1].manufacture.Count; m2++)
                    {//go through manufacture processes
                        RunMnfctr cMan = manufacture[m1].manufacture[m2];
                        if (cMan.enabled && !cMan.running && cash - cMan.price > 0)
                        {
                            //check that the process is allowed and not currently running and has enough cash
                            if (ResourceCheck(m1, m2))
                            {//check that has enough resources and check the stock numbers
                             //now needs to follow the process
                                StartCoroutine(Create(m1, m2));//follow the process
                            }//end if enough resources
                        }//end if running
                    }//end for manufacture processes
                }//end for manufacture groups
            }//end manufacture allow check
        }//end ManfuactureCheck

        bool ResourceCheck(int groupID, int processID)
        {//check that the manufacturing process has enough resources to work, and numbers are not above or below min values if option selected
            Mnfctr cProcess = controller.manufacture[groupID].manufacture[processID];
            List<NeedMake> check = cProcess.needing;//check the needing list
            for (int n = 0; n < check.Count; n++)
            {//go through all needing
                NeedMake cCheck = check[n];
                Stock cStock = stock[cCheck.groupID].stock[cCheck.itemID];
                if (cStock.number < cCheck.number || (cStock.minMax && stopProcesses && cStock.number <= cStock.min))//if not enough, or has below minimum with stop processes enabled
                    return false;//then return false as it cannot be made
            }//end for needing

            check = cProcess.making;//check the making list
            for (int m = 0; m < check.Count; m++)
            {//go through all making
                NeedMake cCheck = check[m];
                Stock cStock = stock[cCheck.groupID].stock[cCheck.itemID];
                if (cStock.minMax && stopProcesses && cStock.number >= cStock.max)//if already has too many items, and has stop processes enabled
                    return false;//then return false as it cannot be made
            }//end for making
             //if has managed to pass all of the checks
            return true;//return true as the process can be done
        }//end ResourceCheck

        IEnumerator Create(int groupID, int processID)
        {//follow the manufacturing process
            Mnfctr process = controller.manufacture[groupID].manufacture[processID];//the manufacturing process in the controller
            RunMnfctr postMan = manufacture[groupID].manufacture[processID];//the manufacturing process at the trade post

            cash -= postMan.price * (controller.expTraders.enabled ? 0 : 1);//remove the amount of money required to run the process
                                                                            //only need to remove credits if expendable is disabled

            postMan.running = true;//set to true so cannot be called again until done
            AddRemove(process.needing, true);//remove the items needed
            yield return new WaitForSeconds(postMan.create);//pause for the creation time
            AddRemove(process.making, false);//add the items made
            yield return new WaitForSeconds(postMan.cooldown);//pause for the cooldown time
            postMan.running = false;//now set to false because has finished the process
        }//end Create

        void AddRemove(List<NeedMake> items, bool needing)
        {//go through all the items in the list, adding or removing them
            for (int i = 0; i < items.Count; i++)
            {//go through all items
                NeedMake cNM = items[i];
                int number = 0;
                number = cNM.number * (needing ? -1 : 1);//the number of items multiply by -1 if needing so they are removed
                stock[cNM.groupID].stock[cNM.itemID].number += number;//add or remove from post stock
                controller.UpdateAverage(cNM.groupID, cNM.itemID, number, 0);//need to update the average number of this item
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
        public void EditProcess(int manufactureGroup, int processNumber, bool enabled, int createTime, int cooldownTime, int price)
        {
            controller.EditProcess(manufacture, manufactureGroup, processNumber, enabled, createTime, cooldownTime, price);
        }//end EditProcess

        /// <summary>
        /// Enable or disable trades or manufacturing completely
        /// </summary>
        /// <param name="enableTrades">If set to <c>true</c> enable trades</param>
        /// <param name="enableManufacture">If set to <c>true</c> enable manufacture.</param>
        public void EnableDisableTradeMan(bool enableTrades, bool enableManufacture)
        {
            //Needs to enable a trade post that has been disabled or disable an enabled trade post
            if (enableTrades != allowTrades)
            {//check that not already the same
             //need to go through all items updating averages
                int change = enableTrades ? 1 : -1;
                for (int g = 0; g < stock.Count; g++)
                {//go through all groups
                    for (int i = 0; i < stock[g].stock.Count; i++)
                    {//go through all items
                        controller.UpdateAverage(g, i, stock[g].stock[i].number * change, change);//update the average for the item
                    }//end for items
                }//end for groups
                allowTrades = enableTrades;//set to new value
                if (!enableTrades)
                    controller.TraderAllHome(postID);//need to sort destinations of traders enroute
            }//end check changing trades
            allowManufacture = enableManufacture;//set to the value
        }//end EnableDisableTradeMan

        /// <summary>
        /// Change the buy, sell and hidden options for a stock item
        /// </summary>
        /// <param name="groupID">Group ID</param>
        /// <param name="itemID">Item ID</param>
        /// <param name="buy">If set to <c>true</c> allow buy.</param>
        /// <param name="sell">If set to <c>true</c> allow sell.</param>
        /// <param name="hidden">If set to <c>true</c> allow hidden if buy and sell are both false.</param>
        public void EnableDisableStock(int groupID, int itemID, bool buy, bool sell, bool hidden)
        {
            Stock cS = stock[groupID].stock[itemID];

            bool change = (cS.buy || cS.sell) != (buy || sell);//check if change in the buy / sell optins so that averages need changing

            if (change)
            {//if there is a change in the buy / sell bools
                bool enable = (!cS.buy && buy) || (!cS.sell && sell);//work out whether the item is being enabled or disabled
                int mult = enable ? 1 : -1;
                controller.UpdateAverage(groupID, itemID, cS.number * mult, mult);//update the average
                UpdateSinglePrice(groupID, itemID);//update the price
            }//end if average needs changing

            cS.buy = buy;//set the buy option
            cS.sell = sell;//set the sell option
            cS.hidden = !buy && !sell && hidden;//only allow the hidden option to be true if buy and sell are false
        }//end EnableDisableStock

        /// <summary>
        /// Adds goods to the trade post
        /// </summary>
        /// <param name="groupID">Group ID of the item</param>
        /// <param name="itemID">Item ID within the group</param>
        /// <param name="number">Number to add</param>
        public void AddRemoveGood(int groupID, int itemID, int number)
        {
            if (stock[groupID].stock[itemID].number + number > 0)
            {//check that there is enough of the item if removing items
                stock[groupID].stock[itemID].number += number;//add the items to the trade post
                controller.UpdateAverage(groupID, itemID, number, 0);//update the average stock numbers required for item pricing
                UpdateSinglePrice(groupID, itemID);//update the item price
            }
            else//else if not enough, display error
                Debug.LogError("Not enough items in stock to remove!");
        }//end AddGood

        public void MovedPost()
        {//called when the post has been moved
            controller.SingleDist(postID, 0);
            controller.GetClosest();//now needs to recalulate the closest posts
        }//end MovedPost

        public void RemovePost()
        {//call when you want to remove the trade post
            EnableDisableTradeMan(false, false);//shut down the trade post
            controller.RemovePost(postID);//sort the IDs and remove the post		
            DeletePost();//called last as this will delete itself	
        }//end RemovePost

        void DeletePost()
        {//this is called by the controller when a post is being removed. Change the code here to use pooling or do something else
            Destroy(gameObject);
        }//end DeletePost
    }//end TradePost
}//end namespace