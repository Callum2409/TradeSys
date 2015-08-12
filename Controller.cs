using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TradeSys
{//use namespace to stop any name conflicts
		public class Controller : MonoBehaviour
		{
		#region options
				public bool showLinesInGame;//whether lines are shown in the game view
				public int selC;//the currently selected tab in controller
				public int selP;//the currently selected tab in trade posts
				public int selGC;//the currently selected goods type in controller
				public int selMC;//the currently selected manufacture group in controller
				public bool showGN;//show the name of the group that the item belongs to in the manufacturing list
				public bool showPrices;//show the price of the good in the post editor
				public bool smallScroll = true;//have a scroll bar in the goods and manufacturing menus
				public Vector2 scrollPosS, scrollPosG, scrollPosM, scrollPosPS, scrollPosPG, scrollPosPM, scrollPosT, scrollPosGG, scrollPosMG;//the scroll positions of different menus
				public bool showHoriz;//show a list of items horizontally or vertically
				public bool genOp, gamOp, traOp, pauOp;//the bools for if each option is showing
				public string[][] allNames;//an array for each group with an array of the names
				public string[][] manufactureNames;//an array with the names of each manufacturing process
				public string[][] manufactureTooltips;//an array of strings with the tooltip for the manufacturng processes
		#endregion
	
		#region used variables
		#region in editor
				public float updateInterval = 0.5f;//set the time between subsequent updates
				public bool showLinks = false;//show possible trade links
	
				public bool generateAtStart = true;//if this is true, then generate distances at start. Disable if posts generated in code
	
				public int closestPosts = 10;//this is the number of closest posts that should be taken into account
				public float distanceWeight = 1;//the weighting of the distance to find the best post
				public float profitWeight = 1;//the weighting of the profits to find the best post
				public float buyMultiple = 1.5f;//used to decide which items should be bought
				public float sellMultiple = 1.5f;//used to decide which items should be sold
				public float purchasePercent = 0.7f;//used for the prices of items when the trade post is buying
				public bool priceUpdates;//if true, will update the price after purchasing each item, so prices may increase / decrease while traders purchase
				public bool randomPost = true;//if there are no trades from the current post, go to a random post, or find the best
	
				public int pauseOption = 0;	//the selected pause option
				public float pauseTime;//the time to pause for
				public bool pauseEnter, pauseExit;//pause on post entry and / or exit
	
				public List<GoodsTypes> goods = new List<GoodsTypes> ();//all of the goods are within this list, and in different categories
				public List<MnfctrTypes> manufacture = new List<MnfctrTypes> ();//all processes are within groups in this list
	
				public GameObject[] tradePosts;//all of the Trade Posts in the game
				public TradePost[] postScripts;//all of the scripts of the trade posts
	
				public GameObject[] traders;//all of the traders in the game
				public Trader[] traderScripts;//all of the scripts of the traders
	
				public EnableList postTags;//all of the tags that can be selected by a trade post
				public EnableList groups;//the different groups that a trade post can be in
				public Factions factions;//the different factions that a trade post and trader can be in
	
				public Units units;//the different units that can be used
		#endregion
	
		#region other
				float[][] postDistances;//This is used so that Vector3.Distance is called as little as possible
				Distances[][] closest;//this is the closest x number of posts, which are used to work out trader targets
	
				int tradePostCount;//these are used so that the length of the array does not need to be used each time
				int traderCount;
				Trade[] tradeLists;//this contains all of the buy and sell items at each trade post
				//needmake has this info, so uses that instead of a new class
		#endregion
		#endregion	
	
//		System.Diagnostics.Stopwatch stoppy = new System.Diagnostics.Stopwatch ();

				void Start ()
				{//at the start, the only thing that can be checked is to see if generate has been selected
						//if is has been selected, then generate and the rest can continue, but if not, then needs to wait to be called
						//this is done once all of the trade posts have been set up from your code.
						if (generateAtStart)//generate the distances if the trade posts have been set up
								GenerateDistances ();
						if (showLinesInGame)//if set debug bool to show lines in the game view
								DrawLines ();
				}//end Start
	
				public void GenerateDistances ()
				{//At the start, needs to generate all of the distances and the closest posts for use later. As this is called first, will call all other methods
						//generate the distances so that dont need to call Vector3.Distance each time, instead it can just be referenced from the array
						traders = GameObject.FindGameObjectsWithTag (Tags.T);
						traderCount = traders.Length;
		
						GetPostScripts ();//get all of the trade posts and their scripts
						//initialise arrays
						postDistances = new float[tradePostCount][];
						closest = new Distances[tradePostCount][];
						tradeLists = new Trade[tradePostCount];
		
						traderScripts = new Trader[traderCount];
		
						for (int t = 0; t<tradePostCount; t++) {//need to initialise second array before getting distances because will be filling up later elements at same time
								postDistances [t] = new float[tradePostCount];
								tradeLists [t] = new Trade ();
						}//end for initialisation distances and trade lists
		
						for (int t = 0; t<traderCount; t++) {//need to add all of the trader scripts, and get the starting postID
								traderScripts [t] = traders [t].GetComponent<Trader> ();
								traderScripts [t].postID = GetPostID (traderScripts [t].target);
						}//end go through traders
		
						#region get distances
						//now needs to go through and get all of the distances between all of the posts, where they are in the same group and faction
						for (int t1 = 0; t1<tradePostCount; t1++) {//go through the posts
								for (int t2 = t1; t2<tradePostCount; t2++) {//second for required so can get distances
										if (t1 == t2)//if the same, needs to be infinity so traders will move
												postDistances [t1] [t2] = Mathf.Infinity;
										else {
												if (!CheckFactionsGroups (postScripts [t1], postScripts [t2]))
														postDistances [t1] [t2] = postDistances [t2] [t1] = Mathf.Infinity;
												else
														postDistances [t1] [t2] = postDistances [t2] [t1] = Vector3.Distance (tradePosts [t1].transform.position, tradePosts [t2].transform.position);
										}
										//can set these the same to save on the number of calculations required
								}//end 2nd for		
						}//end 1st for
						#endregion
	
						#region get closest
						if (closestPosts < tradePostCount) {//check if the closest posts is less than the total number
								//now goes through the distances, taking the closest x posts to the target post
								for (int p = 0; p<tradePostCount; p++) {//go through posts
										closest [p] = new Distances[closestPosts];
										for (int d = 0; d<closestPosts; d++) {//go through enough times to get x number of posts
												closest [p] [d] = new Distances ();
				
												float min;
												int index = FindMinimum (postDistances [p], tradePostCount, out min);
					
												if (min == Mathf.Infinity) {
														for (int dc = d; dc<closestPosts; dc++) {//needs to go through the remaining closest, setting to infinity
																closest [p] [dc] = new Distances ();
																closest [p] [dc].post = -1;
																closest [p] [dc].distance = Mathf.Infinity;
														}
														break;
												}
												postDistances [p] [index] = Mathf.Infinity;//set the postDistances one to infinity, so cannot be used again
												closest [p] [d].post = index;
												closest [p] [d].distance = min;
										}//end for get closest x posts
								}//end for all posts
						} else {//end if closest to find is less than the total number of posts
								closestPosts = tradePostCount;//set the closest posts to the actual number of posts
								for (int p = 0; p<tradePostCount; p++) {
										closest [p] = new Distances[tradePostCount];
										for (int d = 0; d<tradePostCount; d++) {//go through the posts to get the distances to
												closest [p] [d] = new Distances ();
												closest [p] [d].distance = postDistances [p] [d];
												closest [p] [d].post = postDistances [p] [d] != Mathf.Infinity ? d : -1;
										}//end for to posts
								}//end for all posts
						}//end else
						#endregion
		
						//Now needs to call other methods
						Average ();
						for (int p = 0; p<postScripts.Length; p++)
								postScripts [p].UpdatePrices ();
						InvokeRepeating ("UpdateMethods", 0, updateInterval);
				}//end GenerateDistances
	
				public void GetPostScripts ()
				{//get all of the trade posts and get the scripts
						tradePosts = GameObject.FindGameObjectsWithTag (Tags.TP);//get all of the trade posts
						tradePostCount = tradePosts.Length;//set the post count to the length
						postScripts = new TradePost[tradePostCount];
		
						for (int t = 0; t<tradePostCount; t++) //go through trade posts getting script
								postScripts [t] = tradePosts [t].GetComponent<TradePost> ();
				}//end GetPostScripts
	
		#region faction group check
				bool CheckFactionsGroups (TradePost post1, TradePost post2)
				{//check that the posts have a same group and faction
						if (CheckFactions (post1, post2) && CheckGroups (post1, post2))
								return true;
						else
								return false;
				}//end CheckFactionsGroups
	
				bool CheckFactions (TradePost post1, TradePost post2)
				{//check in same faction
						if (factions.enabled) {//check if factions enabled
								for (int f = 0; f<factions.factions.Count; f++)//go through factions
										if (post1.factions.enabled [f] && post2.factions.enabled [f])//if both have the factions enabled
												return true;
								return false;//if no factions match, return false
						} else//end if factions enabled
								return true;
				}//CheckFactions
	
				bool CheckGroups (TradePost post1, TradePost post2)
				{//Check in same group
						if (groups.enabled) {//check if groups enabled
								for (int g = 0; g<groups.names.Count; g++)//go through groups
										if (post1.groups.enabled [g] && post2.groups.enabled [g])//if both have the groups enabled
												return true;
								return false;//if no groups match, return false
						} else//end if groups enanled
								return true;
				}//CheckGroups
	
				public bool CheckTraderFaction (Trader trader, TradePost post)
				{//Check that the trader is in the same faction as the trade post
						if (factions.enabled) {//check if factions enabled
								for (int f = 0; f<factions.factions.Count; f++) {//go through factions
										if (trader.factions [f] && post.factions.enabled [f])//if both have the factions enabled
												return true;
								}//end for factions
								return false;//if no factions match, return false
						} else//end if factions enabled
								return true;
				}//end CheckTraderFaction
		#endregion
	
				int FindMinimum (float[] toCheck, int tradePostLength, out float distance)
				{//go through the given list, finding the minimum value, and the index
						int index = 0;
						float min = toCheck [0];
						float currentCheck = 0;
						for (int c = 0; c<toCheck.Length; c++) {
								currentCheck = toCheck [c];
								if (currentCheck < min) {
										index = c;
										min = currentCheck;
								}//end if minimum
						}//end for all checks
						distance = min;
						return index;
				}//end FindMinimum
	
				void Average ()
				{//Go through all of the goods, getting the numbers, so an average can be found
						//only called at start because goes through and gets items at the trade posts, so does not see what is carried
						for (int g = 0; g<goods.Count; g++) {//go through all groups
								for (int i = 0; i<goods[g].goods.Count; i++) {//go through all items
										int count = 0;//the number of trade posts it is enabled at
										int total = 0;//the number of items available
										for (int p = 0; p<postScripts.Length; p++) {//go through all posts
												Stock current = postScripts [p].stock [g].stock [i];
												if ((current.buy || current.sell) && postScripts[p].allowTrades) {//if is enabled at the post and trade post allowed to trade
														count++;//increase the post count
														total += current.number;//increase the total
												}//end check is enabled
										}//end for all posts
										//now has got total number of items, and the post count
										goods [g].goods [i].postCount = count;
										if (total == 0)//if no items, then set average to 0
												goods [g].goods [i].average = 0;
										else//else if there are items
												goods [g].goods [i].average = total / (count * 1f);
								}//end for all items
						}//end for all groups
				}//end Average
	
				public void UpdateAverage (int groupID, int itemID, int numberChange, int postChange)
				{//called to update the average of the number of an item after the first initial averages
						//can update averages from number changes and / or post changes
						Goods good = goods [groupID].goods [itemID];//the good that is changed
						good.average = (good.average * good.postCount + numberChange) / (good.postCount + postChange);//update the average
						good.postCount += postChange;//update the post count
				}//end UpdateAverage
	
				void UpdateMethods ()
				{//Update the post prices and sort the traders
						for (int p = 0; p< tradePostCount; p++) { //set updates to false, so only the required prices are updated and only once
								postScripts [p].updated = false;//set to false so that prices can be updated if necessary
								postScripts [p].ManufactureCheck ();
						}
						TradeCall ();//call the TradeCall method, to tell traders where to go
				}//end UpdateMethods
	
				public void SortTradePosts ()
				{//make sure that all trade posts have the required settings
						for (int t = 0; t<postScripts.Length; t++)//go throuh all trade posts, sorting out the requried settings
								SortTradePost (postScripts [t]);
				}//end SortTradePosts
	
				public void SortTradePost (TradePost thisPost)
				{//sort out the trade post so shows correct items
						#region sort goods
						while (thisPost.stock.Count<goods.Count)//while not enough groups
								thisPost.stock.Add (new StockGroup{});
						if (thisPost.stock.Count > goods.Count)//if too many, remove the extra groups
								thisPost.stock.RemoveRange (goods.Count, thisPost.stock.Count - goods.Count);
						//should now have the correct number of groups
		
						for (int g = 0; g<thisPost.stock.Count; g++) {//for each group
								while (thisPost.stock[g].stock.Count<goods[g].goods.Count) //while not enough goods
										thisPost.stock [g].stock.Add (new Stock{buy = true, sell = true});
								if (thisPost.stock [g].stock.Count > goods [g].goods.Count)//if too many, remove extra
										thisPost.stock [g].stock.RemoveRange (goods [g].goods.Count, thisPost.stock [g].stock.Count - goods [g].goods.Count);
						}
						#endregion
		
						#region sort manufacturing
						while (thisPost.manufacture.Count<manufacture.Count)//while not enough
								thisPost.manufacture.Add (new MnfctrGroup{});
						if (thisPost.manufacture.Count > manufacture.Count)//if too many, remove extra
								thisPost.manufacture.RemoveRange (manufacture.Count, thisPost.manufacture.Count - manufacture.Count);
						//should now have the correct number of groups
			
						for (int m = 0; m<thisPost.manufacture.Count; m++) {//for each group
								while (thisPost.manufacture[m].manufacture.Count<manufacture[m].manufacture.Count)//while not enough
										thisPost.manufacture [m].manufacture.Add (new PostMnfctr{});
								if (thisPost.manufacture [m].manufacture.Count > manufacture [m].manufacture.Count)//if too many, remove extra
										thisPost.manufacture [m].manufacture.RemoveRange (manufacture [m].manufacture.Count, thisPost.manufacture [m].manufacture.Count - manufacture [m].manufacture.Count);
						}
						#endregion
	
						#region sort tags
						if (postTags.enabled) {//only need to sort if enabled
								while (thisPost.tags.enabled.Count<postTags.names.Count)//while not enough tags
										thisPost.tags.enabled.Add (false);
								if (thisPost.tags.enabled.Count > postTags.names.Count)//if too many tags
										thisPost.tags.enabled.RemoveRange (postTags.names.Count, thisPost.tags.enabled.Count - postTags.names.Count);
						}//end if enabled
						#endregion
		
						#region sort groups
						if (groups.enabled) {//only need to sort if enabled
								while (thisPost.groups.enabled.Count<groups.names.Count)//while not enough groups
										thisPost.groups.enabled.Add (false);
								if (thisPost.groups.enabled.Count > groups.names.Count)//if too many groups
										thisPost.groups.enabled.RemoveRange (groups.names.Count, thisPost.groups.enabled.Count - groups.names.Count);
						}//end if enabled
						#endregion
		
						#region sort factions
						if (factions.enabled) {//only need to sort if enabled
								while (thisPost.factions.enabled.Count<factions.factions.Count)//while not enough factions
			
										thisPost.factions.enabled.Add (false);
								if (thisPost.factions.enabled.Count > factions.factions.Count)//if too many factions
										thisPost.factions.enabled.RemoveRange (factions.factions.Count, thisPost.factions.enabled.Count - factions.factions.Count);
						}//end if enabled
						#endregion
				}//end SortTradePost
	
				public void SortTraders ()
				{//make sure that all of the traders have the required settings
						for (int t = 0; t<traderScripts.Length; t++)
								SortTrader (traderScripts [t]);
				}//end SortTraders
	
				public void SortTrader (Trader thisTrader)
				{//sort out the trader so shows correct items
						#region sort goods
						while (thisTrader.allowItems.Count < goods.Count)//while not enough groups
								thisTrader.allowItems.Add (new AllowGroup{});
						if (thisTrader.allowItems.Count > goods.Count)//if too many, remove extra
								thisTrader.allowItems.RemoveRange (goods.Count, thisTrader.allowItems.Count - goods.Count);
						//should now have the correct number of groups
		
						for (int a = 0; a<thisTrader.allowItems.Count; a++) {//for each group
								while (thisTrader.allowItems[a].allowItems.Count < goods[a].goods.Count)//while not enough goods
										thisTrader.allowItems [a].allowItems.Add (new AllowItem{ enabled = true});
								if (thisTrader.allowItems [a].allowItems.Count > goods [a].goods.Count) //if too many, remove extra
										thisTrader.allowItems [a].allowItems.RemoveRange (goods [a].goods.Count, thisTrader.allowItems [a].allowItems.Count - goods [a].goods.Count);
						}
						#endregion
						while (thisTrader.factions.Count<factions.factions.Count)//while not enough factions
								thisTrader.factions.Add (false);
						if (thisTrader.factions.Count > factions.factions.Count)//if too many factions
								thisTrader.factions.RemoveRange (factions.factions.Count, thisTrader.factions.Count - factions.factions.Count);
						#region factions

						#endregion
				}//end SortTrader
	
				void TradeCall ()
				{//go through all of the traders, checking the nearest posts and working out the best to go to
						int count = 0;
						for (int t = 0; t<traderCount; t++) {//go through all of the traders
								Trader trader = traderScripts [t];
								if (!trader.onCall) {//check that the trader is not already doing something
										count++;
										int traderPostID = trader.postID;
										UpdateLists (traderPostID);//update the buy and sell lists for closest posts
										List<TradeInfo> trades = BestTrade (traderPostID, t);//find the best trades
				
										if (trades != null) {//needs to check that there is a trade that it can do
												TradePost post = postScripts [traderPostID];//the trade post that the trader is currently at
				
												float totalTime = 0;//this is the time that the trader needs to pause for if pause option set to cargo mass (specific)
				
												for (int c = 0; c<trades.Count; c++) {//go through all of the cargo trades, adding as many as possible to the trader cargo
														TradeInfo cTrade = trades [c];//the current trade
														Goods cGood = goods [cTrade.groupID].goods [cTrade.itemID];//the current good
														double mass = cGood.mass;//the mass of the good
														Stock stock = post.stock [cTrade.groupID].stock [cTrade.itemID];//the stock
														int price = stock.price;//the price of the item
						
														//need to check quantity again because are loading more than one type of item, and quantity was based on full loads
														int quantity = Mathf.Min ((int)System.Math.Floor (trader.spaceRemaining / mass), cTrade.quantity,
							Mathf.FloorToInt (trader.cash / price));
						
														if (quantity > 0) {//check that adding cargo
																trader.cargo.Add (new NeedMake{ groupID = cTrade.groupID, itemID = cTrade.itemID, number = quantity});//add to lists
																trader.spaceRemaining -= quantity * mass;//remove the space at the start, if cant afford, space and quantity updated at end
																totalTime += quantity * cGood.pausePerUnit;//multiply the time by the quantity
							
																for (int q = 0; q<quantity; q++) {//add the quantity individually
																		if (trader.cash >= price) {
																				//make sure that the trader can afford to buy more items
																				stock.number --;//remove single item of stock
																				trader.cash -= price;//pay for the items
																				post.cash += price;//give the money to the trade post
																				if (priceUpdates)//if updating the price after each item
																						post.UpdateSinglePrice (cTrade.groupID, cTrade.itemID);
																		} else {//trader cannot afford to pay
																				//this is unlikely to be called, but there is still the slight possiblity that the changes in prices mean that the
																				//trader can no longer afford all of the items
																				int unable = quantity - q;//the number of items that the trader was unable to purchase
																				trader.spaceRemaining += unable * mass;//give the space remaining back
																				trader.cargo [trader.cargo.Count - 1].number -= unable;//reduce the number carried
																				totalTime -= unable * cGood.pausePerUnit;//reduce the pause time
																		}//end else can't afford
																}//end for all items to add
														}//check enough space remaining
						
														if (trader.spaceRemaining == 0)//if no space left, break as no point continuing
																break;
														//still continues through loop if there is some space left because may be smaller items to be added
												}//end for all cargo trades
												post.UpdatePrices ();//update the prices at the trade post
												trader.onCall = true;//set the trader to be doing something, so will not be given new trades
												trader.finalPost = postScripts [trades [0].postID];//set the final post to the TradePost script to make it easier when trader gets there
												trader.target = trader.finalPost.gameObject;//set the target to the trade post game object
												trader.postID = trades [0].postID;//set the postID
												StartCoroutine (trader.PauseExit (totalTime));//now pause
										} else {//if no trades, then needs to move
												if (randomPost) {//if random posts selected, go to a random post in the closest
														if (CheckTraderFaction (trader, postScripts [traderPostID])) {//check that the trader is in the same faction as the trade post in the first place
																Distances[] reachable = System.Array.FindAll<Distances> (closest [traderPostID], x => (x.post != -1) && CheckTraderFaction (trader, postScripts [x.post]) && postScripts[x.post].allowTrades);//get all of the reachable posts
																//make the reachable array up of posts that are possible to get to, by making sure not -1 and in the same faction
																if (reachable.Length == 0) {
																		Debug.LogError (trader.name + " has no reachable posts! This may be due to incorrect factions or groups");
																		break;
																}
																int randomIndex = Random.Range (0, reachable.Length);
																int selectedPost = reachable [randomIndex].post;
																trader.onCall = true;//set to true, so will not be told other posts
																trader.finalPost = postScripts [selectedPost];//set the final post
																trader.target = trader.finalPost.gameObject;//set the target gameobject
																trader.postID = selectedPost;//set the postID
																StartCoroutine (trader.PauseExit (0));//now pause
														} else//end check in the same faction as the trader
																Debug.LogError (trader.name + " is not in the same faction as the starting trade post, so is not able to make any trades ");
												} else {//else needs to work out which post to go to
														//needs to go through all of the closest reachable posts, and find which has the best trade
														//done by following the best trade and update methods from this, and adding the best trades into another array
														//compare this best of the best array, and then select the destination post
												}//end else find post
										}//end else move
								}//end check not already trading
						}//end for traders
				}//end TradeCall
	
				int GetPostID (GameObject post)
				{//find out which index in the arrays the target post is
						for (int p = 0; p<tradePostCount; p++)//go through trade posts
								if (tradePosts [p] == post)//check if the same
										return p;//return the index
						return -1;//if not found, return -1
				}//end GetPostID
	
				void UpdateLists (int postID)
				{//for the current post, update the sell lists, for each of the closest posts, update the buy lists
						#region price updates
						//price updates are called here so that they are not pointlessly called in each update interval
						if (!postScripts [postID].updated) {//check that has not updated the prices already
								postScripts [postID].UpdatePrices ();//update the current post prices
								postScripts [postID].updated = true;
						}//end if not updated current post
						for (int p = 0; p<closestPosts; p++) {
								int cPID = closest [postID] [p].post;
								if (cPID != -1 && !postScripts [cPID].updated && postScripts[cPID].allowTrades) {//check not -1 or price already updated and allowed to trade
										postScripts [cPID].UpdatePrices ();
										postScripts [cPID].updated = true;
								}//end if not -1 and not updated post
						}//go through the closest posts and update the prices
						#endregion
	
						for (int g = 0; g<goods.Count; g++) {//go through all groups
								for (int i = 0; i<goods[g].goods.Count; i++) {//go through all items	
										float itemAverage = (float)goods [g].goods [i].average;
										#region add current
										//sort the sell list
										Stock currentStock = postScripts [postID].stock [g].stock [i];
										if (currentStock.sell) {//check to see if the item is enabled
												if (currentStock.number > Mathf.RoundToInt (itemAverage * sellMultiple))
														tradeLists [postID].sell.Add (new BuySell{groupID = g, itemID = i});
										}//end if enabled
										tradeLists [postID].sell = tradeLists [postID].sell.Distinct ().ToList ();
										#endregion
				
										#region add closest
										//sort the buy list
										for (int p = 0; p<closestPosts; p++) {//go through the closest posts
												int currentPostID = closest [postID] [p].post;
												if (currentPostID != -1) {//check that the post is not -1. If it is, then can't get there anyway
														Stock currentPostStock = postScripts [currentPostID].stock [g].stock [i];
														if (currentPostStock.buy) {//check if item is enabled
																if (Mathf.RoundToInt (currentPostStock.number * buyMultiple) < itemAverage ) //check that post wants to buy
																tradeLists [currentPostID].buy.Add (new BuySell{groupID = g, itemID = i});
														}//end enabled check
														tradeLists [currentPostID].buy = tradeLists [p].buy.Distinct ().ToList ();
												}//end check not -1
										}//end for closest posts
										#endregion
								}//end for items
						}//end for groups
		
						#region remove current
						TradePost cP = postScripts [postID];
						for (int s = 0; s<tradeLists[postID].sell.Count; s++) {//go through sell list
								BuySell cSell = tradeLists [postID].sell [s];//the current sell item
								Stock cStock = cP.stock [cSell.groupID].stock [cSell.itemID];//the current stock item at the trade post
								if (!cStock.sell || !(cStock.number > Mathf.Round ((float)goods [cSell.groupID].goods [cSell.itemID].average * sellMultiple))) {//if no longer enabled or not enough to sell
										tradeLists [postID].sell.RemoveAt (s);//remove item from the sell list
										s--;//reduce s
								}//end if need to remove
						}//end for all sell
						#endregion
		
						#region remove closest
						for (int p = 0; p<closestPosts; p++) {//go through closest posts
								int cPID = closest [postID] [p].post;
								if (cPID != -1) {//check not -1
										cP = postScripts [cPID];
										for (int b = 0; b<tradeLists[cPID].buy.Count; b++) {//go through buy list
												BuySell cBuy = tradeLists [cPID].buy [b];//current buy item
												Stock cStock = cP.stock [cBuy.groupID].stock [cBuy.itemID];//current stock item at the trade post
												if (!cStock.buy || !(Mathf.RoundToInt (cStock.number * buyMultiple) < goods [cBuy.groupID].goods [cBuy.itemID].average)) {//if not enabled, or has enough so does not need to buy, then remove
														tradeLists [cPID].buy.RemoveAt (b);
														b--;
												}//end if need to remove
										}//end for buy list
								}//end check not -1
						}//end for closest posts
						#endregion
				}//end UpdateLists
	
				List<TradeInfo> BestTrade (int postID, int traderID)
				{//go through the closest posts, finding the most profitable which is in sell at current and buy at next
						//gets the distance, and uses the different weightings to decide which is the best post
						List<BuySell> sell = tradeLists [postID].sell;//the sell list
						List<BuySell> intersect = new List<BuySell> ();//this is the list containing all of the items which are found in both lists
						List<TradeInfo> trades = new List<TradeInfo> ();

						for (int p = 0; p<closestPosts; p++) {//go through closest posts
								int currentPostID = closest [postID] [p].post;
								if (currentPostID != -1 && CheckTraderFaction (traderScripts [traderID], postScripts [currentPostID]) && postScripts[currentPostID].allowTrades) {
										//check not -1, so can actually get to the trade post, and check that they are in the same faction, and check that trading is allowed
										intersect = sell.Intersect (tradeLists [p].buy).ToList ();				
										for (int i = 0; i<intersect.Count; i++) {//go through all items in intersect
												BuySell cI = intersect [i];
												if (traderScripts [traderID].allowItems [cI.groupID].allowItems [cI.itemID].enabled) {//check that the trader is allowed to take the cargo
														int quantity = TradeQuantity (postID, currentPostID, cI.groupID, cI.itemID, traderID);
					
														if (quantity > 0) {//only work out profit and add if there is more than one to trade		
																int profit = (postScripts [currentPostID].stock [cI.groupID].stock [cI.itemID].price * quantity) -
																		(postScripts [postID].stock [cI.groupID].stock [cI.itemID].price * quantity);
																trades.Add (new TradeInfo{ postID = currentPostID, groupID = intersect [i].groupID, itemID = intersect [i].itemID,
									val = profit * profitWeight - closest [postID] [p].distance * distanceWeight, quantity = quantity});
														}
												}//end check trader allowed to take cargo
										}//end for intersect
								}//end -1 check
						}//end for closest posts
						//now needs to find the best of the trades
						if (trades.Count > 0) {//only sort if there is something to sort
								trades.Sort ();
								return trades.FindAll (x => x.postID == trades [0].postID);//only return those in the same group as the best
						}
						return null;//retun null if no trades available
				}//end BestTrade
	
				int TradeQuantity (int c, int t, int gID, int iID, int traderID)
				{//the IDs of the current and target posts and groupID and itemID//work out the quantity that should be traded
						Goods good = goods [gID].goods [iID];
						float avg = (float)good.average;
						Stock cP = postScripts [c].stock [gID].stock [iID];//the stock at the current post
						int sellQuantity = cP.number - Mathf.RoundToInt (avg * sellMultiple);
						int buyQuantity = Mathf.RoundToInt (avg - (postScripts [t].stock [gID].stock [iID].number * buyMultiple));
						//will now return the minimum of the buy and sell quantities, the number that can fit in the cargo space, 
						//and the number that the trader can purchase, and the number that the trade post can purchase
						return (int)Mathf.Min (sellQuantity, buyQuantity, (float)(traderScripts [traderID].cargoSpace / good.mass), 
				Mathf.FloorToInt (traderScripts [traderID].cash / cP.price), Mathf.FloorToInt (postScripts [t].cash / (postScripts [t].stock [gID].stock [iID].price * purchasePercent)));
				}//end TradeQuantity
	
				public void OnDrawGizmos ()
				{//if show lines enabled, draw trade lines between the posts
						if (showLinks)//check that showLinks has been enabled
								DrawLines ();
				}//end OnDrawGizmos
		
				void DrawLines ()
				{//draw lines showing the possible trade links. Is here so that lines can also be drawn in the game view
						GetPostScripts ();
						for (int p1 = 0; p1<postScripts.Length; p1++) {//go through trade posts
								for (int p2 = p1+1; p2<postScripts.Length; p2++) {//go through possible connection posts
										if (CheckFactionsGroups (postScripts [p1], postScripts [p2])) {//check in the same faction / group
												if (!factions.enabled) {//if factions not enabled, then only green lines
														if (Application.isPlaying && showLinesInGame) {//if is playing, use line renderer
																LineRenderer line = new GameObject ().AddComponent<LineRenderer> ();
																line.transform.parent = this.transform;
																line.material = new Material (Shader.Find ("Self-Illumin/Diffuse"));
																line.material.color = Color.green;
																line.SetPosition (0, tradePosts [p1].transform.position);
																line.SetPosition (1, tradePosts [p2].transform.position);
														} else {//else in editor
																Gizmos.color = Color.green;
																Gizmos.DrawLine (tradePosts [p1].transform.position, tradePosts [p2].transform.position);
														}//end else in editor
												} else {//else, needs to get the colours of the factions
														List<Color> colours = new List<Color> ();
														for (int f = 0; f<factions.factions.Count; f++) {//go through factions, getting the colour if the faction matches
																if (postScripts [p1].factions.enabled [f] && postScripts [p2].factions.enabled [f])//check in the same faction
																		colours.Add (factions.factions [f].colour);//add the colour to the list
														}//end for factions
														int coloursCount = colours.Count;//the number of colours
														Vector3 line = (tradePosts [p2].transform.position - tradePosts [p1].transform.position) / coloursCount;//work out the angle of the line
														//now has got all of the colours to be shown
														for (int c = 0; c<coloursCount; c++) {//go through all of the colours
																if (Application.isPlaying && showLinesInGame) {//is is playing, use line renderer
																		LineRenderer linef = new GameObject ().AddComponent<LineRenderer> ();
																		linef.transform.parent = this.transform;
																		linef.name = tradePosts[p1].name+" "+tradePosts[p2].name+" "+c;
																		linef.material = new Material (Shader.Find ("Self-Illumin/Diffuse"));
																		linef.material.color = colours [c];
																		linef.SetPosition (0, (line * c) + tradePosts [p1].transform.position);
																		linef.SetPosition (1, (line * (c + 1)) + postScripts [p1].transform.position);
																} else {//else in editor
																		Gizmos.color = colours [c];//set the colour
																		Gizmos.DrawLine ((line * c) + tradePosts [p1].transform.position, (line * (c + 1)) + postScripts [p1].transform.position);
																}//end else in editor
														}//end for all colours
												}//end else other colours
										}//end for p2
								}//end for p1
						}//end if in the same faction / group
				}//end DrawLines
		
		}//end Controller
}//end namespace