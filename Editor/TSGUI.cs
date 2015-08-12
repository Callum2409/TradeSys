#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define API
#endif
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys
{//use namespace to stop any name conflicts
		public class TSGUI
		{		
				public bool PlusMinus (bool plus)
				{//easier way of getting the layout correct for the plus and minus buttons
						EditorGUILayout.BeginVertical (GUILayout.MaxWidth (18f));//needs a vertical layout to properly align vertically
						GUILayout.Space (3f);//the space required for better alignment
			
						string button = plus ? "OL Plus" : "OL Minus";//the type of button for + -
			
						if (GUILayout.Button ("", button)) {//display the button
								GUIUtility.keyboardControl = 0;
								return true;//if pressed, return true
						}
						EditorGUILayout.EndVertical ();
						return false;//if not pressed, return false
				}//end PlusMinus
	
				public void EnableDisable (SerializedProperty toChange, string enabled, bool enabledString)
				{//have option to enable or disable anything displayed
						GUILayout.FlexibleSpace ();
						if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft))
								EnableDisable (toChange, true, enabled, enabledString);
						if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight))
								EnableDisable (toChange, false, enabled, enabledString);
						EditorGUILayout.EndHorizontal ();
				}//end EnableDisable for single option
	
				public void EnableDisable (SerializedProperty toChange, string[] enabled, bool enabledString)
				{//have option to enable or disable anything displayed for multiple options
						GUILayout.FlexibleSpace ();
						if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft)) {
								for (int e = 0; e<enabled.Length; e++)
										EnableDisable (toChange, true, enabled [e], enabledString);
						}
						if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
								for (int e = 0; e<enabled.Length; e++)
										EnableDisable (toChange, false, enabled [e], enabledString);
						}
						EditorGUILayout.EndHorizontal ();
				}//end EnableDisable for double option
	
				void EnableDisable (SerializedProperty toChange, bool enable, string enabled, bool enabledString)
				{
						for (int c = 0; c<toChange.arraySize; c++) {
								if (enabledString)
										toChange.GetArrayElementAtIndex (c).FindPropertyRelative (enabled).boolValue = enable;
								else
										toChange.GetArrayElementAtIndex (c).boolValue = enable;
						}
				}//end EnableDisable
	
				public void HorizVertOptions (SerializedProperty showHoriz)
				{//show the radio buttons for showing horizontally and vertically
						EditorGUILayout.BeginHorizontal ();
						showHoriz.boolValue = !EditorGUILayout.Toggle (new GUIContent ("Show items vertically", "Show items ascending vertically"), !(showHoriz.boolValue), "Radio");
						showHoriz.boolValue = EditorGUILayout.Toggle (new GUIContent ("Show items horizontally", "Show items ascending horizontally"), showHoriz.boolValue, "Radio");
						EditorGUILayout.EndHorizontal ();
				}//end HorizVert
	
				public void HorizVertDisplay (string[] names, SerializedProperty option, string property, bool showHoriz, int indentLevel)
				{//a list of bool options
						EditorGUI.indentLevel = indentLevel;
						if (showHoriz) {//if showing items horizontally
								for (int b = 0; b<option.arraySize; b=b+2) {//add 2 each time because 2 option are being displayed
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (b).FindPropertyRelative (property), new GUIContent (names [b]));
										if (b < option.arraySize - 1)//show the RH option if is less than length -1
												EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (b + 1).FindPropertyRelative (property), new GUIContent (names [b + 1]));
										else
												EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
								}//end for show items
						} else {//if showing items vertically
								int half = Mathf.CeilToInt (option.arraySize / 2f);//get the halfway item rounded up so that an odd number will be displayed
								for (int b = 0; b<half; b++) {//only need to go through half
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (b).FindPropertyRelative (property), new GUIContent (names [b]));
										if (half + b < option.arraySize)
												EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (half + b).FindPropertyRelative (property), new GUIContent (names [half + b]));
										else
												EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
								}//end for show items
						}//end else show vertically
				}//end HorizVertToDisplay for bools in other options
	
				public void HorizVertDisplay (string[] names, string[] items, string[] tooltip, bool showHoriz, int indentLevel)
				{//a list of labels with two part strings
						EditorGUI.indentLevel = indentLevel;
						if (showHoriz) {//if showing items horizontally
								for (int i = 0; i<items.Length; i=i+2) {//add 2 each time because 2 options are being displayed
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.LabelField (new GUIContent (names [i], tooltip [i]), new GUIContent (items [i], tooltip [i]));
										if (i < items.Length - 1)//show the RH option if is less than length -1
												EditorGUILayout.LabelField (new GUIContent (names [i + 1], tooltip [i + 1]), new GUIContent (items [i + 1], tooltip [i + 1]));
										else
												EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
								}//end for show items
						} else {//if showing items vertically
								int half = Mathf.CeilToInt (items.Length / 2f);//get the halfway item rounded up so that an odd number will be displayed
								for (int i = 0; i<half; i++) {//only need to go through half
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.LabelField (new GUIContent (names [i], tooltip [i]), new GUIContent (items [i], tooltip [i]));
										if (half + i < items.Length)
												EditorGUILayout.LabelField (new GUIContent (names [half + i], tooltip [half + i]), new GUIContent (items [half + i], tooltip [half + i]));
										else
												EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
								}//end for show items
						}//end else show vertically
				}//end HorizVertToDisplay for strings
	
				public void HorizVertDisplay (string[] names, SerializedProperty option, bool showHoriz, bool BSH)
				{//a list with the BSH options, or straight up options
						EditorGUI.indentLevel = 0;
						if (showHoriz) {//if showing items horizontally
								for (int b = 0; b<option.arraySize; b=b+2) {//add 2 each time because 2 option are being displayed
										EditorGUILayout.BeginHorizontal ();
										if (BSH) 
												BSHDisp (names, option, b);
										else
												EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (b), new GUIContent (names [b]));
				
										if (b < option.arraySize - 1) {//show the RH option if is less than length -1
												if (BSH)
														BSHDisp (names, option, b + 1);
												else
														EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (b + 1), new GUIContent (names [b + 1]));
										} else
												EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
								}//end for show items
						} else {//if showing items vertically
								int half = Mathf.CeilToInt (option.arraySize / 2f);//get the halfway item rounded up so that an odd number will be displayed
								for (int b = 0; b<half; b++) {//only need to go through half
										EditorGUILayout.BeginHorizontal ();
										if (BSH)
												BSHDisp (names, option, b);
										else
												EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (b), new GUIContent (names [b]));
					
										if (half + b < option.arraySize) {
												if (BSH)
														BSHDisp (names, option, half + b);
												else
														EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (half + b), new GUIContent (names [half + b]));
										} else
												EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
								}//end for show items
						}//end else show vertically
				}//end HorizVertToDisplay for two bools
		
				void BSHDisp (string[] names, SerializedProperty option, int o)
				{//show the BSH options			
						EditorGUILayout.LabelField (names [o], GUILayout.MaxWidth(100f));
						GUILayout.FlexibleSpace();
						EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (o).FindPropertyRelative ("buy"), GUIContent.none, GUILayout.Width (15f));
						EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (o).FindPropertyRelative ("sell"), GUIContent.none, GUILayout.Width (15f));
						EditorGUILayout.PropertyField (option.GetArrayElementAtIndex (o).FindPropertyRelative ("hidden"), GUIContent.none, GUILayout.Width (15f));
				}//end BSH showing the individual options
	
				public void GetNames (Controller controller)
				{//get all of the item names
						controller.allNames = new string[controller.goods.Count][];
			
						for (int g1 = 0; g1<controller.goods.Count; g1++) {//go through all groups
								List<Goods> currentGroup = controller.goods [g1].goods;
								controller.allNames [g1] = new string[currentGroup.Count];
								for (int g2 = 0; g2<currentGroup.Count; g2++)//go through all goods in group
										controller.allNames [g1] [g2] = currentGroup [g2].name;
						}//end go through all groups
				}//end GetNames
	
				public void ManufactureInfo (Controller controller)
				{//go through all of the manufacturing processes, getting the names and tooltips
						int groupCount = controller.manufacture.Count;
						controller.manufactureNames = new string[groupCount][];//array containing the names of the manufacturing processes
						controller.manufactureTooltips = new string[groupCount][];//array containging the needing and making parts
			
						for (int m1 = 0; m1<groupCount; m1++) {//for manufacture groups 
								int processCount = controller.manufacture [m1].manufacture.Count;
								controller.manufactureNames [m1] = new string[processCount];
								controller.manufactureTooltips [m1] = new string[processCount];
		
								for (int m2 = 0; m2<processCount; m2++) {//for manufacture processes
										controller.manufactureNames [m1] [m2] = controller.manufacture [m1].manufacture [m2].name;//get all of the names of the manufacturing processes
			
										controller.manufacture [m1].manufacture [m2].tooltip = controller.manufactureTooltips [m1] [m2] = 
											ManufactureTooltip (true, controller, m1, m2) + "\n" + ManufactureTooltip (false, controller, m1, m2);
								}//end for manufacture processes
						}//end for manufacture groups
				}//end ManufactureInfo
				
				string ManufactureTooltip (bool needing, Controller controller, int m1, int m2)
				{//get the tooltip for the needing or making
						string tooltip = needing ? "N: " : "M: ";
			
						Mnfctr currentMnfctr = controller.manufacture [m1].manufacture [m2];
						List<NeedMake> currentNM = needing ? currentMnfctr.needing : currentMnfctr.making;
						NeedMake current;
			
						for (int n = 0; n<currentNM.Count; n++) {//go through all needing or making
								current = currentNM [n];
								tooltip += current.number + "×";
								if (current.itemID >= 0)//check item is not undefined
										tooltip += controller.allNames [current.groupID] [current.itemID] + ", ";//get the groupID and itemID to get the name
								else
										tooltip += "Undefined, ";
						}//end for all needing or making
			
						if (tooltip.Length > 3)//only remove if things have been added
								tooltip = tooltip.Remove (tooltip.Length - 2);//remove the space and comma
						else
								tooltip += "Nothing";
				
						return tooltip;
				}//end ManufactureTooltip
	
				public void ExpandCollapse (SerializedProperty grouping, string expand, bool expandMid)
				{//expand all button has an if statement to decide if the mini button is the left most button or a middle button
						if (GUILayout.Button ("Expand all", expandMid ? EditorStyles.miniButtonMid : EditorStyles.miniButtonLeft)) {
								GUIUtility.keyboardControl = 0;
								ExpandAll (grouping, expand, true);
						}
						if (GUILayout.Button ("Collapse all", EditorStyles.miniButtonRight)) {
								GUIUtility.keyboardControl = 0;
								ExpandAll (grouping, expand, false);
						}
				}//end ExpandCollapse
	
				void ExpandAll (SerializedProperty toExpand, string expand, bool expanding)
				{
						for (int e = 0; e<toExpand.arraySize; e++)
								toExpand.GetArrayElementAtIndex (e).FindPropertyRelative (expand).boolValue = expanding;
				}//end ExpandAll
	
				public void Title (GUIContent title, bool horizontal)
				{//begins a group vertical and has a title
						EditorGUILayout.BeginVertical ("HelpBox");
						EditorGUI.indentLevel = 0;
						if (horizontal)
								EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (title, EditorStyles.boldLabel);
						EditorGUI.indentLevel = 1;
				}//end Title
	
				public void Title (GUIContent title, SerializedProperty toggle, GUIContent toggleInfo)
				{//begins a group vertical and has a title also has an enable toggle
						EditorGUILayout.BeginVertical ("HelpBox");
						EditorGUI.indentLevel = 0;
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (title, EditorStyles.boldLabel);
						GUILayout.Space (-20f);
						EditorGUILayout.PropertyField (toggle, toggleInfo);
						EditorGUILayout.EndHorizontal ();
						EditorGUI.indentLevel = 1;
				}//end Title with toggle
	
				public bool TitleGroup (GUIContent title, SerializedProperty toggle, bool horizontal)
				{//begins a group vertical with a title button
						EditorGUILayout.BeginVertical ("HelpBox");
						GUILayout.Space (-1f);
						EditorGUI.indentLevel = 0;
						if (horizontal)
								EditorGUILayout.BeginHorizontal ();
						if (GUILayout.Button (title, "BoldLabel"))
								toggle.boolValue = !toggle.boolValue;
						EditorGUI.indentLevel = 1;
						return toggle.boolValue;
				}//end TitleButton
	
				public void IndentGroup (int indent)
				{//Begins a horizontal to indent the vertical
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Space (indent * 10f);
						EditorGUILayout.BeginVertical ("HelpBox");
						EditorGUI.indentLevel = 0;
				}//end IndentGroup
	
				public void StartScroll (SerializedProperty scrollPos, SerializedProperty enabled)
				{//draw a separating line, and if enabled, start a scroll view
						EditorGUILayout.LabelField ("", "", "ShurikenLine", GUILayout.MaxHeight (1f));//draw a separating line
						if (enabled.boolValue)//if smaller scroll views enabled
								scrollPos.vector2Value = EditorGUILayout.BeginScrollView (scrollPos.vector2Value);	
				}//end StartScroll	
		}//end TSGUI
}//end namespace