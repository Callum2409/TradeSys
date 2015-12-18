using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallumP.TagManagement;

namespace CallumP.TradeSys
{//use namespace to stop any name conflicts
    [CanEditMultipleObjects, CustomEditor(typeof(Controller))]
    public class ControllerEditor : Editor
    {
        [MenuItem("GameObject/Create Other/TradeSys Controller", false, 40)]
        //add an item to the menu
        //simple quick way to make the controller
        static void CreateController()
        {
            GameObject ctrl;

            ctrl = new GameObject();//create the GameObject
            Undo.RegisterCreatedObjectUndo(ctrl, "Create Controller");//create the GameObject

            Controller setup = ctrl.AddComponent<Controller>();
            setup.transform.position = Vector3.zero;//set the position to (0,0,0)
            setup.name = "_TS Controller";//needs to be called controller to make sure that everything works
            setup.tag = Tags.C;
            Selection.activeGameObject = setup.gameObject;//select the new GameObject
        }//end CreateController

        [MenuItem("GameObject/Create Other/TradeSys Controller", true)]
        //validate allowing a controller to be added
        static bool ValidateCreateController()
        {
            return GameObject.FindGameObjectsWithTag(Tags.C).Length == 0;
        }//end ValidateCreateController

        TSGUI GUITools = new TSGUI();//extra gui methods, which are used by TradeSys scripts

        #region options
        private SerializedProperty showGN;
        private SerializedProperty smallScroll;
        private Selected sel;
        private ScrollPos scrollPos;
        #endregion

        #region variables
        private SerializedObject controllerSO;
        private Controller controllerNormal;
        private SerializedObject[] postScripts;
        private SerializedObject[] traderScripts;
        private SerializedObject[] spawnerScripts;
        private SerializedProperty showLinks;
        private SerializedProperty updateInterval;
        private SerializedProperty generateAtStart;
        private SerializedProperty pickUp, defaultCrate;
        private SerializedProperty pauseOption, pauseTime, pauseEnter, pauseExit;
        private SerializedProperty currencies, exchange;
        private SerializedProperty goods;
        private SerializedProperty manufacture;
        private SerializedProperty closestPosts, buyMultiple, sellMultiple, distanceWeight, profitWeight, purchasePercent, priceUpdates, moveType, expTraders;
        private SerializedProperty units;
        bool expendable;
        #endregion

        ///get the required information
        public void OnEnable()
        {
            controllerSO = new SerializedObject(target);
            controllerNormal = (Controller)target;
            controllerNormal.tag = Tags.C;

            controllerNormal.tradePosts = GameObject.FindGameObjectsWithTag(Tags.TP);
            controllerNormal.postScripts = new TradePost[controllerNormal.tradePosts.Length];
            postScripts = new SerializedObject[controllerNormal.tradePosts.Length];
            for (int p = 0; p < controllerNormal.tradePosts.Length; p++)
            {
                controllerNormal.postScripts[p] = controllerNormal.tradePosts[p].GetComponent<TradePost>();
                postScripts[p] = new SerializedObject(controllerNormal.postScripts[p]);
            }

            expendable = controllerNormal.expTraders.enabled;

            if (!expendable)
            {//get traders if not expendable
                controllerNormal.traders = GameObject.FindGameObjectsWithTag(Tags.T);
                controllerNormal.traderScripts = new Trader[controllerNormal.traders.Length];
                traderScripts = new SerializedObject[controllerNormal.traders.Length];
                for (int t = 0; t < controllerNormal.traders.Length; t++)
                {
                    controllerNormal.traderScripts[t] = controllerNormal.traders[t].GetComponent<Trader>();
                    traderScripts[t] = new SerializedObject(controllerNormal.traderScripts[t]);
                }
            }//end if not expendable

            GameObject[] spawners = GameObject.FindGameObjectsWithTag(Tags.S);
            controllerNormal.spawners = new Spawner[spawners.Length];
            spawnerScripts = new SerializedObject[spawners.Length];
            for (int s = 0; s < spawners.Length; s++)
            {
                controllerNormal.spawners[s] = spawners[s].GetComponent<Spawner>();
                spawnerScripts[s] = new SerializedObject(controllerNormal.spawners[s]);
            }

            #region get options
            showGN = controllerSO.FindProperty("showGN");
            smallScroll = controllerSO.FindProperty("smallScroll");
            sel = controllerNormal.selected;
            scrollPos = controllerNormal.scrollPos;
            #endregion

            #region get variables
            showLinks = controllerSO.FindProperty("showLinks");

            updateInterval = controllerSO.FindProperty("updateInterval");

            generateAtStart = controllerSO.FindProperty("generateAtStart");
            pickUp = controllerSO.FindProperty("pickUp");
            defaultCrate = controllerSO.FindProperty("defaultCrate");

            pauseOption = controllerSO.FindProperty("pauseOption");
            pauseTime = controllerSO.FindProperty("pauseTime");
            pauseEnter = controllerSO.FindProperty("pauseEnter");
            pauseExit = controllerSO.FindProperty("pauseExit");

            currencies = controllerSO.FindProperty("currencies");
            exchange = controllerSO.FindProperty("currencyExchange");
            goods = controllerSO.FindProperty("goods");
            manufacture = controllerSO.FindProperty("manufacture");

            closestPosts = controllerSO.FindProperty("closestPosts");
            buyMultiple = controllerSO.FindProperty("buyMultiple");
            sellMultiple = controllerSO.FindProperty("sellMultiple");
            distanceWeight = controllerSO.FindProperty("distanceWeight");
            profitWeight = controllerSO.FindProperty("profitWeight");
            purchasePercent = controllerSO.FindProperty("purchasePercent");
            priceUpdates = controllerSO.FindProperty("priceUpdates");
            moveType = controllerSO.FindProperty("moveType");

            expTraders = controllerSO.FindProperty("expTraders");

            units = controllerSO.FindProperty("units");
            #endregion

            controllerNormal.SortAll();
        }//end OnEnable

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(controllerNormal, "TradeSys Controller");
            EditorGUIUtility.fieldWidth = 30f;

            for (int p = 0; p < postScripts.Length; p++)
                postScripts[p].Update();

            if (!expendable)
                for (int t = 0; t < traderScripts.Length; t++)
                    traderScripts[t].Update();

            for (int s = 0; s < spawnerScripts.Length; s++)
                spawnerScripts[s].Update();

            #region get goods names
            List<string> allNames = new List<string>();//a list of all the names of goods to be shown in manufacturing, not in groups
            int[] groupLengthsG = new int[goods.arraySize];//contains the length of each group, so can convert between int and groupID and itemID

            controllerNormal.allNames = new string[controllerNormal.goods.Count][];//an array of names of goods in groups
            controllerNormal.GetCurrencyNames();

            for (int g1 = 0; g1 < goods.arraySize; g1++)
            {//go through all groups
                SerializedProperty currentGroup = goods.GetArrayElementAtIndex(g1).FindPropertyRelative("goods");
                groupLengthsG[g1] = currentGroup.arraySize;
                controllerNormal.allNames[g1] = new string[currentGroup.arraySize];
                for (int g2 = 0; g2 < currentGroup.arraySize; g2++)
                {//go through all goods in group
                    string itemName = currentGroup.GetArrayElementAtIndex(g2).FindPropertyRelative("name").stringValue;
                    controllerNormal.allNames[g1][g2] = itemName;
                    if (showGN.boolValue)//if show group name
                        allNames.Add(goods.GetArrayElementAtIndex(g1).FindPropertyRelative("name").stringValue + " " + itemName);
                    else
                        allNames.Add(itemName);
                }
            }//end go through all groups
            #endregion

            GUITools.ManufactureInfo(controllerNormal);

            #region sort units
            for (int g = 0; g < controllerNormal.goods.Count; g++)
            {//go through groups
                for (int i = 0; i < controllerNormal.goods[g].goods.Count; i++)
                {//go through items
                    Goods cI = controllerNormal.goods[g].goods[i];
                    cI.unit = UnitString(cI.mass, false);
                }//end for items
            }//end for groups
            #endregion

            controllerNormal.SortController();

            controllerSO.Update();//needs to update

            sel.C = GUITools.Toolbar(sel.C, new string[] {
                                "Settings",
                                "Currencies",
                                "Goods",
                                "Manufacturing"
                        });//show a toolbar

            switch (sel.C)
            {
                #region settings
                case 0:
                    scrollPos.S = GUITools.StartScroll(scrollPos.S, smallScroll);
                    #region general	
                    if (GUITools.TitleGroup(new GUIContent("General options", "These are options which affect how the editors are displayed"), controllerSO.FindProperty("genOp"), false))
                    {//show general options
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(showGN, new GUIContent("Show group names", "In the manufacturing tab, when selecting items, show the name of the group that it belongs to. Means that if there are items with the same name but are in different groups, the names will appear"));
                        bool sSB = smallScroll.boolValue;
                        EditorGUILayout.PropertyField(smallScroll, new GUIContent("Smaller scroll views", "In the other tabs, have a scroll pane of the added elements leaving the options above"));
                        if (smallScroll.boolValue != sSB)//have a check to see if changed. if has changed, break because sometimes get an error
                            break;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(showLinks, new GUIContent("Show trade links", "Show the possible trade links between trade posts"));
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.EndHorizontal();
                    }//end if showing general options
                    EditorGUILayout.EndVertical();
                    #endregion

                    #region game
                    if (GUITools.TitleGroup(new GUIContent("Game options", "These affect how TradeSys works"), controllerSO.FindProperty("gamOp"), false))
                    {//show game options
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(updateInterval, new GUIContent("Update interval", "Set this to a higher value list updates, trade calls and manufacturing updates are not as frequent"));
                        UnitLabel("s", 25);
                        updateInterval.floatValue = 1 / EditorGUILayout.FloatField(new GUIContent("Frequency", "The number of times per second to update"), (1 / updateInterval.floatValue));
                        UnitLabel("Hz", 35);

                        if (updateInterval.floatValue < 0.02f)
                            updateInterval.floatValue = 0.02f;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(generateAtStart, new GUIContent("Generate at start", "If the trade posts have been set, enable this. If they are being added through code, then disable. Call the GenerateDistances method in the controller once they have been added."));
                        EditorGUILayout.PropertyField(pickUp, new GUIContent("Allow pickup", "Allow the collection of items found. If enabled, allows item crates to be set."));

                        if (GameObject.FindGameObjectsWithTag(Tags.S).Length > 0)//if there are spawners, this needs to be true
                            pickUp.boolValue = true;

                        EditorGUILayout.EndHorizontal();

                        if (pickUp.boolValue)
                        {//only show default crate option if pickup has been enabled
                            EditorGUILayout.PropertyField(defaultCrate, new GUIContent("Default crate", "If an item crate has not been set, use this crate instead"));

                            if (defaultCrate.objectReferenceValue != null)
                                SortCrate(defaultCrate);
                        }//end if pickup enabled
                    } //end if showing game options
                    EditorGUILayout.EndVertical();
                    #endregion

                    #region trade
                    if (GUITools.TitleGroup(new GUIContent("Trade options", "These affect how the trader destination post is decided"), controllerSO.FindProperty("traOp"), false))
                    {//show trade options
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(closestPosts, new GUIContent("Closest posts", "The number of closest posts that should be taken into account for finding the best post to go to. Decrease this value to improve performance. Set to 0 for all"));
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(buyMultiple, new GUIContent("Buy multiple", "If the number of items at a trade post multiplied by this value is less than the average number, the trade post will want to buy this item"));
                        EditorGUILayout.PropertyField(sellMultiple, new GUIContent("Sell multiple", "If the number of items at a trade post is greater than the average number multiplied by this value, then it will want to sell this item"));
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(distanceWeight, new GUIContent("Distance weight", "The value that the distance is multiplied by in order to help find the best post to go to. This could for example be the fuel cost per unit"));
                        if (!expendable)//only show profit if not expendable
                            EditorGUILayout.PropertyField(profitWeight, new GUIContent("Profit weight", "The value that the profit is multiplied by in order to help find the best post to go to. This is by taking the profit, multiplying by this value and subtracting the distance multiplied by the distance weight"));
                        else
                            EditorGUILayout.LabelField("");
                        EditorGUILayout.EndHorizontal();

                        if (!expendable)
                        {//only show purchase percent and price updates if not expendable
                            EditorGUILayout.BeginHorizontal();
                            //the purchase percent is as the value before multiplied by 100. This is so that when used, does not need to be constantly divided by 100
                            //it is only in the editor
                            purchasePercent.floatValue = 0.01f * EditorGUILayout.FloatField(new GUIContent("Purchase percent", "This is the percentage of the sale price that the trade post buys items at"), purchasePercent.floatValue * 100);
                            UnitLabel("%", 32);

                            EditorGUILayout.LabelField("");
                            EditorGUILayout.EndHorizontal();

                            if (purchasePercent.floatValue < 0)
                                purchasePercent.floatValue = 0;
                            else if (purchasePercent.floatValue > 1)
                                purchasePercent.floatValue = 1;
                            //constrain between 0 and 1. Any less, gets paid for receiving, any more not making profit

                            EditorGUILayout.PropertyField(priceUpdates, new GUIContent("Update prices", "Update the prices of the item after each individual item has been purchased"));

                            GUIContent[] moveOptions = new GUIContent[] {
                                                        new GUIContent ("Random post", "Move to a random post that the trader is able to reach. Not closest because could just be moving between the same trade posts."),
                                                        new GUIContent ("Items per distance", "Move to a reachable trade post which has the highest value when the number of items the post wants to sell is divided by the distance. " +
                                                        "The highest value will give the trader the best chance of fining a trade with per unit distance."),
                                                        new GUIContent ("Best trade", "Move to the reachable trade post which offers the best trade out of all the reachable trade posts. This requires a lot more computing power and may not result in a valid move.")
                                                };

                            moveType.intValue = EditorGUILayout.Popup(new GUIContent("Move option", "Select what happens when a trader is unable to make a trade at the trade post its at. Processing power required increases down the list."),
            moveType.intValue, moveOptions, "DropDownButton");


                        }//end if not expendable

                        //make sure that the values never go below minimum
                        if (closestPosts.intValue < 0)
                            closestPosts.intValue = 0;

                        if (buyMultiple.floatValue < 1)
                            buyMultiple.floatValue = 1;

                        if (sellMultiple.floatValue < 1)
                            sellMultiple.floatValue = 1;

                        if (distanceWeight.floatValue < 0)
                            distanceWeight.floatValue = 0;

                        if (profitWeight.floatValue < 0)
                            profitWeight.floatValue = 0;

                    }//end if showing trade options
                    EditorGUILayout.EndVertical();
                    #endregion

                    #region pausing
                    if (GUITools.TitleGroup(new GUIContent("Pause options", "These affect when and for how long a trader pauses for. Only affects cargo that is transferred"), controllerSO.FindProperty("pauOp"), false))
                    {//show pause options	
                        string unitText = "1";//the text used for the mass of items
                        if (pauseOption.intValue == 2 || pauseOption.intValue == 3)
                        {//if need to work out which unit
                            for (int u = 0; u < controllerNormal.units.units.Count; u++)
                            {//go through units
                                Unit cU = controllerNormal.units.units[u];
                                if (cU.min <= 1 && cU.max > 1)
                                {//check in range
                                    unitText = (1 / (decimal)cU.min).ToString() + " " + cU.suffix;
                                    break;
                                }//end range check
                            }//end for units
                        }//end find unit

                        GUIContent[] pauseOptions = new GUIContent[] {
                                                        new GUIContent ("Set time", "This is how long all the traders will pause for"),
                                                        new GUIContent ("Trader specific", "Set the pause time on individual traders"),
                                                        new GUIContent ("Cargo mass", "This is how long a trader will pause for when loading / unloading every " + unitText + " of cargo"),
                                                        new GUIContent ("Cargo mass specific", "Set the pause time for loading / unloading every " + unitText + " of individual items")
                                                };

                        EditorGUILayout.BeginHorizontal();
                        pauseOption.intValue = EditorGUILayout.Popup(pauseOption.intValue, pauseOptions, "DropDownButton");
                        if (pauseOption.intValue == 0 || pauseOption.intValue == 2)
                        {//if general times, need to have the time option
                            GUIContent pauseText = new GUIContent("Pause time", "Set the pause time which every trader will pause for");

                            if (pauseOption.intValue == 2)
                                pauseText = new GUIContent("Pause time per " + unitText, "Set the pause time per " + unitText + " of cargo carried");

                            EditorGUILayout.PropertyField(pauseTime, pauseText);
                            UnitLabel("s", 25);

                            if (pauseTime.floatValue < 0)
                                pauseTime.floatValue = 0;

                            if (pauseOption.intValue == 0)
                            {//if set time
                                for (int t = 0; t < controllerNormal.traderScripts.Length; t++)//go through all traders
                                    controllerNormal.traderScripts[t].stopTime = pauseTime.floatValue;
                            }//end if set time

                            if (pauseOption.intValue == 2)
                            {//if each item
                                for (int g = 0; g < controllerNormal.goods.Count; g++)//go through all groups
                                    for (int i = 0; i < controllerNormal.goods[g].goods.Count; i++)//go through all items
                                                                                                   //need to multiply the time [er unit by the mass
                                        controllerNormal.goods[g].goods[i].pausePerUnit = pauseTime.floatValue * controllerNormal.goods[g].goods[i].mass;
                            }//end if each item
                        }
                        else//end if 0 or 2
                            EditorGUILayout.LabelField("");
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(pauseEnter, new GUIContent("Pause on entry", "Make the trader pause when entering a trade post, e.g. for unloading cargo. Pauses after unloading evrything"));
                        EditorGUILayout.PropertyField(pauseExit, new GUIContent("Pause on exit", "Make the trader pause when leaving a trade post, e.g. for loading cargo. Pauses after loading everything"));
                        EditorGUILayout.EndHorizontal();
                    }//end if showing pause options
                    EditorGUILayout.EndVertical();
                    #endregion

                    #region expendable
                    SerializedProperty expanded = expTraders.FindPropertyRelative("expandedC");
                    SerializedProperty enabled = expTraders.FindPropertyRelative("enabled");

                    bool before = controllerNormal.expTraders.enabled;
                    GUITools.TitleGroup(new GUIContent("Expendable traders", "Create traders to carry cargo from one trade post to another. The trader will be deleted when it arrives at its desitination"), expanded, false);

                    if (expanded.boolValue)
                    {//show if expanded
                        SerializedProperty traderList = expTraders.FindPropertyRelative("traders");

                        int number = traderList.arraySize;

                        EditorGUI.indentLevel = 1;
                        EditorGUILayout.PropertyField(enabled, new GUIContent("Enable expendable traders"));

                        if (enabled.boolValue)
                        {//show only if enabled

                            EditorGUILayout.BeginHorizontal();
                            SerializedProperty maxNoTraders = expTraders.FindPropertyRelative("maxNoTraders");
                            EditorGUILayout.PropertyField(maxNoTraders, new GUIContent("Max traders", "The maximum number of expendable traders allowed. Set this to 0 for infinite."));

                            if (maxNoTraders.intValue < 0)
                                maxNoTraders.intValue = 0;

                            GUILayout.FlexibleSpace();
                            EditorGUILayout.LabelField("Number of expendable traders:", number.ToString());

                            if (GUITools.PlusMinus(true))
                            {
                                traderList.InsertArrayElementAtIndex(number);
                                traderList.GetArrayElementAtIndex(number).objectReferenceValue = null;
                            }//if add pressed

                            EditorGUILayout.EndHorizontal();

                            if (number > 0)
                            {//check that there are options to show
                                GUITools.IndentGroup(2);
                                for (int o = 0; o < number; o++)
                                {//for all options
                                    SerializedProperty cO = traderList.GetArrayElementAtIndex(o);//current trader

                                    EditorGUILayout.BeginHorizontal();

                                    if (cO.objectReferenceValue == null)//check if null and make red
                                        GUI.color = Color.red;//makre red if null

                                    EditorGUILayout.PropertyField(cO, new GUIContent());//show editable name field
                                    GUI.color = Color.white;//set back to normal

                                    if (GUITools.PlusMinus(false))
                                    {

                                        cO.objectReferenceValue = null;//needs to be set to null before it can be deleted
                                        traderList.DeleteArrayElementAtIndex(o);
                                        break;
                                    }//end if minus pressed
                                    EditorGUILayout.EndHorizontal();
                                }//end for all traders
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndHorizontal();
                            }//end if showing something
                        }//end if enabled
                    }//end if expanded
                    EditorGUILayout.EndVertical();

                    if (!before && expendable)
                        controllerNormal.SortTraders();//need to sort the traders if just enabled them again

                    #endregion

                    #region units
                    if (GUITools.TitleGroup(new GUIContent("Units", "Set the different units that can be used for the weights of goods. The unit min max must increase down the list, so the max of the first is the same as the min of the one after it"), units.FindPropertyRelative("expanded"), false))
                    {//show the units

                        SerializedProperty unitInfo = units.FindPropertyRelative("units");

                        int numberU = unitInfo.arraySize;

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10f);

                        if (GUILayout.Button(new GUIContent("g, kg, t", "Set up metric units of g, kg and t. 1 = 1 kg"), EditorStyles.miniButton))
                        {

                            controllerNormal.units = new Units
                            {
                                expanded = true,
                                units = new List<Unit>{new Unit{suffix = "g", min = 0.000001f, max = 0.001f},
                                                                                                                                new Unit{suffix = "kg", min = 0.001f, max = 1f},
                                                                                                                                new Unit{suffix = "t", min = 1f, max = Mathf.Infinity}}
                            };
                        }//end if setting standard units


                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Number of units", numberU.ToString());

                        if (GUITools.PlusMinus(true))
                        {
                            unitInfo.InsertArrayElementAtIndex(numberU);
                            SerializedProperty inserted = unitInfo.GetArrayElementAtIndex(numberU);
                            inserted.FindPropertyRelative("suffix").stringValue = "Unit " + numberU;
                            if (numberU == 0)//if is the first, set the min to the minimum mass
                                inserted.FindPropertyRelative("min").floatValue = 0.000001f;
                            else//else set to the max mass of the previous
                                inserted.FindPropertyRelative("min").floatValue = unitInfo.GetArrayElementAtIndex(numberU - 1).FindPropertyRelative("max").floatValue;
                            inserted.FindPropertyRelative("max").floatValue = Mathf.Infinity;//set the max to infinity
                        }//if add pressed

                        EditorGUILayout.EndHorizontal();

                        if (numberU > 0)
                        {//only go through if there is something to show
                            for (int u = 0; u < numberU; u++)
                            {//for all units
                                SerializedProperty cU = unitInfo.GetArrayElementAtIndex(u);//current unit
                                SerializedProperty cUS = cU.FindPropertyRelative("suffix");

                                if (units.FindPropertyRelative("units").GetArrayElementAtIndex(u).FindPropertyRelative("min").floatValue == Mathf.Infinity)
                                    GUI.color = Color.red;
                                GUITools.IndentGroup(0);

                                EditorGUILayout.BeginHorizontal();

                                EditorGUILayout.PropertyField(cUS, new GUIContent("Unit suffix"));//show editable name field
                                if (cUS.stringValue == "")//if name is blank
                                    cUS.stringValue = "Unit " + u;

                                if (GUITools.PlusMinus(false))
                                {
                                    unitInfo.DeleteArrayElementAtIndex(u);
                                    break;
                                }//end if minus pressed
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                EditorGUI.indentLevel = 1;
                                EditorGUILayout.PropertyField(cU.FindPropertyRelative("min"), new GUIContent("Min", "If the mass is greater than or equal to this value and less than the max value, then it will have this unit"));
                                EditorGUILayout.PropertyField(cU.FindPropertyRelative("max"), new GUIContent("Max", "If the mass is less than this value and greater than or equal to the min value, then it will have this unit"));
                                EditorGUILayout.EndHorizontal();

                                if (cU.FindPropertyRelative("min").floatValue < 0.000001f)
                                    cU.FindPropertyRelative("min").floatValue = 0.000001f;
                                if (cU.FindPropertyRelative("max").floatValue <= cU.FindPropertyRelative("min").floatValue)
                                    cU.FindPropertyRelative("max").floatValue = cU.FindPropertyRelative("min").floatValue + 0.000001f;

                                if (u > 0)//set the max of the previous to the min of this unit
                                    unitInfo.GetArrayElementAtIndex(u - 1).FindPropertyRelative("max").floatValue = cU.FindPropertyRelative("min").floatValue;
                                if (u < numberU - 1)//set the min of the next to the max of this unit
                                    unitInfo.GetArrayElementAtIndex(u + 1).FindPropertyRelative("min").floatValue = cU.FindPropertyRelative("max").floatValue;

                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndHorizontal();
                                GUI.color = Color.white;
                            }//end for all units
                        }//end if soemthing showing

                        EditorGUI.indentLevel = 0;
                        if (!InfinityCheck("max") && numberU > 0)
                            EditorGUILayout.HelpBox("None of your units extend to infinity, so some items may not have a unit.\nMake sure that the max value of one of the units is set to infinity", MessageType.Warning);
                        if (InfinityCheck("min"))
                            EditorGUILayout.HelpBox("The min value of one of your units is infinty with the max also being infinity. As a result, the unit will not be able to be used", MessageType.Error);
                    }//end if showing units
                    EditorGUILayout.EndVertical();
                    #endregion

                    if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
                        EditorGUILayout.EndScrollView();
                    break;
                #endregion

                #region currencies
                case 1:
                    EditorGUI.indentLevel = 0;

                    sel.CC = GUITools.Toolbar(sel.CC, new string[] { "Currencies", "Exchanges" });

                    switch (sel.CC)
                    { //switch for currency or exchange tab
                        case 0:
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Number of currencies", currencies.arraySize.ToString());
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button(new GUIContent("Add", "Add a new currency"), EditorStyles.miniButtonLeft))
                            {
                                int index = currencies.arraySize;
                                currencies.InsertArrayElementAtIndex(index);
                                SerializedProperty newCur = currencies.GetArrayElementAtIndex(index);
                                newCur.FindPropertyRelative("single").stringValue = newCur.FindPropertyRelative("plural").stringValue = "New currency " + index;
                                newCur.FindPropertyRelative("formatString").stringValue = "{0} {1}";
                                newCur.FindPropertyRelative("decimals").intValue = 0;
                                newCur.FindPropertyRelative("expanded").boolValue = currencies.GetArrayElementAtIndex(index - 1).FindPropertyRelative("expanded").boolValue;

                                PTCur(postScripts, true, index);
                                PTCur(traderScripts, true, index);
                            }//end if added currency

                            GUITools.ExpandCollapse(currencies, "expanded", true);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorStyles.label.wordWrap = true;
                            EditorGUILayout.LabelField("The currency will be referred to using the plural name given.\n\nIn the format string, symbols or other text can be added before or after the value. Prices may not be given to full number of decimal places if number is large..\n{0} : price\n{1} : single or plural name, depending on amount\n\nMust contain {0}");
                            EditorGUILayout.EndVertical();

                            scrollPos.C = GUITools.StartScroll(scrollPos.C, smallScroll);

                            for (int c = 0; c < currencies.arraySize; c++)
                            { //go through all currencies, displaying them
                                EditorGUI.indentLevel = 0;
                                SerializedProperty currentCur = currencies.GetArrayElementAtIndex(c);//current currency

                                EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));

                                if (currentCur.FindPropertyRelative("expanded").boolValue)//if expanded
                                    EditorGUILayout.BeginVertical("HelpBox");//show everything in a box together

                                //do the title bar things
                                EditorGUILayout.BeginHorizontal();
                                currentCur.FindPropertyRelative("expanded").boolValue = GUITools.TitleButton(new GUIContent(currentCur.FindPropertyRelative("plural").stringValue, ""), currentCur.FindPropertyRelative("expanded"), "ControlLabel");
                                GUILayout.FlexibleSpace();

                                if (currencies.arraySize > 1 && GUITools.PlusMinus(false))
                                { //if remove
                                    currencies.DeleteArrayElementAtIndex(c);//delete the currency

                                    PTCur(postScripts, false, c);
                                    PTCur(traderScripts, false, c);

                                    if (c == currencies.arraySize)
                                    {//only need to reduce if deleted currency was at end of array
                                     //sort controller goods
                                        for (int g = 0; g < controllerNormal.goods.Count; g++)//for all goods
                                            for (int i = 0; i < controllerNormal.goods[g].goods.Count; i++)//for all items
                                                if (controllerNormal.goods[g].goods[i].currencyID >= c)//if is greater or equal to currency removed
                                                    goods.GetArrayElementAtIndex(g).FindPropertyRelative("goods").GetArrayElementAtIndex(i).FindPropertyRelative("currencyID").intValue--;

                                        //sort p, t manufacturing
                                        for (int m = 0; m < controllerNormal.manufacture.Count; m++)
                                        {//for all manufacturing groups
                                            for (int p = 0; p < controllerNormal.manufacture[m].manufacture.Count; p++)
                                            {//for all manufacturing processes
                                                PTMan(postScripts, m, p, c);
                                                PTMan(traderScripts, m, p, c);                                                
                                            }//end for all processes
                                        }//end for all groups
                                    }//end if last currency

                                    for (int e = 0; e < exchange.arraySize; e++)
                                    { //for all exchanges
                                        if (exchange.GetArrayElementAtIndex(e).FindPropertyRelative("IDA").intValue >= c)
                                            exchange.GetArrayElementAtIndex(e).FindPropertyRelative("IDA").intValue--;
                                        if (exchange.GetArrayElementAtIndex(e).FindPropertyRelative("IDB").intValue >= c)
                                            exchange.GetArrayElementAtIndex(e).FindPropertyRelative("IDB").intValue--;
                                    }//end for all excahanges

                                    GUIUtility.keyboardControl = 0;

                                    break;
                                }//end if remove
                                EditorGUILayout.EndHorizontal();

                                if (currentCur.FindPropertyRelative("expanded").boolValue)
                                {//if expanded
                                    EditorGUI.indentLevel = 1;

                                    EditorGUIUtility.labelWidth = 150;

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(currentCur.FindPropertyRelative("single"), new GUIContent("Singular name", "The singlar name for the currency"));
                                    EditorGUILayout.PropertyField(currentCur.FindPropertyRelative("plural"), new GUIContent("Plural name", "The plural name of the currency. This name will be used when referencing the currency"));
                                    EditorGUILayout.EndHorizontal();

                                    if (currentCur.FindPropertyRelative("single").stringValue == "")
                                        currentCur.FindPropertyRelative("single").stringValue = "New currency";
                                    if (currentCur.FindPropertyRelative("plural").stringValue == "")
                                        currentCur.FindPropertyRelative("plural").stringValue = "New currency";

                                    EditorGUILayout.PropertyField(currentCur.FindPropertyRelative("formatString"), new GUIContent("Format", "Set the format to be used when displaying costs"));
                                    EditorGUILayout.IntSlider(currentCur.FindPropertyRelative("decimals"), 0, 5, new GUIContent("Decimal places", "The number of decimal places the currency will be displayed to"));

                                    if (!currentCur.FindPropertyRelative("formatString").stringValue.Contains("{0}"))
                                        currentCur.FindPropertyRelative("formatString").stringValue = "{0} {1}";

                                    string s = controllerNormal.GetPriceFormatted(currentCur.FindPropertyRelative("formatString").stringValue, 1f, currentCur.FindPropertyRelative("decimals").intValue, currentCur.FindPropertyRelative("single").stringValue, currentCur.FindPropertyRelative("plural").stringValue);
                                    string p = controllerNormal.GetPriceFormatted(currentCur.FindPropertyRelative("formatString").stringValue, Mathf.PI, currentCur.FindPropertyRelative("decimals").intValue, currentCur.FindPropertyRelative("single").stringValue, currentCur.FindPropertyRelative("plural").stringValue);

                                    EditorGUILayout.LabelField(string.Format("Examples:\n    {0}\n    {1}", s, p));

                                    EditorGUILayout.EndVertical();
                                }//end if expanded
                            }//end for all currencies
                            controllerNormal.GetCurrencyNames();//update the names if necessary
                            if (currencies.arraySize > 0)
                                EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));
                            break;

                        case 1://exchange tab
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Number of currency exchanges:", exchange.arraySize.ToString());
                            GUILayout.FlexibleSpace();

                            if (GUITools.PlusMinus(true))
                            { //if add new exchange
                                int index = exchange.arraySize;
                                exchange.InsertArrayElementAtIndex(index);
                                SerializedProperty newEx = exchange.GetArrayElementAtIndex(index);
                                newEx.FindPropertyRelative("numberA").floatValue = newEx.FindPropertyRelative("numberB").floatValue = 1;
                                newEx.FindPropertyRelative("IDA").intValue = newEx.FindPropertyRelative("IDB").intValue = 0;
                                newEx.FindPropertyRelative("reverse").boolValue = false;

                                PTEx(postScripts, true, index);
                                PTEx(traderScripts, true, index);
                            }//end add new exchange
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginVertical("HelpBox");
                            EditorGUILayout.LabelField("Enter the number of a curreny and the currency type to then be converted. Tick the checkbox to allow the conversion to happen in reverse. This will also make the arrow two way to indicate.\n\nExchanges where there are errors are highlighted in red.");
                            EditorGUILayout.EndVertical();

                            scrollPos.C = GUITools.StartScroll(scrollPos.C, smallScroll);

                            EditorGUIUtility.labelWidth = Screen.width / 4;

                            for (int e = 0; e < exchange.arraySize; e++)
                            { //for all exchanges
                                SerializedProperty curEx = exchange.GetArrayElementAtIndex(e);
                                EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));

                                GUI.color = ExchangeIssue(curEx, e) ? Color.red : Color.white;

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(curEx.FindPropertyRelative("numberA"), GUIContent.none);

                                EditorGUILayout.BeginVertical();
                                GUILayout.Space(1f);
                                curEx.FindPropertyRelative("IDA").intValue = EditorGUILayout.Popup("", curEx.FindPropertyRelative("IDA").intValue, controllerNormal.currencyNames, "DropDownButton");
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.LabelField(curEx.FindPropertyRelative("reverse").boolValue ? "\u2194" : "\u2192");
                                EditorGUILayout.PropertyField(curEx.FindPropertyRelative("numberB"), GUIContent.none);

                                EditorGUILayout.BeginVertical();
                                GUILayout.Space(1f);
                                curEx.FindPropertyRelative("IDB").intValue = EditorGUILayout.Popup("", curEx.FindPropertyRelative("IDB").intValue, controllerNormal.currencyNames, "DropDownButton");
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.PropertyField(curEx.FindPropertyRelative("reverse"), new GUIContent("", "Allow the exchange to operate in reverse"), GUILayout.Width(15f));

                                if (GUITools.PlusMinus(false))
                                {
                                    exchange.DeleteArrayElementAtIndex(e);

                                    PTEx(postScripts, false, e);
                                    PTEx(traderScripts, false, e);
                                    break;
                                }//end if remove pressed
                                EditorGUILayout.EndHorizontal();

                                GUI.color = Color.white;

                                float a = curEx.FindPropertyRelative("numberA").floatValue;
                                float b = curEx.FindPropertyRelative("numberB").floatValue;

                                if (a <= 0)
                                    a = 1;
                                if (b <= 0)
                                    b = 1;

                                curEx.FindPropertyRelative("numberA").floatValue = a;
                                curEx.FindPropertyRelative("numberB").floatValue = b;
                                curEx.FindPropertyRelative("multiplier").floatValue = b / a;

                            }//end for all exchanges
                            break;
                    }//end currency switch

                    EditorGUI.indentLevel = 0;
                    if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
                        EditorGUILayout.EndScrollView();
                    break;
                #endregion

                #region goods
                case 2:
                    EditorGUI.indentLevel = 0;

                    EditorGUILayout.LabelField(new GUIContent("Total number", "The total number of goods that have been defined across all groups"), new GUIContent(allNames.Count.ToString(), "The total number of goods that have been defined across all groups"));

                    EditorGUILayout.BeginHorizontal();//have a toolbar with the different group names on
                    scrollPos.GG = EditorGUILayout.BeginScrollView(scrollPos.GG, GUILayout.Height(40f));

                    SerializedProperty selGG = controllerSO.FindProperty("selected").FindPropertyRelative("GG");

                    string[] namesG = new string[goods.arraySize];
                    for (int n = 0; n < goods.arraySize; n++)
                        namesG[n] = goods.GetArrayElementAtIndex(n).FindPropertyRelative("name").stringValue;
                    int selGB = selGG.intValue;
                    selGG.intValue = GUILayout.Toolbar(selGG.intValue, namesG);
                    if (selGG.intValue != selGB)
                        GUIUtility.keyboardControl = 0;

                    EditorGUILayout.EndScrollView();

                    GUILayout.Space(3f);

                    if (GUITools.PlusMinus(true) || goods.arraySize == 0) //if add groups pressed
                        AddGoodsGroup(goods.arraySize);

                    EditorGUILayout.EndHorizontal();

                    SerializedProperty currentGoodsGroup = goods.GetArrayElementAtIndex(selGG.intValue).FindPropertyRelative("goods");
                    SerializedProperty goodName = goods.GetArrayElementAtIndex(selGG.intValue).FindPropertyRelative("name");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(goodName);
                    if (goodName.stringValue == "")
                        goodName.stringValue = "Goods group " + selGG.intValue;

                    GUI.enabled = goods.arraySize > 1;
                    if (GUITools.PlusMinus(false))
                    {
                        goods.DeleteArrayElementAtIndex(selGG.intValue);
                        GroupRemove(selGG.intValue);
                        if (selGG.intValue > 0)
                            selGG.intValue--;
                        GUIUtility.keyboardControl = 0;
                        break;
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    //information with number of goods, sort, expand and collapse
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Number of goods:", currentGoodsGroup.arraySize.ToString());
                    GUILayout.FlexibleSpace();
                    GUI.enabled = currentGoodsGroup.arraySize > 0 && !Application.isPlaying;
                    if (GUILayout.Button(new GUIContent("Sort", "Sort the goods by name alphabetically"), EditorStyles.miniButtonLeft, GUILayout.MinWidth(45f)))
                        SortLists(selGG.intValue);
                    GUI.enabled = true;

                    if (GUILayout.Button(new GUIContent("Find crates", "Find the crates for all goods in this group"), EditorStyles.miniButtonMid))
                    {//if button pressed	
                        bool notFound = false;

                        for (int g = 0; g < currentGoodsGroup.arraySize; g++)
                        {//for all goods

                            if (EditorUtility.DisplayCancelableProgressBar("Finding", "Finding crates", g / currentGoodsGroup.arraySize))
                            {
                                EditorUtility.ClearProgressBar();
                                return;
                            }

                            SerializedProperty cG = currentGoodsGroup.GetArrayElementAtIndex(g);//get the current good
                            SerializedProperty cGC = cG.FindPropertyRelative("itemCrate");

                            cGC.objectReferenceValue = (GameObject)Resources.Load(goodName.stringValue + "/" + namesG[selGG.intValue], typeof(GameObject));

                            if (cGC.objectReferenceValue == null)
                                notFound = true;
                            else//else sort the crate
                                SortCrate(cGC);
                        }//end for all goods
                        EditorUtility.ClearProgressBar();

                        if (notFound)
                            Debug.LogError("Item crate could not be found for one or more goods.\nMake sure that the name of the GameObject is the same and is placed in Resources/" + namesG[selGG.intValue]);
                    }//end if find all crates pressed								

                    GUITools.ExpandCollapse(currentGoodsGroup, "expanded", true);
                    EditorGUILayout.EndHorizontal();

                    scrollPos.G = GUITools.StartScroll(scrollPos.G, smallScroll);

                    if (currentGoodsGroup.arraySize == 0 && GUILayout.Button("Add good"))
                    {//if there are no goods added, have one larger add button
                        currentGoodsGroup.InsertArrayElementAtIndex(0);
                        currentGoodsGroup.GetArrayElementAtIndex(0).FindPropertyRelative("mass").floatValue = 1;
                        currentGoodsGroup.GetArrayElementAtIndex(0).FindPropertyRelative("expanded").boolValue = true;
                        EditLists(true, 0, selGG.intValue, true);
                    }

                    for (int g = 0; g < currentGoodsGroup.arraySize; g++)
                    {//go through all goods, displaying them
                        EditorGUI.indentLevel = 0;
                        SerializedProperty currentGood = currentGoodsGroup.GetArrayElementAtIndex(g);//current good so is shorter

                        EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));

                        if (currentGood.FindPropertyRelative("expanded").boolValue)//if expanded
                            EditorGUILayout.BeginVertical("HelpBox");//show everything in a box together

                        EditorGUILayout.BeginHorizontal();
                        currentGood.FindPropertyRelative("expanded").boolValue = GUITools.TitleButton(new GUIContent(currentGood.FindPropertyRelative("name").stringValue, ""), currentGood.FindPropertyRelative("expanded"), "ControlLabel");

                        GUILayout.FlexibleSpace();//used so options are all at the end

                        EditorGUILayout.LabelField(new GUIContent(selGG.intValue.ToString() + " : " + g, "groupID : itemID"), GUILayout.MaxWidth(50f));//display the groupID and itemID

                        GUI.enabled = g > 0 && !Application.isPlaying;//disable move up if already at the top
                        EditorGUILayout.BeginVertical();//vertical to make the set of buttons central vertically
                        GUILayout.Space(1f);//the space
                        EditorGUILayout.BeginHorizontal();//now needs a horizontal so all the buttons dont follow the vertical
                        if (GUILayout.Button(new GUIContent("\u25B2", "Move up"), EditorStyles.miniButtonLeft))
                        {
                            currentGoodsGroup.MoveArrayElement(g, g - 1);
                            MoveFromPoint(g - 1, selGG.intValue, true);
                            ListShuffle(true, g, g - 1, selGG.intValue);
                        }
                        GUI.enabled = !Application.isPlaying;//set back to enabled if not playing
                        if (GUILayout.Button(new GUIContent("+", "Add good after"), EditorStyles.miniButtonMid))
                        {
                            currentGoodsGroup.InsertArrayElementAtIndex(g + 1);
                            EditLists(true, g + 1, selGG.intValue, true);

                            SerializedProperty inserted = currentGoodsGroup.GetArrayElementAtIndex(g + 1);
                            inserted.FindPropertyRelative("name").stringValue = "Element " + (g + 1);
                            inserted.FindPropertyRelative("expanded").boolValue = currentGood.FindPropertyRelative("expanded").boolValue;
                            inserted.FindPropertyRelative("maxPrice").floatValue = 0;
                            inserted.FindPropertyRelative("basePrice").floatValue = inserted.FindPropertyRelative("minPrice").floatValue = 1;
                            inserted.FindPropertyRelative("mass").floatValue = 1;
                            inserted.FindPropertyRelative("itemCrate").objectReferenceValue = null;
                            inserted.FindPropertyRelative("pausePerUnit").floatValue = currentGood.FindPropertyRelative("pausePerUnit").floatValue;
                            MovePointsAfter(g, selGG.intValue, false);

                            GUIUtility.keyboardControl = 0;
                        }
                        if (GUILayout.Button(new GUIContent("C", "Copy good after"), EditorStyles.miniButtonMid))
                        {
                            currentGoodsGroup.GetArrayElementAtIndex(g).DuplicateCommand();
                            currentGoodsGroup.GetArrayElementAtIndex(g + 1).FindPropertyRelative("name").stringValue += " Copy";
                            EditLists(true, g + 1, selGG.intValue, true);
                            MovePointsAfter(g, selGG.intValue, false);

                            GUIUtility.keyboardControl = 0;
                        }
                        if (GUILayout.Button(new GUIContent("-", "Remove good"), EditorStyles.miniButtonMid))
                        {
                            EditLists(true, g, selGG.intValue, false);

                            currentGoodsGroup.DeleteArrayElementAtIndex(g);
                            MovePointsAfter(g, selGG.intValue, true);
                            break;
                        }
                        GUI.enabled = g < currentGoodsGroup.arraySize - 1 && !Application.isPlaying;//disable if already at the bottom
                        if (GUILayout.Button(new GUIContent("\u25BC", "Move down"), EditorStyles.miniButtonRight))
                        {
                            currentGoodsGroup.MoveArrayElement(g, g + 1);
                            MoveFromPoint(g + 1, selGG.intValue, false);
                            ListShuffle(true, g, g + 1, selGG.intValue);
                        }
                        GUI.enabled = true;//make enabled again
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        if (currentGood.FindPropertyRelative("name").stringValue == "")//make sure that the name isn't blank - may cause problems if it is
                            currentGood.FindPropertyRelative("name").stringValue = "Element " + g;

                        if (currentGood.FindPropertyRelative("expanded").boolValue)
                        {//if is expanded
                            EditorGUI.indentLevel = 1;

                            string unitString = "";
                            if (currentGood.FindPropertyRelative("unit").stringValue.Length > 0)
                                unitString = " (" + currentGood.FindPropertyRelative("unit").stringValue + ")";

                            SerializedProperty name = currentGood.FindPropertyRelative("name");
                            EditorGUILayout.PropertyField(name, new GUIContent("Name", "This is the name of the item"));

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(currentGood.FindPropertyRelative("mass"), new GUIContent("Mass" + unitString, "The mass will affect the unit, and how much can be carried by a trader"));
                            currentGood.FindPropertyRelative("mass").floatValue = Mathf.Max(0.000001f, currentGood.FindPropertyRelative("mass").floatValue);
                            //limit the mass, so is not <=0 so that masses correctly work
                            EditorGUILayout.LabelField("");
                            EditorGUILayout.EndHorizontal();

                            if (pauseOption.intValue == 3)
                            {//if the pause option if for specific items
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(currentGood.FindPropertyRelative("pausePerUnit"), new GUIContent("Pause time", "Set how long a trader needs to pause for per unit of this item"));
                                EditorGUILayout.LabelField("");
                                EditorGUILayout.EndHorizontal();

                                if (currentGood.FindPropertyRelative("pausePerUnit").floatValue < 0)
                                    currentGood.FindPropertyRelative("pausePerUnit").floatValue = 0;
                            }//end pause time

                            if (pickUp.boolValue)
                            {//only show these options if it is possible to collect items
                                EditorGUILayout.BeginHorizontal();
                                SerializedProperty itemCrate = currentGood.FindPropertyRelative("itemCrate");

                                if (itemCrate.objectReferenceValue == null)
                                    GUI.color = Color.red;//make red if null

                                EditorGUILayout.PropertyField(itemCrate, new GUIContent("Item crate", "This is what the item looks like when you see it in the game, so is likely to be in a box or crate"));//item crate field
                                GUI.color = Color.white;

                                if (GUILayout.Button(new GUIContent("Find crate", "If the item has the same name as the crate and is in Resources/" + namesG[selGG.intValue] + ", press this to find it"), EditorStyles.miniButton, GUILayout.MaxWidth(75f)))
                                {//find crate button for quick finding

                                    itemCrate.objectReferenceValue = (GameObject)Resources.Load(goodName.stringValue + "/" + name.stringValue, typeof(GameObject));

                                    if (itemCrate.objectReferenceValue == null)
                                        Debug.LogError("Item crate could not be found.\nMake sure that the name of the GameObject is the same and is placed in Resources/" + namesG[selGG.intValue]);
                                    else//else make sure has Item component and tag
                                        SortCrate(itemCrate);//sort out the item crate
                                }//end if find crate pressed
                                EditorGUILayout.EndHorizontal();
                            }

                            int currencyID = currentGood.FindPropertyRelative("currencyID").intValue;

                            if (!expendable)
                            {//only show the prices if not expendable
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Prices", EditorStyles.boldLabel);//bold prices label for sub section

                                currencyID = EditorGUILayout.Popup(currencyID, controllerNormal.currencyNames, "MiniPullDown");

                                EditorGUILayout.EndHorizontal();
                                EditorGUI.indentLevel = 2;
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(currentGood.FindPropertyRelative("basePrice"), new GUIContent("Base price", "If a trade post has the average number of this item, this is the price. The prices are set against this"));
                                EditorGUILayout.LabelField("");//blank label so does not cover whole width
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(currentGood.FindPropertyRelative("minPrice"), new GUIContent("Min price", "This is the minimum price the item can be"));
                                EditorGUILayout.PropertyField(currentGood.FindPropertyRelative("maxPrice"), new GUIContent("Max price", "This is the highest price the item can be"));
                                EditorGUILayout.EndHorizontal();
                            }//end if not expendable

                            //get the prices
                            float mi = currentGood.FindPropertyRelative("minPrice").floatValue;
                            float ma = currentGood.FindPropertyRelative("maxPrice").floatValue;
                            float ba = currentGood.FindPropertyRelative("basePrice").floatValue;

                            if (ba < 1)
                                ba = 1;
                            //set base price to be > 1
                            if (mi < 1)
                                mi = 1;
                            else if (mi > ba)
                                mi = ba;
                            //set min to be > 1 and < base
                            if (ma < ba)
                                ma = ba;
                            //set mas to be > base

                            //sort out the decimal places
                            if (currencyID < controllerNormal.currencies.Count)
                            {
                                int decimals = controllerNormal.currencies[currencyID].decimals;
                                mi = (float)System.Math.Round(mi, decimals, System.MidpointRounding.AwayFromZero);
                                ma = (float)System.Math.Round(ma, decimals, System.MidpointRounding.AwayFromZero);
                                ba = (float)System.Math.Round(ba, decimals, System.MidpointRounding.AwayFromZero);
                            }
                            // else
                            //    currencyID = Mathf.Max(0, currencyID-1);

                            currentGood.FindPropertyRelative("currencyID").intValue = currencyID;

                            //set the prices
                            currentGood.FindPropertyRelative("minPrice").floatValue = mi;
                            currentGood.FindPropertyRelative("maxPrice").floatValue = ma;
                            currentGood.FindPropertyRelative("basePrice").floatValue = ba;

                            EditorGUILayout.EndVertical();
                        }//end if expanded
                    }//end for all goods
                    EditorGUI.indentLevel = 0;
                    if (currentGoodsGroup.arraySize > 0)
                        EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));
                    if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
                        EditorGUILayout.EndScrollView();
                    break;
                #endregion

                #region manufacturing
                case 3:
                    EditorGUI.indentLevel = 0;
                    int[] groupLengthsM = new int[manufacture.arraySize];//a running total of the number of processes
                    int manufactureCount = 0;//the total number of manufacturing processes;

                    controllerNormal.ManufactureMass();

                    SerializedProperty selMG = controllerSO.FindProperty("selected").FindPropertyRelative("MG");

                    for (int m = 0; m < manufacture.arraySize; m++)
                    {//go through all manufacture
                        manufactureCount += manufacture.GetArrayElementAtIndex(m).FindPropertyRelative("manufacture").arraySize;
                        if (m == 0)
                            groupLengthsM[m] = 0;
                        else
                        {
                            int prev = manufacture.GetArrayElementAtIndex(m - 1).FindPropertyRelative("manufacture").arraySize;
                            if (m == 1)
                                groupLengthsM[m] = prev;
                            else
                                groupLengthsM[m] = groupLengthsM[m - 1] + prev;
                        }
                    }//end for manufacture

                    EditorGUILayout.LabelField(new GUIContent("Total number", "The total number of manufacturing processes that have been defined across all groups"), new GUIContent(manufactureCount.ToString(), "The total number of manufacturing processes that have been defined across all groups"));

                    EditorGUILayout.BeginHorizontal();//have a toolbar with the different group names on
                    scrollPos.MG = EditorGUILayout.BeginScrollView(scrollPos.MG, GUILayout.Height(40f));

                    string[] namesM = new string[manufacture.arraySize];
                    for (int n = 0; n < manufacture.arraySize; n++)
                        namesM[n] = manufacture.GetArrayElementAtIndex(n).FindPropertyRelative("name").stringValue;

                    int selMB = selMG.intValue;
                    selMG.intValue = GUILayout.Toolbar(selMG.intValue, namesM);

                    if (selMG.intValue != selMB)
                        GUIUtility.keyboardControl = 0;

                    EditorGUILayout.EndScrollView();

                    GUILayout.Space(3f);
                    if (GUITools.PlusMinus(true) || manufacture.arraySize == 0) //if add groups pressed
                        AddManGroup(manufacture.arraySize);
                    EditorGUILayout.EndHorizontal();

                    SerializedProperty currentManGroup = manufacture.GetArrayElementAtIndex(selMG.intValue).FindPropertyRelative("manufacture");
                    SerializedProperty manName = manufacture.GetArrayElementAtIndex(selMG.intValue).FindPropertyRelative("name");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(manName);
                    if (manName.stringValue == "")
                        manName.stringValue = "Manufacture group " + selMG.intValue;

                    GUI.enabled = manufacture.arraySize > 1;
                    if (GUITools.PlusMinus(false))
                    {
                        manufacture.DeleteArrayElementAtIndex(selMG.intValue);
                        GroupRemove(postScripts, "manufacture", selMG.intValue);
                        GroupRemove(traderScripts, "manufacture", selMG.intValue);
                        if (selMG.intValue > 0)
                            selMG.intValue--;
                        GUIUtility.keyboardControl = 0;
                        break;
                    }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    //information with number of processes, expand and collapse
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Number of processes:", currentManGroup.arraySize.ToString());
                    GUILayout.FlexibleSpace();

                    GUI.enabled = manufacture.arraySize > 0 && CheckMan();
                    if (GUILayout.Button(new GUIContent("Check", "Check all manufacturing at trade posts to give an estimate on changes of numbers of each item"), EditorStyles.miniButtonLeft, GUILayout.MinWidth(45f)))
                    {//CheckManufacturing (manufactureCount, groupLengthsM, groupLengthsG, manufacture);
                        CheckManufacturingWindow window = (CheckManufacturingWindow)EditorWindow.GetWindow(typeof(CheckManufacturingWindow), true, "Manufacturing check");
                        window.position = new Rect(Screen.currentResolution.width / 2 - 275, Screen.currentResolution.height / 2 - 200, 550, 400);
                        window.minSize = new Vector2(550, 400);
                        window.maxSize = new Vector2(550, Screen.currentResolution.height);

                    }
                    GUI.enabled = true;

                    GUITools.ExpandCollapse(currentManGroup, "expanded", true);
                    EditorGUILayout.EndHorizontal();

                    scrollPos.M = GUITools.StartScroll(scrollPos.M, smallScroll);

                    if (currentManGroup.arraySize == 0 && GUILayout.Button("Add process"))
                    {//if there are no processes added, have one larger add button
                        currentManGroup.InsertArrayElementAtIndex(0);
                        currentManGroup.GetArrayElementAtIndex(0).FindPropertyRelative("expanded").boolValue = true;
                        currentManGroup.GetArrayElementAtIndex(0).FindPropertyRelative("needing").arraySize = currentManGroup.GetArrayElementAtIndex(0).FindPropertyRelative("making").arraySize = 0;
                        EditLists(false, 0, selMG.intValue, true);
                        if (allNames.Count == 0)//if there are no goods, then show error
                            Debug.LogError("There are no possible types to manufacture.");
                    }

                    for (int m = 0; m < currentManGroup.arraySize; m++)
                    {
                        EditorGUI.indentLevel = 0;
                        SerializedProperty currentManufacture = currentManGroup.GetArrayElementAtIndex(m);

                        EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));

                        if (currentManufacture.FindPropertyRelative("expanded").boolValue)
                            EditorGUILayout.BeginVertical("HelpBox");

                        EditorGUILayout.BeginHorizontal();
                        currentManufacture.FindPropertyRelative("expanded").boolValue = GUITools.TitleButton(new GUIContent(currentManufacture.FindPropertyRelative("name").stringValue, currentManufacture.FindPropertyRelative("tooltip").stringValue), currentManufacture.FindPropertyRelative("expanded"), "ControlLabel");

                        GUILayout.FlexibleSpace();//used so options are all at the end

                        EditorGUILayout.LabelField(new GUIContent(selMG.intValue.ToString() + " : " + m, "groupID : itemID"), GUILayout.MaxWidth(50f));//display the groupID and itemID

                        GUI.enabled = m > 0 && !Application.isPlaying;//disable move up if already at the top
                        EditorGUILayout.BeginVertical();//vertical to make the set of buttons central vertically
                        GUILayout.Space(1f);//the space
                        EditorGUILayout.BeginHorizontal();//now needs a horizontal so all the buttons dont follow the vertical
                        if (GUILayout.Button(new GUIContent("\u25B2", "Move up"), EditorStyles.miniButtonLeft))
                        {
                            currentManGroup.MoveArrayElement(m, m - 1);
                            ListShuffle(false, m, m - 1, selMG.intValue);
                        }
                        GUI.enabled = !Application.isPlaying;//set back to enabled if not playing
                        if (GUILayout.Button(new GUIContent("+", "Add process after"), EditorStyles.miniButtonMid))
                        {
                            currentManGroup.InsertArrayElementAtIndex(m + 1);
                            EditLists(false, m + 1, selMG.intValue, true);

                            SerializedProperty inserted = currentManGroup.GetArrayElementAtIndex(m + 1);
                            inserted.FindPropertyRelative("name").stringValue = "Element " + (m + 1);
                            inserted.FindPropertyRelative("expanded").boolValue = currentManGroup.GetArrayElementAtIndex(m).FindPropertyRelative("expanded").boolValue;
                            inserted.FindPropertyRelative("needing").arraySize = inserted.FindPropertyRelative("making").arraySize = 0;
                            GUIUtility.keyboardControl = 0;
                        }
                        if (GUILayout.Button(new GUIContent("C", "Copy process after"), EditorStyles.miniButtonMid))
                        {
                            currentManGroup.GetArrayElementAtIndex(m).DuplicateCommand();
                            currentManGroup.GetArrayElementAtIndex(m + 1).FindPropertyRelative("name").stringValue += " Copy";

                            EditLists(false, m + 1, selMG.intValue, true);
                            GUIUtility.keyboardControl = 0;
                        }
                        if (GUILayout.Button(new GUIContent("-", "Remove process"), EditorStyles.miniButtonMid))
                        {
                            currentManGroup.DeleteArrayElementAtIndex(m);
                            EditLists(false, m, selMG.intValue, false);
                            break;
                        }
                        GUI.enabled = m < currentManGroup.arraySize - 1 && !Application.isPlaying;//disable if already at the bottom
                        if (GUILayout.Button(new GUIContent("\u25BC", "Move down"), EditorStyles.miniButtonRight))
                        {
                            currentManGroup.MoveArrayElement(m, m + 1);
                            ListShuffle(false, m, m + 1, selMG.intValue);
                        }
                        GUI.enabled = true;//make enabled again
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        if (currentManufacture.FindPropertyRelative("name").stringValue == "")//make sure that the name isn't blank - may cause problems if it is
                            currentManufacture.FindPropertyRelative("name").stringValue = "Element " + m;

                        if (currentManufacture.FindPropertyRelative("expanded").boolValue)
                        {
                            EditorGUI.indentLevel = 1;
                            EditorGUILayout.PropertyField(currentManufacture.FindPropertyRelative("name"), new GUIContent("Name", "This is the name of the process"));

                            EditorGUILayout.LabelField(new GUIContent(UnitString(currentManufacture.FindPropertyRelative("needingMass").floatValue, false) + " \u25B6 " + UnitString(currentManufacture.FindPropertyRelative("makingMass").floatValue, false),
                                                "Shows the mass conversion of the needing and making items"));

                            EditorGUI.indentLevel = 1;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Needing", EditorStyles.boldLabel);
                            if (GUITools.PlusMinus(true))
                            {//add needing element
                                controllerNormal.manufacture[selMG.intValue].manufacture[m].needing.Add(new NeedMake());
                                controllerNormal.manufacture[selMG.intValue].manufacture[m].needing[controllerNormal.manufacture[selMG.intValue].manufacture[m].needing.Count - 1].number = 1;
                            }
                            EditorGUILayout.EndHorizontal();

                            ShowNM(currentManufacture, true, groupLengthsG, allNames.ToArray());

                            EditorGUI.indentLevel = 1;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Making", EditorStyles.boldLabel);
                            if (GUITools.PlusMinus(true))
                            {//add making element
                                controllerNormal.manufacture[selMG.intValue].manufacture[m].making.Add(new NeedMake());
                                controllerNormal.manufacture[selMG.intValue].manufacture[m].making[controllerNormal.manufacture[selMG.intValue].manufacture[m].making.Count - 1].number = 1;
                            }
                            EditorGUILayout.EndHorizontal();

                            ShowNM(currentManufacture, false, groupLengthsG, allNames.ToArray());

                            EditorGUILayout.EndVertical();
                        }//end if expanded
                    }//end for all manufacture
                    EditorGUI.indentLevel = 0;
                    if (currentManGroup.arraySize > 0)
                        EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));
                    if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
                        EditorGUILayout.EndScrollView();
                    break;
                    #endregion
            }//end switch
            controllerSO.ApplyModifiedProperties();//needs to apply modified properties

            for (int p = 0; p < postScripts.Length; p++)
                postScripts[p].ApplyModifiedProperties();
            if (!expendable)
                for (int t = 0; t < traderScripts.Length; t++)
                    traderScripts[t].ApplyModifiedProperties();
            for (int s = 0; s < spawnerScripts.Length; s++)
                spawnerScripts[s].ApplyModifiedProperties();
        }//end OnInspectorGUI

        ///this is to go through the manufacturing lists made, and make sure that each itemID is still pointing to the correct one
        void SortLists(int groupNo)
        {
            Goods[] before = controllerNormal.goods[groupNo].goods.ToArray();
            controllerNormal.goods[groupNo].goods.Sort();

            for (int m1 = 0; m1 < controllerNormal.manufacture.Count; m1++)
            {//go through all manufacturing groups
                for (int m2 = 0; m2 < controllerNormal.manufacture[m1].manufacture.Count; m2++)
                {//go through all manufacturing processes
                    for (int n = 0; n < controllerNormal.manufacture[m1].manufacture[m2].needing.Count; n++)
                    {//go through all needing
                        if (controllerNormal.manufacture[m1].manufacture[m2].needing[n].groupID == groupNo)//if is in the group that had been sorted
                            controllerNormal.manufacture[m1].manufacture[m2].needing[n].itemID = controllerNormal.goods[groupNo].goods.FindIndex(x => x.name == before[controllerNormal.manufacture[m1].manufacture[m2].needing[n].itemID].name);
                    }//end for needing
                    for (int n = 0; n < controllerNormal.manufacture[m1].manufacture[m2].making.Count; n++)
                    {//go through all making
                        if (controllerNormal.manufacture[m1].manufacture[m2].making[n].groupID == groupNo)//if is in the group that had been sorted
                            controllerNormal.manufacture[m1].manufacture[m2].making[n].itemID = controllerNormal.goods[groupNo].goods.FindIndex(x => x.name == before[controllerNormal.manufacture[m1].manufacture[m2].making[n].itemID].name);
                    }//end for making
                }//end for all manufacturing processes
            }//end for all manufacturing groups

            for (int p = 0; p < controllerNormal.postScripts.Length; p++)
            {//go through all posts
                StockGroup groupStock = controllerNormal.postScripts[p].stock[groupNo];//the stock group at the post
                for (int s = 0; s < groupStock.stock.Count; s++)//go throigh all items
                    groupStock.stock[s].name = before[s].name;//and set the name to what it was before sorting
                groupStock.stock.Sort();//now sort the items, and will be sorted the same as the other stock list
            }//end for all posts

            for (int t = 0; t < controllerNormal.traderScripts.Length; t++)//go through all traders
                SortLists(controllerNormal.traderScripts[t].items[groupNo], before);

            for (int s = 0; s < controllerNormal.spawners.Length; s++)//go through all spawners
                SortLists(controllerNormal.spawners[s].items[groupNo], before);

        }//end SortLists

        void SortLists(ItemGroup ig, Goods[] before)
        {//sorts the items in traders and spawners
            for (int a = 0; a < ig.items.Count; a++)//go through all allow
                ig.items[a].name = before[a].name;//and set the name to what it was before sorting
            ig.items.Sort();
        }//end SortLists for traders and spawners

        ///when a good gets moved up or down, the items in manufacturing also need changing
        ///itemNo is the destination array number of the changed
        void MoveFromPoint(int itemNo, int groupNo, bool up)
        {
            for (int m1 = 0; m1 < manufacture.arraySize; m1++)
            {//go through manufacturing groups
                SerializedProperty manufactureGroup = manufacture.GetArrayElementAtIndex(m1).FindPropertyRelative("manufacture");
                for (int m2 = 0; m2 < manufactureGroup.arraySize; m2++)
                {//go through processes
                    SerializedProperty currentManufacture = manufactureGroup.GetArrayElementAtIndex(m2);

                    for (int n = 0; n < currentManufacture.FindPropertyRelative("needing").arraySize; n++)
                    {
                        SerializedProperty currentNeeding = currentManufacture.FindPropertyRelative("needing").GetArrayElementAtIndex(n);
                        if (currentNeeding.FindPropertyRelative("groupID").intValue == groupNo)//if is in the group that has been changed
                            ManufactureMove(currentNeeding.FindPropertyRelative("itemID"), itemNo, up);
                    }//end for sort all needing
                    for (int n = 0; n < currentManufacture.FindPropertyRelative("making").arraySize; n++)
                    {
                        SerializedProperty currentMaking = currentManufacture.FindPropertyRelative("making").GetArrayElementAtIndex(n);
                        if (currentMaking.FindPropertyRelative("groupID").intValue == groupNo)
                            ManufactureMove(currentMaking.FindPropertyRelative("itemID"), itemNo, up);
                    }//end for sort all making

                }//end for manufacturing processes
            }//end for all manufacturing groups
        }//end MoveFromPoint

        ///move the manufacturing items 
        void ManufactureMove(SerializedProperty currentNMID, int itemNo, bool up)
        {
            if ((up && currentNMID.intValue == itemNo + 1) || (!up && currentNMID.intValue == itemNo))
                //move up
                currentNMID.intValue--;
            else if ((up && currentNMID.intValue == itemNo) || (!up && currentNMID.intValue == itemNo - 1))
                //move down
                currentNMID.intValue++;
        }//end ManufactureMove

        ///removing a single manufacturing item
        void MovePointsAfter(int itemAfter, int groupNo, bool removing)
        {
            for (int m1 = 0; m1 < manufacture.arraySize; m1++)
            {//for manufacture groups
                for (int m2 = 0; m2 < manufacture.GetArrayElementAtIndex(m1).FindPropertyRelative("manufacture").arraySize; m2++)
                {//for manufacture processes
                    SerializedProperty currentManufacture = manufacture.GetArrayElementAtIndex(m1).FindPropertyRelative("manufacture").GetArrayElementAtIndex(m2);

                    for (int n = 0; n < currentManufacture.FindPropertyRelative("needing").arraySize; n++)
                    {
                        SerializedProperty currentNeeding = currentManufacture.FindPropertyRelative("needing").GetArrayElementAtIndex(n);
                        if (currentNeeding.FindPropertyRelative("groupID").intValue == groupNo)
                        {//check in the correct group
                            if (removing)
                                RemoveUp(currentNeeding.FindPropertyRelative("itemID"), itemAfter);
                            else
                                MoveDown(currentNeeding.FindPropertyRelative("itemID"), itemAfter);
                        }
                    }//end for sort all needing
                    for (int n = 0; n < currentManufacture.FindPropertyRelative("making").arraySize; n++)
                    {
                        SerializedProperty currentMaking = currentManufacture.FindPropertyRelative("making").GetArrayElementAtIndex(n);
                        if (currentMaking.FindPropertyRelative("groupID").intValue == groupNo)
                        {
                            if (removing)
                                RemoveUp(currentMaking.FindPropertyRelative("itemID"), itemAfter);
                            else
                                MoveDown(currentMaking.FindPropertyRelative("itemID"), itemAfter);
                        }
                    }//end for sort all making
                }//end for all manufacture processes
            }//end for all manufacture groups
        }//end MovePointsAfter

        ///decrease the values of all of the other items, and set to -1 if using deleted item
        void RemoveUp(SerializedProperty currentNMID, int itemRemoved)
        {
            if (currentNMID.intValue == itemRemoved)
            {
                currentNMID.intValue = -1;
                Debug.LogError("Some elements in manufacturing processes are undefined");
            }
            else if (currentNMID.intValue > itemRemoved)
                currentNMID.intValue--;
        }//end RemoveUp

        ///increase the value of items after the inserted item
        void MoveDown(SerializedProperty currentNMID, int itemAfter)
        {
            if (currentNMID.intValue > itemAfter)
                currentNMID.intValue++;
        }//end MoveDown

        ///show the needing or making elements
        void ShowNM(SerializedProperty currentManufacture, bool needing, int[] groupLengthsG, string[] allNames)
        {
            SerializedProperty currentNM;
            if (needing)
                currentNM = currentManufacture.FindPropertyRelative("needing");
            else
                currentNM = currentManufacture.FindPropertyRelative("making");
            for (int nm = 0; nm < currentNM.arraySize; nm++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel = 2;
                if (IsDuplicate(currentManufacture, nm, needing) || currentNM.GetArrayElementAtIndex(nm).FindPropertyRelative("itemID").intValue == -1)
                    GUI.color = Color.red;
                int selected = ConvertToSelected(currentNM.GetArrayElementAtIndex(nm), groupLengthsG);
                selected = EditorGUILayout.Popup(selected, allNames, "DropDownButton");
                ConvertFromSelected(currentNM.GetArrayElementAtIndex(nm), groupLengthsG, selected);

                EditorGUILayout.PropertyField(currentNM.GetArrayElementAtIndex(nm).FindPropertyRelative("number"), new GUIContent("Number"));

                if (currentNM.GetArrayElementAtIndex(nm).FindPropertyRelative("number").intValue < 1)
                    currentNM.GetArrayElementAtIndex(nm).FindPropertyRelative("number").intValue = 1;
                GUI.color = Color.white;

                if (GUITools.PlusMinus(false))
                    currentNM.DeleteArrayElementAtIndex(nm);
                EditorGUILayout.EndHorizontal();
            }//end for all NM
        }//end ShowNM

        ///check if a needing or making element is duplicated
        bool IsDuplicate(SerializedProperty currentManufacture, int check, bool needing)
        {
            SerializedProperty currentNeeding = currentManufacture.FindPropertyRelative("needing");
            SerializedProperty currentMaking = currentManufacture.FindPropertyRelative("making");

            SerializedProperty currentCheck;

            if (needing)
                currentCheck = currentNeeding.GetArrayElementAtIndex(check);
            else
                currentCheck = currentMaking.GetArrayElementAtIndex(check);

            for (int n = 0; n < currentNeeding.arraySize; n++)
            {
                SerializedProperty currentN = currentNeeding.GetArrayElementAtIndex(n);
                if (needing)
                {//if is originally in needing
                    if (n != check) //check that is not checking itself
                        if (IsDuplicate(currentN, currentCheck))
                            return true;
                }
                else//if is from making, can check all, but needs to be from making if (IsDuplicate (currentN, currentCheck))
    if (IsDuplicate(currentN, currentCheck))
                    return true;
            }//check in needing

            for (int m = 0; m < currentMaking.arraySize; m++)
            {
                SerializedProperty currentM = currentMaking.GetArrayElementAtIndex(m);
                if (!needing)
                {//if is originally in making
                    if (m != check)//check that is not checking itself
                        if (IsDuplicate(currentM, currentCheck))
                            return true;
                }
                else//if is from needing, can check all, but needs to be from needing if (IsDuplicate (currentM, currentCheck))
    if (IsDuplicate(currentM, currentCheck))
                    return true;
            }//check in making

            return false;
        }//end IsDuplicate

        ///check the current item and the checking item
        bool IsDuplicate(SerializedProperty currentNM, SerializedProperty currentCheck)
        {
            if (currentNM.FindPropertyRelative("itemID").intValue == currentCheck.FindPropertyRelative("itemID").intValue &&
                    currentNM.FindPropertyRelative("groupID").intValue == currentCheck.FindPropertyRelative("groupID").intValue)
                return true;
            return false;
        }//end IsDuplicate

        ///convert the itemID and groupID to a single int for manufacture selection
        public int ConvertToSelected(SerializedProperty currentNM, int[] lengths)
        {
            if (currentNM.FindPropertyRelative("itemID").intValue == -1)//if undefined, return -1
                return -1;
            int selected = 0;
            for (int g = 0; g < currentNM.FindPropertyRelative("groupID").intValue; g++)
                selected += lengths[g];
            selected += currentNM.FindPropertyRelative("itemID").intValue;
            return selected;
        }//end ConvertToSelected

        ///convert from single int to a groupID and an itemID
        void ConvertFromSelected(SerializedProperty currentNM, int[] lengths, int selected)
        {
            if (selected == -1)
            {
                currentNM.FindPropertyRelative("itemID").intValue = currentNM.FindPropertyRelative("groupID").intValue = -1;
                return;//no need to go through the rest, so return
            }//end if -1

            int groupNo = 0;
            while (selected >= lengths[groupNo])
            {
                selected -= lengths[groupNo];
                groupNo++;
            }
            currentNM.FindPropertyRelative("itemID").intValue = selected;
            currentNM.FindPropertyRelative("groupID").intValue = groupNo;
        }//end ConvertFromSelected

        ///remove a goods group, sorting all manufacture pointers
        void GroupRemove(int groupNo)
        {
            #region manufacture
            for (int m1 = 0; m1 < manufacture.arraySize; m1++)
            {//manufacture groups
                for (int m2 = 0; m2 < manufacture.GetArrayElementAtIndex(m1).FindPropertyRelative("manufacture").arraySize; m2++)
                {//manufacture processes
                    SerializedProperty currentManufacture = manufacture.GetArrayElementAtIndex(m1).FindPropertyRelative("manufacture").GetArrayElementAtIndex(m2);

                    for (int n = 0; n < currentManufacture.FindPropertyRelative("needing").arraySize; n++)
                    {
                        SerializedProperty currentNeeding = currentManufacture.FindPropertyRelative("needing").GetArrayElementAtIndex(n);
                        if (currentNeeding.FindPropertyRelative("groupID").intValue == groupNo)
                        {//if same group as removed
                            currentNeeding.FindPropertyRelative("groupID").intValue = -1;
                            currentNeeding.FindPropertyRelative("itemID").intValue = -1;
                            Debug.LogError("Some elements in manufacturing processes are undefined");
                        }
                        else if (currentNeeding.FindPropertyRelative("groupID").intValue > groupNo)//if greater than removed, then reduce selected group
                            currentNeeding.FindPropertyRelative("groupID").intValue--;
                    }//end for all needing

                    for (int n = 0; n < currentManufacture.FindPropertyRelative("making").arraySize; n++)
                    {
                        SerializedProperty currentMaking = currentManufacture.FindPropertyRelative("making").GetArrayElementAtIndex(n);
                        if (currentMaking.FindPropertyRelative("groupID").intValue == groupNo)
                        {//if same group as removed
                            currentMaking.FindPropertyRelative("groupID").intValue = -1;
                            currentMaking.FindPropertyRelative("itemID").intValue = -1;
                            Debug.LogError("Some elements in manufacturing processes are undefined");
                        }
                        else if (currentMaking.FindPropertyRelative("groupID").intValue > groupNo)//if greater than removed, then reduce selected group
                            currentMaking.FindPropertyRelative("groupID").intValue--;
                    }//end for all making
                }//end for all manufacture processes
            }//end for all manufacture groups
            #endregion

            GroupRemove(postScripts, "stock", groupNo);
            GroupRemove(traderScripts, "items", groupNo);
            GroupRemove(spawnerScripts, "items", groupNo);
        }//end GroupRemove

        ///goes through all items in option, and removes an element in the property array
        void GroupRemove(SerializedObject[] options, string property, int groupNo)
        {
            for (int o = 0; o < options.Length; o++)
            {//for all options
             //options [o].Update ();
                options[o].FindProperty(property).DeleteArrayElementAtIndex(groupNo);
                //options [o].ApplyModifiedProperties ();
            }//end for options
        }//end GroupRemove

        ///called when a good or manufacturing process is moved
        void ListShuffle(bool goods, int moveFrom, int moveTo, int groupNo)
        {
            for (int p = 0; p < postScripts.Length; p++)
            {//go through all posts
                if (goods)
                    postScripts[p].FindProperty("stock").GetArrayElementAtIndex(groupNo).FindPropertyRelative("stock").MoveArrayElement(moveFrom, moveTo);
                else
                    postScripts[p].FindProperty("manufacture").MoveArrayElement(moveFrom, moveTo);
            }//end for posts
            for (int t = 0; t < traderScripts.Length; t++)
            {//go through all traders
                if (goods)
                    traderScripts[t].FindProperty("items").GetArrayElementAtIndex(groupNo).FindPropertyRelative("items").MoveArrayElement(moveFrom, moveTo);
                else
                    traderScripts[t].FindProperty("manufacture").MoveArrayElement(moveFrom, moveTo);
            }//end for traders
            if (goods)
            {//only need to sort spawners if goods moved
                for (int s = 0; s < spawnerScripts.Length; s++)//go through all spawners
                    spawnerScripts[s].FindProperty("items").GetArrayElementAtIndex(groupNo).FindPropertyRelative("items").MoveArrayElement(moveFrom, moveTo);
            }//end if goods moved
        }//end ListShuffle

        /// <summary>
        /// called when a good is added, copied or removed
        /// </summary>
        /// <param name="goods"> if goods or manufacturing</param>
        /// <param name="point"> int of the location</param>
        /// <param name="groupNo"> group number of the added</param>
        /// <param name="adding"> whether adding or removing</param>
        void EditLists(bool goods, int point, int groupNo, bool adding)
        {
            for (int p = 0; p < postScripts.Length; p++)
            {//go through all posts
                if (goods)
                {
                    SerializedProperty stock = postScripts[p].FindProperty("stock").GetArrayElementAtIndex(groupNo).FindPropertyRelative("stock");
                    if (adding)
                    {
                        stock.InsertArrayElementAtIndex(point);
                        SerializedProperty inserted = stock.GetArrayElementAtIndex(point);
                        inserted.FindPropertyRelative("buy").boolValue = inserted.FindPropertyRelative("sell").boolValue = true;
                        inserted.FindPropertyRelative("number").intValue = inserted.FindPropertyRelative("min").intValue =
    inserted.FindPropertyRelative("max").intValue = 0;
                        inserted.FindPropertyRelative("minMax").boolValue = inserted.FindPropertyRelative("minMax").boolValue = false;
                    }
                    else
                        stock.DeleteArrayElementAtIndex(point);
                }
                else
                {//if manufacturing
                    SerializedProperty manufacturing = postScripts[p].FindProperty("manufacture").GetArrayElementAtIndex(groupNo).FindPropertyRelative("manufacture");
                    if (adding)
                    {
                        manufacturing.InsertArrayElementAtIndex(point);
                        manufacturing.GetArrayElementAtIndex(point).FindPropertyRelative("enabled").boolValue = false;
                    }
                    else
                        manufacturing.DeleteArrayElementAtIndex(point);
                }//else manufacturing
            }//end for all posts
            for (int t = 0; t < traderScripts.Length; t++)
            {//go through all traders
                if (goods)
                    EditLists(traderScripts[t], groupNo, adding, point);
                else
                {//if manufacturing
                    SerializedProperty manufacturing = traderScripts[t].FindProperty("manufacture").GetArrayElementAtIndex(groupNo).FindPropertyRelative("manufacture");
                    if (adding)
                    {
                        manufacturing.InsertArrayElementAtIndex(point);
                        manufacturing.GetArrayElementAtIndex(point).FindPropertyRelative("enabled").boolValue = false;
                    }
                    else
                        manufacturing.DeleteArrayElementAtIndex(point);
                }//end else manufacturing
            }//end for all traders
            if (goods)
            {//only need to edit spawners if goods edited
                for (int s = 0; s < spawnerScripts.Length; s++)//go through all spawners
                    EditLists(spawnerScripts[s], groupNo, adding, point);
            }//end if goods
        }//end EditLists

        void EditLists(SerializedObject ts, int groupNo, bool adding, int point)
        {//edit the goods lists for traders and spawners
            SerializedProperty items = ts.FindProperty("items").GetArrayElementAtIndex(groupNo).FindPropertyRelative("items");
            if (adding)
            {
                items.InsertArrayElementAtIndex(point);
                items.GetArrayElementAtIndex(point).FindPropertyRelative("enabled").boolValue = true;
            }
            else
                items.DeleteArrayElementAtIndex(point);
        }//end EditLists traders and spawners goods

        ///go through all of the posts, and get the number of each item available
        string[][] GetItemNumbers(List<GoodsTypes> goods, out long total)
        {
            string[][] itemNumbers = new string[goods.Count][];
            int[][] postCount = new int[goods.Count][];
            int[][] totals = new int[goods.Count][];
            total = 0;

            for (int g = 0; g < goods.Count; g++)
            {//go through all groups
                itemNumbers[g] = new string[goods[g].goods.Count];
                postCount[g] = new int[goods[g].goods.Count];
                totals[g] = new int[goods[g].goods.Count];
                for (int s = 0; s < itemNumbers[g].Length; s++)
                {//go through all stock
                    if (!Application.isPlaying)
                    {//if is playing, then can use averages to get the numbers instead of going through all of the posts
                        for (int p = 0; p < controllerNormal.postScripts.Length; p++)
                        {//go through all posts
                            Stock current = controllerNormal.postScripts[p].stock[g].stock[s];
                            if (current.buy)
                            {
                                postCount[g][s]++;//increase the post count by 1
                                totals[g][s] += current.number;
                                total += current.number;
                            }//end if enabled at post
                        }//end for all posts
                    }
                    else
                    {//end if not playing, else use averages
                        postCount[g][s] = controllerNormal.goods[g].goods[s].postCount;
                        totals[g][s] = (int)(controllerNormal.goods[g].goods[s].average * postCount[g][s]);
                        total += totals[g][s];
                    }//end else if playing
                    itemNumbers[g][s] = "" + postCount[g][s] + ", " + totals[g][s];
                }//end for all stock
            }//end for all groups
            return itemNumbers;
        }//end GetItemNumbers

        ///add a goods group
        void AddGoodsGroup(int loc)
        {
            for (int p = 0; p < postScripts.Length; p++)
            {//go through posts, adding
                SerializedProperty stock = postScripts[p].FindProperty("stock");
                stock.InsertArrayElementAtIndex(loc);
                stock.GetArrayElementAtIndex(loc).FindPropertyRelative("stock").arraySize = 0;
            }//end for posts
            for (int t = 0; t < traderScripts.Length; t++) //go through traders, adding
                AddGoodsGroup(traderScripts[t], loc);

            for (int s = 0; s < spawnerScripts.Length; s++) //go through spawners, adding
                AddGoodsGroup(spawnerScripts[s], loc);

            goods.InsertArrayElementAtIndex(loc);
            SerializedProperty newGood = goods.GetArrayElementAtIndex(loc);
            newGood.FindPropertyRelative("goods").arraySize = 0;
            newGood.FindPropertyRelative("name").stringValue = "Goods group " + loc;
            GUIUtility.keyboardControl = 0;
            controllerSO.FindProperty("selected").FindPropertyRelative("GG").intValue = loc;
        }//end AddGoodsGroup

        void AddGoodsGroup(SerializedObject ts, int loc)
        {//add a goods group to traders and spawners
            SerializedProperty items = ts.FindProperty("items");
            items.InsertArrayElementAtIndex(loc);
            items.GetArrayElementAtIndex(loc).FindPropertyRelative("items").arraySize = 0;
        }//end AddGoodsGroup traders and spawners

        ///add a manufacturing group
        void AddManGroup(int loc)
        {
            AddManGroup(loc, postScripts);
            AddManGroup(loc, traderScripts);
            manufacture.InsertArrayElementAtIndex(loc);
            SerializedProperty newMan = manufacture.GetArrayElementAtIndex(loc);
            newMan.FindPropertyRelative("manufacture").arraySize = 0;
            newMan.FindPropertyRelative("name").stringValue = "Manufacture group " + loc;
            controllerSO.FindProperty("selected").FindPropertyRelative("MG").intValue = loc;
            GUIUtility.keyboardControl = 0;
        }//end AddManGroup

        void AddManGroup(int loc, SerializedObject[] pt)
        {//add manufacturing groups to posts and traders
            for (int p = 0; p < pt.Length; p++)
            {//go through, adding
                SerializedProperty ptMan = pt[p].FindProperty("manufacture");
                ptMan.InsertArrayElementAtIndex(loc);
                ptMan.GetArrayElementAtIndex(loc).FindPropertyRelative("manufacture").arraySize = 0;
            }//end for
        }//end AddManGroup

        void NumberChange(RunMnfctr man, Mnfctr cMan, float total, float[][] perItem)
        {//calculate the number changes from trade posts and traders
            if (man.enabled)
            {//check that item is enabled
                float toChange = 0;
                float deniminator = man.create + man.cooldown;//get the denominator. This is the minimum time between subsequent manufactures
                for (int nm = 0; nm < cMan.needing.Count; nm++)
                {//go through needing, reducing perItem
                    toChange = cMan.needing[nm].number / deniminator;
                    total -= toChange;
                    perItem[cMan.needing[nm].groupID][cMan.needing[nm].itemID] -= toChange;
                }//end for needing

                for (int nm = 0; nm < cMan.making.Count; nm++)
                {//go through making, increasing perItem
                    toChange = cMan.making[nm].number / deniminator;
                    total += toChange;
                    perItem[cMan.making[nm].groupID][cMan.making[nm].itemID] += toChange;
                }//end for needing
            }//end enabled check
        }//end NumberChange

        ///check whether the value is infinity
        bool InfinityCheck(string minMax)
        {
            SerializedProperty unitsList = units.FindPropertyRelative("units");
            for (int u = 0; u < unitsList.arraySize; u++)
            {//go through units
                if (unitsList.GetArrayElementAtIndex(u).FindPropertyRelative(minMax).floatValue == Mathf.Infinity)
                    return true;
            }//end for units
            return false;
        }//end InfinityCheck

        ///have a gui label with a unit which goes over the field box
        void UnitLabel(string label, float width)
        {
            GUILayout.Space(-(width + 4));
            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(width));
        }//end UnitLabel

        /// get the string containing the mass and the unit in the correct form
        ///zero is used to get the units if the mass is 1. Needed if none of the units have limits of exactly 1
        string UnitString(float mass, bool zero)
        {
            if (mass == 0)
                return UnitString(1, true);

            for (int u = 0; u < controllerNormal.units.units.Count; u++)
            {//go through each unit
                Unit cU = controllerNormal.units.units[u];
                if (mass >= cU.min && mass < cU.max)
                {//if the mass of the item is in the range
                    return zero ? "0 " + cU.suffix : ((decimal)mass / (decimal)cU.min).ToString() + " " + cU.suffix;//return the correct unit
                }//end if mass in range
            }//end for each unit
            return zero ? "0" : mass.ToString();
        }//end UnitString

        void SortCrate(SerializedProperty itemCrate)
        {//sort out the information of the item crate
            GameObject crate = (GameObject)itemCrate.objectReferenceValue;
            crate.tag = Tags.I;//set tag to item
            Item itemScript = crate.GetComponent<Item>();

            if (itemScript == null)
                itemScript = crate.AddComponent<Item>();
        }//end SortCrate

        bool CheckMan()
        {//check that all of the manufacturing processes point to an item, returns false if not
            for (int g = 0; g < controllerNormal.manufacture.Count; g++)
            {//for all groups
                for (int p = 0; p < controllerNormal.manufacture[g].manufacture.Count; p++)
                {//for all processes
                    for (int n = 0; n < controllerNormal.manufacture[g].manufacture[p].needing.Count; n++)
                    {//for all needing
                        if (controllerNormal.manufacture[g].manufacture[p].needing[n].itemID == -1)
                            return false;
                    }//end for needing
                    for (int m = 0; m < controllerNormal.manufacture[g].manufacture[p].making.Count; m++)
                    {//for all making
                        if (controllerNormal.manufacture[g].manufacture[p].making[m].itemID == -1)
                            return false;
                    }//end for making
                }//end for processes
            }//end for groups
            return true;
        }//end CheckMan

        bool ExchangeIssue(SerializedProperty curEx, int index)
        { //return whether there is an issue with the selected exchange
            int a1 = curEx.FindPropertyRelative("IDA").intValue;
            int b1 = curEx.FindPropertyRelative("IDB").intValue;
            bool r1 = curEx.FindPropertyRelative("reverse").boolValue;

            if (a1 == b1)
                return true;//return true if both the same on same exchange

            for (int e = 0; e < exchange.arraySize; e++)
            { //go through all, making sure not duplicated
                if (e != index)
                { //if not the same as the one to be checked
                    SerializedProperty ex = exchange.GetArrayElementAtIndex(e);
                    int a2 = ex.FindPropertyRelative("IDA").intValue;
                    int b2 = ex.FindPropertyRelative("IDB").intValue;
                    bool r2 = ex.FindPropertyRelative("reverse").boolValue;

                    if ((a1 == a2 && b1 == b2) ||//if same a1, a2 and b1, b2
                        (r1 || r2) && a1 == b2 && b1 == a2)//or if reverse and check a1, b2 and b1, a2
                        return true;//has same values, so return true
                }//end not same
            }//end for duplicate check

            return false;
        }//end ExchangeIssue

        void PTCur(SerializedObject[] postsTraders, bool add, int index) { //add or remove a currency from posts and traders
                                                                           //sort traders
            for (int pt = 0; pt < postsTraders.Length; pt++)
            { //for all
                if (add)
                {
                    postsTraders[pt].FindProperty("currencies").InsertArrayElementAtIndex(index);//add a currency
                    postsTraders[pt].FindProperty("currencies").GetArrayElementAtIndex(index).floatValue = 0;//set to 0
                }
                else
                    postsTraders[pt].FindProperty("currencies").InsertArrayElementAtIndex(index);//add a currency
            }//end for all
        }//end PTCur

        void PTEx(SerializedObject[] postsTraders, bool add, int index) { //add or remove an exchange from the posts and traders
            for (int pt = 0; pt< postsTraders.Length; pt++)
            {//for all
                if (add)
                {
                    postsTraders[pt].FindProperty("exchanges").InsertArrayElementAtIndex(index);
                    postsTraders[pt].FindProperty("exchanges").GetArrayElementAtIndex(index).boolValue = false;
                }
                else
                    postsTraders[pt].FindProperty("exchanges").DeleteArrayElementAtIndex(index);
            }//end for all
        }//end PTEx

        void PTMan(SerializedObject[] postsTraders, int groupID, int processID, int index)
        {
            for (int pt = 0; pt < postsTraders.Length; pt++)
            {//for all 
                SerializedProperty curID = postsTraders[pt].FindProperty("manufacture").GetArrayElementAtIndex(groupID).FindPropertyRelative("manufacture").GetArrayElementAtIndex(processID).FindPropertyRelative("currencyID");
                if (curID.intValue >= index)//if is greater or equal to currency removed
                    curID.intValue--;//reduce
            }//end for all
        }//end PTMan
    }//end ControllerEditor
}//end namespace