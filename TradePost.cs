using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class Stock
{
	public string name;
	public int number;
	public int price;
	public bool allow = true;
}

[System.Serializable]
public class Mnfctr
{
	public bool allow;
	public int seconds;
}

[System.Serializable]
public class Group
{
	public bool allow;
	public int selection;
	
	public bool Equals (Group other)
	{
		if (System.Object.ReferenceEquals (other, null))
			return false;
		if (System.Object.ReferenceEquals (this, other))
			return true;
		return 	allow.Equals (other.allow) &&
				selection.Equals (other.selection);
	}
	
	public override int GetHashCode ()
	{
		int hashAllow = allow.GetHashCode ();
		int hashSelection = selection == -1 ? 0 : selection.GetHashCode ();
		
		return hashAllow ^ hashSelection;
	}
}

public class TradePost : MonoBehaviour
{
	public List<Stock> stock = new List<Stock> ();
	public List<Mnfctr> manufacture = new List<Mnfctr> ();
	Controller controller;
	float[] times;
	public List<Group> groups = new List<Group>();
	
	void Start ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		UpdatePrice ();
		times = new float[controller.manufacturing.Count];
		for (int t = 0; t<times.Length; t++)
			times [t] = Time.time;
	}
	
	public void UpdatePrice ()
	{				
		for (int x = 0; x<stock.Count; x++) {
			if (stock [x].number == 0)
				stock [x].price = controller.goodsArray  [x].maxPrice;
			else {				
				stock [x].price = Mathf.Clamp ((int)(controller.goodsArray  [x].basePrice * 
				((float)controller.goodsArray  [x].average / stock [x].number)), 
				controller.goodsArray  [x].minPrice, controller.goodsArray  [x].maxPrice);
			}
		}
	}
	
	public void NewPost (int[] itemNumbers, int[] manufactureTimes)
	{//only used if it is a new trading post
		Start ();
		if (itemNumbers.Length != controller.goodsArray.Length)
			Debug.LogError ("Please ensure that the item numbers sent to the new trade post is the correct length\nIt needs to have a value for each item. set as -1 to disable an item.");
		for (int g = 0; g<controller.goodsArray.Length; g++) {
			if (itemNumbers [g] >= 0) {
				stock.Add (new Stock{name = controller.goodsArray [g].name, number = itemNumbers [g], allow = true});
				controller.UpdateAverage (g, itemNumbers [g], 1);
			} else
				stock.Add (new Stock{name = controller.goodsArray [g].name, number = 0, allow = false});
		}
		if (manufactureTimes.Length != controller.manufacturing.Count)
			Debug.LogError ("Please ensure that the manufacture times sent to the new trade post is the correct length\nIt needs to have a value for each manufacturing process");
		else {
			for (int m = 0; m<manufactureTimes.Length; m++) {   
				if (manufactureTimes [m] > 0)
					manufacture.Add (new Mnfctr{allow = true, seconds = manufactureTimes [m]});
				else
					manufacture.Add (new Mnfctr{allow = false, seconds = 1});
			}
		}
		controller.posts.Add (gameObject);
		controller.postScripts.Add (this);
		this.gameObject.tag = "Trade Post";
	}//end NewPost
	
	void Update ()
	{
		for (int t = 0; t<times.Length; t++) {
			if (manufacture [t].allow && Time.time >= times [t] + manufacture [t].seconds && Check (t)) {
				times [t] = Time.time;
				
				AddRemove (controller.manufacturing [t].needing, true);
				AddRemove (controller.manufacturing [t].making, false);
				UpdatePrice();
			}
		}	
	}
	
	bool Check (int number)
	{
		for (int x = 0; x<controller.manufacturing[number].needing.Count; x++) {
			NeedMake good = controller.manufacturing [number].needing [x];
			if (good.item == -1) {
				Debug.LogError ("Some items in manufacturing " + controller.manufacturing [number].name + " are undefined" +
						"\nPlease ensure that all types have been selected");
				return false;
			} 
			if (stock [good.item].number < good.number)
				return false;
		}
		return true;
	}
	
	void AddRemove (List<NeedMake> goods, bool need)
	{
		for (int x = 0; x<goods.Count; x++) {
			int index = goods [x].item;
			if (need) {
				stock [index].number -= goods [x].number;
				controller.UpdateAverage (index, -goods [x].number, 0);
			} else {
				stock [index].number += goods [x].number;
				controller.UpdateAverage (index, goods [x].number, 0);
			}
		}
	}
	
	public void PostEnableDisable (bool enable)
	{//this is used to disable a post, or enable a post that has previously been enabled.
		//If the post has not been enabled, then the NewPost method should be called instead
		int mult = 1;
		if (enable)
			controller.posts.Add (this.gameObject);
		else {
			mult = -1;
			controller.posts.Remove(this.gameObject);
		}
		
		for (int s = 0; s<stock.Count; s++) {
			if (stock [s].allow)
				controller.UpdateAverage (s, mult * stock [s].number, mult);
		}
	}
	
	public void ItemEnableDisable (bool enable, int productID)
	{
		if (productID > stock.Count - 1) 
			Debug.LogError ("The productID is greater than the number of items available.\nMake sure that the productID is correct.");
		else {
			if (stock [productID].allow != enable) {
				if (enable) 
					controller.UpdateAverage (productID, stock [productID].number, +1);
				else 
					controller.UpdateAverage (productID, -stock [productID].number, -1);
				stock [productID].allow = enable;
			}
		}
	}
	
	public void ManufactureEnableDisable (bool enable, int manufactureID, int time)
	{
		if (manufactureID > controller.manufacturing.Count - 1)
			Debug.LogError ("The manufactureID is greater than the number of manufactureing processes set up.\nMake sure that the manufactureID is correct.");
		else {		
			manufacture [manufactureID].allow = enable;
			manufacture [manufactureID].seconds = time;
		}
	}
}