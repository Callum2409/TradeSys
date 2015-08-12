﻿#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define API
#endif
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TradeSys
{//use namespace to stop any name conflicts
		[CanEditMultipleObjects, CustomEditor(typeof(Controller))]
		public class ControllerEditor : Editor
		{
				[MenuItem("GameObject/Create Other/TradeSys Controller", false, 40)]
				//add an item to the menu
		//simple quick way to make the controller
		static void CreateController ()
				{
						GameObject ctrl;
			
						#if API
						Undo.RegisterSceneUndo ("Create Controller");
						#endif
						ctrl = new GameObject ();//create the GameObject
						#if !API
						Undo.RegisterCreatedObjectUndo (ctrl, "Create Controller");//create the GameObject
						#endif
						
						Controller setup = ctrl.AddComponent<Controller> ();					
						setup.transform.position = Vector3.zero;//set the position to (0,0,0)
						setup.name = "_TS Controller";//needs to be called controller to make sure that everything works
						setup.tag = Tags.C;
						Selection.activeGameObject = setup.gameObject;//select the new GameObject
				}//end CreateController
	
				TSGUI GUITools = new TSGUI ();//extra gui methods, which are used by TradeSys scripts
	
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
				private SerializedProperty showLinks;
				private SerializedProperty updateInterval;
				private SerializedProperty generateAtStart;
				private SerializedProperty pauseOption, pauseTime, pauseEnter, pauseExit;
				private SerializedProperty goods;
				private SerializedProperty manufacture;
				private SerializedProperty closestPosts, buyMultiple, sellMultiple, distanceWeight, profitWeight, purchasePercent, priceUpdates;
				private SerializedProperty postTags, groups, factions;
				private SerializedProperty units;
		#endregion

				///get the required information
				public void OnEnable ()
				{
						controllerSO = new SerializedObject (target);
						controllerNormal = (Controller)target;
						controllerNormal.tag = Tags.C;
		
						controllerNormal.tradePosts = GameObject.FindGameObjectsWithTag (Tags.TP);
						controllerNormal.postScripts = new TradePost[controllerNormal.tradePosts.Length];
						postScripts = new SerializedObject[controllerNormal.tradePosts.Length];
						for (int p = 0; p<controllerNormal.tradePosts.Length; p++) {
								controllerNormal.postScripts [p] = controllerNormal.tradePosts [p].GetComponent<TradePost> ();
								postScripts [p] = new SerializedObject (controllerNormal.postScripts [p]);
						}
		
						controllerNormal.traders = GameObject.FindGameObjectsWithTag (Tags.T);
						controllerNormal.traderScripts = new Trader[controllerNormal.traders.Length];
						traderScripts = new SerializedObject[controllerNormal.traders.Length];
						for (int t = 0; t<controllerNormal.traders.Length; t++) {
								controllerNormal.traderScripts [t] = controllerNormal.traders [t].GetComponent<Trader> ();
								traderScripts [t] = new SerializedObject (controllerNormal.traderScripts [t]);
						}
		
						#region get options
						showGN = controllerSO.FindProperty ("showGN");
						smallScroll = controllerSO.FindProperty ("smallScroll");
						sel = controllerNormal.selected;
						scrollPos = controllerNormal.scrollPos;
						#endregion
		
						#region get variables
						showLinks = controllerSO.FindProperty ("showLinks");
		
						updateInterval = controllerSO.FindProperty ("updateInterval");
		
						generateAtStart = controllerSO.FindProperty ("generateAtStart");
		
						pauseOption = controllerSO.FindProperty ("pauseOption");
						pauseTime = controllerSO.FindProperty ("pauseTime");
						pauseEnter = controllerSO.FindProperty ("pauseEnter");
						pauseExit = controllerSO.FindProperty ("pauseExit");
		
						goods = controllerSO.FindProperty ("goods");
						manufacture = controllerSO.FindProperty ("manufacture");
		
						closestPosts = controllerSO.FindProperty ("closestPosts");
						buyMultiple = controllerSO.FindProperty ("buyMultiple");
						sellMultiple = controllerSO.FindProperty ("sellMultiple");
						distanceWeight = controllerSO.FindProperty ("distanceWeight");
						profitWeight = controllerSO.FindProperty ("profitWeight");
						purchasePercent = controllerSO.FindProperty ("purchasePercent");
						priceUpdates = controllerSO.FindProperty ("priceUpdates");
		
						postTags = controllerSO.FindProperty ("postTags");
						groups = controllerSO.FindProperty ("groups");
						factions = controllerSO.FindProperty ("factions");
		
						units = controllerSO.FindProperty ("units");
						#endregion
		
						controllerNormal.SortAll ();
				}//end OnEnable
	
				public override void OnInspectorGUI ()
				{
						#if !API
						Undo.RecordObject (controllerNormal, "TradeSys Controller");
						EditorGUIUtility.fieldWidth = 30f;
						#endif	
			
						controllerSO.Update ();//needs to update
						for (int p = 0; p<postScripts.Length; p++) 
								postScripts [p].Update ();
				
						for (int t = 0; t<traderScripts.Length; t++)
								traderScripts [t].Update ();
		
						#region get goods names
						List<string> allNames = new List<string> ();//a list of all the names of goods to be shown in manufacturing, not in groups
						int[] groupLengthsG = new int[goods.arraySize];//contains the length of each group, so can convert between int and groupID and itemID
						controllerNormal.allNames = new string[controllerNormal.goods.Count][];//an array of names of goods in groups
			
						for (int g1 = 0; g1<goods.arraySize; g1++) {//go through all groups
								SerializedProperty currentGroup = goods.GetArrayElementAtIndex (g1).FindPropertyRelative ("goods");
								groupLengthsG [g1] = currentGroup.arraySize;
								controllerNormal.allNames [g1] = new string[currentGroup.arraySize];
								for (int g2 = 0; g2<currentGroup.arraySize; g2++) {//go through all goods in group
										string itemName = currentGroup.GetArrayElementAtIndex (g2).FindPropertyRelative ("name").stringValue;
										controllerNormal.allNames [g1] [g2] = itemName;
										if (showGN.boolValue)//if show group name
												allNames.Add (goods.GetArrayElementAtIndex (g1).FindPropertyRelative ("name").stringValue + " " + itemName);
										else
												allNames.Add (itemName);
								}
						}//end go through all groups
						#endregion
		
						GUITools.ManufactureInfo (controllerNormal);
		
						#region sort units
						for (int g = 0; g<controllerNormal.goods.Count; g++) {//go through groups
								for (int i = 0; i<controllerNormal.goods[g].goods.Count; i++) {//go through items
										Goods cI = controllerNormal.goods [g].goods [i];
										cI.unit = UnitString (cI.mass, false);
								}//end for items
						}//end for groups
						#endregion
		
						sel.C = GUITools.Toolbar (sel.C, new string[] {
								"Settings",
								"Goods",
								"Manufacturing"
						});//show a toolbar
		
						switch (sel.C) {
				#region settings
						case 0:
								scrollPos.S = GUITools.StartScroll (scrollPos.S, smallScroll);
					#region general	
								if (GUITools.TitleGroup (new GUIContent ("General options", "These are options which affect how the editors are displayed"), controllerSO.FindProperty ("genOp"), false)) {//show general options
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (showGN, new GUIContent ("Show group names", "In the manufacturing tab, when selecting items, show the name of the group that it belongs to. Means that if there are items with the same name but are in different groups, the names will appear"));
										bool sSB = smallScroll.boolValue;
										EditorGUILayout.PropertyField (smallScroll, new GUIContent ("Smaller scroll views", "In the other tabs, have a scroll pane of the added elements leaving the options above"));
										if (smallScroll.boolValue != sSB)//have a check to see if changed. if has changed, break because sometimes get an error
												break;
										EditorGUILayout.EndHorizontal ();
			
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (showLinks, new GUIContent ("Show trade links", "Show the possible trade links between trade posts"));
										EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
								}//end if showing general options
								EditorGUILayout.EndVertical ();
					#endregion
			
					#region game
								if (GUITools.TitleGroup (new GUIContent ("Game options", "These affect how TradeSys works"), controllerSO.FindProperty ("gamOp"), false)) {//show game options
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (updateInterval, new GUIContent ("Update interval", "Set this to a higher value list updates, trade calls and manufacturing updates are not as frequent"));
										UnitLabel ("s", 25);
										updateInterval.floatValue = 1 / EditorGUILayout.FloatField (new GUIContent ("Frequency", "The number of times per second to update"), (1 / updateInterval.floatValue));
										UnitLabel ("Hz", 35);
				
										if (updateInterval.floatValue < 0.02f)
												updateInterval.floatValue = 0.02f;
										EditorGUILayout.EndHorizontal ();
			
										EditorGUILayout.PropertyField (generateAtStart, new GUIContent ("Generate at start", "If the trade posts have been set, enable this. If they are being added through code, then disable. Call the GenerateDistances method in the controller once they have been added."));
								} //end if showing game options
								EditorGUILayout.EndVertical ();
					#endregion
			
					#region trade
								if (GUITools.TitleGroup (new GUIContent ("Trade options", "These affect how the trader destination post is decided"), controllerSO.FindProperty ("traOp"), false)) {//show trade options
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (closestPosts, new GUIContent ("Closest posts", "The number of closest posts that should be taken into account for finding the best post to go to. Decrease this value to improve performance"));
										EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
			
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (buyMultiple, new GUIContent ("Buy multiple", "If the number of items at a trade post multiplied by this value is less than the average number, the trade post will want to buy this item"));
										EditorGUILayout.PropertyField (sellMultiple, new GUIContent ("Sell multiple", "If the number of items at a trade post is greater than the average number multiplied by this value, then it will want to sell this item"));
										EditorGUILayout.EndHorizontal ();			
			
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (distanceWeight, new GUIContent ("Distance weight", "The value that the distance is multiplied by in order to help find the best post to go to. This could for example be the fuel cost per unit"));
										EditorGUILayout.PropertyField (profitWeight, new GUIContent ("Profit weight", "The value that the profit is multiplied by in order to help find the best post to go to. This is by taking the profit, multiplying by this value and subtracting the distance multiplied by the distance weight"));
										EditorGUILayout.EndHorizontal ();
			
										EditorGUILayout.BeginHorizontal ();
										//the purchase percent is as the value before multiplied by 100. This is so that when used, does not need to be constantly divided by 100
										//it is only in the editor
										purchasePercent.floatValue = 0.01f * EditorGUILayout.FloatField (new GUIContent ("Purchase percent", "This is the percentage of the sale price that the trade post buys items at"), purchasePercent.floatValue * 100);
										UnitLabel ("%", 32);
										EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();

										if (purchasePercent.floatValue < 0)
												purchasePercent.floatValue = 0;
										else if (purchasePercent.floatValue > 1)
												purchasePercent.floatValue = 1;
										//constrain between 0 and 1. Any less, gets paid for receiving, any more not making profit
			
										EditorGUILayout.PropertyField (priceUpdates, new GUIContent ("Update prices", "Update the prices of the item after each individual item has been purchased"));
			
										//make sure that the values never go below minimum
										if (closestPosts.intValue < 1)
												closestPosts.intValue = 1;
			
										if (buyMultiple.floatValue < 1)
												buyMultiple.floatValue = 1;
			
										if (sellMultiple.floatValue < 1)
												sellMultiple.floatValue = 1;
			
										if (distanceWeight.floatValue < 0)
												distanceWeight.floatValue = 0;
			
										if (profitWeight.floatValue < 0)
												profitWeight.floatValue = 0;
			
								}//end if showing trade options
								EditorGUILayout.EndVertical ();
					#endregion
			
					#region pausing
								if (GUITools.TitleGroup (new GUIContent ("Pause options", "These affect when and for how long a trader pauses for. Only affects cargo that is transferred"), controllerSO.FindProperty ("pauOp"), false)) {//show pause options	
										string unitText = "1";//the text used for the mass of items
										if (pauseOption.intValue == 2 || pauseOption.intValue == 3) {//if need to work out which unit
												for (int u = 0; u<controllerNormal.units.units.Count; u++) {//go through units
														Unit cU = controllerNormal.units.units [u];
														if (cU.min <= 1 && cU.max > 1) {//check in range
																unitText = (1 / (decimal)cU.min).ToString () + " " + cU.suffix;
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
									
										EditorGUILayout.BeginHorizontal ();
										pauseOption.intValue = EditorGUILayout.Popup (pauseOption.intValue, pauseOptions, "DropDownButton");
										if (pauseOption.intValue == 0 || pauseOption.intValue == 2) {//if general times, need to have the time option
												GUIContent pauseText = new GUIContent ("Pause time", "Set the pause time which every trader will pause for");
				
												if (pauseOption.intValue == 2)
														pauseText = new GUIContent ("Pause time per " + unitText, "Set the pause time per " + unitText + " of cargo carried");
				
												EditorGUILayout.PropertyField (pauseTime, pauseText);
												UnitLabel ("s", 25);
					
												if (pauseTime.floatValue < 0)
														pauseTime.floatValue = 0;
					
												if (pauseOption.intValue == 0) {//if set time
														for (int t = 0; t<controllerNormal.traderScripts.Length; t++)//go through all traders
																controllerNormal.traderScripts [t].stopTime = pauseTime.floatValue;
												}//end if set time
				
												if (pauseOption.intValue == 2) {//if each item
														for (int g = 0; g<controllerNormal.goods.Count; g++)//go through all groups
																for (int i = 0; i<controllerNormal.goods[g].goods.Count; i++)//go through all items
							//need to multiply the time [er unit by the mass
																		controllerNormal.goods [g].goods [i].pausePerUnit = pauseTime.floatValue * controllerNormal.goods [g].goods [i].mass;
												}//end if each item
										} else//end if 0 or 2
												EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
			
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (pauseEnter, new GUIContent ("Pause on entry", "Make the trader pause when entering a trade post, e.g. for unloading cargo. Pauses after unloading evrything"));
										EditorGUILayout.PropertyField (pauseExit, new GUIContent ("Pause on exit", "Make the trader pause when leaving a trade post, e.g. for loading cargo. Pauses after loading everything"));
										EditorGUILayout.EndHorizontal ();
								}//end if showing pause options
								EditorGUILayout.EndVertical ();
					#endregion
		
					#region post tags
								TGF (0, new GUIContent ("Post tags", "These can be added to any post, and can be used to affect how the post information is displayed, or what the post does. TradeSys does not do this, but could be useful when developing your scripts"),
						postTags, "tags", "Tag ");
					#endregion
			
					#region groups
								TGF (0, new GUIContent ("Groups", "Restrict which posts a trade post can trade with by selecting groups"),
						groups, "groups", "Group ");
					#endregion
			
					#region factions
								TGF (1, new GUIContent ("Factions", "Trade posts and factions can belong to factions, and will only trade if they are in the same faction"),
						factions, "factions", "Faction ");
					#endregion
			
					#region units
								if (GUITools.TitleGroup (new GUIContent ("Units", "Set the different units that can be used for the weights of goods. The unit min max must increase down the list, so the max of the first is the same as the min of the one after it"), units.FindPropertyRelative ("expanded"), false)) {//show the units
			
										SerializedProperty unitInfo = units.FindPropertyRelative ("units");
				
										int numberU = unitInfo.arraySize;
			
										EditorGUILayout.BeginHorizontal ();
										GUILayout.Space (10f);
												
										if (GUILayout.Button (new GUIContent ("g, kg, t", "Set up metric units of g, kg and t. 1 = 1 kg"), EditorStyles.miniButton)) {
													
												#if API
													Undo.RegisterUndo ((Controller)target, "TradeSys Controller");
												#endif
													
												controllerNormal.units = new Units{expanded = true, units = new List<Unit>{new Unit{suffix = "g", min = 0.000001f, max = 0.001f},
																																new Unit{suffix = "kg", min = 0.001f, max = 1f},
																																new Unit{suffix = "t", min = 1f, max = Mathf.Infinity}}};
										}//end if setting standard units
												
												
										GUILayout.FlexibleSpace ();
										EditorGUILayout.LabelField ("Number of units", numberU.ToString ());
				
										if (GUITools.PlusMinus (true)) {
												unitInfo.InsertArrayElementAtIndex (numberU);
												SerializedProperty inserted = unitInfo.GetArrayElementAtIndex (numberU);
												inserted.FindPropertyRelative ("suffix").stringValue = "Unit " + numberU;
												if (numberU == 0)//if is the first, set the min to the minimum mass
														inserted.FindPropertyRelative ("min").floatValue = 0.000001f;
												else//else set to the max mass of the previous
														inserted.FindPropertyRelative ("min").floatValue = unitInfo.GetArrayElementAtIndex (numberU - 1).FindPropertyRelative ("max").floatValue;
												inserted.FindPropertyRelative ("max").floatValue = Mathf.Infinity;//set the max to infinity
										}//if add pressed
			
										EditorGUILayout.EndHorizontal ();
				
										if (numberU > 0) {//only go through if there is something to show
												for (int u = 0; u<numberU; u++) {//for all units
														SerializedProperty cU = unitInfo.GetArrayElementAtIndex (u);//current unit
														SerializedProperty cUS = cU.FindPropertyRelative ("suffix");
					
														if (units.FindPropertyRelative ("units").GetArrayElementAtIndex (u).FindPropertyRelative ("min").floatValue == Mathf.Infinity)
																GUI.color = Color.red;
														GUITools.IndentGroup (0);
						
														EditorGUILayout.BeginHorizontal ();

														EditorGUILayout.PropertyField (cUS, new GUIContent ("Unit suffix"));//show editable name field
														if (cUS.stringValue == "")//if name is blank
																cUS.stringValue = "Unit " + u;
						
														if (GUITools.PlusMinus (false)) {
																unitInfo.DeleteArrayElementAtIndex (u);
																break;
														}//end if minus pressed
														EditorGUILayout.EndHorizontal ();
					
														EditorGUILayout.BeginHorizontal ();
														EditorGUI.indentLevel = 1;
														EditorGUILayout.PropertyField (cU.FindPropertyRelative ("min"), new GUIContent ("Min", "If the mass is greater than or equal to this value and less than the max value, then it will have this unit"));
														EditorGUILayout.PropertyField (cU.FindPropertyRelative ("max"), new GUIContent ("Max", "If the mass is less than this value and greater than or equal to the min value, then it will have this unit"));
														EditorGUILayout.EndHorizontal ();
						
														if (cU.FindPropertyRelative ("min").floatValue < 0.000001f)
																cU.FindPropertyRelative ("min").floatValue = 0.000001f;
														if (cU.FindPropertyRelative ("max").floatValue <= cU.FindPropertyRelative ("min").floatValue)
																cU.FindPropertyRelative ("max").floatValue = cU.FindPropertyRelative ("min").floatValue + 0.000001f;
					
														if (u > 0)//set the max of the previous to the min of this unit
																unitInfo.GetArrayElementAtIndex (u - 1).FindPropertyRelative ("max").floatValue = cU.FindPropertyRelative ("min").floatValue;
														if (u < numberU - 1)//set the min of the next to the max of this unit
																unitInfo.GetArrayElementAtIndex (u + 1).FindPropertyRelative ("min").floatValue = cU.FindPropertyRelative ("max").floatValue;
					
														EditorGUILayout.EndVertical ();
														EditorGUILayout.EndHorizontal ();
														GUI.color = Color.white;
												}//end for all units
										}//end if soemthing showing
			
										EditorGUI.indentLevel = 0;
										if (!InfinityCheck ("max") && numberU > 0)
												EditorGUILayout.HelpBox ("None of your units extend to infinity, so some items may not have a unit.\nMake sure that the max value of one of the units is set to infinity", MessageType.Warning);
										if (InfinityCheck ("min"))
												EditorGUILayout.HelpBox ("The min value of one of your units is infinty with the max also being infinity. As a result, the unit will not be able to be used", MessageType.Error);
								}//end if showing units
								EditorGUILayout.EndVertical ();
					#endregion
			
								if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
										EditorGUILayout.EndScrollView ();
								break;
				#endregion
			
				#region goods
						case 1:
								EditorGUI.indentLevel = 0;
			
								EditorGUILayout.LabelField (new GUIContent ("Total number", "The total number of goods that have been defined across all groups"), new GUIContent (allNames.Count.ToString (), "The total number of goods that have been defined across all groups"));
			
								EditorGUILayout.BeginHorizontal ();//have a toolbar with the different group names on
								scrollPos.GG = EditorGUILayout.BeginScrollView (scrollPos.GG, GUILayout.Height (40f));
					
								SerializedProperty selGG = controllerSO.FindProperty ("selected").FindPropertyRelative ("GG");
										
								string[] namesG = new string[goods.arraySize];
								for (int n = 0; n<goods.arraySize; n++)
										namesG [n] = goods.GetArrayElementAtIndex (n).FindPropertyRelative ("name").stringValue;
								int selGB = selGG.intValue;
								selGG.intValue = GUILayout.Toolbar (selGG.intValue, namesG);
								if (selGG.intValue != selGB)
										GUIUtility.keyboardControl = 0;
												
								EditorGUILayout.EndScrollView ();
												
								GUILayout.Space (3f);
												
								if (GUITools.PlusMinus (true) || goods.arraySize == 0) //if add groups pressed
										AddGoodsGroup (goods.arraySize);
				
								EditorGUILayout.EndHorizontal ();
			
								SerializedProperty currentGoodsGroup = goods.GetArrayElementAtIndex (selGG.intValue).FindPropertyRelative ("goods");
								SerializedProperty goodName = goods.GetArrayElementAtIndex (selGG.intValue).FindPropertyRelative ("name");
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.PropertyField (goodName);
								if (goodName.stringValue == "")
										goodName.stringValue = "Goods group " + selGG.intValue;

								GUI.enabled = goods.arraySize > 1;
								if (GUITools.PlusMinus (false)) {
										goods.DeleteArrayElementAtIndex (selGG.intValue);
										GroupRemove (selGG.intValue);
										if (selGG.intValue > 0)
												selGG.intValue--;
										GUIUtility.keyboardControl = 0;
										break;
								}
								GUI.enabled = true;
								EditorGUILayout.EndHorizontal ();
					//information with number of goods, sort, expand and collapse
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Number of goods", currentGoodsGroup.arraySize.ToString ());
								GUILayout.FlexibleSpace ();
								GUI.enabled = currentGoodsGroup.arraySize > 0 && !Application.isPlaying;
								if (GUILayout.Button (new GUIContent ("Sort", "Sort the goods by name alphabetically"), EditorStyles.miniButtonLeft, GUILayout.MinWidth (45f)))
										SortLists (selGG.intValue);
								GUI.enabled = true;
								GUITools.ExpandCollapse (currentGoodsGroup, "expanded", true);
								EditorGUILayout.EndHorizontal ();
			
								scrollPos.G = GUITools.StartScroll (scrollPos.G, smallScroll);
				
								if (currentGoodsGroup.arraySize == 0 && GUILayout.Button ("Add good")) {//if there are no goods added, have one larger add button
										currentGoodsGroup.InsertArrayElementAtIndex (0);
										currentGoodsGroup.GetArrayElementAtIndex (0).FindPropertyRelative ("mass").floatValue = 1;
										currentGoodsGroup.GetArrayElementAtIndex (0).FindPropertyRelative ("expanded").boolValue = true;
										EditLists (true, 0, selGG.intValue, true);
								}
		
								for (int g = 0; g<currentGoodsGroup.arraySize; g++) {//go through all goods, displaying them
										EditorGUI.indentLevel = 0;
										SerializedProperty currentGood = currentGoodsGroup.GetArrayElementAtIndex (g);//current good so is shorter
				
										EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
				
										if (currentGood.FindPropertyRelative ("expanded").boolValue)//if expanded
												EditorGUILayout.BeginVertical ("HelpBox");//show everything in a box together
				
										EditorGUILayout.BeginHorizontal ();
										currentGood.FindPropertyRelative ("expanded").boolValue = GUITools.TitleButton (new GUIContent (currentGood.FindPropertyRelative ("name").stringValue, ""), currentGood.FindPropertyRelative ("expanded"), "ControlLabel");
					
										GUILayout.FlexibleSpace ();//used so options are all at the end
										GUI.enabled = g > 0 && !Application.isPlaying;//disable move up if already at the top
										EditorGUILayout.BeginVertical ();//vertical to make the set of buttons central vertically
										GUILayout.Space (1f);//the space
										EditorGUILayout.BeginHorizontal ();//now needs a horizontal so all the buttons dont follow the vertical
										if (GUILayout.Button (new GUIContent ("▲", "Move up"), EditorStyles.miniButtonLeft)) {
												currentGoodsGroup.MoveArrayElement (g, g - 1);
												MoveFromPoint (g - 1, selGG.intValue, true);
												ListShuffle (true, g, g - 1, selGG.intValue);
										}
										GUI.enabled = !Application.isPlaying;//set back to enabled if not playing
										if (GUILayout.Button (new GUIContent ("+", "Add good after"), EditorStyles.miniButtonMid)) {
												currentGoodsGroup.InsertArrayElementAtIndex (g + 1);
												EditLists (true, g + 1, selGG.intValue, true);
					
												SerializedProperty inserted = currentGoodsGroup.GetArrayElementAtIndex (g + 1);
												inserted.FindPropertyRelative ("name").stringValue = "Element " + (g + 1);
												inserted.FindPropertyRelative ("expanded").boolValue = currentGood.FindPropertyRelative ("expanded").boolValue;
												inserted.FindPropertyRelative ("maxPrice").intValue = 0;
												inserted.FindPropertyRelative ("basePrice").intValue = inserted.FindPropertyRelative ("minPrice").intValue = 1;
												inserted.FindPropertyRelative ("mass").floatValue = 1;
												inserted.FindPropertyRelative ("itemCrate").objectReferenceValue = null;
												inserted.FindPropertyRelative ("pausePerUnit").floatValue = currentGood.FindPropertyRelative ("pausePerUnit").floatValue;
												MovePointsAfter (g, selGG.intValue, false);
					
												GUIUtility.keyboardControl = 0;
										}
										if (GUILayout.Button (new GUIContent ("C", "Copy good after"), EditorStyles.miniButtonMid)) {
												currentGoodsGroup.InsertArrayElementAtIndex (g + 1);
												EditLists (true, g + 1, selGG.intValue, true);
												//although this most times duplicates the point, if a normal addition has been made, doesnt set the correct values
												SerializedProperty inserted = currentGoodsGroup.GetArrayElementAtIndex (g + 1);
												inserted.FindPropertyRelative ("name").stringValue = currentGood.FindPropertyRelative ("name").stringValue + " Copy";
												inserted.FindPropertyRelative ("expanded").boolValue = currentGood.FindPropertyRelative ("expanded").boolValue;
												inserted.FindPropertyRelative ("basePrice").intValue = currentGood.FindPropertyRelative ("basePrice").intValue;
												inserted.FindPropertyRelative ("minPrice").intValue = currentGood.FindPropertyRelative ("minPrice").intValue;
												inserted.FindPropertyRelative ("maxPrice").intValue = currentGood.FindPropertyRelative ("maxPrice").intValue;
												inserted.FindPropertyRelative ("mass").floatValue = currentGood.FindPropertyRelative ("mass").floatValue;
												inserted.FindPropertyRelative ("itemCrate").objectReferenceValue = currentGood.FindPropertyRelative ("itemCrate").objectReferenceValue;
												inserted.FindPropertyRelative ("pausePerUnit").floatValue = currentGood.FindPropertyRelative ("pausePerUnit").floatValue;
												MovePointsAfter (g, selGG.intValue, false);
					
												GUIUtility.keyboardControl = 0;
										}
										if (GUILayout.Button (new GUIContent ("-", "Remove good"), EditorStyles.miniButtonMid)) {
												EditLists (true, g, selGG.intValue, false);
				
												currentGoodsGroup.DeleteArrayElementAtIndex (g);
												MovePointsAfter (g, selGG.intValue, true);
												break;
										}
										GUI.enabled = g < currentGoodsGroup.arraySize - 1 && !Application.isPlaying;//disable if already at the bottom
										if (GUILayout.Button (new GUIContent ("▼", "Move down"), EditorStyles.miniButtonRight)) {
												currentGoodsGroup.MoveArrayElement (g, g + 1);
												MoveFromPoint (g + 1, selGG.intValue, false);
												ListShuffle (true, g, g + 1, selGG.intValue);
										}
										GUI.enabled = true;//make enabled again
										EditorGUILayout.EndHorizontal ();
										EditorGUILayout.EndVertical ();
										EditorGUILayout.EndHorizontal ();
				
										if (currentGood.FindPropertyRelative ("name").stringValue == "")//make sure that the name isn't blank - may cause problems if it is
												currentGood.FindPropertyRelative ("name").stringValue = "Element " + g;
				
										if (currentGood.FindPropertyRelative ("expanded").boolValue) {//if is expanded
												EditorGUI.indentLevel = 1;
					
												string unitString = "";
												if (currentGood.FindPropertyRelative ("unit").stringValue.Length > 0)
														unitString = " (" + currentGood.FindPropertyRelative ("unit").stringValue + ")";
														
												SerializedProperty name = currentGood.FindPropertyRelative ("name");
												EditorGUILayout.PropertyField (name, new GUIContent ("Name", "This is the name of the item"));
							
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.PropertyField (currentGood.FindPropertyRelative ("mass"), new GUIContent ("Mass" + unitString, "The mass will affect the unit, and how much can be carried by a trader"));
												currentGood.FindPropertyRelative ("mass").floatValue = Mathf.Max (0.000001f, currentGood.FindPropertyRelative ("mass").floatValue);
												//limit the mass, so is not <=0 so that masses correctly work
												EditorGUILayout.LabelField ("");
												EditorGUILayout.EndHorizontal ();
					
												if (pauseOption.intValue == 3) {//if the pause option if for specific items
														EditorGUILayout.BeginHorizontal ();
														EditorGUILayout.PropertyField (currentGood.FindPropertyRelative ("pausePerUnit"), new GUIContent ("Pause time", "Set how long a trader needs to pause for per unit of this item"));
														EditorGUILayout.LabelField ("");
														EditorGUILayout.EndHorizontal ();
						
														if (currentGood.FindPropertyRelative ("pausePerUnit").floatValue < 0)
																currentGood.FindPropertyRelative ("pausePerUnit").floatValue = 0;
												}//end pause time
					
												EditorGUILayout.LabelField ("Prices", EditorStyles.boldLabel);//bold prices label for sub section
												EditorGUI.indentLevel = 2;
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.PropertyField (currentGood.FindPropertyRelative ("basePrice"), new GUIContent ("Base price", "If a trade post has the average number of this item, this is the price. The prices are set against this"));
												EditorGUILayout.LabelField ("");//blank label so does not cover whole width
												EditorGUILayout.EndHorizontal ();
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.PropertyField (currentGood.FindPropertyRelative ("minPrice"), new GUIContent ("Min price", "This is the minimum price the item can be"));
												EditorGUILayout.PropertyField (currentGood.FindPropertyRelative ("maxPrice"), new GUIContent ("Max price", "This is the highest price the item can be"));
												EditorGUILayout.EndHorizontal ();
					
												//get the prices
												int mi = currentGood.FindPropertyRelative ("minPrice").intValue;
												int ma = currentGood.FindPropertyRelative ("maxPrice").intValue;
												int ba = currentGood.FindPropertyRelative ("basePrice").intValue;
					
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
					
												//set the prices
												currentGood.FindPropertyRelative ("minPrice").intValue = mi;
												currentGood.FindPropertyRelative ("maxPrice").intValue = ma;
												currentGood.FindPropertyRelative ("basePrice").intValue = ba;
					
												EditorGUILayout.EndVertical ();
										}//end if expanded
								}//end for all goods
								EditorGUI.indentLevel = 0;
								if (currentGoodsGroup.arraySize > 0)
										EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
								if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
										EditorGUILayout.EndScrollView ();
								break;
				#endregion
			
				#region manufacturing
						case 2:
								EditorGUI.indentLevel = 0;
								int[] groupLengthsM = new int[manufacture.arraySize];//a running total of the number of processes
								int manufactureCount = 0;//the total number of manufacturing processes;
			
								SerializedProperty selMG = controllerSO.FindProperty ("selected").FindPropertyRelative ("MG");
			
								for (int m = 0; m<manufacture.arraySize; m++) {//go through all manufacture
										manufactureCount += manufacture.GetArrayElementAtIndex (m).FindPropertyRelative ("manufacture").arraySize;
										if (m == 0)
												groupLengthsM [m] = 0;
										else { 
												int prev = manufacture.GetArrayElementAtIndex (m - 1).FindPropertyRelative ("manufacture").arraySize;
												if (m == 1)
														groupLengthsM [m] = prev;
												else
														groupLengthsM [m] = groupLengthsM [m - 1] + prev;
										}
								}//end for manufacture
			
								EditorGUILayout.LabelField (new GUIContent ("Total number", "The total number of manufacturing processes that have been defined across all groups"), new GUIContent (manufactureCount.ToString (), "The total number of manufacturing processes that have been defined across all groups"));
			
								EditorGUILayout.BeginHorizontal ();//have a toolbar with the different group names on
								scrollPos.MG = EditorGUILayout.BeginScrollView (scrollPos.MG, GUILayout.Height (40f));

								string[] namesM = new string[manufacture.arraySize];
								for (int n = 0; n<manufacture.arraySize; n++)
										namesM [n] = manufacture.GetArrayElementAtIndex (n).FindPropertyRelative ("name").stringValue;
										
								int selMB = selMG.intValue;
								selMG.intValue = GUILayout.Toolbar (selMG.intValue, namesM);
										
								if (selMG.intValue != selMB)
										GUIUtility.keyboardControl = 0;
										
								EditorGUILayout.EndScrollView ();										
			
			
								GUILayout.Space (3f);
								if (GUITools.PlusMinus (true) || manufacture.arraySize == 0) //if add groups pressed
										AddManGroup (manufacture.arraySize);
								EditorGUILayout.EndHorizontal ();
										
								SerializedProperty currentManGroup = manufacture.GetArrayElementAtIndex (selMG.intValue).FindPropertyRelative ("manufacture");
								SerializedProperty manName = manufacture.GetArrayElementAtIndex (selMG.intValue).FindPropertyRelative ("name");
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.PropertyField (manName);
								if (manName.stringValue == "")
										manName.stringValue = "Manufacture group " + selMG.intValue;

								GUI.enabled = manufacture.arraySize > 1;
								if (GUITools.PlusMinus (false)) {
										manufacture.DeleteArrayElementAtIndex (selMG.intValue);
										GroupRemove (postScripts, "manufacture", selMG.intValue);
										GroupRemove (traderScripts, "manufacture", selMG.intValue);
										if (selMG.intValue > 0)
												selMG.intValue--;
										GUIUtility.keyboardControl = 0;
										break;
								}
								GUI.enabled = true;
								EditorGUILayout.EndHorizontal ();
					//information with number of processes, expand and collapse
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Number of processes", currentManGroup.arraySize.ToString ());
								GUILayout.FlexibleSpace ();
			
								GUI.enabled = manufacture.arraySize > 0;
								if (GUILayout.Button (new GUIContent ("Check", "Check all manufacturing at trade posts to give an estimate on changes of numbers of each item"), EditorStyles.miniButtonLeft, GUILayout.MinWidth (45f)))
										CheckManufacturing (manufactureCount, groupLengthsM, groupLengthsG, manufacture);
								GUI.enabled = true;
			
								GUITools.ExpandCollapse (currentManGroup, "expanded", true);
								EditorGUILayout.EndHorizontal ();
			
								scrollPos.M = GUITools.StartScroll (scrollPos.M, smallScroll);
			
								if (currentManGroup.arraySize == 0 && GUILayout.Button ("Add process")) {//if there are no processes added, have one larger add button
										currentManGroup.InsertArrayElementAtIndex (0);
										currentManGroup.GetArrayElementAtIndex (0).FindPropertyRelative ("expanded").boolValue = true;
										currentManGroup.GetArrayElementAtIndex (0).FindPropertyRelative ("needing").arraySize = currentManGroup.GetArrayElementAtIndex (0).FindPropertyRelative ("making").arraySize = 0;
										EditLists (false, 0, selMG.intValue, true);
										if (allNames.Count == 0)//if there are no goods, then show error
												Debug.LogError ("There are no possible types to manufacture.");
								}
			
								for (int m = 0; m<currentManGroup.arraySize; m++) {
										EditorGUI.indentLevel = 0;
										SerializedProperty currentManufacture = currentManGroup.GetArrayElementAtIndex (m);
				
										EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
				
										if (currentManufacture.FindPropertyRelative ("expanded").boolValue)
												EditorGUILayout.BeginVertical ("HelpBox");
				
										EditorGUILayout.BeginHorizontal ();
										currentManufacture.FindPropertyRelative ("expanded").boolValue = GUITools.TitleButton (new GUIContent (currentManufacture.FindPropertyRelative ("name").stringValue, currentManufacture.FindPropertyRelative ("tooltip").stringValue), currentManufacture.FindPropertyRelative ("expanded"), "ControlLabel");

										GUILayout.FlexibleSpace ();//used so options are all at the end
										GUI.enabled = m > 0 && !Application.isPlaying;//disable move up if already at the top
										EditorGUILayout.BeginVertical ();//vertical to make the set of buttons central vertically
										GUILayout.Space (1f);//the space
										EditorGUILayout.BeginHorizontal ();//now needs a horizontal so all the buttons dont follow the vertical
										if (GUILayout.Button (new GUIContent ("▲", "Move up"), EditorStyles.miniButtonLeft)) {
												currentManGroup.MoveArrayElement (m, m - 1);
												ListShuffle (false, m, m - 1, selMG.intValue);
										}
										GUI.enabled = !Application.isPlaying;//set back to enabled if not playing
										if (GUILayout.Button (new GUIContent ("+", "Add process after"), EditorStyles.miniButtonMid)) {
												currentManGroup.InsertArrayElementAtIndex (m + 1);
												EditLists (false, m + 1, selMG.intValue, true);
					
												SerializedProperty inserted = currentManGroup.GetArrayElementAtIndex (m + 1);
												inserted.FindPropertyRelative ("name").stringValue = "Element " + (m + 1);
												inserted.FindPropertyRelative ("expanded").boolValue = currentManGroup.GetArrayElementAtIndex (m).FindPropertyRelative ("expanded").boolValue;
												inserted.FindPropertyRelative ("needing").arraySize = inserted.FindPropertyRelative ("making").arraySize = 0;
												GUIUtility.keyboardControl = 0;
										}
										if (GUILayout.Button (new GUIContent ("C", "Copy process after"), EditorStyles.miniButtonMid)) {
												currentManGroup.InsertArrayElementAtIndex (m + 1);
												EditLists (false, m + 1, selMG.intValue, true);
												CopyManufacture (currentManufacture, currentManGroup.GetArrayElementAtIndex (m + 1));
												GUIUtility.keyboardControl = 0;
										}
										if (GUILayout.Button (new GUIContent ("-", "Remove process"), EditorStyles.miniButtonMid)) {
												currentManGroup.DeleteArrayElementAtIndex (m);
												EditLists (false, m, selMG.intValue, false);
												break;
										}
										GUI.enabled = m < currentManGroup.arraySize - 1 && !Application.isPlaying;//disable if already at the bottom
										if (GUILayout.Button (new GUIContent ("▼", "Move down"), EditorStyles.miniButtonRight)) {
												currentManGroup.MoveArrayElement (m, m + 1);
												ListShuffle (false, m, m + 1, selMG.intValue);
										}
										GUI.enabled = true;//make enabled again
										EditorGUILayout.EndHorizontal ();
										EditorGUILayout.EndVertical ();
										EditorGUILayout.EndHorizontal ();
			
										if (currentManufacture.FindPropertyRelative ("name").stringValue == "")//make sure that the name isn't blank - may cause problems if it is
												currentManufacture.FindPropertyRelative ("name").stringValue = "Element " + m;
					
										if (currentManufacture.FindPropertyRelative ("expanded").boolValue) {
												EditorGUI.indentLevel = 1;
												EditorGUILayout.PropertyField (currentManufacture.FindPropertyRelative ("name"), new GUIContent ("Name", "This is the name of the process"));

												controllerNormal.ManufactureMass();
												EditorGUILayout.LabelField (new GUIContent (UnitString(currentManufacture.FindPropertyRelative("needingMass").floatValue, false) + " ► " + UnitString(currentManufacture.FindPropertyRelative("makingMass").floatValue, false),
						                                            "Shows the mass conversion of the needing and making items"));
														
												EditorGUI.indentLevel = 1;
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.LabelField ("Needing", EditorStyles.boldLabel);
												if (GUITools.PlusMinus (true)) {//add needing element
																
														#if API
													Undo.RegisterUndo ((Controller)target, "TradeSys Controller");
														#endif
													
														controllerNormal.manufacture [selMG.intValue].manufacture [m].needing.Add (new NeedMake ());
														controllerNormal.manufacture [selMG.intValue].manufacture [m].needing [controllerNormal.manufacture [selMG.intValue].manufacture [m].needing.Count - 1].number = 1;
												}
												EditorGUILayout.EndHorizontal ();
					
												ShowNM (currentManufacture, true, groupLengthsG, allNames.ToArray ());
				
												EditorGUI.indentLevel = 1;
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.LabelField ("Making", EditorStyles.boldLabel);
												if (GUITools.PlusMinus (true)) {//add making element
																
														#if API
													Undo.RegisterUndo ((Controller)target, "TradeSys Controller");
														#endif
													
														controllerNormal.manufacture [selMG.intValue].manufacture [m].making.Add (new NeedMake ());
														controllerNormal.manufacture [selMG.intValue].manufacture [m].making [controllerNormal.manufacture [selMG.intValue].manufacture [m].making.Count - 1].number = 1;
												}
												EditorGUILayout.EndHorizontal ();
					
												ShowNM (currentManufacture, false, groupLengthsG, allNames.ToArray ());
					
												EditorGUILayout.EndVertical ();
										}//end if expanded
								}//end for all manufacture
								EditorGUI.indentLevel = 0;
								if (currentManGroup.arraySize > 0)
										EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
								if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
										EditorGUILayout.EndScrollView ();
								break;
				#endregion
						}//end switch
						controllerSO.ApplyModifiedProperties ();//needs to apply modified properties
		
						for (int p = 0; p<postScripts.Length; p++) 
								postScripts [p].ApplyModifiedProperties ();
						for (int t = 0; t<traderScripts.Length; t++)
								traderScripts [t].ApplyModifiedProperties ();
				}//end OnInspectorGUI
	
				///this is to go through the manufacturing lists made, and make sure that each itemID is still pointing to the correct one
				void SortLists (int groupNo)
				{
						
						#if API
													Undo.RegisterUndo ((Controller)target, "TradeSys Controller");
						#endif
													
						Goods[] before = controllerNormal.goods [groupNo].goods.ToArray ();
						controllerNormal.goods [groupNo].goods.Sort ();
	
						for (int m1 = 0; m1<controllerNormal.manufacture.Count; m1++) {//go through all manufacturing groups
								for (int m2 = 0; m2<controllerNormal.manufacture[m1].manufacture.Count; m2++) {//go through all manufacturing processes
										for (int n = 0; n<controllerNormal.manufacture[m1].manufacture[m2].needing.Count; n++) {//go through all needing
												if (controllerNormal.manufacture [m1].manufacture [m2].needing [n].groupID == groupNo)//if is in the group that had been sorted
														controllerNormal.manufacture [m1].manufacture [m2].needing [n].itemID = controllerNormal.goods [groupNo].goods.FindIndex (x => x.name == before [controllerNormal.manufacture [m1].manufacture [m2].needing [n].itemID].name);
										}//end for needing
										for (int n = 0; n<controllerNormal.manufacture[m1].manufacture[m2].making.Count; n++) {//go through all making
												if (controllerNormal.manufacture [m1].manufacture [m2].making [n].groupID == groupNo)//if is in the group that had been sorted
														controllerNormal.manufacture [m1].manufacture [m2].making [n].itemID = controllerNormal.goods [groupNo].goods.FindIndex (x => x.name == before [controllerNormal.manufacture [m1].manufacture [m2].making [n].itemID].name);
										}//end for making
								}//end for all manufacturing processes
						}//end for all manufacturing groups
		
						for (int p = 0; p<controllerNormal.postScripts.Length; p++) {//go through all posts
								StockGroup groupStock = controllerNormal.postScripts [p].stock [groupNo];//the stock group at the post
								for (int s = 0; s<groupStock.stock.Count; s++)//go throigh all items
										groupStock.stock [s].name = before [s].name;//and set the name to what it was before sorting
								groupStock.stock.Sort ();//now sort the items, and will be sorted the same as the other stock list
						}//end for all posts
		
						for (int t = 0; t<controllerNormal.traderScripts.Length; t++) {//go through all traders
								ItemGroup itemGroup = controllerNormal.traderScripts [t].items [groupNo];//allow group for the trader
								for (int a = 0; a<itemGroup.items.Count; a++)//go through all allow
										itemGroup.items [a].name = before [a].name;//and set the name to what it was before sorting
								itemGroup.items.Sort ();
						}//end for all traders
				}//end SortLists
	
				///when a good gets moved up or down, the items in manufacturing also need changing
				///itemNo is the destination array number of the changed
				void MoveFromPoint (int itemNo, int groupNo, bool up)
				{
						for (int m1 = 0; m1<manufacture.arraySize; m1++) {//go through manufacturing groups
								SerializedProperty manufactureGroup = manufacture.GetArrayElementAtIndex (m1).FindPropertyRelative ("manufacture");
								for (int m2 = 0; m2<manufactureGroup.arraySize; m2++) {//go through processes
										SerializedProperty currentManufacture = manufactureGroup.GetArrayElementAtIndex (m2);
								
										for (int n = 0; n<currentManufacture.FindPropertyRelative("needing").arraySize; n++) {
												SerializedProperty currentNeeding = currentManufacture.FindPropertyRelative ("needing").GetArrayElementAtIndex (n);
												if (currentNeeding.FindPropertyRelative ("groupID").intValue == groupNo)//if is in the group that has been changed
														ManufactureMove (currentNeeding.FindPropertyRelative ("itemID"), itemNo, up);
										}//end for sort all needing
										for (int n = 0; n<currentManufacture.FindPropertyRelative("making").arraySize; n++) {
												SerializedProperty currentMaking = currentManufacture.FindPropertyRelative ("making").GetArrayElementAtIndex (n);
												if (currentMaking.FindPropertyRelative ("groupID").intValue == groupNo)
														ManufactureMove (currentMaking.FindPropertyRelative ("itemID"), itemNo, up);
										}//end for sort all making
								
								}//end for manufacturing processes
						}//end for all manufacturing groups
				}//end MoveFromPoint
	
				///move the manufacturing items 
				void ManufactureMove (SerializedProperty currentNMID, int itemNo, bool up)
				{
						if ((up && currentNMID.intValue == itemNo + 1) || (!up && currentNMID.intValue == itemNo))
				//move up
								currentNMID.intValue --;
						else if ((up && currentNMID.intValue == itemNo) || (!up && currentNMID.intValue == itemNo - 1))
				//move down
								currentNMID.intValue ++;
				}//end ManufactureMove
	
				///removing a single manufacturing item
				void MovePointsAfter (int itemAfter, int groupNo, bool removing)
				{
						for (int m1 = 0; m1<manufacture.arraySize; m1++) {//for manufacture groups
								for (int m2 = 0; m2<manufacture.GetArrayElementAtIndex (m1).FindPropertyRelative("manufacture").arraySize; m2++) {//for manufacture processes
										SerializedProperty currentManufacture = manufacture.GetArrayElementAtIndex (m1).FindPropertyRelative ("manufacture").GetArrayElementAtIndex (m2);
			
										for (int n = 0; n<currentManufacture.FindPropertyRelative("needing").arraySize; n++) {
												SerializedProperty currentNeeding = currentManufacture.FindPropertyRelative ("needing").GetArrayElementAtIndex (n);
												if (currentNeeding.FindPropertyRelative ("groupID").intValue == groupNo) {//check in the correct group
														if (removing)
																RemoveUp (currentNeeding.FindPropertyRelative ("itemID"), itemAfter);
														else
																MoveDown (currentNeeding.FindPropertyRelative ("itemID"), itemAfter);
												}
										}//end for sort all needing
										for (int n = 0; n<currentManufacture.FindPropertyRelative("making").arraySize; n++) {
												SerializedProperty currentMaking = currentManufacture.FindPropertyRelative ("making").GetArrayElementAtIndex (n);
												if (currentMaking.FindPropertyRelative ("groupID").intValue == groupNo) {
														if (removing)
																RemoveUp (currentMaking.FindPropertyRelative ("itemID"), itemAfter);
														else
																MoveDown (currentMaking.FindPropertyRelative ("itemID"), itemAfter);
												}
										}//end for sort all making
								}//end for all manufacture processes
						}//end for all manufacture groups
				}//end MovePointsAfter
	
				///decrease the values of all of the other items, and set to -1 if using deleted item
				void RemoveUp (SerializedProperty currentNMID, int itemRemoved)
				{
						if (currentNMID.intValue == itemRemoved) {
								currentNMID.intValue = -1;
								Debug.LogError ("Some elements in manufacturing processes are undefined");
						} else if (currentNMID.intValue > itemRemoved)
								currentNMID.intValue --;
				}//end RemoveUp
	
				///increase the value of items after the inserted item
				void MoveDown (SerializedProperty currentNMID, int itemAfter)
				{
						if (currentNMID.intValue > itemAfter)
								currentNMID.intValue ++;
				}//end MoveDown
	
				///copy the manufacture process
				void CopyManufacture (SerializedProperty original, SerializedProperty created)
				{
						created.FindPropertyRelative ("name").stringValue = original.FindPropertyRelative ("name").stringValue + " Copy";
						created.FindPropertyRelative ("tooltip").stringValue = original.FindPropertyRelative ("tooltip").stringValue;
						created.FindPropertyRelative ("expanded").boolValue = original.FindPropertyRelative ("expanded").boolValue;
		
						created.FindPropertyRelative ("needing").arraySize = original.FindPropertyRelative ("needing").arraySize;
						for (int n = 0; n<created.FindPropertyRelative("needing").arraySize; n++) {
								SerializedProperty currentNeeding = created.FindPropertyRelative ("needing").GetArrayElementAtIndex (n);
								currentNeeding.FindPropertyRelative ("itemID").intValue = original.FindPropertyRelative ("needing").GetArrayElementAtIndex (n).FindPropertyRelative ("itemID").intValue;
								currentNeeding.FindPropertyRelative ("number").intValue = original.FindPropertyRelative ("needing").GetArrayElementAtIndex (n).FindPropertyRelative ("number").intValue;
						}//end for copy needing
		
						created.FindPropertyRelative ("making").arraySize = original.FindPropertyRelative ("making").arraySize;
						for (int n = 0; n<created.FindPropertyRelative("making").arraySize; n++) {
								SerializedProperty currentMaking = created.FindPropertyRelative ("making").GetArrayElementAtIndex (n);
								currentMaking.FindPropertyRelative ("itemID").intValue = original.FindPropertyRelative ("making").GetArrayElementAtIndex (n).FindPropertyRelative ("itemID").intValue;
								currentMaking.FindPropertyRelative ("number").intValue = original.FindPropertyRelative ("making").GetArrayElementAtIndex (n).FindPropertyRelative ("number").intValue;
						}//end for copy making
				}//end CopyManufacture
	
				///show the needing or making elements
				void ShowNM (SerializedProperty currentManufacture, bool needing, int[] groupLengthsG, string[] allNames)
				{
						SerializedProperty currentNM;
						if (needing)
								currentNM = currentManufacture.FindPropertyRelative ("needing");
						else
								currentNM = currentManufacture.FindPropertyRelative ("making");
						for (int nm = 0; nm<currentNM.arraySize; nm++) {
								EditorGUILayout.BeginHorizontal ();
								EditorGUI.indentLevel = 2;
								if (IsDuplicate (currentManufacture, nm, needing) || currentNM.GetArrayElementAtIndex (nm).FindPropertyRelative ("itemID").intValue == -1)
										GUI.color = Color.red;
								int selected = ConvertToSelected (currentNM.GetArrayElementAtIndex (nm), groupLengthsG);
								selected = EditorGUILayout.Popup (selected, allNames, "DropDownButton");
								ConvertFromSelected (currentNM.GetArrayElementAtIndex (nm), groupLengthsG, selected);
					
								EditorGUILayout.PropertyField (currentNM.GetArrayElementAtIndex (nm).FindPropertyRelative ("number"), new GUIContent ("Number"));
						
								if (currentNM.GetArrayElementAtIndex (nm).FindPropertyRelative ("number").intValue < 1)
										currentNM.GetArrayElementAtIndex (nm).FindPropertyRelative ("number").intValue = 1;
								GUI.color = Color.white;				

								if (GUITools.PlusMinus (false))
										currentNM.DeleteArrayElementAtIndex (nm);
								EditorGUILayout.EndHorizontal ();
						}//end for all NM
				}//end ShowNM
	
				///check if a needing or making element is duplicated
				bool IsDuplicate (SerializedProperty currentManufacture, int check, bool needing)
				{
						SerializedProperty currentNeeding = currentManufacture.FindPropertyRelative ("needing");
						SerializedProperty currentMaking = currentManufacture.FindPropertyRelative ("making");
		
						SerializedProperty currentCheck;
		
						if (needing)
								currentCheck = currentNeeding.GetArrayElementAtIndex (check);
						else 
								currentCheck = currentMaking.GetArrayElementAtIndex (check);
		
						for (int n = 0; n<currentNeeding.arraySize; n++) {
								SerializedProperty currentN = currentNeeding.GetArrayElementAtIndex (n);
								if (needing) {//if is originally in needing
										if (n != check) //check that is not checking itself
										if (IsDuplicate (currentN, currentCheck))
												return true;
								} else//if is from making, can check all, but needs to be from making if (IsDuplicate (currentN, currentCheck))
					if (IsDuplicate (currentN, currentCheck))
										return true;
						}//check in needing
		
						for (int m = 0; m<currentMaking.arraySize; m++) {
								SerializedProperty currentM = currentMaking.GetArrayElementAtIndex (m);
								if (!needing) {//if is originally in making
										if (m != check)//check that is not checking itself
										if (IsDuplicate (currentM, currentCheck))
												return true;
								} else//if is from needing, can check all, but needs to be from needing if (IsDuplicate (currentM, currentCheck))
					if (IsDuplicate (currentM, currentCheck))
										return true;
						}//check in making
		
						return false;
				}//end IsDuplicate
		
				///check the current item and the checking item
				bool IsDuplicate (SerializedProperty currentNM, SerializedProperty currentCheck)
				{
						if (currentNM.FindPropertyRelative ("itemID").intValue == currentCheck.FindPropertyRelative ("itemID").intValue &&
								currentNM.FindPropertyRelative ("groupID").intValue == currentCheck.FindPropertyRelative ("groupID").intValue)
								return true;
						return false;
				}//end IsDuplicate
	
				///convert the itemID and groupID to a single int for manufacture selection
				public int ConvertToSelected (SerializedProperty currentNM, int[] lengths)
				{
						if (currentNM.FindPropertyRelative ("itemID").intValue == -1)//if undefined, return -1
								return -1;
						int selected = 0;
						for (int g = 0; g<currentNM.FindPropertyRelative("groupID").intValue; g++)
								selected += lengths [g];
						selected += currentNM.FindPropertyRelative ("itemID").intValue;
						return selected;
				}//end ConvertToSelected
	
				///convert from single int to a groupID and an itemID
				void ConvertFromSelected (SerializedProperty currentNM, int[] lengths, int selected)
				{
						if (selected == -1)
								currentNM.FindPropertyRelative ("itemID").intValue = currentNM.FindPropertyRelative ("groupID").intValue = -1;
				
						int groupNo = 0;
						while (selected >= lengths[groupNo]) {
								selected -= lengths [groupNo];
								groupNo++;
						}
						currentNM.FindPropertyRelative ("itemID").intValue = selected;
						currentNM.FindPropertyRelative ("groupID").intValue = groupNo;
				}//end ConvertFromSelected
	
				///remove a goods group, sorting all manufacture pointers
				void GroupRemove (int groupNo)
				{
						#region manufacture
						for (int m1= 0; m1<manufacture.arraySize; m1++) {//manufacture groups
								for (int m2 = 0; m2<manufacture.GetArrayElementAtIndex(m1).FindPropertyRelative("manufacture").arraySize; m2++) {//manufacture processes
										SerializedProperty currentManufacture = manufacture.GetArrayElementAtIndex (m1).FindPropertyRelative ("manufacture").GetArrayElementAtIndex (m2);
			
										for (int n = 0; n<currentManufacture.FindPropertyRelative("needing").arraySize; n++) {
												SerializedProperty currentNeeding = currentManufacture.FindPropertyRelative ("needing").GetArrayElementAtIndex (n);
												if (currentNeeding.FindPropertyRelative ("groupID").intValue == groupNo) {//if same group as removed
														currentNeeding.FindPropertyRelative ("groupID").intValue = -1;
														currentNeeding.FindPropertyRelative ("itemID").intValue = -1;
														Debug.LogError ("Some elements in manufacturing processes are undefined");
												} else if (currentNeeding.FindPropertyRelative ("groupID").intValue > groupNo)//if greater than removed, then reduce selected group
														currentNeeding.FindPropertyRelative ("groupID").intValue--;
										}//end for all needing
		
										for (int n = 0; n<currentManufacture.FindPropertyRelative("making").arraySize; n++) {
												SerializedProperty currentMaking = currentManufacture.FindPropertyRelative ("making").GetArrayElementAtIndex (n);
												if (currentMaking.FindPropertyRelative ("groupID").intValue == groupNo) {//if same group as removed
														currentMaking.FindPropertyRelative ("groupID").intValue = -1;
														currentMaking.FindPropertyRelative ("itemID").intValue = -1;
														Debug.LogError ("Some elements in manufacturing processes are undefined");
												} else if (currentMaking.FindPropertyRelative ("groupID").intValue > groupNo)//if greater than removed, then reduce selected group
														currentMaking.FindPropertyRelative ("groupID").intValue--;
										}//end for all making
								}//end for all manufacture processes
						}//end for all manufacture groups
						#endregion
		
						#region trade posts
						GroupRemove (postScripts, "stock", groupNo);
						#endregion
		
						#region traders
						GroupRemove (traderScripts, "items", groupNo);
						#endregion
				}//end GroupRemove
	
				///goes through all items in option, and removes an element in the property array
				void GroupRemove (SerializedObject[] options, string property, int groupNo)
				{
						for (int o = 0; o<options.Length; o++) {//for all options
								//options [o].Update ();
								options [o].FindProperty (property).DeleteArrayElementAtIndex (groupNo);
								//options [o].ApplyModifiedProperties ();
						}//end for options
				}//end GroupRemove
	
				///called when a good or manufacturing process is moved
				void ListShuffle (bool goods, int moveFrom, int moveTo, int groupNo)
				{
						for (int p = 0; p<postScripts.Length; p++) {//go through all posts
								if (goods)
										postScripts [p].FindProperty ("stock").GetArrayElementAtIndex (groupNo).FindPropertyRelative ("stock").MoveArrayElement (moveFrom, moveTo);
								else
										postScripts [p].FindProperty ("manufacture").MoveArrayElement (moveFrom, moveTo);
						}//end for posts
						for (int t = 0; t<postScripts.Length; t++) {//go through all traders
								if (goods)
										traderScripts [t].FindProperty ("items").GetArrayElementAtIndex (groupNo).FindPropertyRelative ("items").MoveArrayElement (moveFrom, moveTo);
								else
										traderScripts [t].FindProperty ("manufacture").MoveArrayElement (moveFrom, moveTo);
						}//end for traders
				}//end ListShuffle
	
				///called when a good is added, copied or removed
				void EditLists (bool goods, int point, int groupNo, bool adding)
				{
						for (int p = 0; p<postScripts.Length; p++) {//go through all posts
								if (goods) {
										SerializedProperty stock = postScripts [p].FindProperty ("stock").GetArrayElementAtIndex (groupNo).FindPropertyRelative ("stock");
										if (adding) {
												stock.InsertArrayElementAtIndex (point);
												SerializedProperty inserted = stock.GetArrayElementAtIndex (point);
												inserted.FindPropertyRelative ("buy").boolValue = inserted.FindPropertyRelative ("sell").boolValue = true;
												inserted.FindPropertyRelative ("number").intValue = inserted.FindPropertyRelative ("min").intValue = 
							inserted.FindPropertyRelative ("max").intValue = 0;
												inserted.FindPropertyRelative ("minMax").boolValue = inserted.FindPropertyRelative ("minMax").boolValue = false;
										} else
												stock.DeleteArrayElementAtIndex (point);
								} else {//if manufacturing
										SerializedProperty manufacturing = postScripts [p].FindProperty ("manufacture").GetArrayElementAtIndex (groupNo).FindPropertyRelative ("manufacture");
										if (adding) {
												manufacturing.InsertArrayElementAtIndex (point);
												manufacturing.GetArrayElementAtIndex (point).FindPropertyRelative ("enabled").boolValue = false;
										} else
												manufacturing.DeleteArrayElementAtIndex (point);
								}//else manufacturing
						}//end for all posts
						for (int t = 0; t<traderScripts.Length; t++) {//go through all traders
								if (goods) {//only need to edit traders if it is the goods list being edited
										SerializedProperty items = traderScripts [t].FindProperty ("items").GetArrayElementAtIndex (groupNo).FindPropertyRelative ("items");
										if (adding) {
												items.InsertArrayElementAtIndex (point);
												items.GetArrayElementAtIndex (point).FindPropertyRelative ("enabled").boolValue = true;
										} else
												items.DeleteArrayElementAtIndex (point);
								} else {//if manufacturing
										SerializedProperty manufacturing = traderScripts [t].FindProperty ("manufacture").GetArrayElementAtIndex (groupNo).FindPropertyRelative ("manufacture");
										if (adding) {
												manufacturing.InsertArrayElementAtIndex (point);
												manufacturing.GetArrayElementAtIndex (point).FindPropertyRelative ("enabled").boolValue = false;
										} else
												manufacturing.DeleteArrayElementAtIndex (point);
								}//end else manufacturing
						}//end for all traders
				}//end EditLists
	
				///go through all of the posts, and get the number of each item available
				string[][] GetItemNumbers (List<GoodsTypes> goods, out long total)
				{
						string[][] itemNumbers = new string[goods.Count][];
						int[][] postCount = new int[goods.Count][];
						int[][] totals = new int[goods.Count][];
						total = 0;
		
						for (int g = 0; g<goods.Count; g++) {//go through all groups
								itemNumbers [g] = new string[goods [g].goods.Count];
								postCount [g] = new int[goods [g].goods.Count];
								totals [g] = new int[goods [g].goods.Count];
								for (int s = 0; s<itemNumbers[g].Length; s++) {//go through all stock
										if (!Application.isPlaying) {//if is playing, then can use averages to get the numbers instead of going through all of the posts
												for (int p = 0; p<controllerNormal.postScripts.Length; p++) {//go through all posts
														Stock current = controllerNormal.postScripts [p].stock [g].stock [s];
														if (current.buy) {
																postCount [g] [s]++;//increase the post count by 1
																totals [g] [s] += current.number;
																total += current.number;
														}//end if enabled at post
												}//end for all posts
										} else {//end if not playing, else use averages
												postCount [g] [s] = controllerNormal.goods [g].goods [s].postCount;
												totals [g] [s] = (int)(controllerNormal.goods [g].goods [s].average * postCount [g] [s]);
												total += totals [g] [s];
										}//end else if playing
										itemNumbers [g] [s] = "" + postCount [g] [s] + ", " + totals [g] [s];
								}//end for all stock
						}//end for all groups
						return itemNumbers;
				}//end GetItemNumbers
	
				///add a goods group
				void AddGoodsGroup (int loc)
				{
						for (int p = 0; p<postScripts.Length; p++) {//go through posts, adding
								SerializedProperty stock = postScripts [p].FindProperty ("stock");
								stock.InsertArrayElementAtIndex (loc);
								stock.GetArrayElementAtIndex (loc).FindPropertyRelative ("stock").arraySize = 0;
						}//end for posts
						for (int t = 0; t<traderScripts.Length; t++) {//go through traders, adding
								SerializedProperty items = traderScripts [t].FindProperty ("items");
								items.InsertArrayElementAtIndex (loc);
								items.GetArrayElementAtIndex (loc).FindPropertyRelative ("items").arraySize = 0;
						}//end for traders
						goods.InsertArrayElementAtIndex (loc);
						SerializedProperty newGood = goods.GetArrayElementAtIndex (loc);
						newGood.FindPropertyRelative ("goods").arraySize = 0;
						newGood.FindPropertyRelative ("name").stringValue = "Goods group " + loc;
						GUIUtility.keyboardControl = 0;
						controllerSO.FindProperty ("selected").FindPropertyRelative ("GG").intValue = loc;
				}//end AddGoodsGroup
	
				///add a manufacturing group
				void AddManGroup (int loc)
				{
						AddManGroup (loc, postScripts);
						AddManGroup (loc, traderScripts);
						manufacture.InsertArrayElementAtIndex (loc);
						SerializedProperty newMan = manufacture.GetArrayElementAtIndex (loc);
						newMan.FindPropertyRelative ("manufacture").arraySize = 0;
						newMan.FindPropertyRelative ("name").stringValue = "Manufacture group " + loc;
						controllerSO.FindProperty ("selected").FindPropertyRelative ("MG").intValue = loc;
						GUIUtility.keyboardControl = 0;
				}//end AddManGroup
		
				void AddManGroup (int loc, SerializedObject[] pt)
				{//add manufacturing groups to posts and traders
						for (int p = 0; p<pt.Length; p++) {//go through, adding
								SerializedProperty ptMan = pt [p].FindProperty ("manufacture");
								ptMan.InsertArrayElementAtIndex (loc);
								ptMan.GetArrayElementAtIndex (loc).FindPropertyRelative ("manufacture").arraySize = 0;
						}//end for
				}//end AddManGroup
	
				///called when the check button is pressed. Goes through all manufacturing, gets item changes, and checks pricing before sending to a window to display
				void CheckManufacturing (int manufactureCount, int[] groupLengthsM, int[] groupLengthsG, SerializedProperty manufacture)
				{
						float[][] perItem = new float[groupLengthsG.Length][];//the change for each item
						string[][] allNamesG = new string[groupLengthsG.Length][];
						float total = 0;//the total
						string[][] pricing = new string[groupLengthsM.Length][];
						string[] goodsTitles = new string[groupLengthsG.Length];
						string[] manTitles = new string[groupLengthsM.Length];
		
						for (int g = 0; g<groupLengthsG.Length; g++) {
								goodsTitles [g] = controllerNormal.goods [g].name;
								perItem [g] = new float[groupLengthsG [g]];
								allNamesG [g] = new string[groupLengthsG [g]];
								for (int n = 0; n<groupLengthsG[g]; n++)
										allNamesG [g] [n] = controllerNormal.goods [g].goods [n].name;
						}
		
						for (int m1 = 0; m1<manufacture.arraySize; m1++) {//go through all manufacture groups
								manTitles [m1] = controllerNormal.manufacture [m1].name;
								int processCount = manufacture.GetArrayElementAtIndex (m1).FindPropertyRelative ("manufacture").arraySize;
								pricing [m1] = new string[processCount];
		
								for (int m2 = 0; m2<processCount; m2++) {//go through all processes
										if (EditorUtility.DisplayCancelableProgressBar ("Checking", "Checking manufacturing", (m2 + groupLengthsM [m1]) / (manufactureCount * 1f))) {
												EditorUtility.ClearProgressBar ();
												return;
										}
										
										#region number change
										Mnfctr cMan = controllerNormal.manufacture [m1].manufacture [m2];
										for (int p = 0; p<controllerNormal.postScripts.Length; p++)//go through all posts
												NumberChange (controllerNormal.postScripts [p].manufacture [m1].manufacture [m2], cMan, total, perItem);
										for (int t = 0; t<controllerNormal.traderScripts.Length; t++)//go through all traders
												NumberChange (controllerNormal.traderScripts [t].manufacture [m1].manufacture [m2], cMan, total, perItem);
										#endregion
				
										#region pricing
										int profit = 0;
										Mnfctr current = controllerNormal.manufacture [m1].manufacture [m2];
										for (int nm = 0; nm < current.needing.Count; nm++) {//go through all needing, getting min cost
												NeedMake currentNM = current.needing [nm];
												profit -= controllerNormal.goods [currentNM.groupID].goods [currentNM.itemID].minPrice * currentNM.number;
										}//end for needing
										for (int nm = 0; nm < current.making.Count; nm++) {//go through all making, getting max price
												NeedMake currentNM = current.making [nm];
												profit += controllerNormal.goods [currentNM.groupID].goods [currentNM.itemID].maxPrice * currentNM.number;
										}//end for needing
										pricing [m1] [m2] = profit.ToString ();
										#endregion
								}//end for all processes			
						}//end for manufacture groups
						EditorUtility.ClearProgressBar ();
			
						//now that the values have been calculated, send to the window			
						CheckManufacturingWindow window = (CheckManufacturingWindow)EditorWindow.GetWindow (typeof(CheckManufacturingWindow), true, "Manufacturing check");
						window.position = new Rect (Screen.currentResolution.width / 2 - 275, Screen.currentResolution.height / 2 - 200, 550, 400);
						window.maxSize = new Vector2 (550, 400);
						window.minSize = window.maxSize;
				
						window.perItem = perItem;
						window.total = total;
						window.allNamesG = allNamesG;
						window.pricing = pricing;
						window.allNamesM = controllerNormal.manufactureNames;
						window.tooltipsM = controllerNormal.manufactureTooltips;
						window.goodsTitles = goodsTitles;
						window.manTitles = manTitles;
				}//end CheckManufacturing
		
				void NumberChange (RunMnfctr man, Mnfctr cMan, float total, float[][] perItem)
				{//calculate the number changes from trade posts and traders
						if (man.enabled) {//check that item is enabled
								float toChange = 0;
								float deniminator = man.create + man.cooldown;//get the denominator. This is the minimum time between subsequent manufactures
								for (int nm = 0; nm<cMan.needing.Count; nm++) {//go through needing, reducing perItem
										toChange = cMan.needing [nm].number / deniminator;
										total -= toChange;
										perItem [cMan.needing [nm].groupID] [cMan.needing [nm].itemID] -= toChange;
								}//end for needing
				
								for (int nm = 0; nm<cMan.making.Count; nm++) {//go through making, increasing perItem
										toChange = cMan.making [nm].number / deniminator;
										total += toChange;
										perItem [cMan.making [nm].groupID] [cMan.making [nm].itemID] += toChange;
								}//end for needing
						}//end enabled check
				}//end NumberChange
	
				///the GUI used for adding and removing tags, groups, factions
				///int option
				///0 = tags, groups
				///1 = factions
				void TGF (int option, GUIContent title, SerializedProperty options, string plural, string singleCapSpace)
				{
						SerializedProperty expanded = options.FindPropertyRelative ("expandedC");
						SerializedProperty enabled = options.FindPropertyRelative ("enabled");
			
						GUITools.TitleGroup (title, expanded, false);
		
						if (expanded.boolValue) {//show if expanded
								SerializedProperty optionInfo = options.FindPropertyRelative ("names");
								if (option == 1)
										optionInfo = options.FindPropertyRelative ("factions");
								int number = optionInfo.arraySize;
			
								EditorGUI.indentLevel = 1;
								if (enabled.boolValue)//only have the horizontal if showing other info too
										EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.PropertyField (enabled, new GUIContent ("Enable " + plural));
		
								if (enabled.boolValue) {//show only if enabled		
										GUILayout.FlexibleSpace ();
										EditorGUILayout.LabelField ("Number of " + plural, number.ToString ());
				
										if (GUITools.PlusMinus (true)) {
												optionInfo.InsertArrayElementAtIndex (number);
												SerializedProperty inserted = optionInfo.GetArrayElementAtIndex (number);
				
												switch (option) {//different option types have different requirements
												case 0:
														inserted.stringValue = singleCapSpace + number;
														break;
												case 1:
														inserted.FindPropertyRelative ("name").stringValue = "Faction " + number;
														inserted.FindPropertyRelative ("colour").colorValue = Color.green;
														break;
												}//end switch for different options
				
												for (int p = 0; p<postScripts.Length; p++)//add the new element to all trade posts
														postScripts [p].FindProperty (plural).InsertArrayElementAtIndex (number);
												if (option == 1) {//only need to update trader information if factions are being edited
														for (int t = 0; t<traderScripts.Length; t++) 
																traderScripts [t].FindProperty ("factions").InsertArrayElementAtIndex (number);
												}//end if factions
										}//if add pressed
				
										EditorGUILayout.EndHorizontal ();		
										if (number > 0) {//check that there are options to show
												GUITools.IndentGroup (2);
												for (int o = 0; o<number; o++) {//for all options
														SerializedProperty cO = optionInfo.GetArrayElementAtIndex (o);//current option
														SerializedProperty cON = cO;//current option name - only really useful if is showing factions
														if (option == 1)
																cON = cO.FindPropertyRelative ("name");
						
														GUIContent displayName = new GUIContent (cON.stringValue);
						
														EditorGUILayout.BeginHorizontal ();
														EditorGUILayout.PropertyField (cON, displayName);//show editable name field
														if (cON.stringValue == "")//if name is blank
																cON.stringValue = singleCapSpace + o;
						
														if (option == 1)//only need to show colour option if it is the faction
																EditorGUILayout.PropertyField (cO.FindPropertyRelative ("colour"), new GUIContent (), GUILayout.MaxWidth (80f));
							
														if (GUITools.PlusMinus (false)) {
																for (int p = 0; p<postScripts.Length; p++) 
																		postScripts [p].FindProperty (plural).DeleteArrayElementAtIndex (o);
																if (option == 1) {//only need to update trader information if factions are being edited
																		for (int t = 0; t<traderScripts.Length; t++)
																				traderScripts [t].FindProperty ("factions").DeleteArrayElementAtIndex (o);
																}//end if factions
																optionInfo.DeleteArrayElementAtIndex (o);
																break;
														}//end if minus pressed
														EditorGUILayout.EndHorizontal ();
												}//end for all options
												EditorGUILayout.EndVertical ();
												EditorGUILayout.EndHorizontal ();
										}//end if showing something
								}//end if enabled
						}//end if expanded
						EditorGUILayout.EndVertical ();
				}//end TGF
	
				///check whether the value is infinity
				bool InfinityCheck (string minMax)
				{
						SerializedProperty unitsList = units.FindPropertyRelative ("units");
						for (int u = 0; u<unitsList.arraySize; u++) {//go through units
								if (unitsList.GetArrayElementAtIndex (u).FindPropertyRelative (minMax).floatValue == Mathf.Infinity)
										return true;
						}//end for units
						return false;
				}//end InfinityCheck
	
				///have a gui label with a unit which goes over the field box
				void UnitLabel (string label, float width)
				{
						GUILayout.Space (-(width + 4));
						EditorGUILayout.LabelField (label, GUILayout.MaxWidth (width));
				}//end UnitLabel
				
				/// get the string containing the mass and the unit in the correct form
				///zero is used to get the units if the mass is 1. Needed if none of the units have limits of exactly 1
				string UnitString (float mass, bool zero)
				{
						if (mass == 0)
								return UnitString (1, true);
				
						for (int u = 0; u<controllerNormal.units.units.Count; u++) {//go through each unit
								Unit cU = controllerNormal.units.units [u];
								if (mass >= cU.min && mass < cU.max) {//if the mass of the item is in the range
										return zero ? "0 " + cU.suffix : ((decimal)mass / (decimal)cU.min).ToString () + " " + cU.suffix;//return the correct unit
								}//end if mass in range
						}//end for each unit
						return mass.ToString ();
				}//end UnitString
		}//end ControllerEditor
}//end namespace