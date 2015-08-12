using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
/*
This script is example code for the use of TradeSys in your game. It is intended to show you how to get the different useful variables out of
the relevent scripts. There will be other ways of doing this, and better ways of displaying this information, but should hopefully be sufficient to
demonstrate the use and show you the methods and variables that store the required information.

I hope this is useful in creating your GUI!
*/

	TradeSys.Controller controller; //This is the TradeSys controller to make it easier to get the variables from later
	
	public float transMult = 0.1f, rotMult = 1f;//multipliers used to change translation and rotation speeds
	
	public float closeDistance = 1;//how close the player needs to be before counting as being at the trade post
	public double cargoSpace = 10;//the cargo space that the player has
	public int cash = 1000;//the cash that the player has
	List<TradeSys.NeedMake> cargo = new List<TradeSys.NeedMake> ();//the cargo being carried by the player
	
	double spaceRemaining;//the amount of cargo space remaining
	bool enter, shop;//if the player is close enough to enter, if is in shop mode or not
	
	TradeSys.TradePost nearPost;//this is the trade post that the player is in or is near enough to
	
	string unitLabel = " ";//this is just the unit of the maximum mass to be used with space remaining
	
	int selected = 1;//this is used with the toolbar for exit, buy and sell
	
	Vector2 scrollPos;//the position of the scrollbar showing all of the items
	
	void Start ()
	{
		controller = GameObject.FindGameObjectWithTag (TradeSys.Tags.C).GetComponent<TradeSys.Controller> ();//Get the controller. This should be the only gameobject with the controller tag.
		//The TradeSys tags are stored like this to make it easier to find and means that there are no typos which could cause issues!
		
		spaceRemaining = cargoSpace;//set the space remaining to the cargo space allowed. This is because it is easier to use the remaining space as a variable
		//and not have to recalculate it each time
		
		int unitCount = controller.units.units.Count;//get the number of units defined in the controller
		if (unitCount > 0)//if there are units defined
			unitLabel += controller.units.units [unitCount - 1].suffix;//then get the suffix of the highest one
			
		InvokeRepeating ("UpdatePrices", 0, controller.updateInterval);//call the update prices method so will automatically update the prices at the same
		//frequency as the controller may be
	}//end Start
	
	void UpdatePrices ()
	{//if at a trade post, needs to make sure that the prices will be kept up to date
		//depending on the update interval, you may wish to call the update prices method as the player enters the trade post
		if (shop)//can only be updated when in shop mode
			nearPost.UpdatePrices ();//need to call the update prices method on the trade post. this will update prices for all items
	}//end UpdatePrices
	
	void Update ()
	{
		if (!shop) {//if not showing shop
			if (Input.GetKey (KeyCode.Space)) //if pressing space
				Time.timeScale = 0;//pause
				//means that it is only possible to pause when is not showing the shop
			else
				Time.timeScale = 1;//else dont pause
				
			//this is so that it will stop the player moving, but will still allow rotation
			this.transform.Translate (Vector3.forward * transMult * Time.timeScale);//translate the player
		
			this.transform.Rotate (Input.GetAxisRaw ("Vertical") * rotMult, Input.GetAxisRaw ("Horizontal") * rotMult, 0);//rotate the player
			
			enter = CheckPos ();//allowed to enter if close enough to a trade post
		}//end if not in shop
	}//end Update
	
	bool CheckPos ()
	{//Check if there are any trade posts within close distance
		Collider[] nearbyObjects = Physics.OverlapSphere (this.transform.position, closeDistance);
	
		for (int n = 0; n<nearbyObjects.Length; n++) {//go through nearby objects and see if they have the trade post tag
			if (nearbyObjects [n].tag == TradeSys.Tags.TP) {//check has the trade post tag
				nearPost = nearbyObjects [n].GetComponent<TradeSys.TradePost> ();//set the near post to the trade post script
				return true;//return true so doesnt go through the rest of the nearby objects
			}//end if trade post tag
		}//end for all nearby objects
		return false;//return false as has not found anything
	}//end CheckPos
	
	void OnGUI ()
	{//display the GUI
		if (enter && !shop && GUI.Button (new Rect (Screen.width - 110, 10, 100, 30), "Enter Post"))//if allowed to enter and enter post button pressed
			shop = true;
			
		if (shop) {//if entered shop menu
		
			GUI.skin.box.fontSize = 35;//set the font sizes
			GUI.skin.label.fontSize = 20;
		
			GUI.Box (new Rect (5, 5, Screen.width - 10, Screen.height - 10), nearPost.name);//create a box with the name of the trade post at the top
			
			GUI.Label (new Rect (10, 50, 250, 80), "Cash: " + cash + "\nSpace remaining: " + spaceRemaining.ToString ("F2") + unitLabel);
			//a label showing the cash that the player has and the space remaining plus the maximum unit
			
			GUI.skin.label.alignment = TextAnchor.UpperRight;//set anchor to right for post cash
			
			GUI.Label (new Rect (Screen.width - 210, 50, 200, 30), "Post Cash: " + nearPost.cash);//show how much cash the trade post has because it is not able
			//to continue to make trades if it does not have sufficient cash
			
			GUI.skin.label.alignment = TextAnchor.UpperLeft;//set anchor back to left
			
			selected = GUI.Toolbar (new Rect (Screen.width - 200, 10, 190, 30), selected, new string[]{"Exit", "Buy", "Sell"});
			//show a toolbar with the options to exit, buy and sell goods
			
			if (selected == 0) {//if exit has been selected
				selected = 1;//needs to set selected to 1 so that wont exit straight away
				shop = false;//has no left the shop, so set to false
			}//end if exit
			
			
			scrollPos = GUI.BeginScrollView (new Rect (10, 100, Screen.width - 20, Screen.height - 110), scrollPos, new Rect (0, 0, 910, HeightCalc (selected)));
			//show all of the items inside a scroll view
				
			int labelPos = 0;
				
			for (int g= 0; g<controller.goods.Count; g++) {//go through all groups
				if (ShowGroup (g, selected)) {//only need to go through items if at least one is enabled
					GUI.Label (new Rect (10, labelPos * 30, 100, 30), controller.goods [g].name);//show the name of the group
					labelPos++;//increase label pos so next will be displayed a line below
					for (int i = 0; i<controller.goods[g].goods.Count; i++) {//go through all items
						if (ShowItem (g, i, selected)) {//if the item is to be shown
							
							TradeSys.Goods cG = controller.goods [g].goods [i];//get the controller good details
							TradeSys.Stock pS = nearPost.stock [g].stock [i];//get the trade post stock details
						
							GUI.Label (new Rect (50, labelPos * 30, 150, 30), cG.name);//show the name of the item
							GUI.Label (new Rect (210, labelPos * 30, 150, 30), "Mass: " + cG.unit);//show the mass of the item with the unit
							GUI.Label (new Rect (370, labelPos * 30, 150, 30), "Post has: " + pS.number);//the number that the trade post has
							
							int itemCost = selected==1?pS.price:Mathf.RoundToInt (pS.price * controller.purchasePercent);
							//if is in buy mode, the item cost is the standard price, if selling, need to multiply by purhcase percent
							
							GUI.Label (new Rect (530, labelPos * 30, 150, 30), "Price: " + itemCost);
							//show the price of the item. if it is in sell mode, then multiply the price by the trade psost purchase percent
							
							int cargoIndex = cargo.FindIndex (x => x.groupID == g && x.itemID == i);//get the index of the cargo item. if it is not being carried, 
							//this will be -1
							
							GUI.Label (new Rect (690, labelPos * 30, 150, 30), "You have: " + (cargoIndex == -1 ? 0 : cargo [cargoIndex].number));
							//above shows a label with the number the player is carrying. if cargo index is -1, then it is not carrying the item
							
							if (GUI.Button (new Rect (850, labelPos * 30, 50, 30), (selected == 1 ? "Buy" : "Sell"))) {//show a buy/sell button
								if (selected == 1) {//if is buy mode
									if (cash >= itemCost && spaceRemaining >= cG.mass && pS.number > 0) {//check if valid purchase
										pS.number--;//remove from trade post
										nearPost.cash += itemCost;//pay trade post cash
										cash -= itemCost;//withdraw cash from trader
										spaceRemaining -= cG.mass;//remove cargo space
							
										if (cargoIndex != -1)//if already carrying item
											cargo [cargoIndex].number++;//then increase the number
										else//if not carrying
											cargo.Add (new TradeSys.NeedMake (){ groupID=g,itemID=i, number=1});//create new cargo index
									}//end purchase validity check
								} else {//else sell mode
									if (nearPost.cash >= itemCost && cargoIndex != -1) {//check if valid sale
										pS.number++;//add to trade post
										nearPost.cash -= itemCost;//withdraw cash from trade post
										cash += itemCost;//receive cash
										spaceRemaining += cG.mass;//add space back
							
										cargo [cargoIndex].number--;//decrease the number in manifest
										if (cargo [cargoIndex].number == 0)//if is carrying 0
											cargo.RemoveAt (cargoIndex);//then remove index
									}//end sale validity check
								}//end buy/sell mode
							}//end if pressed buy/sell button
							
							labelPos++;//increase labelPos
						}//end if showing item
					}//end for all items
				}//end if group showing
			}//end for goods groups
				
			GUI.EndScrollView ();//end the scroll view
		}//end show shop
	}//end OnGUI()
	
	int HeightCalc (int sel)
	{//calculate the height necessary to display all of the goods and the group titles
		//an int is sent in because the displayed items may be differerent because it is possible to select that an item is buy only or sell only
		int count = 0;//set the count to 0
		for (int g = 0; g<controller.goods.Count; g++) {//for all groups
			bool addgroup = false; //used to keep track if the group has already been counted for
			for (int i = 0; i<controller.goods[g].goods.Count; i++) {//go through all of the goods in the group
				if (ShowItem (g, i, sel)) {//check if the item is allowed to be shown
					if (!addgroup) {//if the group hasnt already been added
						addgroup = true;//set to true
						count++;//and add another so the title can be displayed
					}//end if group title shown
					count++;//increase the count
				}//end if allowed to show the item
			}//end for all goods
		}//end for all groups
		
		return count * 30;//multiply the count by 30 so will be the correct height
	}//end HeightCalc
	
	bool ShowGroup (int g, int sel)
	{//returns a bool depending on if the group needs to be shown or not
		for (int i = 0; i<controller.goods[g].goods.Count; i++) {//go through all goods in the group to be checked
			if (ShowItem (g, i, sel))//check that the item is enabled
				return true;//if an item is, then dont need to check any further
		}
		return false;//else return false
	}//end ShowGroup
	
	bool ShowItem (int g, int i, int sel)
	{//return a bool depending on if the item has buy or sell selected
		TradeSys.Stock stock = nearPost.stock [g].stock [i];
		if (sel == 1 && stock.buy || sel == 2 && stock.sell)//if buy check and buy enabled, or sell check and sell enabled
			return true;//return true
		return false;//else return false
	}//end ShowItem
	
	
}//end class