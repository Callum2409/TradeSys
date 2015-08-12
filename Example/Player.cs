using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	/*This script is intended to aid the creation of a GUI using TradeSys, so the GUI itself is fairly basuc. 
	 * It is an example to show what is possible, so the script should be edited and improved upon.
	 * 
	 * This also includes the code to display trader info when a trader has been clicked on. In order to be able to use it, the
	 * HitMe script needs to be added to the traders. 
	 * 
	 * Hope this is useful in creating your GUI!
	*/
	Controller controller;//this is the controller script, easier to set it as this than using GetComponent each time
	bool enter, shop, edit;//there is a bool for if it is possible to enter a trade post and if the player is currently 
	//in the shop. Edit is so that the groups of teh trade post can be changed
	GameObject nearPost;//needs to know the post that the player is at so can get the correct prices etc
	int[] cargo;//an array containing the numbers of each item. Easier to do this than a list containing those 
	//carried as will be 0 if none, when would have to go through the list each time to see if is being carried
	public int cash;//cash is required to buy and sell items to make a profit
	public float cargoSpace;//this is the maximum cargo space
	double spaceRemaining;//this is how much space is remaining. setting this means that it does not need to be 
	//worked out each time there is cargo added or removed, as this value is changed at the same time
	//using a double here because a float doesn't quite give enough accuracy needed, so may sometimes give a small error
	//but could cause problems if some of the cargo masses are that small
	int eBS = 1;//this is to show whether to show the Buy or Sell screens, or Exit
	Vector2 scrollPos = Vector2.zero;
	public float saleP = 0.7f;//this is the % of the normal price that the trade post will buy items at. Should be > 0 and < 1
	internal GameObject focus;//this is set when an object using the HitMe script, or the code within it
	public float closeDistance = 2f;
	Collider[] nearby;
	string pickup = "";//use this so that if an item is collected, it will be displayed
	float displayTimer;//used so that after a certain amount of time, the message will go away
	
	void Start ()
	{//set up the variables that will be used throughout
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		cargo = new int[controller.goodsArray.Length];
		//the cargo is stored as an int[] as this makes it easier to get the currently carried cargo
		spaceRemaining = cargoSpace;//the player at the start is not carrying anything, so the remaining will be 
		//the max space, this is done here because it means that it does not need to be set twice in the inspector	
	}
	
	#region example code
	void Update ()
	{		
		//pressing space will pause the movement of the player
		if (!shop && !edit) { //if not in the shop
			
			//this is to move and rotate the player
			this.transform.Rotate (new Vector3 (Input.GetAxisRaw ("Vertical") * 2, 
					Input.GetAxisRaw ("Horizontal") * 2, 0));
			this.transform.Translate (Vector3.forward * 0.15f * Time.timeScale);
			
			if (Input.GetKey (KeyCode.Space)) //if pressing space, pause. Will be unpaused when in the shop
				Time.timeScale = 0;
			else
				Time.timeScale = 1;
		} else 
			Time.timeScale = 1;
		
		
		if (Input.GetKeyUp (KeyCode.Escape))
			focus = null;//if the player presses escape, then deslect and hide trader info

		enter = CheckPos ();//will now check if the player is close to any trade post
		PickupCheck ();
		
		if (Time.timeSinceLevelLoad - displayTimer > 3)
			pickup = "";
	}
	
	bool CheckPos ()
	{//needs to go through each trade post, and check the distance, if close, will return true indicating it is near, and will
		//set nearPost
		for (int p = 0; p<controller.posts.Count; p++) {
			if (Vector3.Distance (this.transform.position, controller.posts [p].transform.position) < closeDistance) {//check the distance
				nearPost = controller.posts [p];//get the correct trade post
				return true;
			}
		}//will return false if it has gone through all of the posts and is not close to any
		return false;
	}
	
	void PickupCheck ()
	{
		nearby = Physics.OverlapSphere (this.transform.position, closeDistance);//check all items that are within close range
		for (int n = 0; n<nearby.Length; n++) {
			if (nearby [n].tag == "Item") {//need to check that each one is an item
				int itemNo = controller.spawned.FindIndex (x => x.item == nearby [n].gameObject);//get the location of the spawned item in the spawned list
				int cargoNo = controller.spawned [itemNo].goodID;//get the cargoID
				if (spaceRemaining >= controller.goods [cargoNo].mass) {//check there is enough space to collect the item
					cargo [cargoNo]++;//add to cargo
					spaceRemaining -= controller.goods [cargoNo].mass;//decrease space remaining
					controller.spawned.RemoveAt (itemNo);//remove the item from the spawned list
					Destroy (nearby [n].gameObject);//destroy the object
					
					pickup = "You collected " + controller.goods [cargoNo].name;//display that something was collected
					
					for (int t = 0; t < controller.traderScripts.Count; t++)//need to go through all the traders and check nearest so that none are heading
						//for the item that has been picked up;
						controller.traderScripts [t].CheckNearest ();
				}else
				pickup = "Could not collect item. Not enough space in cargo hold.";//not enough space, so display
				displayTimer = Time.timeSinceLevelLoad;//start the timer. this is here so that it does not need to be duplicated
			}
		}
	}
	#endregion
	void OnGUI ()
	{//changing the font size to make it easier to view
		GUI.skin.box.fontSize = 42;
		GUI.skin.label.fontSize = 21;
		GUI.skin.button.richText = true;
		#region example code
		if (enter && !shop && !edit) {//if it is close to a trade post
			if (GUI.Button (new Rect (Screen.width - 160, 10, 150, 50), "Enter trade post") || Input.GetKeyUp (KeyCode.T)) {
				//if pressed trade and not currently in the shop
				//if the player has pressed the button to enter the shop, then set the shop view bool to true, so will display
				shop = true;
			}
			if (controller.allowGroups && GUI.Button (new Rect (Screen.width - 320, 10, 150, 50), "Edit groups") || Input.GetKeyUp (KeyCode.Y)) {
				//if pressed edit and not currently in edit mode
				//if the player has pressed the button to enter edit mode, then set the edit bool to true, so will display
				edit = true;
			}
		}
		#endregion
		#region show shop
		if (shop) {
			//can use most of the same code to display the shop for both buy and sell modes, but will need some different settings
			string title = "";
			if (eBS == 1)//need to set the correct title
				title = "Buy";
			else
				title = "Sell";
			GUI.Box (new Rect (0, 0, Screen.width, Screen.height), title);//fill the screen with a box
			eBS = GUI.Toolbar (new Rect (Screen.width - 180, 10, 170, 30), eBS, new string[]{"Exit","Buy", "Sell"});
			//line above will show a set of buttons
			
			GUI.Label (new Rect (Screen.width - 680, 50, 200, 30), "Cash: " + cash);//display cash
			GUI.Label (new Rect (Screen.width - 470, 50, 200, 30), "Cargo space: " + cargoSpace);//display cargo space
			GUI.Label (new Rect (Screen.width - 260, 50, 250, 30), "Space remaining: " + System.Math.Ceiling (spaceRemaining));
			//using System.Math.Ceiling (spaceRemaining) because then doesnt need to show many decimals, and that small 
			//items e.g. mass 1g do not reduce the space remaining by 1t
			List<Stock> stock = nearPost.GetComponent<TradePost> ().stock;//get the stock of the trade post so dont have to keep
			//calling this method
			int showCount = stock.FindAll (x => x.allow == true).Count;//get the number of lines required, so can create a scroll
			//pane large enough
			scrollPos = GUI.BeginScrollView (new Rect (10, 100, Screen.width - 20, Screen.height - 110), scrollPos, new Rect (0, 0, 1190, showCount * 30));
			//creating a scrollview for all of the items. the y size is set using the number of items that have been enabled
			//and need to be shown
			int s = 0;//this is so that each item will be on a different line
			for (int g = 0; g<controller.goodsArray.Length; g++) {//need to go through all of the items to show and display the info
				if (stock [g].allow) {
					GUI.Label (new Rect (0, s * 30, 200, 30), stock [g].name);//show the name
					int price = stock [g].price;//get the price
					if (eBS == 2)
						price = (int)(price * saleP);//if on the sell screen, the price is multiplied by the sale %
					GUI.Label (new Rect (210, s * 30, 200, 30), "Price: " + price);//show the price to buy / sell items at
					GUI.Label (new Rect (420, s * 30, 250, 30), "Number available: " + stock [g].number);//show the number the post has
					GUI.Label (new Rect (680, s * 30, 250, 30), "You have: " + cargo [g]);//display the number the player has.
					//this is where it is easier to have all the items in an array because then they will automatically be 0 rather than have to check a (probably unsorted) list
					string mass = controller.goodsArray [g].mass.ToString ();//the mass is added to a string because if there are no units, then display normally
					if (controller.units.Count > 0 && controller.units.Count - 1 >= controller.goodsArray [g].unit)//need to check that units can be used, and the units set in the controller still exist
						mass = (controller.goodsArray [g].mass / controller.units [controller.goodsArray [g].unit].min).ToString () + controller.units [controller.goodsArray [g].unit].suffix;
					//line above converts the mass which was as a decimal into an integer which can then be displayed with the 
					//units. e.g. 1g in the controller may have been 0.000001, but we dont want to display it as this, so the 
					//mass of the item is divided by the minimum required to have a certain unit, so here, would display as 1 instead
					GUI.Label (new Rect (940, s * 30, 180, 30), "Mass: " + mass);//show the mass in the GUI
				
					if (eBS == 0 || (shop && Input.GetKeyDown (KeyCode.E))) {
						shop = false;
						eBS = 1;
					}//if the exit button has been pressed, stop showing the shop, and needs to be set to show the buy screen 
					//next time otherwise will just quit again
				
					if (eBS == 1 && GUI.Button (new Rect (1130, s * 30, 50, 30), "Buy")) {//if buying
						if (cash >= stock [g].price && spaceRemaining >= controller.goodsArray [g].mass && stock [g].number > 0) {
							//line above checks if there is enough cash, cargo space and number available to make a sale
							spaceRemaining -= controller.goodsArray [g].mass;//reduce the space remaining
							cash -= stock [g].price;//pay for the item
							cargo [g]++;//add the item to the cargo
							stock [g].number--;//reduce the number available at the trade post
							nearPost.GetComponent<TradePost> ().UpdatePrice ();//and update the price of the items at the trade post as is now carrying fewer, so the price may change
						}
					}
					if (eBS == 2 && GUI.Button (new Rect (1130, s * 30, 50, 30), "Sell")) {//if selling
						if (cargo [g] > 0) {//check that it is possible to sell an item to the trade post
							spaceRemaining += controller.goodsArray [g].mass;//get the cargo space back
							cash += (int)(stock [g].price * saleP);//get paid for the item, but is a proportion of the normal price
							cargo [g]--;//reduce the number of the cargo
							stock [g].number++;//increase the stock count at the trade post
							nearPost.GetComponent<TradePost> ().UpdatePrice ();//now has more items, so the price may change
						}
					}
					s++;//increase the value of s, so can display the info on different lines
				}//if has been enabled
			}//for
			GUI.EndScrollView ();//needs to end the scrollview as there are no more items that need to be displayed in the scroll panel
		} else {//if the shop is not showing, then it is ok to display selected item information
			#endregion
			#region edit
			if (edit) {
				//if edit
				GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "Edit groups");//fill the screen with a box
				if (GUI.Button (new Rect (Screen.width - 55, 10, 45, 30), "Exit") || Input.GetKeyDown (KeyCode.E))
					edit = false;//add exit button
				if (GUI.Button (new Rect (10, 50, 80, 30), "Add group")) {
					controller.groups.Add ("Group "+controller.groups.Count);
					//add a group to the controller, and needs to add groups to all of the trade posts
					for (int p = 0; p<controller.postScripts.Count; p++)
						controller.postScripts [p].groups.Add (false);
				}
				scrollPos = GUI.BeginScrollView (new Rect (10, 100, Screen.width - 20, Screen.height - 110), scrollPos, new Rect (0, 0, 300, controller.groups.Count * 40));
				List<bool> groupEnabled = nearPost.GetComponent<TradePost> ().groups;
				for (int g = 0; g<controller.groups.Count; g++) {
					controller.groups [g] = GUI.TextField (new Rect (0, g * 40, 250, 30), controller.groups [g]);
					groupEnabled[g] = GUI.Toggle(new Rect(260, g*40+5, 50, 30), groupEnabled[g], "");
				}
				GUI.EndScrollView ();
			}//end if in edit mode
			#endregion
			#region show trader info
			if (focus != null && focus.transform.parent.name == "Traders") {
				GUI.skin.box.fontSize = 23;
				GUI.Box (new Rect (0, 0, 350, Screen.height), focus.name);//creates a background box which will have the info displayed on
				Trader trader = focus.GetComponent<Trader> (); //This is getting the trader script of the clicked trader. The focus is just a GameObject
				List<NoType> rows = trader.trading;//get all of the cargo items to display
				int rowCount = rows.Count;//get the number of rows
				GUI.Label (new Rect (10, 30, 330, 30), "Target: " + trader.target);//show target, post or item
				GUI.Label (new Rect (10, 60, 330, 30), "Final Post: " + trader.finalPost);//show final post
				GUI.Label (new Rect (10, 90, 330, 30), "Cargo space: " + trader.cargoSpace);//show cargo space
				GUI.Label (new Rect (10, 120, 330, 30), "Space remaining: " + trader.spaceRemaining.ToString ("f1"));//show the space remaining to 1dp
				if (rowCount > 0) {//if the trader is carrying something, display
					GUI.Label (new Rect (10, 150, 280, 30), "Trading");
					scrollPos = GUI.BeginScrollView (new Rect (10, 180, 330, Screen.height - 70), scrollPos, new Rect (0, 0, 310, rowCount * 30));
					//line above uses a scroll view just in case the trader is carrying many different items
					for (int r = 0; r<rowCount; r++) {//go through all of the carrying items and display
						GUI.Label (new Rect (0, r * 30, 30, 30), controller.goodsArray [rows [r].goodID].name);//show the item name
						GUI.Label (new Rect (50, r * 30, 120, 30), "Number: " + rows [r].number);//show the number
						
						string unit = "";//set the string to display the unit to be blank
						string mass = controller.goodsArray [trader.trading [r].goodID].mass.ToString ();//get the standard mass
						for (int u = 0; u<controller.units.Count; u++) {//need to cycle through the units and find the correct one
							if (controller.goodsArray [trader.trading [r].goodID].mass >= controller.units [u].min && 
								controller.goodsArray [trader.trading [r].goodID].mass < controller.units [u].max) {
								unit = controller.units [u].suffix;//if the mass fits between the specified unit values, then get the suffix
								mass = Mathf.RoundToInt ((float)controller.goodsArray [trader.trading [r].goodID].mass / controller.units [u].min).ToString ();
								//show the mass, but round this up so that lots of decimals dont need to be shown
							}
						}
						
						GUI.Label (new Rect (170, r * 30, 170, 30), "Mass: " + mass + unit);//show the mass of the items
					}
					GUI.EndScrollView ();
				} else {//if the trader is not carrying anything, then is moving to a different post
					GUI.Label (new Rect (10, 150, 330, 30), "Moving to a different post");
				}
			}
			#endregion
			GUI.Label (new Rect (10, Screen.height - 50, 500, 30), pickup);//display that item was collected
		}
	}
}