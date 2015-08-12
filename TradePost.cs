using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Stock
{
	public string name;
	public int number;
	public int price;
}

[System.Serializable]
public class Mnfctr
{
	public bool yesNo;
	public int seconds;
}

public class TradePost : MonoBehaviour
{
	public List<Stock> stock = new List<Stock> ();
	public List<Mnfctr> manufacture = new List<Mnfctr> ();
	Controller controller;
	float[] times;
	public List<bool> allowGoods = new List<bool>();
	
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
				stock [x].price = controller.goods [x].maxPrice;
			else {				
				stock [x].price = Mathf.Clamp ((int)(controller.goods [x].basePrice * 
				((float)controller.goods [x].average / stock [x].number)), 
				controller.goods [x].minPrice, controller.goods [x].maxPrice);
			}
		}
	}
	
	public void NewPost (int[] manufactureTimes)
	{//only used if it is a new trading post
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		Start ();
		for (int g = 0; g<controller.goods.Count; g++) {
			int number = (int)(Random.value * 30);
			stock.Add (new Stock{name = controller.goods [g].name, number = number});
			controller.UpdateAverage (g);
		}
		UpdatePrice ();
		if (manufactureTimes.Length != controller.manufacturing.Count)
			Debug.LogError ("Please ensure that the manufacture times sent to the new trade post is the correct length\nIt needs to have a value for each manufacturing process");
		else {
			for (int m = 0; m<manufactureTimes.Length; m++) {   
				if (manufactureTimes [m] > 0)
					manufacture.Add (new Mnfctr{yesNo = true, seconds = manufactureTimes [m]});
				else
					manufacture.Add (new Mnfctr{yesNo = false, seconds = 1});
			}
		}
		controller.posts.Add (gameObject);
		this.gameObject.tag = "Trade Post";
	}//end NewPost
	
	void Update ()
	{
		for (int t = 0; t<times.Length; t++) {
			if (manufacture [t].yesNo && Time.time >= times [t] + manufacture [t].seconds && Check (t)) {
				times [t] = Time.time;
				
				AddRemove (controller.manufacturing [t].needing, true);
				AddRemove (controller.manufacturing [t].making, false);
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
				controller.UpdateAverage (index);
			} else {
				stock [index].number += goods [x].number;
				controller.UpdateAverage (index);
			}
		}
	}
}