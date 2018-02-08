#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define API
#endif
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CallumP.TradeSys {//use namespace to stop any name conflicts
    public class TSGUI {
        public bool PlusMinus(bool plus) {//easier way of getting the layout correct for the plus and minus buttons
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(18f));//needs a vertical layout to properly align vertically
            GUILayout.Space(3f);//the space required for better alignment

            string button = plus ? "OL Plus" : "OL Minus";//the type of button for + -

            if (GUILayout.Button("", button)) {//display the button
                GUIUtility.keyboardControl = 0;
                return true;//if pressed, return true
            }
            EditorGUILayout.EndVertical();
            return false;//if not pressed, return false
        }//end PlusMinus

        public void EnableDisable(SerializedProperty toChange, string enabled, bool enabledString) {//have option to enable or disable anything displayed
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(0f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select all", EditorStyles.miniButtonLeft))
                EnableDisable(toChange, true, enabled, enabledString);
            if (GUILayout.Button("Select none", EditorStyles.miniButtonRight))
                EnableDisable(toChange, false, enabled, enabledString);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }//end EnableDisable for single option

        public void EnableDisable(SerializedProperty toChange, string[] enabled, bool enabledString) {//have option to enable or disable anything displayed for multiple options
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select all", EditorStyles.miniButtonLeft)) {
                for (int e = 0; e < enabled.Length; e++)
                    EnableDisable(toChange, true, enabled[e], enabledString);
            }
            if (GUILayout.Button("Select none", EditorStyles.miniButtonRight)) {
                for (int e = 0; e < enabled.Length; e++)
                    EnableDisable(toChange, false, enabled[e], enabledString);
            }
            EditorGUILayout.EndHorizontal();
        }//end EnableDisable for double option

        void EnableDisable(SerializedProperty toChange, bool enable, string enabled, bool enabledString) {
            for (int c = 0; c < toChange.arraySize; c++) {
                if (enabledString)
                    toChange.GetArrayElementAtIndex(c).FindPropertyRelative(enabled).boolValue = enable;
                else
                    toChange.GetArrayElementAtIndex(c).boolValue = enable;
            }
        }//end EnableDisable

        public void HorizVertOptions(SerializedProperty showHoriz) {//show the radio buttons for showing horizontally and vertically
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent("Show items", "Show items ascending horizontally or vertically"), "MiniLabelRight");

            showHoriz.boolValue = GUILayout.Toggle(showHoriz.boolValue, new GUIContent("Horizontally", "Show items ascending horizontally"), EditorStyles.miniButtonLeft);
            showHoriz.boolValue = !GUILayout.Toggle(!(showHoriz.boolValue), new GUIContent("Vertically", "Show items ascending vertically"), EditorStyles.miniButtonRight);

            EditorGUILayout.EndHorizontal();
        }//end HorizVert

        public void HorizVertDisplay(string[] names, SerializedProperty option, string property, bool showHoriz, int indentLevel, bool line) {//a list of bool options
            if (line) {
                EditorGUI.indentLevel = indentLevel - 1;
                EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));
            }

            EditorGUI.indentLevel = indentLevel;
            if (showHoriz) {//if showing items horizontally
                for (int b = 0; b < option.arraySize; b = b + 2) {//add 2 each time because 2 option are being displayed

                    if (line && b > 0)
                        EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(option.GetArrayElementAtIndex(b).FindPropertyRelative(property), new GUIContent(names[b]));
                    if (b < option.arraySize - 1)//show the RH option if is less than length -1
                        EditorGUILayout.PropertyField(option.GetArrayElementAtIndex(b + 1).FindPropertyRelative(property), new GUIContent(names[b + 1]));
                    else
                        EditorGUILayout.LabelField("");
                    EditorGUILayout.EndHorizontal();
                }//end for show items
            } else {//if showing items vertically
                int half = Mathf.CeilToInt(option.arraySize / 2f);//get the halfway item rounded up so that an odd number will be displayed
                for (int b = 0; b < half; b++) {//only need to go through half

                    if (line && b > 0)
                        EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(option.GetArrayElementAtIndex(b).FindPropertyRelative(property), new GUIContent(names[b]));
                    if (half + b < option.arraySize)
                        EditorGUILayout.PropertyField(option.GetArrayElementAtIndex(half + b).FindPropertyRelative(property), new GUIContent(names[half + b]));
                    else
                        EditorGUILayout.LabelField("");
                    EditorGUILayout.EndHorizontal();
                }//end for show items
            }//end else show vertically
        }//end HorizVertToDisplay for bools in other options

        public void HorizVertDisplay(string[] names, string[] items, string[] tooltip, bool showHoriz, int indentLevel) {//a list of labels with two part strings
            EditorGUI.indentLevel = indentLevel;
            if (showHoriz) {//if showing items horizontally
                for (int i = 0; i < items.Length; i = i + 2) {//add 2 each time because 2 options are being displayed
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(names[i], tooltip[i]), new GUIContent(items[i], tooltip[i]));
                    if (i < items.Length - 1)//show the RH option if is less than length -1
                        EditorGUILayout.LabelField(new GUIContent(names[i + 1], tooltip[i + 1]), new GUIContent(items[i + 1], tooltip[i + 1]));
                    else
                        EditorGUILayout.LabelField("");
                    EditorGUILayout.EndHorizontal();
                }//end for show items
            } else {//if showing items vertically
                int half = Mathf.CeilToInt(items.Length / 2f);//get the halfway item rounded up so that an odd number will be displayed
                for (int i = 0; i < half; i++) {//only need to go through half
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent(names[i], tooltip[i]), new GUIContent(items[i], tooltip[i]));
                    if (half + i < items.Length)
                        EditorGUILayout.LabelField(new GUIContent(names[half + i], tooltip[half + i]), new GUIContent(items[half + i], tooltip[half + i]));
                    else
                        EditorGUILayout.LabelField("");
                    EditorGUILayout.EndHorizontal();
                }//end for show items
            }//end else show vertically
        }//end HorizVertToDisplay for strings

        public void HorizVertDisplay(string[] names, SerializedProperty option, bool showHoriz) {//a list with straight up options with no property
            EditorGUI.indentLevel = 0;
            if (showHoriz) {//if showing items horizontally
                for (int b = 0; b < option.arraySize; b = b + 2) {//add 2 each time because 2 option are being displayed
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(option.GetArrayElementAtIndex(b), new GUIContent(names[b]));

                    if (b < option.arraySize - 1) {//show the RH option if is less than length -1
                        EditorGUILayout.PropertyField(option.GetArrayElementAtIndex(b + 1), new GUIContent(names[b + 1]));
                    } else
                        EditorGUILayout.LabelField("");
                    EditorGUILayout.EndHorizontal();
                }//end for show items
            } else {//if showing items vertically
                int half = Mathf.CeilToInt(option.arraySize / 2f);//get the halfway item rounded up so that an odd number will be displayed
                for (int b = 0; b < half; b++) {//only need to go through half
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(option.GetArrayElementAtIndex(b), new GUIContent(names[b]));

                    if (half + b < option.arraySize) {
                        EditorGUILayout.PropertyField(option.GetArrayElementAtIndex(half + b), new GUIContent(names[half + b]));
                    } else
                        EditorGUILayout.LabelField("");
                    EditorGUILayout.EndHorizontal();
                }//end for show items
            }//end else show vertically
        }//end HorizVertToDisplay for straight up options

        public void GetNames(Controller controller) {//get all of the item names
            controller.allNames = new string[controller.goods.Count][];

            for (int g1 = 0; g1 < controller.goods.Count; g1++) {//go through all groups
                List<Goods> currentGroup = controller.goods[g1].goods;
                controller.allNames[g1] = new string[currentGroup.Count];
                for (int g2 = 0; g2 < currentGroup.Count; g2++)//go through all goods in group
                    controller.allNames[g1][g2] = currentGroup[g2].name;
            }//end go through all groups
        }//end GetNames

        public void ManufactureInfo(Controller controller) {//go through all of the manufacturing processes, getting the names and tooltips
            int groupCount = controller.manufacture.Count;
            controller.manufactureNames = new string[groupCount][];//array containing the names of the manufacturing processes
            controller.manufactureTooltips = new string[groupCount][];//array containging the needing and making parts

            for (int m1 = 0; m1 < groupCount; m1++) {//for manufacture groups 
                int processCount = controller.manufacture[m1].manufacture.Count;
                controller.manufactureNames[m1] = new string[processCount];
                controller.manufactureTooltips[m1] = new string[processCount];

                for (int m2 = 0; m2 < processCount; m2++) {//for manufacture processes
                    controller.manufactureNames[m1][m2] = controller.manufacture[m1].manufacture[m2].name;//get all of the names of the manufacturing processes

                    controller.manufacture[m1].manufacture[m2].tooltip = controller.manufactureTooltips[m1][m2] =
                        ManufactureTooltip(true, controller, m1, m2) + "\n" + ManufactureTooltip(false, controller, m1, m2);
                }//end for manufacture processes
            }//end for manufacture groups
        }//end ManufactureInfo

        string ManufactureTooltip(bool needing, Controller controller, int m1, int m2) {//get the tooltip for the needing or making
            string tooltip = needing ? "N: " : "M: ";

            Mnfctr currentMnfctr = controller.manufacture[m1].manufacture[m2];
            List<NeedMake> currentNM = needing ? currentMnfctr.needing : currentMnfctr.making;
            NeedMake current;

            for (int n = 0; n < currentNM.Count; n++) {//go through all needing or making
                current = currentNM[n];
                tooltip += current.number + "\u00D7";
                if (current.itemID >= 0)//check item is not undefined
                    tooltip += controller.allNames[current.groupID][current.itemID] + ", ";//get the groupID and itemID to get the name
                else
                    tooltip += "Undefined, ";
            }//end for all needing or making

            if (tooltip.Length > 3)//only remove if things have been added
                tooltip = tooltip.Remove(tooltip.Length - 2);//remove the space and comma
            else
                tooltip += "Nothing";

            return tooltip;
        }//end ManufactureTooltip

        public void ExpandCollapse(SerializedProperty grouping, string expand, bool expandMid) {//expand all button has an if statement to decide if the mini button is the left most button or a middle button
            if (GUILayout.Button("Expand all", expandMid ? EditorStyles.miniButtonMid : EditorStyles.miniButtonLeft)) {
                GUIUtility.keyboardControl = 0;
                ExpandAll(grouping, expand, true);
            }
            if (GUILayout.Button("Collapse all", EditorStyles.miniButtonRight)) {
                GUIUtility.keyboardControl = 0;
                ExpandAll(grouping, expand, false);
            }
        }//end ExpandCollapse

        void ExpandAll(SerializedProperty toExpand, string expand, bool expanding) {
            for (int e = 0; e < toExpand.arraySize; e++)
                toExpand.GetArrayElementAtIndex(e).FindPropertyRelative(expand).boolValue = expanding;
        }//end ExpandAll

        public void Title(GUIContent title, bool horizontal) {//begins a group vertical and has a title
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel = 0;
            if (horizontal)
                EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
        }//end Title

        public void Title(GUIContent title, SerializedProperty toggle, GUIContent toggleInfo) {//begins a group vertical and has a title also has an enable toggle
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.Space(-20f);
            EditorGUILayout.PropertyField(toggle, toggleInfo);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel = 1;
        }//end Title with toggle

        public bool TitleGroup(GUIContent title, SerializedProperty toggle, bool horizontal) {//begins a group vertical with a title button
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Space(-1f);
            EditorGUI.indentLevel = 0;
            if (horizontal)
                EditorGUILayout.BeginHorizontal();
            TitleButton(title, toggle, "BoldLabel");
            EditorGUI.indentLevel = 1;
            return toggle.boolValue;
        }//end TitleGroup

        public bool TitleButton(GUIContent title, SerializedProperty toggle, string style) {//the title button to show or hide a group
            EditorGUILayout.BeginVertical();
            GUILayout.Space(1f);
            if (GUILayout.Button(title, style, GUILayout.MaxWidth(146f))) {
                toggle.boolValue = !toggle.boolValue;
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.EndVertical();
            return toggle.boolValue;
        }//end TitleButton

        public void IndentGroup(int indent) {//Begins a horizontal to indent the vertical
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 10f);
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel = 0;
        }//end IndentGroup

        public Vector2 StartScroll(Vector2 scrollPos, SerializedProperty enabled) {//draw a separating line, and if enabled, start a scroll view
            EditorGUILayout.LabelField("", "", "ShurikenLine", GUILayout.MaxHeight(1f));//draw a separating line
            if (enabled.boolValue)//if smaller scroll views enabled
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            return scrollPos;
        }//end StartScroll	

        public int Toolbar(int sel, string[] names) {//show a toolbar with space either side
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();//flexible space so the size of the toolbar remains the same

            int selB = sel;
            sel = GUILayout.Toolbar(sel, names);//show the toolbar
            if (sel != selB)
                GUIUtility.keyboardControl = 0;

            GUILayout.FlexibleSpace();//another space so is in the middle
            EditorGUILayout.EndHorizontal();
            return sel;
        }//end Toolbar

        public bool AnyGoods(SerializedProperty goods, string property) {//go through goods groups and see if there are any items
            for (int g = 0; g < goods.arraySize; g++)
                if (goods.GetArrayElementAtIndex(g).FindPropertyRelative(property).arraySize > 0)
                    return true;
            return false;
        }//end AnyGoods

        public Vector2 PTMan(SerializedProperty manufacturing, Vector2 scrollPos, SerializedProperty smallScroll, SerializedProperty controllerMan, Controller controller, SerializedObject obj, bool post) {//the manufacturing options for posts and traders
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Manufacturing processes", EditorStyles.boldLabel);

            EnableDisable(manufacturing, "enabled", true);

            scrollPos = StartScroll(scrollPos, smallScroll);

            if (AnyManufacturing(controllerMan, obj, post)) {
                EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));

                for (int m = 0; m < controllerMan.arraySize; m++) {//go through manufacturing groups
                    SerializedProperty cManG = manufacturing.GetArrayElementAtIndex(m);
                    SerializedProperty cManGC = controllerMan.GetArrayElementAtIndex(m);

                    if (ShowGroup(cManGC.FindPropertyRelative("manufacture"), obj, post)) {//check that something in the group is showing to decide whether to show the title or not
                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.indentLevel = 0;

                        SerializedProperty cManEn = cManG.FindPropertyRelative("enabled");

                        if (TitleButton(new GUIContent(cManGC.FindPropertyRelative("name").stringValue, "Checkbox on the right enables the process group"), cManGC.FindPropertyRelative("expanded" + (post ? "P" : "T")), "BoldLabel") && cManEn.boolValue) {//if group showing

                            EditorGUILayout.BeginVertical();
                            GUILayout.Space(0f);
                            EditorGUILayout.PropertyField(cManEn, GUIContent.none, GUILayout.Width(15f));
                            EditorGUILayout.EndVertical();
                            GUILayout.FlexibleSpace();

                            EnableDisable(cManG.FindPropertyRelative("manufacture"), "enabled", true);
                            for (int p = 0; p < cManG.FindPropertyRelative("manufacture").arraySize; p++) {//go through current manufacture processes
                                SerializedProperty cMan = cManG.FindPropertyRelative("manufacture").GetArrayElementAtIndex(p);
                                SerializedProperty cManC = cManGC.FindPropertyRelative("manufacture").GetArrayElementAtIndex(p);

                                if (CorrectEnabled(cManC, obj, post)) {
                                    if (p == 0)
                                        EditorGUI.indentLevel = 0;
                                    EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));
                                    EditorGUI.indentLevel = 1;

                                    EditorGUILayout.PropertyField(cMan.FindPropertyRelative("enabled"),
                               new GUIContent(cManC.FindPropertyRelative("name").stringValue, controller.manufactureTooltips[m][p]));

                                    if (cMan.FindPropertyRelative("enabled").boolValue) {//if enabled, allow times to be edited
                                        EditorGUI.indentLevel = 2;

                                        EditorGUILayout.BeginHorizontal();//have the create and cooldown times horizontal
                                        EditorGUILayout.PropertyField(cMan.FindPropertyRelative("create"), new GUIContent("Create time", "This is how long it takes between removing the items from sale to be manufactured and when the new items are available"));
                                        EditorGUILayout.PropertyField(cMan.FindPropertyRelative("cooldown"), new GUIContent("Cooldown time", "This is how long between one manufacture of this process and another"));
                                        EditorGUILayout.EndHorizontal();

                                        if (cMan.FindPropertyRelative("enabled").boolValue && !controller.expTraders.enabled) {//if enabled and not expendable, show price
                                            EditorGUILayout.BeginHorizontal();

                                            if (obj.FindProperty("currencies").GetArrayElementAtIndex(cMan.FindPropertyRelative("currencyID").intValue).floatValue == -1)//if is not allowed, make red
                                                GUI.color = Color.red;
                                            cMan.FindPropertyRelative("currencyID").intValue = EditorGUILayout.Popup(cMan.FindPropertyRelative("currencyID").intValue, controller.currencyNames, "MiniPullDown");
                                            GUI.color = Color.white;
                                            //EditorGUILayout.PropertyField(cMan.FindPropertyRelative("currencyID"), new GUIContent("", "The currency for the cost of the process to be displayed as"), "DropDownButton");
                                            EditorGUILayout.PropertyField(cMan.FindPropertyRelative("price"), new GUIContent("\tPrice", "The cost to manufacture the item. Can be negative so receives money from manufacture"));
                                            EditorGUILayout.EndHorizontal();

                                            //set correct decimals
                                            cMan.FindPropertyRelative("price").floatValue = (float) System.Math.Round(cMan.FindPropertyRelative("price").floatValue, controller.currencies[cMan.FindPropertyRelative("currencyID").intValue].decimals, System.MidpointRounding.AwayFromZero);

                                        }//end if not expendable
                                    }//end if enabled
                                    if (cMan.FindPropertyRelative("create").intValue < 1)
                                        cMan.FindPropertyRelative("create").intValue = 1;

                                    if (cMan.FindPropertyRelative("cooldown").intValue < 0)
                                        cMan.FindPropertyRelative("cooldown").intValue = 0;

                                } else//end check if not able to, make sure is disabled
                                    cMan.FindPropertyRelative("enabled").boolValue = false;
                            }//end for manufacturing processes														
                        } else {//end if showing group
                            EditorGUILayout.BeginVertical();
                            GUILayout.Space(0f);
                            EditorGUILayout.PropertyField(cManEn, GUIContent.none, GUILayout.Width(15f));
                            EditorGUILayout.EndVertical();
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel = 0;
                        EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));
                    }//check that something is enabled in order to show the title
                }//end for manufacturing groups
            } else//end if something to show
                EditorGUILayout.HelpBox("No processes have been added or the required items have not been enabled.\nCheck that processes have been set up in the controller and that items have been enabled.", MessageType.Info);
            return scrollPos;
        }//end PTMan

        bool CorrectEnabled(SerializedProperty check, SerializedObject obj, bool post) {//go through all needing and making lists, checking that the correct items have been enabled to allow manufacture
            SerializedProperty currentNM = check.FindPropertyRelative("needing");
            for (int n = 0; n < currentNM.arraySize; n++) {//go through all needing lists
                if (!IsEnabled(currentNM.GetArrayElementAtIndex(n).FindPropertyRelative("itemID").intValue,
                currentNM.GetArrayElementAtIndex(n).FindPropertyRelative("groupID").intValue, true, obj, post))
                    return false;
            }

            //check the making list is correct
            currentNM = check.FindPropertyRelative("making");
            for (int m = 0; m < currentNM.arraySize; m++) {//go through all needing lists
                if (!IsEnabled(currentNM.GetArrayElementAtIndex(m).FindPropertyRelative("itemID").intValue,
                currentNM.GetArrayElementAtIndex(m).FindPropertyRelative("groupID").intValue, false, obj, post))
                    return false;
            }
            return true;
        }//end CorrectEnabled

        bool IsEnabled(int itemID, int groupID, bool needing, SerializedObject obj, bool post) {//check to see if the current needing / making item is enable
            if (post) {//if post
                SerializedProperty checking = obj.FindProperty("stock").GetArrayElementAtIndex(groupID).FindPropertyRelative("stock").GetArrayElementAtIndex(itemID);
                if (groupID == -1 || itemID == -1)
                    return false;
                return checking.FindPropertyRelative("hidden").boolValue || checking.FindPropertyRelative(needing ? "buy" : "sell").boolValue;
                //if hidden, will return true, otherwise, if item in needing, check that can buy or if making, check can sell
            } else //else is trader
                return obj.FindProperty("items").GetArrayElementAtIndex(groupID).FindPropertyRelative("items").GetArrayElementAtIndex(itemID).FindPropertyRelative("enabled").boolValue;
        }//end IsEnabled

        bool ShowGroup(SerializedProperty check, SerializedObject obj, bool post) {//needs to check that there is something enabled in order to show the title
            for (int c = 0; c < check.arraySize; c++) {//go through all checking group
                if (CorrectEnabled(check.GetArrayElementAtIndex(c), obj, post))
                    return true;
            }//end for checking group
            return false;
        }//end ShowGroup

        bool AnyManufacturing(SerializedProperty manufacturing, SerializedObject obj, bool post) {//go through manufacturing and see if any are shown
            for (int m = 0; m < manufacturing.arraySize; m++)
                if (ShowGroup(manufacturing.GetArrayElementAtIndex(m).FindPropertyRelative("manufacture"), obj, post))
                    return true;
            return false;
        }//end AnyManufacturing

        public Vector2 EnableDisableItems(string title, string exp, Vector2 scrollPos, SerializedObject controllerSO, SerializedProperty item, SerializedProperty smallScroll, Controller controllerNormal) {//the option GUI used for traders and spawners
            SerializedProperty controllerGoods = controllerSO.FindProperty("goods");

            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            ExpandCollapse(controllerGoods, exp, false);
            EditorGUILayout.EndHorizontal();

            HorizVertOptions(controllerSO.FindProperty("showHoriz"));//show display options

            scrollPos = StartScroll(scrollPos, smallScroll);

            if (AnyGoods(item, "items")) {
                EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));

                for (int g = 0; g < controllerNormal.goods.Count; g++) {
                    SerializedProperty itemGroup = item.GetArrayElementAtIndex(g).FindPropertyRelative("items");
                    if (itemGroup.arraySize > 0) {//dont show anything if nothing in group
                        EditorGUI.indentLevel = 0;

                        SerializedProperty currentGroup = controllerGoods.GetArrayElementAtIndex(g);
                        SerializedProperty expanded = currentGroup.FindPropertyRelative(exp);

                        if (expanded.boolValue)
                            EditorGUILayout.BeginHorizontal();

                        if (TitleButton(new GUIContent(currentGroup.FindPropertyRelative("name").stringValue), expanded, "BoldLabel")) {//if foldout for goods group open											

                            EnableDisable(itemGroup, "enabled", true);

                            string[] nameNo = new string[itemGroup.arraySize];
                            for (int i = 0; i < itemGroup.arraySize; i++)
                                nameNo[i] = controllerNormal.allNames[g][i] + " (" + itemGroup.GetArrayElementAtIndex(i).FindPropertyRelative("number").intValue + ")";

                            HorizVertDisplay(nameNo, itemGroup, "enabled", controllerSO.FindProperty("showHoriz").boolValue, 1, true);
                        }//end if group open
                        EditorGUI.indentLevel = 0;
                        EditorGUILayout.LabelField("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight(0f));
                    }//end if something in group
                }//end for all groups
            } else//end if something to show
                EditorGUILayout.HelpBox("No goods have been added. Add goods in the controller first.", MessageType.Info);

            return scrollPos;//need to return this so that scrolling work
        }//end EnableDisableGoods

        public Vector2 PTCur(bool TP, Controller controller, SerializedObject controllerSO, Vector2 scrollPos, SerializedProperty currencies, SerializedProperty exchanges) { //show the currency tab for the trade post and trader

            SerializedProperty selected = controllerSO.FindProperty("selected").FindPropertyRelative(TP ? "PC" : "TC");//get the currency toolbar selected option
            int sel = selected.intValue;

            selected.intValue = Toolbar(selected.intValue, new string[] { "Currencies", "Exchange" });

            HorizVertOptions(controllerSO.FindProperty("showHoriz"));//show a horiz vert option

            EditorGUILayout.BeginHorizontal();//show a select all/none button for both the currencies and exchanges

            if (sel == 0) {
                EditorGUILayout.LabelField(new GUIContent("Enable currencies", "Allow trade post to use currencies. Set the number to be -1 to disable the curency from being used. If currency disabled that an item is set to use, will not be available for trade"));
                EditorGUILayout.EndHorizontal();
            } else {
                EditorGUILayout.LabelField(new GUIContent("Enable exchanges", "Allow trade post to have exchanges. Exchange cannot be enabled if the currency is not enabled"));
                EnableDisable(exchanges, "", false);
            }

            controller.GetCurrencyNames();//update the currency names

            scrollPos = StartScroll(scrollPos, controllerSO.FindProperty("smallScroll"));

            List<CurrencyExchange> curEx = controller.currencyExchange;//get the currency exchanges

            string[] currencyNames = controller.currencyNames;
            string[] exchangeNames = new string[curEx.Count];//create an array for the exchange names
            for (int n = 0; n < exchangeNames.Length; n++) {//go through all exchanges
                CurrencyExchange thisEx = curEx[n];
                exchangeNames[n] = string.Format("{0} {1} {2}", currencyNames[thisEx.IDA], thisEx.reverse ? "\u2194" : "\u2192", currencyNames[thisEx.IDB]);//create the name of the exchange to display

                if (currencies.GetArrayElementAtIndex(thisEx.IDA).floatValue == -1 || currencies.GetArrayElementAtIndex(thisEx.IDB).floatValue == -1)//if one of the exchanges has the currency disabled, dont allow it to be selected
                    exchanges.GetArrayElementAtIndex(n).boolValue = false;
            }//end for all exchanges

            HorizVertDisplay(sel == 0 ? currencyNames : exchangeNames, sel == 0 ? currencies : exchanges, controllerSO.FindProperty("showHoriz").boolValue);//show the items

            if (sel == 0) { //if is currencies
                for (int c = 0; c < currencies.arraySize; c++) { //for all currencies
                    SerializedProperty thisCurrency = currencies.GetArrayElementAtIndex(c);
                    if (thisCurrency.floatValue < 0 && thisCurrency.floatValue != -1)//if between 0 and -1
                        thisCurrency.floatValue = -1;//set to be -1

                    thisCurrency.floatValue = (float) System.Math.Round(thisCurrency.floatValue, controller.currencies[c].decimals, System.MidpointRounding.AwayFromZero);//set the decimals correctly
                }//end for currencies
            }//end if currencies
            return scrollPos;
        }//end PTCur
    }//end TSGUI
}//end namespace