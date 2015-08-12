#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define API
#endif
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TradeSys
{//use namespace to stop any name conflicts
		[CanEditMultipleObjects, CustomEditor(typeof(TradePost))]
		public class PostEditor : Editor
		{
	
				TSGUI GUITools = new TSGUI ();//extra gui methods which are used by all TradeSys scripts
	
		#region options
				private SerializedProperty selP;
				private SerializedProperty showLinks;
				private SerializedProperty smallScroll;
				private SerializedProperty scrollPosPS;
				private SerializedProperty scrollPosPG;
				private SerializedProperty scrollPosPM;
		#endregion
	
		#region variables
				private SerializedObject controllerSO;
				private Controller controllerNormal;
				private SerializedObject postSO;
				private TradePost postNormal;
				private SerializedProperty stock;
				private SerializedProperty controllerGoods;
				private SerializedProperty manufacturing;
				private SerializedProperty controllerMan;
				private SerializedProperty customPricing, cash;
				private SerializedProperty tags, groups, factions;
				private SerializedProperty stopProcesses;
		#endregion
	
				void OnEnable ()
				{
						controllerNormal = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
						controllerSO = new SerializedObject (controllerNormal);
	
						postSO = new SerializedObject (targets);
						postNormal = (TradePost)target;
						postNormal.tag = Tags.TP;
	
						selP = controllerSO.FindProperty ("selP");
						showLinks = controllerSO.FindProperty ("showLinks");
						smallScroll = controllerSO.FindProperty ("smallScroll");
		
						scrollPosPS = controllerSO.FindProperty ("scrollPosPS");
						scrollPosPG = controllerSO.FindProperty ("scrollPosPG");
						scrollPosPM = controllerSO.FindProperty ("scrollPosPM");
	
						stock = postSO.FindProperty ("stock");
						controllerGoods = controllerSO.FindProperty ("goods");
						manufacturing = postSO.FindProperty ("manufacture");
						controllerMan = controllerSO.FindProperty ("manufacture");
						customPricing = postSO.FindProperty ("customPricing");
						cash = postSO.FindProperty ("cash");	
		
						tags = postSO.FindProperty ("tags").FindPropertyRelative ("enabled");
						groups = postSO.FindProperty ("groups").FindPropertyRelative ("enabled");
						factions = postSO.FindProperty ("factions").FindPropertyRelative ("enabled");	
		
						stopProcesses = postSO.FindProperty ("stopProcesses");
		
						GUITools.GetNames (controllerNormal);
						GUITools.ManufactureInfo (controllerNormal);
						controllerNormal.SortTradePost (postNormal);
				}//end OnEnable
	
				public override void OnInspectorGUI ()
				{
						#if !API
						Undo.RecordObject (controllerNormal, "TradeSys Trade Post");
						EditorGUIUtility.fieldWidth = 30f;
						#endif	
							
						postSO.Update ();
						controllerSO.Update ();
						//show a toolbar with space either side
						EditorGUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace ();//flexible space so the size of the toolbar remains the same
						selP.intValue = GUILayout.Toolbar (selP.intValue, new string[] {
								"Settings",
								"Stock",
								"Manufacturing"
						});//show the toolbar
						GUILayout.FlexibleSpace ();//another space so is in the middle
						EditorGUILayout.EndHorizontal ();
						switch (selP.intValue) {
				#region settings
						case 0:
								EditorGUI.indentLevel = 0;
								GUITools.HorizVertOptions (controllerSO.FindProperty ("showHoriz"));//show display options
			
								GUITools.StartScroll (scrollPosPS, smallScroll);
		
					#region general
								EditorGUILayout.BeginVertical ("HelpBox");
								EditorGUILayout.PropertyField (showLinks, new GUIContent ("Show trade links", "Show the possible trade links between trade posts"));
			
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.PropertyField (customPricing, new GUIContent ("Custom pricing", "Manually set the pricing of each item. Prices will be static"));
								EditorGUILayout.PropertyField (cash, new GUIContent ("Credits", "This is the amout of money that the trade post has in order to buy and sell items"));
								EditorGUILayout.EndHorizontal ();
			
								EditorGUILayout.PropertyField (postSO.FindProperty ("stopProcesses"), new GUIContent ("Stop processes", "Stop manufacturing processes if it will result in the number of an item going out of the specified range"));
								EditorGUILayout.EndVertical ();
					#endregion
			
					#region goods
								GUITools.Title (new GUIContent ("Allow item at this post", "Allow the item to be bought and sold at this trade post"), true);
								GUILayout.FlexibleSpace ();
								GUITools.ExpandCollapse (controllerGoods, "expandedP", false);
								EditorGUILayout.EndHorizontal ();
								for (int g = 0; g<controllerNormal.goods.Count; g++) {//go through groups
										EditorGUI.indentLevel = 1;
										SerializedProperty currentGroup = controllerGoods.GetArrayElementAtIndex (g);
										SerializedProperty currentStock = stock.GetArrayElementAtIndex (g).FindPropertyRelative ("stock");
				
										EditorGUILayout.BeginHorizontal ();
										currentGroup.FindPropertyRelative ("expandedP").boolValue = GUILayout.Toggle (currentGroup.FindPropertyRelative ("expandedP").boolValue, currentGroup.FindPropertyRelative ("name").stringValue, "Foldout");//toggle with name of group
		
										bool[][] before = new bool[currentStock.arraySize][];//an array containing all of the enabled or disabled selection
										for (int b = 0; b<currentStock.arraySize; b++) {
												before [b] = new bool[3];
												before [b] [0] = currentStock.GetArrayElementAtIndex (b).FindPropertyRelative ("buy").boolValue;
												before [b] [1] = currentStock.GetArrayElementAtIndex (b).FindPropertyRelative ("sell").boolValue;
												before [b] [2] = currentStock.GetArrayElementAtIndex (b).FindPropertyRelative ("hidden").boolValue;
										}
		
										GUITools.EnableDisable (currentStock, new string[] {
												"buy",
												"sell",
												"hidden"
										}, true);
				
										if (currentGroup.FindPropertyRelative ("expandedP").boolValue) {//if is expanded
												GUITools.IndentGroup (1);
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.BeginVertical ();
												EditorGUILayout.BeginHorizontal ();
												GUILayout.Label (new GUIContent ("Buy", "Select whether it is possible for the trade post to buy this item"));
												GUITools.EnableDisable (currentStock, "buy", true);
												EditorGUILayout.BeginHorizontal ();
												GUILayout.Label (new GUIContent ("Sell", "Select whether it is possible for the trade post to sell this item"));
												GUITools.EnableDisable (currentStock, "sell", true);
												EditorGUILayout.EndVertical ();
												GUILayout.FlexibleSpace ();
												EditorGUILayout.BeginHorizontal ();
												GUILayout.Label (new GUIContent ("Hidden", "Select whether the items are hidden from being traded, but the post can still use them for manufacturing processes"));
												GUITools.EnableDisable (currentStock, "hidden", true);
												EditorGUILayout.EndHorizontal ();
				
												EditorGUILayout.BeginHorizontal ();
												for (int x = 0; x< 2; x++) {//show these options twice
														EditorGUILayout.BeginHorizontal ();
														EditorGUILayout.LabelField ("", GUILayout.MaxWidth (100f));
														GUILayout.FlexibleSpace ();
														EditorGUILayout.LabelField (new GUIContent ("B", "Select whether it is possible for the trade post to buy this item"), GUILayout.Width (15f));
														EditorGUILayout.LabelField (new GUIContent ("S", "Select whether it is possible for the trade post to sell this item"), GUILayout.Width (15f));
														EditorGUILayout.LabelField (new GUIContent ("H", "Select whether this item is not avialable for trading, but can still be used by the trading post for manufacturing processes"), GUILayout.Width (15f));
														EditorGUILayout.EndHorizontal ();
												}//end show twice
												EditorGUILayout.EndHorizontal ();
				
												GUITools.HorizVertDisplay (controllerNormal.allNames [g], currentStock, controllerSO.FindProperty ("showHoriz").boolValue, true);
												EditorGUILayout.EndVertical ();
												EditorGUILayout.EndHorizontal ();
										}//end if expanded
				
										for (int i = 0; i<currentStock.arraySize; i++) {//go through all items
												//use the enabled or disabled selection to find what has changed
												//this is so that if the hidden option is pressed, buy and sell are disabled
												//but if buy or sell are pressed, then hidden becomes disabled
												if ((currentStock.GetArrayElementAtIndex (i).FindPropertyRelative ("buy").boolValue && !before [i] [0]) || (currentStock.GetArrayElementAtIndex (i).FindPropertyRelative ("sell").boolValue && !before [i] [1]))
								//if buy or sell have just been enabled
														currentStock.GetArrayElementAtIndex (i).FindPropertyRelative ("hidden").boolValue = false;
												if (currentStock.GetArrayElementAtIndex (i).FindPropertyRelative ("hidden").boolValue && !before [i] [2])
								//if hidden has been enabled
														currentStock.GetArrayElementAtIndex (i).FindPropertyRelative ("buy").boolValue = currentStock.GetArrayElementAtIndex (i).FindPropertyRelative ("sell").boolValue = false;
										}//end for all items
								}//end for all groups
								EditorGUILayout.EndVertical ();
					#endregion
			
					#region tags
								if (controllerNormal.postTags.enabled)//check that tags have been enabled before showing anything
										TGF (new GUIContent ("Post tags", "Select what tags the trade post should have. This doesn't do anything, but allows you to write code that changes how the trade post buy sell / window changes based on what the tag is"), tags, false, controllerNormal.postTags.names.ToArray (), "tags");
					#endregion
			
					#region groups
								if (controllerNormal.groups.enabled)//check that groups have been enabled before showing anything
										TGF (new GUIContent ("Groups", "Select which groups the trader belongs to. In order to be able to trade with another post, they both have to have a group in common"), groups, false, controllerNormal.groups.names.ToArray (), "groups");
					#endregion
			
					#region factions
								if (controllerNormal.factions.enabled)//check that the factions have been enabled before showing anything
										TGF (new GUIContent ("Factions", "Select which factions the trade post belongs to. In order to be able to trade with another post, they both have to have a faction in common"), factions, true, new string[]{}, "factions");
					#endregion
			
								if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
										EditorGUILayout.EndScrollView ();
								break;
				#endregion
			
				#region stock
						case 1:
								EditorGUI.indentLevel = 0;
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Set stock numbers", EditorStyles.boldLabel);
								if (!customPricing.boolValue)//only show this option if prices are set automatically
										EditorGUILayout.PropertyField (controllerSO.FindProperty ("showPrices"), new GUIContent ("Show prices", "Show the prices of the different items. Not editable, and will show 1 or what the custom pricing was set until the game is playing where it will then be set automatically. If the item is marked as hidden, then the price will not be set because it is not required"));
								EditorGUILayout.EndHorizontal ();
			
								GUITools.StartScroll (scrollPosPG, smallScroll);
								if (AnyGoods (stock))
										EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
				
								for (int g = 0; g<controllerNormal.goods.Count; g++) {
										SerializedProperty stockGroup = stock.GetArrayElementAtIndex (g).FindPropertyRelative ("stock");
										if (AnyEnabled (stockGroup)) {//dont show anything if nothing is enabled
												EditorGUI.indentLevel = 0;
				
												SerializedProperty currentGroup = controllerGoods.GetArrayElementAtIndex (g);
					
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.LabelField (currentGroup.FindPropertyRelative ("name").stringValue, EditorStyles.boldLabel);
												GUILayout.FlexibleSpace ();
												GUITools.ExpandCollapse (stockGroup, "minMaxS", false);
												EditorGUILayout.EndHorizontal ();
				
												for (int s = 0; s<stockGroup.arraySize; s++) {//go through all goods in stock
														SerializedProperty currentStock = stockGroup.GetArrayElementAtIndex (s);
														if (GoodEnabled (currentStock)) {
																if (s == 0)
																		EditorGUI.indentLevel = 0;
																EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
																EditorGUI.indentLevel = 1;
							
																EditorGUILayout.BeginHorizontal ();
							
																EditorGUILayout.BeginVertical ();
																if (controllerSO.FindProperty ("showPrices").boolValue || customPricing.boolValue)
																		GUILayout.Space (10f);
										
																currentStock.FindPropertyRelative ("minMaxS").boolValue = EditorGUILayout.Toggle (new GUIContent (controllerNormal.allNames [g] [s], "The foldout on the right shows options for number limitations"), currentStock.FindPropertyRelative ("minMaxS").boolValue, "Foldout");
																if (currentStock.FindPropertyRelative ("minMaxE").boolValue)
																		currentStock.FindPropertyRelative ("minMaxS").boolValue = true;
																//name of good with the min max option
																EditorGUILayout.EndVertical ();
							
																EditorGUILayout.BeginVertical ();// use vertical to also show price if enabled
																EditorGUILayout.PropertyField (currentStock.FindPropertyRelative ("number"), new GUIContent ("Number", "The number of items found at the trade post"));
							
																if (currentStock.FindPropertyRelative ("number").intValue < 0)
																		currentStock.FindPropertyRelative ("number").intValue = 0;
																//make sure that is not less than 0
							
																if (customPricing.boolValue) {//if custom pricing enabled, allow price edits
																		SerializedProperty price = currentStock.FindPropertyRelative ("price");
																		SerializedProperty controllerGood = currentGroup.FindPropertyRelative ("goods").GetArrayElementAtIndex (s);
																		EditorGUILayout.PropertyField (price, new GUIContent ("Price", "Min: " + controllerGood.FindPropertyRelative ("minPrice").intValue + "\nMax: " + controllerGood.FindPropertyRelative ("maxPrice").intValue));
																		//show the min and max prices in the tooltip to give an idea of what price should be set
																		if (price.intValue < 1)
																				price.intValue = 1;
																} else if (controllerSO.FindProperty ("showPrices").boolValue)
																		EditorGUILayout.LabelField ("Price", currentStock.FindPropertyRelative ("price").intValue.ToString ());
							
																EditorGUILayout.EndVertical ();//end show price vertical
																EditorGUILayout.EndHorizontal ();//end name number horizontal
							
																if (currentStock.FindPropertyRelative ("minMaxS").boolValue) {//only show min max options if enabled
																		EditorGUI.indentLevel = 2;
								
																		EditorGUILayout.PropertyField (currentStock.FindPropertyRelative ("minMaxE"), new GUIContent ("Number limits", "Tick the box to set the min and max number of this item at this trade post"));
								
																		if (currentStock.FindPropertyRelative ("minMaxE").boolValue) {//show options for number limits if it has been enabled
									
																				string[] tooltipExtra = {
																						"This is the ",
																						" number of this item that the trade post should have", 
																						". If the number is ",
																						" than this, manufacture processes ",
																						" this item will be stopped"
																				};
																				string tooltipMin, tooltipMax;
									
																				tooltipMin = tooltipExtra [0] + "minimum" + tooltipExtra [1];
																				tooltipMax = tooltipExtra [0] + "maximum" + tooltipExtra [1];
									
																				if (stopProcesses.boolValue) {//if stop processes has been enabled, then add to the tooltip string
																						tooltipMin += tooltipExtra [2] + "fewer" + tooltipExtra [3] + "needing" + tooltipExtra [4];
																						tooltipMax += tooltipExtra [2] + "greater" + tooltipExtra [3] + "making" + tooltipExtra [4];
																				}//end creating the full tooltip if stop processes enabled
									
																				EditorGUILayout.BeginHorizontal ();
																				EditorGUILayout.PropertyField (currentStock.FindPropertyRelative ("min"), new GUIContent ("Min", tooltipMin));
																				EditorGUILayout.PropertyField (currentStock.FindPropertyRelative ("max"), new GUIContent ("Max", tooltipMax));
																				EditorGUILayout.EndHorizontal ();
									
																				//get the numbers
																				int mi = currentStock.FindPropertyRelative ("min").intValue;
																				int ma = currentStock.FindPropertyRelative ("max").intValue;
											
																				if (mi < 0)
																						mi = 0;
																				else if (mi > ma)
																						mi = ma;
																				//set min to be > 0 and < max
									
																				//set the numbers
																				currentStock.FindPropertyRelative ("min").intValue = mi;
																				currentStock.FindPropertyRelative ("max").intValue = ma;
									
																		}//end if number limits
																		EditorGUI.indentLevel = 1;
																}//end show min max
														}//end if is enabled
												}//end for all goods
												EditorGUI.indentLevel = 0;
					
												EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
										}//end if something enabled
								}//end for all groups
				
								if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
										EditorGUILayout.EndScrollView ();
								break;
				#endregion
			
				#region manufacturing
						case 2:
								EditorGUI.indentLevel = 0;
			
								EditorGUILayout.LabelField ("Manufacturing processes", EditorStyles.boldLabel);
			
								GUITools.StartScroll (scrollPosPM, smallScroll);
			
								if (AnyManufacturing (controllerMan))
										EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
			
								for (int m = 0; m<controllerMan.arraySize; m++) {//go through manufacturing groups
										SerializedProperty cManG = manufacturing.GetArrayElementAtIndex (m);
										SerializedProperty cManGC = controllerMan.GetArrayElementAtIndex (m);
				
										if (ShowGroup (cManGC.FindPropertyRelative ("manufacture"))) {//check that something in the group is showing to decide whether to show the title or not
												EditorGUILayout.BeginHorizontal ();
												EditorGUI.indentLevel = 0;
												EditorGUILayout.LabelField (cManGC.FindPropertyRelative ("name").stringValue, EditorStyles.boldLabel);
												GUITools.EnableDisable (cManG.FindPropertyRelative ("manufacture"), "enabled", true);
				
												for (int p = 0; p<cManG.FindPropertyRelative ("manufacture").arraySize; p++) {//go through current manufacture processes
														SerializedProperty cMan = cManG.FindPropertyRelative ("manufacture").GetArrayElementAtIndex (p);
														SerializedProperty cManC = cManGC.FindPropertyRelative ("manufacture").GetArrayElementAtIndex (p);
						
														if (CorrectEnabled (cManC)) {
						
																if (p == 0)
																		EditorGUI.indentLevel = 0;
																EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
																EditorGUI.indentLevel = 1;
					
																EditorGUILayout.BeginHorizontal ();//horizontal to also show the times
																EditorGUILayout.BeginVertical ();//align the name vertically
																if (cMan.FindPropertyRelative ("enabled").boolValue)
																		GUILayout.Space (10f);
							
																EditorGUILayout.PropertyField (cMan.FindPropertyRelative ("enabled"), 
								new GUIContent (cManC.FindPropertyRelative ("name").stringValue, controllerNormal.manufactureTooltips [m] [p]));
																EditorGUILayout.EndVertical ();
						
																if (cMan.FindPropertyRelative ("enabled").boolValue) {//if enabled, allow times to be edited
																		EditorGUILayout.BeginVertical ();//have the create and cooldown times vertically
																		GUILayout.Space (1f);
																		EditorGUILayout.PropertyField (cMan.FindPropertyRelative ("create"), new GUIContent ("Create time", "This is how long it takes between removing the items from sale to be manufactured and when the new items are available"));
																		EditorGUILayout.PropertyField (cMan.FindPropertyRelative ("cooldown"), new GUIContent ("Cooldown time", "This is how long between one manufacture of this process and another"));
																		EditorGUILayout.EndVertical ();
																}
																if (cMan.FindPropertyRelative ("create").intValue < 1)
																		cMan.FindPropertyRelative ("create").intValue = 1;
					
																if (cMan.FindPropertyRelative ("cooldown").intValue < 0)
																		cMan.FindPropertyRelative ("cooldown").intValue = 0;
					
																EditorGUILayout.EndHorizontal ();
							
														} else//end check if not able to, make sure is disabled
																cMan.FindPropertyRelative ("enabled").boolValue = false;
												}//end for manufacturing processes
												EditorGUI.indentLevel = 0;
												EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
										}//check that something is enabled in order to show the title
								}//end for manufacturing groups
			
								if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
										EditorGUILayout.EndScrollView ();
								break;
				#endregion
						}//end switch
						postSO.ApplyModifiedProperties ();
						controllerSO.ApplyModifiedProperties ();
				}//end OnInspectorGUI
		
				bool AnyGoods (SerializedProperty goods)
				{//go through goods and seeing if there is anything getting shown
						for (int g = 0; g<goods.arraySize; g++)
								if (AnyEnabled (goods.GetArrayElementAtIndex (g).FindPropertyRelative ("stock")))
										return true;
						return false;
				}//end AnyGoods
	
				bool AnyEnabled (SerializedProperty stockGroup)
				{//check to see if there are any enabled goods in the group before showing the title
						for (int s = 0; s<stockGroup.arraySize; s++)
								if (GoodEnabled (stockGroup.GetArrayElementAtIndex (s)))
										return true;
						return false;
				}//end AnyEnabled
	
				bool GoodEnabled (SerializedProperty checkItem)
				{//check to see if the current item is enabled at all
						if (checkItem.FindPropertyRelative ("buy").boolValue ||
								checkItem.FindPropertyRelative ("sell").boolValue || 
								checkItem.FindPropertyRelative ("hidden").boolValue)
								return true;
						return false;
				}//end GoodEnabled
	
				bool CorrectEnabled (SerializedProperty check)
				{//go through all needing and making lists, checking that the correct items have been enabled to allow manufacture
						SerializedProperty currentNeeding = check.FindPropertyRelative ("needing");
						for (int n = 0; n<currentNeeding.arraySize; n++) {//go through all needing lists
								if (!IsEnabled (currentNeeding.GetArrayElementAtIndex (n).FindPropertyRelative ("itemID").intValue, 
					currentNeeding.GetArrayElementAtIndex (n).FindPropertyRelative ("groupID").intValue, true))
										return false;
						}
		
						SerializedProperty currentMaking = check.FindPropertyRelative ("making");
						for (int m = 0; m<currentMaking.arraySize; m++) {//go through all making lists
								if (!IsEnabled (currentMaking.GetArrayElementAtIndex (m).FindPropertyRelative ("itemID").intValue, 
					currentMaking.GetArrayElementAtIndex (m).FindPropertyRelative ("groupID").intValue, false))
										return false;
						}
						return true;
				}//end CorrectEnabled
	
				bool IsEnabled (int itemID, int groupID, bool needing)
				{//check to see if the current needing / making item is enable
						Stock checking = postNormal.stock [groupID].stock [itemID];
						if (checking.hidden)//if the item is hidden, then count as enabled
								return true;
						if (needing)//if the item is in the needing, post needs to be able to buy the item
								return checking.buy;
						else//else if in the making, needs to be able to sell
								return checking.sell;//
				}//end IsEnabled
	
				bool ShowGroup (SerializedProperty check)
				{//needs to check that there is something enabled in order to show the title
						for (int c = 0; c<check.arraySize; c++) {//go through all checking group
								if (CorrectEnabled (check.GetArrayElementAtIndex (c)))
										return true;
						}//end for checking group
						return false;
				}//end ShowGroup
	
				bool AnyManufacturing (SerializedProperty manufacturing)
				{//go through manufacturing and see if any are shown
						for (int m = 0; m< manufacturing.arraySize; m++)
								if (ShowGroup (manufacturing.GetArrayElementAtIndex (m).FindPropertyRelative ("manufacture")))
										return true;
						return false;
				}//end AnyManufacturing
	
				void TGF (GUIContent title, SerializedProperty option, bool factions, string[] names, string name)
				{//show the tags, groups and factions options
						GUITools.Title (title, false);
		
						EditorGUILayout.BeginHorizontal ();
						SerializedProperty expanded = postSO.FindProperty (name).FindPropertyRelative ("expanded");
						expanded.boolValue = GUILayout.Toggle (expanded.boolValue, "Select " + name, "Foldout");
						if (option.arraySize == 0)
								expanded.boolValue = false;
		
						GUITools.EnableDisable (option, "", false);
				
						if (expanded.boolValue) {
								GUITools.IndentGroup (1);
								if (factions) {
										names = new string[option.arraySize];//need the name of the factions in an array
										for (int f = 0; f<option.arraySize; f++)
												names [f] = controllerNormal.factions.factions [f].name;
								}
								GUITools.HorizVertDisplay (names, option, controllerSO.FindProperty ("showHoriz").boolValue, false);
			
								EditorGUILayout.EndVertical ();
								EditorGUILayout.EndHorizontal ();
						}
		
						EditorGUI.indentLevel = 0;
		
						if (option.arraySize == 0)
								EditorGUILayout.HelpBox ("No " + name + " have been added, but " + name + " have been enabled. " + char.ToUpper (name [0]) + name.Substring (1)
										+ " can be added in the controller", MessageType.Error);
						EditorGUILayout.EndVertical ();
				}//end TGF
		}//end PostEditor
}//end namespace