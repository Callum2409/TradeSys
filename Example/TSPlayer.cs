using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TSPlayer : MonoBehaviour
{
    /*
    This script is example code for the use of TradeSys in your game. It is intended to show you how to get the different useful variables out of
    the relevent scripts. There will be other ways of doing this, and better ways of displaying this information, but should hopefully be sufficient to
    demonstrate the use and show you the methods and variables that store the required information.

    I hope this is useful in creating your GUI!
    */

    CallumP.TradeSys.Controller controller; //This is the TradeSys controller to make it easier to get the variables from later

    public float transMult = 0.1f, rotMult = 1f;//multipliers used to change translation and rotation speeds

    public float closeDistance = 1;//how close the player needs to be before counting as being at the trade post or to collect an item
    public double cargoSpace = 10;//the cargo space that the player has
    public float cash = 1000;//the cash that the player has
    int[][] cargo;//the cargo being carried by the player

    double spaceRemaining;//the amount of cargo space remaining
    bool enter, inside;//if the player is close enough to enter, if is in inside mode or not

    CallumP.TradeSys.TradePost nearPost;//this is the trade post that the player is in or is near enough to
    CallumP.TradeSys.Item nearItem;//the item that the player is near enough to to pick up the item

    string unitLabel = " ";//this is just the unit of the maximum mass to be used with space remaining

    int selected = 0;//this is used with the toolbar for exit, buy and sell

    Vector2 scrollPos;//the position of the scrollbar showing all of the items

    string pickup;//the text saying that you have collected an item
    float textshow;//the time when the pickup text was first shown. this is so that after a time, it will disappear

    void Start()
    {
        controller = GameObject.FindGameObjectWithTag(CallumP.TradeSys.Tags.C).GetComponent<CallumP.TradeSys.Controller>();//Get the controller. This should be the only gameobject with the controller tag.
                                                                                                                           //The TradeSys tags are stored like this to make it easier to find and means that there are no typos which could cause issues!

        spaceRemaining = cargoSpace;//set the space remaining to the cargo space allowed. This is because it is easier to use the remaining space as a variable
                                    //and not have to recalculate it each time

        cargo = new int[controller.goods.Count][];
        for (int g = 0; g < controller.goods.Count; g++)
            cargo[g] = new int[controller.goods[g].goods.Count];

        int unitCount = controller.units.units.Count;//get the number of units defined in the controller
        if (unitCount > 0)//if there are units defined
            unitLabel += controller.units.units[unitCount - 1].suffix;//then get the suffix of the highest one

        InvokeRepeating("UpdatePrices", 0, controller.updateInterval);//call the update prices method so will automatically update the prices at the same
                                                                      //frequency as the controller may be
    }//end Start

    void UpdatePrices()
    {//if at a trade post, needs to make sure that the prices will be kept up to date
     //depending on the update interval, you may wish to call the update prices method as the player enters the trade post
        if (inside)//can only be updated when in inside mode
            nearPost.UpdatePrices();//need to call the update prices method on the trade post. this will update prices for all items
    }//end UpdatePrices

    void Update()
    {
        if (!inside)
        {//if not showing inside
         //this is so that it will stop the player moving, but will still allow rotation
            this.transform.Translate(Vector3.forward * transMult * Time.timeScale);//translate the player

            this.transform.Rotate(Input.GetAxisRaw("Vertical") * rotMult, Input.GetAxisRaw("Horizontal") * rotMult, 0);//rotate the player

            enter = CheckPos();//allowed to enter if close enough to a trade post
        }//end if not in inside

        if (!inside && Input.GetKey(KeyCode.Space)) //if pressing space
            Time.timeScale = 0;//pause
                               //means that it is only possible to pause when is not showing the inside
        else
            Time.timeScale = 1;//else dont pause

        if (nearItem != null)
        {//if not null, then there is something to collect
            double mass = controller.goods[nearItem.groupID].goods[nearItem.itemID].mass * nearItem.number;//the mass of the items in the crate
            if (spaceRemaining >= mass)
            {//if has enough space remaining
                spaceRemaining -= mass;//remove the space remaining
                cargo[nearItem.groupID][nearItem.itemID] += nearItem.number;//add the items to the cargo hold
                pickup = "You collected " + nearItem.number + "\u00D7" + controller.goods[nearItem.groupID].goods[nearItem.itemID].name;
                textshow = Time.timeSinceLevelLoad;//set when the text first shown
                nearItem.Collected();//need to tell the item that it has been collected
            }//end if not enough space
        }//end if item in radar


        if (Time.timeSinceLevelLoad - textshow > 3)//if shown for more than 3 seconds, dont
            pickup = "";//set to blank so that the label doesnt need to be disabled
    }//end Update

    bool CheckPos()
    {//Check if there are any trade posts within close distance
        Collider[] nearbyObjects = Physics.OverlapSphere(this.transform.position, closeDistance);

        for (int n = 0; n < nearbyObjects.Length; n++)
        {//go through nearby objects and see if they have the trade post tag
            if (nearbyObjects[n].tag == CallumP.TradeSys.Tags.TP)
            {//check has the trade post tag
                nearPost = nearbyObjects[n].GetComponent<CallumP.TradeSys.TradePost>();//set the near post to the trade post script
                return true;//return true so doesnt go through the rest of the nearby objects
            }
            else if (nearbyObjects[n].tag == CallumP.TradeSys.Tags.I && controller.pickUp)
            {//if item tag and allowed to collect
                nearItem = nearbyObjects[n].GetComponent<CallumP.TradeSys.Item>();//set the near item to this
                return false;//needs to return false so is not seen to be at a trade post
            }//end if item
        }//end for all nearby objects
        return false;//return false as has not found anything
    }//end CheckPos

    void OnGUI()
    {//display the GUI

        GUI.skin.box.fontSize = 35;//set the font sizes
        GUI.skin.label.fontSize = 20;

        if (enter && !inside && GUI.Button(new Rect(Screen.width - 110, 10, 100, 30), "Enter Post"))//if allowed to enter and enter post button pressed
            inside = true;

        if (inside)
        {//if entered inside menu

            GUI.Box(new Rect(5, 5, Screen.width - 10, Screen.height - 10), nearPost.name);//create a box with the name of the trade post at the top

            if (GUI.Button(new Rect(Screen.width - 60, 10, 50, 30), "Exit"))//if exit button pressed
                inside = false;

            if (CallumP.TagManagement.TagManager.ShareEnabled(this.gameObject, nearPost.gameObject, "Factions"))
            {//can see if they share a faction to allow trading
             //get the post tags if has been added
                List<bool> enabled = CallumP.TagManagement.ObjectTags.GetTagsList(nearPost.gameObject, "Post tags");

                if (enabled.Count < 3)//if doesnt have post tags
                    ShowShop();
                else
                {//else can show the selected window				
                    if (enabled[0])
                        EstateAgent();
                    else if (enabled[1])
                        ShowPurchasable();
                    else if (enabled[2])
                        ShowOwned();
                    else
                        ShowShop();
                }//end else show options

            }
            else
            {//else show message that they are not allowing trading
                ShowEnemyFaction();
            }//end else no trading
        }//end show inside
        GUI.Label(new Rect(10, Screen.height - 50, 500, 30), pickup);//display that item was collected
    }//end OnGUI()

    void EstateAgent()
    {//show the buy trade post screen
        GUI.Label(new Rect(10, 50, 250, 80), "Cash: " + cash);//show the player cash

        List<CallumP.TradeSys.TradePost> purchasable = new List<CallumP.TradeSys.TradePost>();//the list containing all of the trade posts which can be purchased
        for (int p = 0; p < controller.postScripts.Length; p++)
        {//go through all posts
            if (CallumP.TagManagement.ObjectTags.GetTag(controller.postScripts[p].gameObject, "Post tags", 1))//if has the purchasable tag checked
                purchasable.Add(controller.postScripts[p]);//add the trade post to the list of purchasable posts
        }//end for all posts
        scrollPos = GUI.BeginScrollView(new Rect(10, 100, Screen.width - 20, Screen.height - 110), scrollPos, new Rect(0, 0, 500, purchasable.Count * 30));
        //show all of the items inside a scroll view

        for (int p = 0; p < purchasable.Count; p++)
        {//go through all purchasable posts
            GUI.Label(new Rect(10, p * 30, 100, 30), purchasable[p].name);//show the name of the group
            GUI.Label(new Rect(120, p * 30, 200, 30), "Price: " + purchasable[p].currencies[0]);//show how much it is to purchase the trade post. uses the cash the trade post has for the price

            if (GUI.Button(new Rect(330, p * 30, 100, 30), "Purchase") && cash >= purchasable[p].currencies[0])
            {//purchase the trade post if has enough cash
                cash -= purchasable[p].currencies[0];//pay for the trade post
                nearPost.currencies[0] += purchasable[p].currencies[0];//add funds to estate agent as might be doing trading
                purchasable[p].currencies[0] = 0;//set the cash of the purhcased post to 0

                //can get the ObjectTags component so doesnt need to repeat this step in setting the tag
                CallumP.TagManagement.ObjectTags postTagObj = CallumP.TagManagement.ObjectTags.GetTagComponent(purchasable[p].gameObject, "Post tags");
                postTagObj.tags[1].selected = false;
                postTagObj.tags[2].selected = true;
                //alternatively, the above SetTags could be changed to be
                //these are more useful if is a sigle tag or dont have the ObjectTags script already
                //CallumP.TagManagement.ObjectTags.SetTag (postTagObj, 1, false);
                //CallumP.TagManagement.ObjectTags.SetTag (postTagObj, 2, true);

            }//end if purchase clicked
        }//end for all purchasable

        GUI.EndScrollView();//end the scroll view
    }//end EstateAgent

    void ShowPurchasable()
    {//show info that the trade post is for sale
        GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 75, 400, 150), "This trade post is for sale. " +
                "Visit the estate agents (magenta trade post) to purchase this trade post for " + nearPost.currencies[0]);
    }//end ShowPurchasable*/

    void ShowOwned()
    {//example GUI for an owned post
        int labelPos = 0;
        scrollPos = GUI.BeginScrollView(new Rect(10, 100, Screen.width - 20, Screen.height - 110), scrollPos, new Rect(0, 0, 500, HeightCalc()));
        for (int g = 0; g < controller.goods.Count; g++)
        {//go through all groups

            GUI.Label(new Rect(10, labelPos * 30, 100, 30), controller.goods[g].name);//the name of the group
            labelPos++;//increase so will show the next label on a different line

            if (g != 3)
            {//make sure that not the machinery group
             //another use for the item groups is so that certain items can have different properties etc to others
             //for example, the manchinery group that has been set up will have an option to fit the machinery to the trade post
             //then, the manufacturing groups can be used by enabling a specific group of manufacturing items to be manufactured					

                for (int i = 0; i < controller.goods[g].goods.Count; i++)
                {//go through all items
                    GUI.Label(new Rect(50, labelPos * 30, 100, 30), controller.goods[g].goods[i].name);//show the name of the item
                    nearPost.stock[g].stock[i].buy = GUI.Toggle(new Rect(160, labelPos * 30, 90, 30), nearPost.stock[g].stock[i].buy, "Buy");//show a buy toggle
                    nearPost.stock[g].stock[i].sell = GUI.Toggle(new Rect(260, labelPos * 30, 90, 30), nearPost.stock[g].stock[i].sell, "Sell");//show a sell toggle

                    GUI.Label(new Rect(360, labelPos * 30, 140, 30), "You have: " + cargo[g][i]);
                    GUI.Label(new Rect(610, labelPos * 30, 140, 30), "Post has: " + nearPost.stock[g].stock[i].number);

                    if (GUI.Button(new Rect(760, labelPos * 30, 50, 30), "Unload") && cargo[g][i] > 0)
                    {//if unload and has cargo
                        cargo[g][i]--;//remove cargo
                        nearPost.stock[g].stock[i].number++;//add cargo
                    }//end if unloading

                    if (GUI.Button(new Rect(820, labelPos * 30, 50, 30), "Load") && nearPost.stock[g].stock[i].number > 0)
                    {//if load and has cargo
                        nearPost.stock[g].stock[i].number--;//remove cargo
                        cargo[g][i]++;//add cargo
                    }//end if unloading

                    labelPos++;
                }//end for all items
            }
            else
            {//end if not machinery group
             //if is the machinery group, then need to check that the machinery has not already been added
             //this could be edited and the create and cooldown times changed so is lower so they are produced quicker
                for (int i = 0; i < controller.goods[g].goods.Count; i++)
                {//for all machinery items
                    GUI.Label(new Rect(50, labelPos * 30, 100, 30), controller.goods[g].goods[i].name);//show the name of the machinery

                    if (!nearPost.manufacture[i].enabled)
                    {//if the manufacturing process is not already enabled, then have an option to fit the machinery to enable it
                        if (cargo[g][i] > 0)
                        {//make sure has one to fit
                            if (GUI.Button(new Rect(160, labelPos * 30, 150, 30), "Add to trade post"))
                            {//if add to trade post pressed
                                cargo[g][i]--;//remove the machinery
                                nearPost.manufacture[i].enabled = true;//enable the manufacturing process
                            }//end if add to trade post
                        }
                        else//say need to buy
                            GUI.Label(new Rect(160, labelPos * 30, 250, 30), "Need to purchase first!");//show message saying need to buy first
                    }
                    else//end if can add machinery
                        GUI.Label(new Rect(160, labelPos * 30, 250, 30), "Already fitted to trade post!");//show message saying already added
                }//end for machinery
            }//end else maachinery group
        }//end for all goods groups
        GUI.EndScrollView();
    }

    void ShowShop()
    {//show the shop view
        GUI.Label(new Rect(10, 50, 250, 80), "Cash: " + cash + "\nSpace remaining: " + spaceRemaining.ToString("F2") + unitLabel);
        //a label showing the cash that the player has and the space remaining plus the maximum unit

        selected = GUI.Toolbar(new Rect(Screen.width - 200, 10, 130, 30), selected, new string[] {
                        "Buy",
                        "Sell"
                });
        //show a toolbar with the options to exit, buy and sell goods		

        scrollPos = GUI.BeginScrollView(new Rect(10, 100, Screen.width - 20, Screen.height - 110), scrollPos, new Rect(0, 0, 910, HeightCalc(selected)));
        //show all of the items inside a scroll view

        int labelPos = 0;

        for (int g = 0; g < controller.goods.Count; g++)
        {//go through all groups
            if (ShowGroup(g, selected))
            {//only need to go through items if at least one is enabled
                GUI.Label(new Rect(10, labelPos * 30, 100, 30), controller.goods[g].name);//show the name of the group
                labelPos++;//increase label pos so next will be displayed a line below
                for (int i = 0; i < controller.goods[g].goods.Count; i++)
                {//go through all items
                    if (ShowItem(g, i, selected))
                    {//if the item is to be shown

                        CallumP.TradeSys.Goods cG = controller.goods[g].goods[i];//get the controller good details
                        CallumP.TradeSys.Stock pS = nearPost.stock[g].stock[i];//get the trade post stock details

                        GUI.Label(new Rect(50, labelPos * 30, 150, 30), cG.name);//show the name of the item
                        GUI.Label(new Rect(210, labelPos * 30, 150, 30), "Mass: " + cG.unit);//show the mass of the item with the unit
                        GUI.Label(new Rect(370, labelPos * 30, 150, 30), "Post has: " + pS.number);//the number that the trade post has

                        float itemCost = selected == 0 ? pS.price : Mathf.RoundToInt(pS.price * controller.purchasePercent);
                        //if is in buy mode, the item cost is the standard price, if selling, need to multiply by purhcase percent

                        GUI.Label(new Rect(530, labelPos * 30, 150, 30), "Price: " + itemCost);
                        //show the price of the item. if it is in sell mode, then multiply the price by the trade psost purchase percent

                        GUI.Label(new Rect(690, labelPos * 30, 150, 30), "You have: " + cargo[g][i]);
                        //above shows a label with the number the player is carrying. if cargo index is -1, then it is not carrying the item

                        if (GUI.Button(new Rect(850, labelPos * 30, 50, 30), (selected == 0 ? "Buy" : "Sell")))
                        {//show a buy/sell button
                            if (selected == 0)
                            {//if is buy mode
                                if (cash >= itemCost && spaceRemaining >= cG.mass && pS.number > 0)
                                {//check if valid purchase
                                    pS.number--;//remove from trade post
                                    nearPost.currencies[cG.currencyID] += itemCost;//pay trade post cash
                                    cash -= itemCost;//withdraw cash from trader
                                    spaceRemaining -= cG.mass;//remove cargo space

                                    cargo[g][i]++;//add to the cargo being carried
                                }//end purchase validity check
                            }
                            else
                            {//else sell mode
                                if (nearPost.currencies[cG.currencyID] >= itemCost && cargo[g][i] > 0)
                                {//check if valid sale
                                    pS.number++;//add to trade post
                                    nearPost.currencies[cG.currencyID] -= itemCost;//withdraw cash from trade post
                                    cash += itemCost;//receive cash
                                    spaceRemaining += cG.mass;//add space back

                                    cargo[g][i]--;
                                }//end sale validity check
                            }//end buy/sell mode
                        }//end if pressed buy/sell button

                        labelPos++;//increase labelPos
                    }//end if showing item
                }//end for all items
            }//end if group showing
        }//end for goods groups

        GUI.EndScrollView();//end the scroll view
    }//end ShowShop

    void ShowEnemyFaction()
    {//show message that they are not allowing trades as they are not in the same faction
        GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 75, 400, 150), "You belong to a faction we refuse to trade with.\n\nPlease exit immediately.");
    }//end ShowEnemyFaction

    int HeightCalc(int sel)
    {//calculate the height necessary to display all of the goods and the group titles
     //an int is sent in because the displayed items may be differerent because it is possible to select that an item is buy only or sell only
        int count = 0;//set the count to 0
        for (int g = 0; g < controller.goods.Count; g++)
        {//for all groups
            bool addgroup = false; //used to keep track if the group has already been counted for
            for (int i = 0; i < controller.goods[g].goods.Count; i++)
            {//go through all of the goods in the group
                if (ShowItem(g, i, sel))
                {//check if the item is allowed to be shown
                    if (!addgroup)
                    {//if the group hasnt already been added
                        addgroup = true;//set to true
                        count++;//and add another so the title can be displayed
                    }//end if group title shown
                    count++;//increase the count
                }//end if allowed to show the item
            }//end for all goods
        }//end for all groups

        return count * 30;//multiply the count by 30 so will be the correct height
    }//end HeightCalc

    int HeightCalc()
    {//go through all items, counting
        int count = 0;//set count to 0
        for (int g = 0; g < controller.goods.Count; g++)//for all groups
            count += controller.goods[g].goods.Count + 1;//increase count by the number of items in the group and 1 for the group title
        return count * 30;
    }//end HeightCalc for all items

    bool ShowGroup(int g, int sel)
    {//returns a bool depending on if the group needs to be shown or not
        for (int i = 0; i < controller.goods[g].goods.Count; i++)
        {//go through all goods in the group to be checked
            if (ShowItem(g, i, sel))//check that the item is enabled
                return true;//if an item is, then dont need to check any further
        }
        return false;//else return false
    }//end ShowGroup

    bool ShowItem(int g, int i, int sel)
    {//return a bool depending on if the item has buy or sell selected
        CallumP.TradeSys.Stock stock = nearPost.stock[g].stock[i];
        if (sel == 0 && stock.sell || sel == 1 && stock.buy)//if buy check and sell at tp enabled, or sell check and buy at tp enabled
            return true;//return true
        return false;//else return false
    }//end ShowItem
}//end class