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
				private SerializedProperty showLinks;
				private SerializedProperty smallScroll;
				private int sel;
				private ScrollPos scrollPos;
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
				private SerializedProperty allowTrades, allowMan;
				bool expendable;
		#endregion
	
				void OnEnable ()
				{
						controllerNormal = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
						controllerSO = new SerializedObject (controllerNormal);
	
						postSO = new SerializedObject (targets);
						postNormal = (TradePost)target;
						postNormal.tag = Tags.TP;
	
						sel = controllerNormal.selected.P;
						showLinks = controllerSO.FindProperty ("showLinks");
						smallScroll = controllerSO.FindProperty ("smallScroll");
		
						scrollPos = controllerNormal.scrollPos;
	
						stock = postSO.FindProperty ("stock");
						controllerGoods = controllerSO.FindProperty ("goods");
						manufacturing = postSO.FindProperty ("manufacture");
						controllerMan = controllerSO.FindProperty ("manufacture");
						customPricing = postSO.FindProperty ("customPricing");
						cash = postSO.FindProperty ("cash");	
		
						tags = postSO.FindProperty ("tags");
						groups = postSO.FindProperty ("groups");
						factions = postSO.FindProperty ("factions");	
		
						stopProcesses = postSO.FindProperty ("stopProcesses");
						allowTrades = postSO.FindProperty ("allowTrades");
						allowMan = postSO.FindProperty ("allowManufacture");
		
						GUITools.GetNames (controllerNormal);
						GUITools.ManufactureInfo (controllerNormal);
						
						expendable = controllerNormal.expTraders.enabled;
						
						if (!Application.isPlaying)//only do this if it isnt playing
								controllerNormal.SortAll ();
				}//end OnEnable
	
				public override void OnInspectorGUI ()
				{
						#if !API
						Undo.RecordObject (postNormal, "TradeSys Trade Post");
						EditorGUIUtility.fieldWidth = 30f;
						#endif	
							
						postSO.Update ();
						controllerSO.Update ();
						
						sel = GUITools.Toolbar (sel, new string[] {
								"Settings",
								"Stock",
								"Manufacturing"
						});//show a toolbar
						
						switch (sel) {
				#region settings
						case 0:
								EditorGUI.indentLevel = 0;
								if (controllerNormal.factions.enabled || controllerNormal.groups.enabled)//only have horiz vert if showing factions or groups
										GUITools.HorizVertOptions (controllerSO.FindProperty ("showHoriz"));//show display options
			
								scrollPos.PS = GUITools.StartScroll (scrollPos.PS, smallScroll);
		
					#region options
								if (GUITools.TitleGroup (new GUIContent ("Options", "Set information about how the trade post works"), controllerSO.FindProperty ("opTP"), false)) {//if showing options
										EditorGUILayout.PropertyField (showLinks, new GUIContent ("Show trade links", "Show the possible trade links between trade posts"));
			
										if (!expendable) {//show pricing things if not expendable
												EditorGUILayout.BeginHorizontal ();
												EditorGUILayout.PropertyField (customPricing, new GUIContent ("Custom pricing", "Manually set the pricing of each item. Prices will be static"));
												EditorGUILayout.PropertyField (cash, new GUIContent ("Credits", "This is the amout of money that the trade post has in order to buy and sell items"));
												EditorGUILayout.EndHorizontal ();
										} else//end if no pricing
												postNormal.customPricing = false;//set to false so wont display custom pricing
			
										EditorGUILayout.PropertyField (postSO.FindProperty ("stopProcesses"), new GUIContent ("Stop processes", "Stop manufacturing processes if it will result in the number of an item going out of the specified range"));
								
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (allowTrades, new GUIContent ("Allow trades", "Select if the trade post is allowed to trade. This will not prevent any options and is so that it can be turned on/off easily"));
										EditorGUILayout.PropertyField (allowMan, new GUIContent ("Allow manufacture", "Select if the trade post is allowed to manufacture. This will not prevent any options and is so that it can be turned on/off easily"));
										EditorGUILayout.EndHorizontal ();
								}//end if showing options
								EditorGUILayout.EndVertical ();
					#endregion
			
					#region tags
								if (controllerNormal.postTags.enabled)//check that tags have been enabled before showing anything
										GUITools.TGF (controllerNormal, controllerSO, "expandedP", new GUIContent ("Post tags", "Select what tags the trade post should have. This doesn't do anything, but allows you to write code that changes how the trade post buy sell / window changes based on what the tag is"), tags, false, controllerNormal.postTags.names.ToArray (), "tags", "postTags");
					#endregion
			
					#region groups
								if (controllerNormal.groups.enabled)//check that groups have been enabled before showing anything
										GUITools.TGF (controllerNormal, controllerSO, "expandedP", new GUIContent ("Groups", "Select which groups the trader belongs to. In order to be able to trade with another post, they both have to have a group in common"), groups, false, controllerNormal.groups.names.ToArray (), "groups", "groups");
					#endregion
			
					#region factions
								if (controllerNormal.factions.enabled)//check that the factions have been enabled before showing anything
										GUITools.TGF (controllerNormal, controllerSO, "expandedP", new GUIContent ("Factions", "Select which factions the trade post belongs to. In order to be able to trade with another post, they both have to have a faction in common"), factions, true, new string[]{}, "factions", "factions");
					#endregion
								break;
				#endregion
			
				#region stock
						case 1:
								EditorGUI.indentLevel = 0;
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Stock information", EditorStyles.boldLabel);
								GUILayout.FlexibleSpace ();
								
								if (!customPricing.boolValue && !expendable)//only show this option if prices are set automatically and not expendable traders
										controllerSO.FindProperty ("showPrices").boolValue = GUILayout.Toggle (controllerSO.FindProperty ("showPrices").boolValue, new GUIContent ("Show prices", "Show the prices of the different items. Not editable, and will show 1 or what the custom pricing was set until the game is playing where it will then be set automatically. If the item is marked as hidden, then the price will not be set because it is not required"), "minibuttonleft");
								else
										controllerSO.FindProperty ("showPrices").boolValue = false;//if expendable or not able to show, set to false
						
								GUITools.ExpandCollapse (controllerGoods, "expandedP", !customPricing.boolValue && !expendable);
								EditorGUILayout.EndHorizontal ();
			
								scrollPos.PG = GUITools.StartScroll (scrollPos.PG, smallScroll);
			
								if (GUITools.AnyGoods (stock, "stock")) {
										EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
				
										for (int g = 0; g<controllerNormal.goods.Count; g++) {
												SerializedProperty stockGroup = stock.GetArrayElementAtIndex (g).FindPropertyRelative ("stock");
												if (stockGroup.arraySize > 0) {//dont show anything if nothing in group
														EditorGUI.indentLevel = 0;
				
														SerializedProperty currentGroup = controllerGoods.GetArrayElementAtIndex (g);
												
														if (GUITools.TitleButton (new GUIContent (currentGroup.FindPropertyRelative ("name").stringValue), currentGroup.FindPropertyRelative ("expandedP"), "BoldLabel")) {//if foldout for goods group open
												
																bool[][] before = new bool[stockGroup.arraySize][];//an array containing all of the enabled or disabled selection
																for (int b = 0; b<stockGroup.arraySize; b++) {//go through items in stock group
																		before [b] = new bool[3];
																		before [b] [0] = stockGroup.GetArrayElementAtIndex (b).FindPropertyRelative ("buy").boolValue;
																		before [b] [1] = stockGroup.GetArrayElementAtIndex (b).FindPropertyRelative ("sell").boolValue;
																		before [b] [2] = stockGroup.GetArrayElementAtIndex (b).FindPropertyRelative ("hidden").boolValue;
																}//end for all in group
												
																GUITools.IndentGroup (1);
																EditorGUILayout.BeginHorizontal ();
						
																EditorGUILayout.BeginVertical ();
						
																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label (new GUIContent ("Buy", "Select whether it is possible for the trade post to buy this item"));
																GUITools.EnableDisable (stockGroup, "buy", true);
						
																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label (new GUIContent ("Sell", "Select whether it is possible for the trade post to sell this item"));
																GUITools.EnableDisable (stockGroup, "sell", true);
						
																EditorGUILayout.EndVertical ();
						
																GUILayout.FlexibleSpace ();
						
																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label (new GUIContent ("Hidden", "Select whether the items are hidden from being traded, but the post can still use them for manufacturing processes"));
																GUITools.EnableDisable (stockGroup, "hidden", true);
																EditorGUILayout.EndHorizontal ();
						
																EditorGUILayout.EndVertical ();
																EditorGUILayout.EndHorizontal ();
												
																EditorGUILayout.BeginHorizontal ();
																GUILayout.Space (15f);
																EditorGUILayout.PrefixLabel (" ");
																EditorGUILayout.LabelField (new GUIContent ("B", "Select whether it is possible for the trade post to buy this item"), GUILayout.Width (15f));
																EditorGUILayout.LabelField (new GUIContent ("S", "Select whether it is possible for the trade post to sell this item"), GUILayout.Width (15f));
																EditorGUILayout.LabelField (new GUIContent ("H", "Select whether this item is not avialable for trading, but can still be used by the trading post for manufacturing processes"), GUILayout.Width (15f));
																EditorGUILayout.LabelField (new GUIContent ("L", "Place limits on the number of the item that the trade post can have"), GUILayout.Width (15f));
						
																GUILayout.FlexibleSpace ();
																EditorGUILayout.EndHorizontal ();												
												
																EditorGUI.indentLevel = 0;
																for (int s = 0; s<stockGroup.arraySize; s++) {//go through all goods in stock
																		SerializedProperty currentStock = stockGroup.GetArrayElementAtIndex (s);
																		
																		EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
							
																		EditorGUILayout.BeginHorizontal ();
							
																		EditorGUILayout.BeginVertical ();
																		if ((controllerSO.FindProperty ("showPrices").boolValue || customPricing.boolValue) && GoodEnabled (currentStock))
																				GUILayout.Space (10f);
										
										
																		EditorGUILayout.BeginHorizontal ();
														
																		EditorGUI.indentLevel = 1;
																		EditorGUILayout.PrefixLabel (new GUIContent (controllerNormal.allNames [g] [s]));
																		EditorGUI.indentLevel = 0;
														
																		GUILayout.Space (10f);	
															
																		currentStock.FindPropertyRelative ("buy").boolValue = EditorGUILayout.Toggle (GUIContent.none, currentStock.FindPropertyRelative ("buy").boolValue, GUILayout.Width (15f));
																		currentStock.FindPropertyRelative ("sell").boolValue = EditorGUILayout.Toggle (GUIContent.none, currentStock.FindPropertyRelative ("sell").boolValue, GUILayout.Width (15f));
																		currentStock.FindPropertyRelative ("hidden").boolValue = EditorGUILayout.Toggle (GUIContent.none, currentStock.FindPropertyRelative ("hidden").boolValue, GUILayout.Width (15f));
							
																		if (GoodEnabled (currentStock))
																				currentStock.FindPropertyRelative ("minMax").boolValue = EditorGUILayout.Toggle (GUIContent.none, currentStock.FindPropertyRelative ("minMax").boolValue, GUILayout.Width (15f));

																		EditorGUILayout.EndHorizontal ();
																		EditorGUILayout.EndVertical ();
														
																		EditorGUI.indentLevel = 1;
							
																		if (GoodEnabled (currentStock)) {
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
							
																				if (currentStock.FindPropertyRelative ("minMax").boolValue) {//only show min max options if enabled
																						EditorGUI.indentLevel = 2;
								
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
																						if (ma < 0)
																								ma = 0;
																						//set min to be > 0 and < max
									
																						//set the numbers
																						currentStock.FindPropertyRelative ("min").intValue = mi;
																						currentStock.FindPropertyRelative ("max").intValue = ma;
																		
																				}//end show min max
																		} else //end if is enabled
																				EditorGUILayout.EndHorizontal ();
																		EditorGUI.indentLevel = 1;
																}//end for all goods
												
																//needs to go through all items and see if any had BSH changes
																for (int s = 0; s<stockGroup.arraySize; s++) {//go through all items
																		//use the enabled or disabled selection to find what has changed
																		//this is so that if the hidden option is pressed, buy and sell are disabled
																		//but if buy or sell are pressed, then hidden becomes disabled
																		if ((stockGroup.GetArrayElementAtIndex (s).FindPropertyRelative ("buy").boolValue && !before [s] [0]) || (stockGroup.GetArrayElementAtIndex (s).FindPropertyRelative ("sell").boolValue && !before [s] [1]))
								//if buy or sell have just been enabled
																				stockGroup.GetArrayElementAtIndex (s).FindPropertyRelative ("hidden").boolValue = false;
																		if (stockGroup.GetArrayElementAtIndex (s).FindPropertyRelative ("hidden").boolValue && !before [s] [2])
								//if hidden has been enabled
																				stockGroup.GetArrayElementAtIndex (s).FindPropertyRelative ("buy").boolValue = stockGroup.GetArrayElementAtIndex (s).FindPropertyRelative ("sell").boolValue = false;
																}//end for all items
														}//end if group open
														EditorGUI.indentLevel = 0;
														EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
												}//end if something in group
										}//end for all groups
								} else//end if something to show
										EditorGUILayout.HelpBox ("No goods have been added. Add goods in the controller first.", MessageType.Info);
								break;
				#endregion
			
				#region manufacturing
						case 2:
								scrollPos.PM = GUITools.PTMan (manufacturing, scrollPos.PM, smallScroll, controllerMan, controllerNormal, postSO, true);
								break;
				#endregion
						}//end switch
						
						if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
								EditorGUILayout.EndScrollView ();
						
						postSO.ApplyModifiedProperties ();
						controllerSO.ApplyModifiedProperties ();
						controllerNormal.selected.P = sel;
				}//end OnInspectorGUI
	
				bool GoodEnabled (SerializedProperty checkItem)
				{//check to see if the current item is enabled at all
						if (checkItem.FindPropertyRelative ("buy").boolValue ||
								checkItem.FindPropertyRelative ("sell").boolValue || 
								checkItem.FindPropertyRelative ("hidden").boolValue)
								return true;
						return false;
				}//end GoodEnabled
		}//end PostEditor
}//end namespace