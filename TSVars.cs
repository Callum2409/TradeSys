using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys
{//use namespace to stop any name conflicts
		public class Tags : MonoBehaviour
		{//contains all of the tags used in TradeSys
				public const string 
						C = "TS Controller",
						TP = "TS Trade Post",
						T = "TS Trader",
						S = "TS Spawner",
						I = "TS Item";
		}
	
	#if UNITY_EDITOR
	[System.Serializable]
	public class Selected
	{//contains all of the toolbar selections
		public int
			C, //controller
			P, //trade post
			T,//trader
			S,//spawner
			GG, //goods group
			MG;//manufacture group
	}
	
		[System.Serializable]
		public class ScrollPos
		{//contains all of the scroll positions
				public Vector2
						S, //controller settings
						G, //controller goods
						M, //controller manufacturing
						GG, //goods group (horiz)
						MG,//manufacture group (horiz)
						PS, //post settings
						PG, //post goods
						PM, //post manufacturing
						TS, //trader settings
						TG, //trader goods
						TM, //trader manufacturing
						SS, //spawner settings
						SG; //spawner goods						
		}
	#endif

		[System.Serializable]
		public class Goods : IComparable <Goods>
		{//contains all of the information about each good
				public string name, unit;
				public bool expanded;
				public int basePrice, minPrice, maxPrice, postCount;
				public float mass;
				public double average;
				public GameObject itemCrate;
				public float pausePerUnit;
	
				public int CompareTo (Goods other)
				{
						return name.CompareTo (other.name);
				}
		}

		[System.Serializable]
		public class GoodsTypes
		{//the groups of goods
				public string name;
				public List<Goods> goods = new List<Goods> ();
				public bool expandedP, expandedT, expandedS;
		}

		[System.Serializable]
		public class Mnfctr
		{//the manufacturing processes
				public string name, tooltip;
				public bool expanded;
				public List<NeedMake> needing = new List<NeedMake> ();
				public List<NeedMake> making = new List<NeedMake> ();
				public int postCount;
				public double needingMass, makingMass;
		}

		[System.Serializable]
		public class NeedMake
		{//the list of the needing or making items for the manufacturing process
				public int groupID, itemID, number;
		}

		[System.Serializable]
		public class MnfctrTypes
		{//the groups of manufacture
				public string name;
				public bool expandedP, expandedT;
				public List<Mnfctr> manufacture = new List<Mnfctr> ();
		}

		[System.Serializable]
		public class Stock : IComparable<Stock>
		{//the stock found at each trade post
				public string name;
				public bool buy, sell, hidden, minMax;
				public int number;
				public int price = 0;
				public int min, max;
	
				public int CompareTo (Stock other)
				{
						return  name.CompareTo (other.name);
				}
		}

		[System.Serializable]
		public class StockGroup
		{//this is so that the stock is shown in the inspector as it is useful
				public List<Stock> stock = new List<Stock> ();
		}

		[System.Serializable]
		public class RunMnfctr
		{//the manufacturing process information for each trade post and trader
				public bool enabled;
				public int create, cooldown;
				internal bool running;
		}

		[System.Serializable]
		public class MnfctrGroup
		{//this is so that the manufacture is shown in the inspector as it is useful
				public bool enabled = true;
				public List<RunMnfctr> manufacture = new List<RunMnfctr> ();
		}

		[System.Serializable]
		public class Distances : IComparable<Distances>
		{//used so that it is possible to sort the distances ascending, while still knowing which trade posts it is between
				public int post;
				public float distance;
	
				public int CompareTo (Distances other)
				{
						return distance.CompareTo (other.distance);
				}
		}

		[System.Serializable]
		public class Trade
		{//the buy and sell lists for the trade post
				public List<BuySell> buy = new List<BuySell> ();
				public List<BuySell> sell = new List<BuySell> ();
		}

		[System.Serializable]
		public class BuySell : IEquatable<BuySell>
		{//contains the itemID and groupID of the item
				public int itemID, groupID;
	
				public bool Equals (BuySell other)
				{
						//check not null
						if (object.ReferenceEquals (other, null))
								return false;
	
						//check same data
						if (object.ReferenceEquals (this, other))
								return true;
	
						//check equal properties
						return itemID.Equals (other.itemID) &&
								groupID.Equals (other.groupID);
				}
	
				public override int GetHashCode ()
				{
						//get hash for itemID if not null
						int hashItemID = itemID == -1 ? 0 : itemID.GetHashCode ();
	
						//get hash for groupID id not null
						int hashGroupID = groupID == -1 ? 0 : groupID.GetHashCode ();
						
						//calculate hash code
						//done this way to ensure that each is distinct. This allows for up to 100 items per stock group
						return (10 ^ (hashGroupID * 2)) * (hashItemID + 1);
				}
		}

		[System.Serializable]
		public class TradeInfo : IComparable<TradeInfo>
		{//contains the best trades for each trade post
				public float val;
				public int postID, groupID, itemID, quantity;
	
				public int CompareTo (TradeInfo other)
				{
						return val.CompareTo (other.val);
				}
		}

		[System.Serializable]
		public class ItemCargo : IComparable<ItemCargo>
		{//the allow items for traders
				public string name;
				public bool enabled;
				public int number;
	
				public int CompareTo (ItemCargo other)
				{
						return name.CompareTo (other.name);
				}
		}

		[System.Serializable]
		public class ItemGroup
		{//have the allow items in groups
				public List<ItemCargo> items = new List<ItemCargo> ();
		}

		[System.Serializable]
		public class EnableList
		{//this is used by post tags and groups, has a bool for allowed and expanded, and a string list
				public bool enabled, expandedC, expandedP;
				public List<string> names = new List<string> ();
		}

		[System.Serializable]
		public class Factions
		{//this is used for factions, has enabled and expanded bools, and a list with strings and colours
				public bool enabled, expandedC, expandedP, expandedT;
				public List<Faction> factions = new List<Faction> ();
		}

		[System.Serializable]
		public class Faction
		{//contains a string name and a colour for the faction
				public string name;
				public Color colour = Color.green;
		}
		
		[System.Serializable]
		public class ExpendableList
		{//used for the expendable traders
				public bool enabled, expandedC;
				public int maxNoTraders = 100;
				public List<Trader> traders = new List<Trader> ();
		}

		[System.Serializable]
		public class Units
		{//contains the list of units and a bool for expanded
				public bool expanded;
				public List<Unit> units = new List<Unit> ();
		}

		[System.Serializable]
		public class Unit
		{//contains the suffix and min max values for the unit to be used
				public string suffix;
				public float min, max;
		}
}//end namespace