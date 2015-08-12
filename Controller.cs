using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TradeSys
{//use namespace to stop any name conflicts
		public class Controller : MonoBehaviour
		{
		#if UNITY_EDITOR
				public Selected selected = new Selected();//the selected items in toolbars
				public bool showGN;//show the name of the group that the item belongs to in the manufacturing list
				public bool showPrices;//show the price of the good in the post editor
				public bool smallScroll = true;//have a scroll bar in the goods and manufacturing menus
				public ScrollPos scrollPos = new ScrollPos();//the scroll positions
				public bool showHoriz;//show a list of items horizontally or vertically
				public bool genOp, gamOp, traOp, pauOp;//the bools for if each option is showing in the controller
				public bool opTP, opT;//the bools for if the options are showing for trade posts and traders
				public bool sTi, sNo, sSp;//bools for options and shape for spawners
				public string[][] allNames;//an array for each group with an array of the names
				public string[][] manufactureNames;//an array with the names of each manufacturing process
				public string[][] manufactureTooltips;//an array of strings with the tooltip for the manufacturng processes
		#endif
	
		#region used variables
		#region in editor
				public bool showLinesInGame;//whether lines are shown in the game view
				public float updateInterval = 0.5f;//set the time between subsequent updates
				public bool showLinks = false;//show possible trade links
	
				public bool generateAtStart = true;//if this is true, then generate distances at start. Disable if posts generated in code
				public bool pickUp = false;//whether you can collect items or not. Enabling allows setting of itemCrates
				public GameObject defaultCrate;//the default crate used if a crate has not been specified
	
				public int closestPosts = 10;//this is the number of closest posts that should be taken into account
				public float distanceWeight = 1;//the weighting of the distance to find the best post
				public float profitWeight = 1;//the weighting of the profits to find the best post
				public float buyMultiple = 1.5f;//used to decide which items should be bought
				public float sellMultiple = 1.5f;//used to decide which items should be sold
				public float purchasePercent = 0.7f;//used for the prices of items when the trade post is buying
				public bool priceUpdates;//if true, will update the price after purchasing each item, so prices may increase / decrease while traders purchase
				public ExpendableList expTraders;//the different types of expendable trader that can be selected
				public int moveType = 0;//the different move options the traders have
				//0 = random
				//1 = max(number to sell / distance)
	
				public int pauseOption = 0;	//the selected pause option
				public float pauseTime;//the time to pause for
				public bool pauseEnter, pauseExit;//pause on post entry and / or exit
	
				public List<GoodsTypes> goods = new List<GoodsTypes> ();//all of the goods are within this list, and in different categories
				public List<MnfctrTypes> manufacture = new List<MnfctrTypes> ();//all processes are within groups in this list
	
				public GameObject[] tradePosts;//all of the Trade Posts in the game
				public TradePost[] postScripts;//all of the scripts of the trade posts
	
				public GameObject[] traders;//all of the traders in the game
				public Trader[] traderScripts;//all of the scripts of the traders
				
				public Spawner[] spawners;//all of the spawner scripts in the game
	
				public EnableList postTags;//all of the tags that can be selected by a trade post
				public EnableList groups;//the different groups that a trade post can be in
				public Factions factions;//the different factions that a trade post and trader can be in
	
				public Units units;//the different units that can be used
		#endregion
	
		#region other
				public float[][] postDistances;//This is used so that sqrMagnitude is called as little as possible
				Distances[][] closest;//this is the closest x number of posts, which are used to work out trader targets
	
				int tradePostCount;//these are used so that the length of the array does not need to be used each time
				internal int traderCount;
				Trade[] tradeLists;//this contains all of the buy and sell items at each trade post
				//needmake has this info, so uses that instead of a new class
		#endregion
		#endregion	
	
				//System.Diagnostics.Stopwatch stoppy = new System.Diagnostics.Stopwatch ();

				void Start ()
				{				
						//at the start, the only thing that can be checked is to see if generate has been selected
						//if is has been selected, then generate and the rest can continue, but if not, then needs to wait to be called
						//this is done once all of the trade posts have been set up from your code.	
						if (generateAtStart)//generate the distances if the trade posts have been set up
								GenerateDistances ();
								
						if (showLinesInGame)//if set debug bool to show lines in the game view
								DrawLines ();
								
						if (pickUp && defaultCrate != null) {//if it is possible to pick up items
								for (int g = 0; g<goods.Count; g++) {//for all goods groups
										for (int i = 0; i<goods[g].goods.Count; i++) {//for all items
												if (goods [g].goods [i].itemCrate == null)
														goods [g].goods [i].itemCrate = defaultCrate;//set the item crate to be the default
										}//end for all items
								}//end for goods groups
						}//end if need to sort crates
						
						if (expTraders.enabled)//if expendable, set the profit weight to be 0
								profitWeight = 0;
				}//end Start
				
				float CalcDistance (GameObject tp1, GameObject tp2)
				{//calculate the distance between the two specified trade posts
						//this method is used as the distances between your trade posts may not be a direct striaght line
						//for example, there may be obstacles in the way, so the actual distance will not be equal to the straight line distance
						return (tp1.transform.position - tp2.transform.position).sqrMagnitude;//return the distance
				}//end CalcDistance
				
				public void SingleDist (int postID, int start)
				{//calculate the distance from one trade post to others
						for (int p = start; p<tradePostCount; p++) {//go through posts to get distances
								if (postID == p)//if the same, needs to be infinity so traders will move
										postDistances [postID] [p] = Mathf.Infinity;//need to set this to infinity so that itself is not an optional post
				else
										postDistances [postID] [p] = postDistances [p] [postID] = CalcDistance (tradePosts [postID], tradePosts [p]);
								//can set these the same to save on the number of calculations required, so is 0.5n(n+1)
						}//end 2nd for	
				}//end SingleDist
	
				public void GenerateDistances ()
				{//At the start, needs to generate all of the distances and the closest posts for use later. As this is called first, will call all other methods
						//generate the distances so that dont need to call sqrMagnitude each time, instead it can just be referenced from the array
						SortAll ();//needs to make sure that everything is sorted properly before starting

						//initialise arrays
						postDistances = new float[tradePostCount][];
						closest = new Distances[tradePostCount][];
						tradeLists = new Trade[tradePostCount];
		
						for (int t = 0; t<tradePostCount; t++) {//need to initialise second array before getting distances because will be filling up later elements at same time
								postDistances [t] = new float[tradePostCount];
								tradeLists [t] = new Trade ();
						}//end for initialisation distances and trade lists
						
						for (int t = 0; t<traderCount; t++) {//need to add all of the trader scripts, and get the starting postID
								Trader trader = traderScripts [t];
								int postID = GetPostID (trader.target);
								trader.postID = trader.homeID = postID;
								trader.startPost = trader.finalPost = postScripts [postID];
						}//end go through traders
						
						if (!expTraders.enabled) {//if expendable, check not null
								for (int t = 0; t<expTraders.traders.Count; t++) {//go through all expendable and check arent null
										if (expTraders.traders [t] == null) {//check if null
												Debug.LogError ("One or more expendable traders have been set to null");
												break;
										}//end if null						
								}//end for all expendable
						}//end if expendable
			
						#region get distances
						//now needs to go through and get all of the distances between all of the posts, where they are in the same group and faction
						for (int p = 0; p<tradePostCount; p++)//go through the posts
								SingleDist (p, p);
						#endregion
	
						GetClosest ();//get the closest posts

						//Now needs to call other methods
						Average ();
						if (!expTraders.enabled)//only update the prices if not expendable
								for (int p = 0; p<tradePostCount; p++)
										postScripts [p].UpdatePrices ();
						InvokeRepeating ("UpdateMethods", 0, updateInterval);
				}//end GenerateDistances
	
		#region get scripts
				public void GetPostScripts ()
				{//get all of the trade posts and get the scripts
						tradePosts = GameObject.FindGameObjectsWithTag (Tags.TP);//get all of the trade posts
						tradePostCount = tradePosts.Length;//set the post count to the length
						postScripts = new TradePost[tradePostCount];
		
						for (int t = 0; t<tradePostCount; t++) { //go through trade posts getting script
								postScripts [t] = tradePosts [t].GetComponent<TradePost> ();
								postScripts [t].postID = t;//set the postID
						}//end for all posts
				}//end GetPostScripts
				
				public void GetTraderScripts ()
				{//get all of the traders and get the scripts
						traders = GameObject.FindGameObjectsWithTag (Tags.T);//get all of the traders
						traderCount = traders.Length;//set the post count to the length
						traderScripts = new Trader[traderCount];
			
						for (int t = 0; t<traderCount; t++) //go through traders getting script
								traderScripts [t] = traders [t].GetComponent<Trader> ();
				}//end GetTraderScripts
		#endregion
	
				public void GetClosest ()
				{//get the closest posts from the distance array
						if (closestPosts > 0 && closestPosts < tradePostCount) {//check if the closest posts not set to 0 and is less than the total number
								//now goes through the distances, taking the closest x posts to the target post
								for (int p = 0; p<tradePostCount; p++) {//go through posts
										closest [p] = new Distances[closestPosts];
										for (int d = 0; d<closestPosts; d++) {//go through enough times to get x number of posts
												closest [p] [d] = new Distances ();
					
												float min;
												int index = FindMinimum (postDistances [p], tradePostCount, closest [p], p, out min);
					
												if (min == Mathf.Infinity) {
														for (int dc = d; dc<closestPosts; dc++) {//needs to go through the remaining closest, setting to infinity
																closest [p] [dc] = new Distances ();
																closest [p] [dc].post = -1;
																closest [p] [dc].distance = Mathf.Infinity;
														}
														break;
												}
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
				}//end GetClosest
		
		#region faction group check
				public bool CheckFactionsGroups (TradePost post1, TradePost post2)
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
										if (post1.factions [f] && post2.factions [f])//if both have the factions enabled
												return true;
								return false;//if no factions match, return false
						} else//end if factions enabled
								return true;
				}//CheckFactions
	
				bool CheckGroups (TradePost post1, TradePost post2)
				{//Check in same group
						if (groups.enabled) {//check if groups enabled
								for (int g = 0; g<groups.names.Count; g++)//go through groups
										if (post1.groups [g] && post2.groups [g])//if both have the groups enabled
												return true;
								return false;//if no groups match, return false
						} else//end if groups enanled
								return true;
				}//CheckGroups
	
				public bool CheckTraderFaction (Trader trader, TradePost post)
				{//Check that the trader is in the same faction as the trade post
						if (factions.enabled) {//check if factions enabled
								for (int f = 0; f<factions.factions.Count; f++) {//go through factions
										if (trader.factions [f] && post.factions [f])//if both have the factions enabled
												return true;
								}//end for factions
								return false;//if no factions match, return false
						} else//end if factions enabled
								return true;
				}//end CheckTraderFaction
		#endregion
	
				int FindMinimum (float[] toCheck, int tradePostLength, Distances[] done, int postID, out float distance)
				{//go through the given list, finding the minimum value, and the index
						int index = 0;
						float min = Mathf.Infinity;
						float currentCheck = 0;
						for (int c = 0; c<toCheck.Length; c++) {
								currentCheck = toCheck [c];
								if (currentCheck < min && !done.Any (x => x != null && x.post == c) && 
										CheckFactionsGroups (postScripts [postID], postScripts [c])) {
										//check that the current is less, has not been added and share a group and faction
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
										for (int p = 0; p<tradePostCount; p++) {//go through all posts
												Stock current = postScripts [p].stock [g].stock [i];
												if ((current.buy || current.sell) && postScripts [p].allowTrades) {//if is enabled at the post and trade post allowed to trade
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
								postScripts [p].ManufactureCheck ();//call the manufacturing methods in the trade post
						}//end for all posts
							
						if (expTraders.enabled)
								ExpendableCall ();//call the expendable trader method to tell where to make expendable traders go
						else	
								TradeCall ();//call the TradeCall method, to tell traders where to go
				}//end UpdateMethods
				
				public void SortAll ()
				{//sort out all trade posts and traders
						//only do this if not in the editor
						GetPostScripts ();
						
						if (!expTraders.enabled)//if not expendable, get the trader scripts
								GetTraderScripts ();
				
						SortTradePosts ();
						SortTraders ();
						SortSpawners ();
						ManufactureMass ();
				}//end SortAll
	
				public void SortTradePosts ()
				{//make sure that all trade posts have the required settings
						for (int t = 0; t<tradePostCount; t++)//go throuh all trade posts, sorting out the requried settings
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
										thisPost.manufacture [m].manufacture.Add (new RunMnfctr{});
								if (thisPost.manufacture [m].manufacture.Count > manufacture [m].manufacture.Count)//if too many, remove extra
										thisPost.manufacture [m].manufacture.RemoveRange (manufacture [m].manufacture.Count, thisPost.manufacture [m].manufacture.Count - manufacture [m].manufacture.Count);
						}
						#endregion
	
						#region sort tags
						if (postTags.enabled) {//only need to sort if enabled
								while (thisPost.tags.Count<postTags.names.Count)//while not enough tags
										thisPost.tags.Add (false);
								if (thisPost.tags.Count > postTags.names.Count)//if too many tags
										thisPost.tags.RemoveRange (postTags.names.Count, thisPost.tags.Count - postTags.names.Count);
						}//end if enabled
						#endregion
		
						#region sort groups
						if (groups.enabled) {//only need to sort if enabled
								while (thisPost.groups.Count<groups.names.Count)//while not enough groups
										thisPost.groups.Add (false);
								if (thisPost.groups.Count > groups.names.Count)//if too many groups
										thisPost.groups.RemoveRange (groups.names.Count, thisPost.groups.Count - groups.names.Count);
						}//end if enabled
						#endregion
		
						#region sort factions
						if (factions.enabled) {//only need to sort if enabled
								while (thisPost.factions.Count<factions.factions.Count)//while not enough factions
										thisPost.factions.Add (false);
								if (thisPost.factions.Count > factions.factions.Count)//if too many factions
										thisPost.factions.RemoveRange (factions.factions.Count, thisPost.factions.Count - factions.factions.Count);
						}//end if enabled
						#endregion
				}//end SortTradePost
	
				public void SortTraders ()
				{//make sure that all of the traders have the required settings
						for (int t = 0; t<traderCount; t++)
								SortTrader (traderScripts [t]);
				}//end SortTraders
	
				public void SortTrader (Trader thisTrader)
				{//sort out the trader so shows correct items
						#region sort goods
						while (thisTrader.items.Count < goods.Count)//while not enough groups
								thisTrader.items.Add (new ItemGroup{});
						if (thisTrader.items.Count > goods.Count)//if too many, remove extra
								thisTrader.items.RemoveRange (goods.Count, thisTrader.items.Count - goods.Count);
						//should now have the correct number of groups
		
						for (int a = 0; a<thisTrader.items.Count; a++) {//for each group
								while (thisTrader.items[a].items.Count < goods[a].goods.Count)//while not enough goods
										thisTrader.items [a].items.Add (new ItemCargo{ enabled = true});
								if (thisTrader.items [a].items.Count > goods [a].goods.Count) //if too many, remove extra
										thisTrader.items [a].items.RemoveRange (goods [a].goods.Count, thisTrader.items [a].items.Count - goods [a].goods.Count);
						}
						#endregion
						
						#region sort manufacturing
						while (thisTrader.manufacture.Count<manufacture.Count)//while not enough
								thisTrader.manufacture.Add (new MnfctrGroup{});
						if (thisTrader.manufacture.Count > manufacture.Count)//if too many, remove extra
								thisTrader.manufacture.RemoveRange (manufacture.Count, thisTrader.manufacture.Count - manufacture.Count);
						//should now have the correct number of groups
			
						for (int m = 0; m<thisTrader.manufacture.Count; m++) {//for each group
								while (thisTrader.manufacture[m].manufacture.Count<manufacture[m].manufacture.Count)//while not enough
										thisTrader.manufacture [m].manufacture.Add (new RunMnfctr{});
								if (thisTrader.manufacture [m].manufacture.Count > manufacture [m].manufacture.Count)//if too many, remove extra
										thisTrader.manufacture [m].manufacture.RemoveRange (manufacture [m].manufacture.Count, thisTrader.manufacture [m].manufacture.Count - manufacture [m].manufacture.Count);
						}
						#endregion
						
						#region factions
						if (factions.enabled) {//only need to sort if factions enabled
								while (thisTrader.factions.Count<factions.factions.Count)//while not enough factions
										thisTrader.factions.Add (false);
								if (thisTrader.factions.Count > factions.factions.Count)//if too many factions
										thisTrader.factions.RemoveRange (factions.factions.Count, thisTrader.factions.Count - factions.factions.Count);
						}//end if factions enabled
						#endregion
				}//end SortTrader
				
				public void SortSpawners ()
				{//make sure that all of the spawners have the required settings
						GameObject[] spawnerObjects = GameObject.FindGameObjectsWithTag (Tags.S);
						spawners = new Spawner[spawnerObjects.Length];
						
						if (spawnerObjects.Length > 0 || GameObject.FindGameObjectsWithTag (Tags.I).Length > 0)
						//if spawners are in game or items, needs to be set to true
								pickUp = true;
				
						for (int s = 0; s<spawnerObjects.Length; s++) {//go through spawner objects, getting the scripts and sorting
								spawners [s] = spawnerObjects [s].GetComponent<Spawner> ();
								SortSpawner (spawners [s]);
						}//end for all spawners
				}//end SortSpawners
		
				public void SortSpawner (Spawner thisSpawner)
				{//sort out the spawner so shows correct items
						while (thisSpawner.items.Count < goods.Count)//while not enough groups
								thisSpawner.items.Add (new ItemGroup{});
						if (thisSpawner.items.Count > goods.Count)//if too many, remove extra
								thisSpawner.items.RemoveRange (goods.Count, thisSpawner.items.Count - goods.Count);
						//should now have the correct number of groups
			
						for (int a = 0; a<thisSpawner.items.Count; a++) {//for each group
								while (thisSpawner.items[a].items.Count < goods[a].goods.Count)//while not enough goods
										thisSpawner.items [a].items.Add (new ItemCargo{ enabled = true});
								if (thisSpawner.items [a].items.Count > goods [a].goods.Count) //if too many, remove extra
										thisSpawner.items [a].items.RemoveRange (goods [a].goods.Count, thisSpawner.items [a].items.Count - goods [a].goods.Count);
						}
				}//end SortSpawner
			
				public void ManufactureMass ()
				{//go through manufacture processes and calculate the needing and making masses
						for (int m = 0; m<manufacture.Count; m++) {//go through groups
								for (int p = 0; p<manufacture[m].manufacture.Count; p++) {//go throughprocesses
										Mnfctr cMan = manufacture [m].manufacture [p];
				
										cMan.needingMass = ManufactureMass (cMan.needing);
										cMan.makingMass = ManufactureMass (cMan.making);
			
								}//end for processes
						}//end for groups
				}//end ManufactureMass
				
				float ManufactureMass (List<NeedMake> cNM)
				{//return the mass of the needing or making list provided
						float mass = 0;
						NeedMake cItem = new NeedMake ();
						for (int nm = 0; nm<cNM.Count; nm++) {//go through list
								cItem = cNM [nm];
								if (cItem.groupID != -1 && cItem.itemID != -1)
										mass += goods [cItem.groupID].goods [cItem.itemID].mass * cItem.number;//add to total mass
						}//end for list
						return mass;
				}//end ManufactureMass
	
				void TradeCall ()
				{//go through all of the traders, checking the nearest posts and working out the best to go to
						for (int t = 0; t<traderCount; t++) {//go through all of the traders
								Trader trader = traderScripts [t];
								if (!trader.onCall)//check that the trader is not already doing something
										TraderSet (trader);
						}//end for traders
				}//end TradeCall
				
				void ExpendableCall ()
				{//called if expendable traders. tells traders where to go
						if (expTraders.traders.Count == 0) {//if no expendable traders selected
								Debug.LogError ("Traders must be added to the list in order to have expendable traders");
								return;
						}//end if no expendable traders						
			
						while (traderCount < expTraders.maxNoTraders || expTraders.maxNoTraders == 0) {//while can make more traders
								int postID = Random.Range (0, tradePostCount);//pick a trade post
								Trader trader = (Trader)Instantiate (expTraders.traders [Random.Range (0, expTraders.traders.Count)], 
									tradePosts [postID].transform.position, Quaternion.identity);//make the trader and get the script
								trader.transform.parent = transform;//set the parent to be the controller
								trader.postID = trader.homeID = postID;//set the post ids
								trader.startPost = postScripts [postID];//set the start post straight away
				
								traderCount++;//increase the trader count
				
								if (!TraderSet (trader))//set the trader and check is ok
										break;//break until next time if the trader isnt going anywhere
						}//end while make more traders			
				}//end ExpendableCall
				
				bool TraderSet (Trader trader)
				{//find the best trades etc for the given trader
						//is a bool because if is expendable, it may end up going through the posts an infinite number of times, so returns false if a trader could not be set so wont lock up
						int traderPostID = trader.postID;
						UpdateLists (traderPostID);//update the buy and sell lists for closest posts
						List<TradeInfo> trades = BestTrade (traderPostID, trader);//find the best trades
			
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
										int quantity = Mathf.Min ((int)System.Math.Floor (trader.spaceRemaining / mass), cTrade.quantity);
										
										if (!expTraders.enabled)//if not expendable, also check the prices for min quantity
												quantity = Mathf.Min (quantity, Mathf.FloorToInt (trader.cash / price));//check the price one here as is not expendable
					
										if (quantity > 0) {//check that adding cargo														
												trader.items [cTrade.groupID].items [cTrade.itemID].number += quantity;//add to cargo
												trader.spaceRemaining -= quantity * mass;//remove the space at the start, if cant afford, space and quantity updated at end
												totalTime += quantity * cGood.pausePerUnit;//multiply the time by the quantity
						
												for (int q = 0; q<quantity; q++) {//add the quantity individually
														if (trader.cash >= price || expTraders.enabled) {
																//make sure that the trader can afford to buy more items or is expendable so doesnt matter
																stock.number --;//remove single item of stock
																
																if (!expTraders.enabled) {//only sort cash if not expendable
																		trader.cash -= price;//pay for the items
																		post.cash += price;//give the money to the trade post
																		if (priceUpdates)//if updating the price after each item
																				post.UpdateSinglePrice (cTrade.groupID, cTrade.itemID);
																}//end if not expendable, sort cash
														} else {//trader cannot afford to pay
																//this is unlikely to be called, but there is still the slight possiblity that the changes in prices mean that the
																//trader can no longer afford all of the items
																int unable = quantity - q;//the number of items that the trader was unable to purchase
																trader.spaceRemaining += unable * mass;//give the space remaining back
																trader.items [cTrade.groupID].items [cTrade.itemID].number -= unable;//reduce the number carried
																totalTime -= unable * cGood.pausePerUnit;//reduce the pause time
																break;
														}//end else can't afford
												}//end for all items to add
										}//check enough space remaining
					
										if (trader.spaceRemaining == 0)//if no space left, break as no point continuing
												break;
										//still continues through loop if there is some space left because may be smaller items to be added
								}//end for all cargo trades
								post.UpdatePrices ();//update the prices at the trade post
				
								SendToPost (trader, trades [0].postID, false, totalTime);//send the trader to the trade post
						} else {//if no trades, then needs to move or be deleted
								if (expTraders.enabled) {//if expendable and have got here, delete the trader
										trader.DestroyTrader ();
										return false;//return so dont need to do anything else
								}//end if expendable
								if (trader.tradeType > 0) {//if is depot and nothing to take
										SendToPost (trader, trader.homeID, true, 0);//send the trader back to the depot
										return false;//return so doesnt do the random post bit
								}//end if depot
								
								switch (moveType) {//have a switch for the different options
								case 0://random post
										MoveToRandom (trader, traderPostID);//call the random method										
										break;
								
								case 1://max(number to sell / distance)
										NeedMake[] bestMove = new NeedMake[closestPosts];//use a NeedMake[] as groupID = postID and number = number
								
										for (int p = 0; p<closestPosts; p++) {//for all trade posts
												bestMove [p] = new NeedMake ();
												bestMove [p].groupID = closest [traderPostID] [p].post;//set the groupID to be the postID
												bestMove [p].itemID = p;//set the itemID to be the array element ID
												//the element ID is used so that the closest posts array can be used as it will be faster
												if (bestMove [p].groupID != -1 && CheckTraderFaction (trader, postScripts [bestMove [p].groupID])) {//if is not -1 and reachable
														for (int g = 0; g<goods.Count; g++) {//for all goods
																for (int i = 0; i<goods[g].goods.Count; i++) {//for all items
																		float itemAverage = (float)goods [g].goods [i].average;
																		//sort the sell list
																		Stock currentStock = postScripts [bestMove [p].groupID].stock [g].stock [i];
																		if (currentStock.sell) {//check to see if the item is enabled
																				if (currentStock.number > Mathf.RoundToInt (itemAverage * sellMultiple))
																						bestMove [p].number++;//increase the number in the list
																		}//end if enabled
																}//end for items
														}//end for goods
												}//end if not -1
										}//end for all trade posts 
									//will now have all of the number of items that the closest posts want to sell
									//the aggregate bit will go through and get the object where the number/distance is greatest and send the trader there
										SendToPost (trader, bestMove.Aggregate ((a,b) => a.number / closest [traderPostID] [a.itemID].distance > b.number / closest [traderPostID] [b.itemID].distance ? a : b).groupID, true, 0);
										break;
								case 2:
										List<TradeInfo> bestTrades = new List<TradeInfo> ();//the list of the very best trade from each trade post
									
										for (int c = 0; c<closestPosts; c++) {//for all closest
												int cPID = closest [traderPostID] [c].post;
												if (cPID == -1)//check not -1
														break;//break if -1
										
												List<TradeInfo> thisList = BestTrade (cPID, trader);
												if (thisList != null) {//if not null
														bestTrades.Add (thisList [0]);//add the very best trade to the best trades list
														bestTrades [bestTrades.Count - 1].postID = cPID;//change the postID of the last added so is the trade post that needs to move to and not the final destination post
												}//end if not null
										}//end for all closest
									//now has all of the very best trades
										if (bestTrades.Count == 0)//if no best trades, move to a random post and try again there
												return MoveToRandom (trader, traderPostID);
									
										bestTrades.Sort ();
										SendToPost (trader, bestTrades [0].postID, true, 0);//send the trader to the very best trade post
										break;
								}//end move type switch
						}//end else move
						return true;
				}//end TraderSet
				
				bool MoveToRandom (Trader trader, int traderPostID)
				{//move to a random post. is here as is called by best trades if none can be found
						if (CheckTraderFaction (trader, postScripts [traderPostID])) {//check that the trader is in the same faction as the trade post in the first place
								Distances[] reachable = System.Array.FindAll<Distances> (closest [traderPostID], x => (x.post != -1) && CheckTraderFaction (trader, postScripts [x.post]) && 
										postScripts [x.post].allowTrades);//get all of the reachable posts
								//make the reachable array up of posts that are possible to get to, by making sure not -1 and in the same faction
								if (reachable.Length == 0) {
										Debug.LogError (trader.name + " has no reachable posts! This may be due to incorrect factions or groups");
										return false;
								}
								SendToPost (trader, reachable [Random.Range (0, reachable.Length)].post, true, 0);//send the trader to the post
				
						} else//end check in the same faction as the trader
								Debug.LogError (trader.name + " is not in the same faction as the starting trade post, so is not able to make any trades ");
						return true;
				}//end MoveToRandom
	
				public int GetPostID (GameObject post)
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
						if (!postScripts [postID].updated)//check that has not updated the prices already
								postScripts [postID].UpdatePrices ();//update the current post prices
						for (int p = 0; p<closestPosts; p++) {
								int cPID = closest [postID] [p].post;
								if (cPID != -1 && !postScripts [cPID].updated && postScripts [cPID].allowTrades) {//check not -1 or price already updated and allowed to trade
										postScripts [cPID].UpdatePrices ();
										tradeLists [cPID].buy.Clear ();//clear the buy lists for the trade post if not already updated
								}//end if not -1 and not updated post
						}//go through the closest posts and update the prices
						#endregion
						
						tradeLists [postID].sell.Clear ();//clear current post sell list. cleared here because a trader may have already taken some of the items
	
						//clear the lists so that not duplicating any items, and will not need to check if item needs to be removed
	
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
										#endregion
				
										#region add closest
										//sort the buy list
										for (int p = 0; p<closestPosts; p++) {//go through the closest posts
												int cPID = closest [postID] [p].post;
												if (cPID != -1 && !postScripts [cPID].updated && postScripts [cPID].allowTrades) {
														//check that the post is not -1. If it is, then can't get there anyway
														//also check that hasnt been updated already and that trades can occur
														Stock currentPostStock = postScripts [cPID].stock [g].stock [i];
														if (currentPostStock.buy) {//check if item is enabled
																if (Mathf.RoundToInt (currentPostStock.number * buyMultiple) < itemAverage) //check that post wants to buy
																		tradeLists [cPID].buy.Add (new BuySell{groupID = g, itemID = i});
														}//end enabled check
												}//end check buy list update
										}//end for closest posts
										#endregion
								}//end for items
						}//end for groups
						
						#region updated
						postScripts [postID].updated = true;
						for (int p = 0; p<closestPosts; p++) {
								int cPID = closest [postID] [p].post;
								if (cPID != -1)//check not -1 or price already updated and allowed to trade
										postScripts [cPID].updated = true;
						}//go through the closest posts and update the prices
						#endregion
						
				}//end UpdateLists
	
				List<TradeInfo> BestTrade (int postID, Trader trader)
				{//go through the closest posts, finding the most profitable which is in sell at current and buy at next
						//gets the distance, and uses the different weightings to decide which is the best post
						List<BuySell> sell = tradeLists [postID].sell;//the sell list
						List<BuySell> intersect = new List<BuySell> ();//this is the list containing all of the items which are found in both lists
						List<TradeInfo> trades = new List<TradeInfo> ();

						int tradeType = trader.tradeType;//get the trade type so that can check if depot
						int homeID = trader.homeID;//get the homeID of the trader
						
						if (tradeType == 2 && postID != homeID)//if is return with no backhaul
								return null;//then return null so jumps straight to the return home						
						
						for (int p = 0; p<closestPosts; p++) {//go through closest posts
								int currentPostID = closest [postID] [p].post;
								
								if ((tradeType == 0 || postID == homeID || currentPostID == homeID) && 
								//check if not depot, check if already at home, check if cargo heading home
										currentPostID != -1 && CheckTraderFaction (trader, postScripts [currentPostID]) && postScripts [currentPostID].allowTrades) {
										//check not -1, so can actually get to the trade post, and check that they are in the same faction, check that trading is allowed
										intersect = sell.Intersect (tradeLists [currentPostID].buy).ToList ();
										for (int i = 0; i<intersect.Count; i++) {//go through all items in intersect
												BuySell cI = intersect [i];
												if (trader.items [cI.groupID].items [cI.itemID].enabled) {//check that the trader is allowed to take the cargo
														int quantity = TradeQuantity (postID, currentPostID, cI.groupID, cI.itemID, trader);
					
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
								return  trades.FindAll (x => x.postID == trades [0].postID);//only return those in the same group as the best
						}
						return null;//retun null if no trades available
				}//end BestTrade
	
				int TradeQuantity (int c, int t, int gID, int iID, Trader trader)
				{//the IDs of the current and target posts and groupID and itemID
						//work out the quantity that should be traded
						Goods good = goods [gID].goods [iID];
						float avg = (float)good.average;
						Stock cP = postScripts [c].stock [gID].stock [iID];//the stock at the current post
						int sellQuantity = cP.number - Mathf.RoundToInt (avg * sellMultiple);
						int buyQuantity = Mathf.RoundToInt (avg - (postScripts [t].stock [gID].stock [iID].number * buyMultiple));
						//will now return the minimum of the buy and sell quantities, the number that can fit in the cargo space, 
						//and the number that the trader can purchase, and the number that the trade post can purchase
						int quantity = (int)Mathf.Min (sellQuantity, buyQuantity, (float)(trader.spaceRemaining / good.mass));
						if (!expTraders.enabled)//if expendable, do more calculations
								quantity = (int)Mathf.Min (quantity, Mathf.FloorToInt (trader.cash / cP.price), Mathf.FloorToInt (postScripts [t].cash / (postScripts [t].stock [gID].stock [iID].price * purchasePercent)));
						return quantity;
				}//end TradeQuantity
				
				void SendToPost (Trader trader, int postID, bool empty, float time)
				{//send a trader to a post empty
						if (trader.empty && empty && trader.startPost == postScripts [postID]) {//if was empty and going back to the same post that it was at before
								MoveToRandom (trader, trader.postID);//move to a new random post so doesnt get stuck
								return;
						}//end if going back to where started and was empty
						
						trader.onCall = true;//set to true, so will not be told other posts
						trader.startPost = trader.finalPost;//set the starting post to the post it is at
						trader.finalPost = postScripts [postID];//set the final post
						trader.target = trader.finalPost.gameObject;//set the target gameobject
						trader.postID = postID;//set the postID
						trader.empty = empty;
						StartCoroutine (trader.PauseExit (time));//now pause
				}//end SendToPost
	
				public void OnDrawGizmos ()
				{//if show lines enabled, draw trade lines between the posts
						if (showLinks)//check that showLinks has been enabled
								DrawLines ();
				}//end OnDrawGizmos
		
				void DrawLines ()
				{//draw lines showing the possible trade links. Is here so that lines can also be drawn in the game view
						GetPostScripts ();
						
						for (int p1 = 0; p1<tradePostCount; p1++) {//go through trade posts
								for (int p2 = p1+1; p2<tradePostCount; p2++) {//go through possible connection posts
										if (CheckFactionsGroups (postScripts [p1], postScripts [p2])) {//check in the same faction / group
												if (!factions.enabled) {//if factions not enabled, then only green lines
														if (Application.isPlaying && showLinesInGame) {//if is playing, use line renderer
																LineRenderer line = new GameObject ().AddComponent<LineRenderer> ();
																line.transform.parent = transform;
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
																if (postScripts [p1].factions [f] && postScripts [p2].factions [f])//check in the same faction
																		colours.Add (factions.factions [f].colour);//add the colour to the list
														}//end for factions
														int coloursCount = colours.Count;//the number of colours
														Vector3 line = (tradePosts [p2].transform.position - tradePosts [p1].transform.position) / coloursCount;//work out the angle of the line
														//now has got all of the colours to be shown
														for (int c = 0; c<coloursCount; c++) {//go through all of the colours
																if (Application.isPlaying && showLinesInGame) {//is is playing, use line renderer
																		LineRenderer linef = new GameObject ().AddComponent<LineRenderer> ();
																		linef.transform.parent = transform;
																		linef.name = tradePosts [p1].name + " " + tradePosts [p2].name + " " + c;
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
		
				public void EditProcess (List<MnfctrGroup> manufacture, int manufactureGroup, int processNumber, bool enabled, int createTime, int cooldownTime, int price)
				{//edit the manufacturing process details for a trade post or trader

						//need to check that has received valid changes
						if (manufactureGroup > manufacture.Count || manufactureGroup < 0)//check that group is valid
								Debug.LogError ("Invalid manufacture group number");
						if (processNumber > manufacture [manufactureGroup].manufacture.Count || processNumber < 0)//check that process is valid
								Debug.LogError ("Invalid manufacture process number");
						if (createTime < 1)//check create time
								Debug.LogError ("Create time should be greater than 1");
						if (cooldownTime < 0)//check cooldown time
								Debug.LogError ("Cooldown time should be greater than 0");
				
						RunMnfctr editing = manufacture [manufactureGroup].manufacture [processNumber];//get the process to edit
						editing.enabled = enabled;//set if enabled or not
						editing.create = createTime;//set the create time
						editing.cooldown = cooldownTime;//set the cooldown time
						editing.price = price;//set the price
						manufacture [manufactureGroup].manufacture [processNumber] = editing;//apply the changes
				}//end EditProcess
				
				public void SortTraderDestination (TradePost target)
				{//if the factions, groups or trade options are changed, need to sort out traders enroute
						if (expTraders.enabled)
								GetTraderScripts ();//get the traderscripts as they will need updating now
										
						for (int t = 0; t<traderCount; t++) {//go through all traders
								Trader trader = traderScripts [t];//get the current trader
						
								if (trader.postID == target.postID) {//if the target post of the trader is the same as being edited
										if (!(CheckFactionsGroups (target, trader.startPost) && CheckTraderFaction (trader, target)))
						//if the groups or the factions of the posts dont overlap and if the trader is not in the same faction as the target, then go home
												TraderHome (trader);//send the trader home
										//else, the trader can continue going there
								}//end if same post
						}//end for all traders
				}//end SortTraderDestination
				
				public void TraderAllHome (int postID)
				{//send any traders back to the start post if it is heading for the selected post
						if (expTraders.enabled)
								GetTraderScripts ();//get the traderscripts as they will need updating now
						for (int t = 0; t<traderCount; t++) {//go through all traders
								if (traderScripts [t].postID == postID)//if target post is one which has been edited
										TraderHome (traderScripts [t]);//send back home
						}//end for all traders
				}//end TraderAllHome
				
				public void TraderHome (Trader trader)
				{//send the trader back to the start post
						trader.target = trader.startPost.gameObject;//go back to where the trader came from
						trader.finalPost = trader.startPost;//go back to starting post
						trader.postID = trader.finalPost.postID;//set the postID
				}//end TraderHome
				
				public void AddPost (TradePost newPost)
				{//called when a trade post has been added
						int postID = tradePostCount;//set this as the last entry in the array will be the new trade post

						SortAll ();//can call SortAll because the order will not have changed
	
						System.Array.Resize (ref postDistances, tradePostCount);//resize the distances array for the new post
						postDistances [postID] = new float[tradePostCount];//the new entry needs to be set to the correct size
				
						for (int p = 0; p<postID; p++)//go through all posts up to the new one
								System.Array.Resize (ref postDistances [p], tradePostCount);//resize these too
								
						SingleDist (postID, 0);//fill in the newly added post details

						closest = new Distances[tradePostCount][];//can make a new one because the data will be overwritten anyway
						GetClosest ();//get the new closest posts
				}//end AddPost
				
				public void RemovePost (int postID)
				{//sort the IDs and remove the selected post from the distance matrix and closest distances
						for (int p = 0; p<tradePostCount; p++) {//go through all posts
								if (postScripts [p].postID > postID)//if the ID is greater than the one being deleted, decrease
										postScripts [p].postID--;//decrease the postID by one
						}//end for all posts
						
						if (expTraders.enabled)
								GetTraderScripts ();//get the traderscripts as they will need updating now
			
						for (int t = 0; t<traderCount; t++) {//go through all traders
								if (traderScripts [t].postID > postID)//if the postID is greater, decrease by one
										traderScripts [t].postID--;//decrease the postID by one
								//if is the same, then should already be going home as post is shut down
						}//end for all traders
				
						List<float[]> newDist = postDistances.ToList ();//convert the distances to a list for easier removal
						newDist.RemoveAt (postID);//remove the element
						postDistances = newDist.ToArray ();//put back in an array
					
						for (int p = 0; p<tradePostCount-1; p++) {//go through remaining elements, doing the same
								List<float> newDist2 = postDistances [p].ToList ();//convert the distances to a list for easier removal
								newDist2.RemoveAt (postID);//remove the element
								postDistances [p] = newDist2.ToArray ();//put back in an array
						}//end for remaining posts
						
						tradePostCount--;//need to manually reduce the post count here
						
						List<GameObject> newPosts = tradePosts.ToList ();//put the posts into a list for easy removal
						newPosts.RemoveAt (postID);//remove the post
						tradePosts = newPosts.ToArray ();//put back into an array
			
						List<TradePost> newScripts = postScripts.ToList ();//put the scripts into a list for easy removal
						newScripts.RemoveAt (postID);//remove the script
						postScripts = newScripts.ToArray ();//put back into an array
						
						closest = new Distances[tradePostCount][];//resize the closest posts
						
						GetClosest ();//update the closest posts
				}//end RemovePost
		}//end Controller
}//end namespace