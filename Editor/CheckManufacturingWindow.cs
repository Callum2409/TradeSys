using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CallumP.TradeSys
{//use namespace to stop any name conflicts
    public class CheckManufacturingWindow : EditorWindow
    {
        Controller controller;
        bool showHoriz;
        Vector2 scrollPosN, scrollPosP;
        TSGUI GUITools = new TSGUI();
        int selection;
        public float[][] perItem;//the per item changes
        public float total, cashChange;//the total changes
        string[][] info;//the item changes strings
        string[][] pricing;

        void Awake()
        {
            controller = GameObject.FindGameObjectWithTag(Tags.C).GetComponent<Controller>();
            showHoriz = controller.showHoriz;//everything will be shown with the horizontal or vertical
        }//end Awake

        void OnGUI()
        {
            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;

            CalcInfo();//calculate all of the required info

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            string[] options = new string[] { "Item numbers", "Item pricing" };
            selection = GUILayout.Toolbar(selection, controller.expTraders.enabled ? new string[] { options[0] } : options);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            switch (selection)
            {
                #region item numbers
                case 0:
                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.LabelField("NOTE: This can only be used as a guide because there may be pauses and greater times between " +
                            "manufacturing processes if items are not available.\n\nAs a result, there will be some variances, but will still " +
                            "be useful to give an idea of whether the numbers of an item is expected to increase, decrease or stay the same.\n\n" +
                            "The number is the change in quantity of the item per second so a larger number means that this change is faster.",
                            EditorStyles.wordWrappedLabel);//the text which is always displayed

                    EditorGUILayout.LabelField("", "", "ShurikenLine", GUILayout.MaxHeight(1f));//draw a separating line
                    scrollPosN = EditorGUILayout.BeginScrollView(scrollPosN);

                    EditorGUILayout.BeginVertical("HelpBox");

                    for (int g = 0; g < controller.goods.Count; g++)
                    {//for all goods 
                        EditorGUI.indentLevel = 0;
                        EditorGUILayout.LabelField(controller.goods[g].name, EditorStyles.boldLabel);
                        GUITools.HorizVertDisplay(controller.allNames[g], info[g], new string[info[g].Length], showHoriz, 1);
                    }

                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.Space();//space between all the items and the total change

                    EditorGUILayout.LabelField("Item change", SetChange(total));

                    if (!controller.expTraders.enabled)
                        EditorGUILayout.LabelField("Credit change", SetChange(cashChange));

                    EditorGUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    break;
                #endregion

                #region pricing
                case 1:
                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.LabelField("This is showing the profit per time the manufacturing process occurs. This is a best-case scenario, " +
                            "where the cost of items purchased to manufacture are at their lowest, and the items made are sold at the highest. As a result, the " +
                            "profits are likely to be lower than this.\n\nAny process that shows a negative value here will always have a loss.\n\n" +
                            "This assumes that the item prices are set automatically.", EditorStyles.wordWrappedLabel);

                    EditorGUILayout.LabelField("", "", "ShurikenLine", GUILayout.MaxHeight(1f));//draw a separating line
                    scrollPosP = EditorGUILayout.BeginScrollView(scrollPosP);

                    EditorGUILayout.BeginVertical("HelpBox");
                    for (int m = 0; m < controller.manufacture.Count; m++)
                    {
                        EditorGUI.indentLevel = 0;
                        EditorGUILayout.LabelField(controller.manufacture[m].name, EditorStyles.boldLabel);
                        GUITools.HorizVertDisplay(controller.manufactureNames[m], pricing[m], controller.manufactureTooltips[m], showHoriz, 1);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();
                    break;
                    #endregion
            }//end switch
        }//end OnGUI

        void CalcInfo()
        {//calculate the information
         //reset some of the information so that it will be correct if the number of items changes
            perItem = new float[controller.goods.Count][];//the change for each item
            info = new string[controller.goods.Count][];//an array containing all of the strings to display
            for (int g = 0; g < controller.goods.Count; g++)
            {
                perItem[g] = new float[controller.goods[g].goods.Count];
                info[g] = new string[controller.allNames[g].Length];
            }
            total = cashChange = 0;//reset the totals
            pricing = new string[controller.manufacture.Count][];

            List<MnfctrTypes> manufacture = controller.manufacture;

            for (int m = 0; m < manufacture.Count; m++)
            {//go through all manufacture groups
                int processCount = manufacture[m].manufacture.Count;
                pricing[m] = new string[processCount];

                for (int p = 0; p < processCount; p++)
                {//go through all processes					
                    #region number change
                    Mnfctr cMan = controller.manufacture[m].manufacture[p];
                    for (int tp = 0; tp < controller.postScripts.Length; tp++)//go through all posts
                        if (controller.postScripts[tp].manufacture[m].enabled)//only count if group enabled
                            NumberChange(controller.postScripts[tp].manufacture[m].manufacture[p], cMan);
                    for (int t = 0; t < controller.traderScripts.Length; t++)//go through all traders
                        if (controller.traderScripts[t].manufacture[m].enabled)//only count if group enabled
                            NumberChange(controller.traderScripts[t].manufacture[m].manufacture[p], cMan);
                    #endregion

                    #region pricing
                    pricing[m][p] = "N/A";

                    for (int c = 0; c < controller.currencies.Count; c++)
                    { //for all currencies

                        float percent = Exchange(c, cMan);//get the percent value

                        if (percent != 0)
                        {//if is a value
                            pricing[m][p] = percent.ToString("n2") + "%";//can set the text
                            break;//and can break as valid excahanges were used
                        }//end if usable
                    }//end for currencies
                    #endregion
                }//end for all processes			
            }//end for manufacture groups

            for (int x = 0; x < info.Length; x++)//go through all goods, saying increase, decrease or stay the same and give a value
                for (int y = 0; y < info[x].Length; y++)
                    info[x][y] = SetChange(perItem[x][y]);
        }//end CalcInfo

        void NumberChange(RunMnfctr man, Mnfctr cMan)
        {//calculate the number changes from trade posts and traders
            if (man.enabled)
            {//check that item is enabled
                float toChange = 0;
                float deniminator = man.create + man.cooldown;//get the denominator. This is the minimum time between subsequent manufactures

                cashChange -= man.price / deniminator;//work out how the amount of cash will change

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

        string SetChange(float change)
        {//check to see if increase, decrease or the same
            if (change > 0)
                return "Increase (" + change.ToString("f2") + ")";
            if (change < 0)
                return "Decrease (" + Mathf.Abs(change).ToString("f2") + ")";
            return "Same";
        }//end SetChange

        float Exchange(int currencyID, Mnfctr cMan)
        {//go through nm and see if exchange is ok, if is then add to total, if not, return

            float needing = 0;//the cost of all of the items needed
            float making = 0;//the profit from all of the items sold

            for (int nm = 0; nm < cMan.needing.Count; nm++)
            {//go through all needing, getting min cost
                NeedMake currentNM = cMan.needing[nm];

                Goods currentGood = controller.goods[currentNM.groupID].goods[currentNM.itemID];

                float thisNeeding = currentGood.minPrice * currentNM.number * controller.GetExchangeRate(currentGood.currencyID, currencyID);

                if (thisNeeding != 0)//if is a value
                    needing += thisNeeding;//then add to the needing
                else
                    return 0;//else return 0 as currency is invalid
            }//end for needing
            for (int nm = 0; nm < cMan.making.Count; nm++)
            {//go through all making, getting max price
                NeedMake currentNM = cMan.making[nm];

                Goods currentGood = controller.goods[currentNM.groupID].goods[currentNM.itemID];

                float thisMaking = currentGood.maxPrice * currentNM.number * controller.GetExchangeRate(currentGood.currencyID, currencyID);

                if (thisMaking != 0)//if is a value
                    making += thisMaking;//then add to the making
                else
                    return 0;//else return 0 as currency is invalid
            }//end for needing

            return ((making / needing) - 1) * 100;//return the percent profit
        }//end CurrencyExchange

    }//end CheckManufacturingWindow
}//end namespace