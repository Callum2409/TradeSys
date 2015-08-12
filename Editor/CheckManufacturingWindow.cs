using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys
{//use namespace to stop any name conflicts
		public class CheckManufacturingWindow : EditorWindow
		{
				Controller controller;
				bool showHoriz;
				Vector2 scrollPosN, scrollPosP;
				TSGUI GUITools = new TSGUI ();
				int selection;
				public float[][] perItem;//the per item changes
				public float total;//the total changes
				string[][] info;//the item changes strings
				string totalInfo;
				string[][] pricing;
	
				void Awake ()
				{
						controller = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
						showHoriz = controller.showHoriz;//everything will be shown with the horizontal or vertical
				}//end Awake
	
				void OnGUI ()
				{		
						CalcInfo ();//calculate all of the required info
			
						EditorGUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace ();
						string[] options = new string[]{"Item numbers", "Item pricing"};
			selection = GUILayout.Toolbar (selection, controller.expTraders.enabled ? new string[]{options [0]} : options);
						GUILayout.FlexibleSpace ();
						EditorGUILayout.EndHorizontal ();
		
						switch (selection) {
				#region item numbers
						case 0:
								EditorGUI.indentLevel = 0;
								EditorGUILayout.LabelField ("NOTE: This can only be used as a guide because there may be pauses and greater times between " +
										"manufacturing processes if items are not available.\n\nAs a result, there will be some variances, but will still " +
										"be useful to give an idea of whether the numbers of an item is expected to increase, decrease or stay the same.\n\n" +
										"A larger number means that this change is faster.", EditorStyles.wordWrappedLabel);//the text which is always displayed
		
								EditorGUILayout.LabelField ("", "", "ShurikenLine", GUILayout.MaxHeight (1f));//draw a separating line
								scrollPosN = EditorGUILayout.BeginScrollView (scrollPosN);
			
								EditorGUILayout.BeginVertical ("HelpBox");
			
								for (int g = 0; g<controller.goods.Count; g++) {//for all goods 
										EditorGUI.indentLevel = 0;
										EditorGUILayout.LabelField (controller.goods [g].name, EditorStyles.boldLabel);
										GUITools.HorizVertDisplay (controller.allNames [g], info [g], new string[info [g].Length], showHoriz, 1);
								}
					
								EditorGUI.indentLevel = 0;
								EditorGUILayout.Space ();//space between all the items and the total change
								EditorGUILayout.LabelField ("Total change", totalInfo);
								EditorGUILayout.EndVertical ();
								GUILayout.EndScrollView ();
								break;
				#endregion
		
				#region pricing
						case 1:
								EditorGUI.indentLevel = 0;
								EditorGUILayout.LabelField ("This is showing the profit per time the manufacturing process occurs. This is a best-case scenario, where the cost of items purchased to manufacture are at their lowest, and the items made are sold at the highest. As a result, the profits are likely to be lower than this.\n\nAny process that shows a negative value here will always have a loss.\n\nThis assumes that the item prices are set automatically", EditorStyles.wordWrappedLabel);
		
								EditorGUILayout.LabelField ("", "", "ShurikenLine", GUILayout.MaxHeight (1f));//draw a separating line
								scrollPosP = EditorGUILayout.BeginScrollView (scrollPosP);
		
								EditorGUILayout.BeginVertical ("HelpBox");
								for (int m = 0; m<controller.manufacture.Count; m++) {
										EditorGUI.indentLevel = 0;
										EditorGUILayout.LabelField (controller.manufacture [m].name, EditorStyles.boldLabel);
										GUITools.HorizVertDisplay (controller.manufactureNames [m], pricing [m], controller.manufactureTooltips [m], showHoriz, 1);
								}
								EditorGUILayout.EndVertical ();
			
								EditorGUILayout.EndScrollView ();
								break;
				#endregion
						}//end switch
				}//end OnGUI
		
				void CalcInfo ()
				{//calculate the information
						//reset some of the information so that it will be correct if the number of items changes
						perItem = new float[controller.goods.Count][];//the change for each item
						info = new string[controller.goods.Count][];//an array containing all of the strings to display
						for (int g = 0; g<controller.goods.Count; g++) {
								perItem [g] = new float[controller.goods [g].goods.Count];
								info [g] = new string[controller.allNames [g].Length];
						}
						total = 0;//reset the total
						pricing = new string[controller.manufacture.Count][];
			
						List<MnfctrTypes> manufacture = controller.manufacture;
			
						for (int m1 = 0; m1<manufacture.Count; m1++) {//go through all manufacture groups
								int processCount = manufacture [m1].manufacture.Count;
								pricing [m1] = new string[processCount];
				
								for (int m2 = 0; m2<processCount; m2++) {//go through all processes					
										#region number change
										Mnfctr cMan = controller.manufacture [m1].manufacture [m2];
										for (int p = 0; p<controller.postScripts.Length; p++)//go through all posts
												if (controller.postScripts [p].manufacture [m1].enabled)//only count if group enabled
														NumberChange (controller.postScripts [p].manufacture [m1].manufacture [m2], cMan);
										for (int t = 0; t<controller.traderScripts.Length; t++)//go through all traders
												if (controller.traderScripts [t].manufacture [m1].enabled)//only count if group enabled
														NumberChange (controller.traderScripts [t].manufacture [m1].manufacture [m2], cMan);
										#endregion
					
										#region pricing
										int profit = 0;
										Mnfctr current = controller.manufacture [m1].manufacture [m2];
										for (int nm = 0; nm < current.needing.Count; nm++) {//go through all needing, getting min cost
												NeedMake currentNM = current.needing [nm];
												profit -= controller.goods [currentNM.groupID].goods [currentNM.itemID].minPrice * currentNM.number;
										}//end for needing
										for (int nm = 0; nm < current.making.Count; nm++) {//go through all making, getting max price
												NeedMake currentNM = current.making [nm];
												profit += controller.goods [currentNM.groupID].goods [currentNM.itemID].maxPrice * currentNM.number;
										}//end for needing
										pricing [m1] [m2] = profit.ToString ();
										#endregion
								}//end for all processes			
						}//end for manufacture groups
			
						for (int x = 0; x<info.Length; x++)//go through all goods, saying increase, decrease or stay the same and give a value
								for (int y = 0; y<info[x].Length; y++)
										info [x] [y] = SetChange (perItem [x] [y]);
			
						totalInfo = SetChange (total);//add a change string to the total change
				}//end CalcInfo
		
				void NumberChange (RunMnfctr man, Mnfctr cMan)
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
		
				string SetChange (float change)
				{//check to see if increase, decrease or the same
						if (change > 0)
								return "increase (" + change.ToString ("f2") + ")";
						if (change < 0)
								return "decrease (" + Mathf.Abs (change).ToString ("f2") + ")";
						return "same";
				}//end SetChange
		
		}//end CheckManufacturingWindow
}//end namespace