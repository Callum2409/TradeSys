using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TradeSys{//uses TradeSys namespace to prevent any conflicts

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
	public int create;
	public int cooldown;
	internal bool creating;
}

public class TradePost : MonoBehaviour
{
	public List<Stock> stock = new List<Stock> ();
	public List<Mnfctr> manufacture = new List<Mnfctr> ();
	Controller controller;
	public List<bool> groups = new List<bool> ();
	public List<bool> factions = new List<bool> ();
	string text1 = "Please ensure that the ";
	string text2 = " sent to the new trade post is the correct length.\nIt needs to have a value for each ";
	string text3 = " is greater than the number of ";
	string text4 = " available or less than 0.\nMake sure that the ";
	
	void Start ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		UpdatePrice ();
	}
	
	public void UpdatePrice ()
	{				
		for (int x = 0; x<stock.Count; x++) {
			if (stock [x].number == 0)
				stock [x].price = controller.goodsArray [x].maxPrice;
			else {				
				stock [x].price = Mathf.Clamp ((int)(controller.goodsArray [x].basePrice * 
				((float)controller.goodsArray [x].average / stock [x].number)), 
				controller.goodsArray [x].minPrice, controller.goodsArray [x].maxPrice);
			}
		}
	}
	
	public void NewPost (int[] itemNumbers, int[] manufactureCreate, int[] manufactureCooldown, bool[] groupsAllow, bool[] factionsAllow)
	{//only used if it is a new trading post
		Start ();
		if (itemNumbers.Length != controller.goodsArray.Length)
			Debug.LogError (text1 + "item numbers" + text2 + "item. Set to -1 to disable an item.");
		else {
			for (int g = 0; g<controller.goodsArray.Length; g++) {
				if (itemNumbers [g] >= 0) {
					stock.Add (new Stock{name = controller.goodsArray [g].name, number = itemNumbers [g], allow = true});
					controller.UpdateAverage (g, itemNumbers [g], 1);
				} else
					stock.Add (new Stock{name = controller.goodsArray [g].name, number = 0, allow = false});
			}
		}
		if (manufactureCreate.Length != controller.manufacturing.Count)
			Debug.LogError (text1 + "manufacture times" + text2 + "manufacturing process. Set to less than 1 to disable a process.");
		else if (manufactureCooldown.Length != controller.manufacturing.Count)
			Debug.LogError (text1 + "manufacture cooldown" + text2 + "manufacturing process.");
		else {
			for (int m = 0; m<manufactureCreate.Length; m++) {   
				if (manufactureCreate [m] > 0)
					manufacture.Add (new Mnfctr{allow = true, create = manufactureCreate [m], cooldown = manufactureCooldown [m]});
				else
					manufacture.Add (new Mnfctr{allow = false, create = 1, cooldown = manufactureCooldown [m]});
			}
		}
		if (groupsAllow.Length != controller.groups.Count)
			Debug.LogError (text1 + "groups bool array" + text2 + "group.");
		else {
			for (int g = 0; g<groupsAllow.Length; g++)
				groups.Add (groupsAllow [g]);
		}
		if (factionsAllow.Length != controller.factions.Count)
			Debug.LogError (text1 + "factions bool array" + text2 + "faction.");
		else {
			for (int f = 0; f<factionsAllow.Length; f++)
				factions.Add (factionsAllow [f]);
		}
		controller.posts.Add (gameObject);
		controller.postScripts.Add (this);
		this.gameObject.tag = "Trade Post";
	}//end NewPost
	
	void Update ()
	{
		for (int m = 0; m<manufacture.Count; m++) {
			if (manufacture[m].allow && !manufacture [m].creating && Check (m)) {
				StartCoroutine (Create (m));
			}
		}
	}
	
	IEnumerator Create (int manID)
	{
		manufacture [manID].creating = true;
		AddRemove (controller.manufacturing [manID].needing, true);
		yield return new WaitForSeconds(manufacture [manID].create);
		AddRemove (controller.manufacturing [manID].making, false);
		yield return new WaitForSeconds(manufacture [manID].cooldown);
		manufacture [manID].creating = false;
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
			controller.posts.Remove (this.gameObject);
		}
		
		for (int s = 0; s<stock.Count; s++) {
			if (stock [s].allow)
				controller.UpdateAverage (s, mult * stock [s].number, mult);
		}
	}
	
	public void ItemEnableDisable (bool enable, int productID)
	{
		if (productID > stock.Count - 1 || productID < 0) 
			Debug.LogError ("The productID"+text3+"items"+text4+"productID is correct.");
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
	
	public void ManufactureEnableDisable (bool enable, int manufactureID, int createTime, int coolTime)
	{
		if (manufactureID > controller.manufacturing.Count - 1 || manufactureID < 0)
			Debug.LogError ("The manufactureID" + text3 + "manufactureing processes" + text4 + "manufactureID is correct.");
		else {		
			manufacture [manufactureID].allow = enable;
			manufacture [manufactureID].create = createTime;
			manufacture [manufactureID].cooldown = coolTime;
		}
	}
}
}//end namespace