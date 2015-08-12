#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define API
#endif
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys
{//use namespace to stop any name conflicts
		[CanEditMultipleObjects, CustomEditor(typeof(Trader))]
		public class TraderEditor : Editor
		{
	
				TSGUI GUITools = new TSGUI ();//extra gui methods which are used by all TradeSys scripts
	
		#region options
				private int sel;
				private ScrollPos scrollPos;
		#endregion
	
		#region variables
				private SerializedObject controllerSO;
				private Controller controllerNormal;
				private SerializedObject traderSO;
				private Trader traderNormal;
				private SerializedProperty smallScroll;
				private SerializedProperty controllerGoods;
				private SerializedProperty targetPost;
				private SerializedProperty cargoSpace;
				private SerializedProperty cash;
				private SerializedProperty closeDistance;
				private SerializedProperty item;
				private SerializedProperty factions;
		private SerializedProperty manufacturing;
				GameObject[] posts;
		#endregion
	
				void OnEnable ()
				{
						controllerNormal = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
						controllerSO = new SerializedObject (controllerNormal);
		
						traderSO = new SerializedObject (targets);
						traderNormal = (Trader)target;
						traderNormal.tag = Tags.T;
		
						sel = controllerNormal.selected.T;
						smallScroll = controllerSO.FindProperty ("smallScroll");
		
						scrollPos = controllerNormal.scrollPos;
			
						controllerGoods = controllerSO.FindProperty ("goods");
		
						targetPost = traderSO.FindProperty ("target");
						cargoSpace = traderSO.FindProperty ("cargoSpace");
						cash = traderSO.FindProperty ("cash");
						closeDistance = traderSO.FindProperty ("closeDistance");
		
						posts = GameObject.FindGameObjectsWithTag (Tags.TP);
						item = traderSO.FindProperty ("items");
		
						factions = traderSO.FindProperty ("factions");
						manufacturing = traderSO.FindProperty("manufacture");
	
						GUITools.GetNames (controllerNormal);
						GUITools.ManufactureInfo (controllerNormal);
						controllerNormal.SortAll();
				}//end OnEnable
	
				public override void OnInspectorGUI ()
				{
						#if !API
						Undo.RecordObject (controllerNormal, "TradeSys Trader");
						#endif	
			
						if (PrefabUtility.GetPrefabType (traderNormal.gameObject) == PrefabType.Prefab)//if is prefab, show info to why no options can be set
								EditorGUILayout.HelpBox ("Nothing here can be edited because this is a prefab."/*\nAllow expendable traders in the controller and add this as one in order to be able to set options"*/, MessageType.Info);
						else {//else show options because is not a prefab
								traderSO.Update ();
								controllerSO.Update ();
				
								sel = GUITools.Toolbar (sel, new string[] {
										"Settings",
										"Items",
										"Manufacturing"
								});//show a toolbar
				
								switch (sel) {
					#region settings
								case 0:
										EditorGUI.indentLevel = 0;
										GUITools.HorizVertOptions (controllerSO.FindProperty ("showHoriz"));//show display options
					
										scrollPos.TS = GUITools.StartScroll (scrollPos.TS, smallScroll);
					
					
					#region options
					if(GUITools.TitleGroup(new GUIContent("Options", "Set general information for the trader which will affect trading"), controllerSO.FindProperty("opT"), false)){//if showing options
										EditorGUILayout.BeginHorizontal ();
										if (targetPost.objectReferenceValue == null)//if no target has been selected
												GUI.color = Color.red;//make it red so is obvious
					
										EditorGUILayout.PropertyField (targetPost, new GUIContent ("Target post", "This is the trade post that the trader is currently at"));
										traderSO.ApplyModifiedProperties ();
					
										if (targetPost.objectReferenceValue != null && ((GameObject)targetPost.objectReferenceValue).tag != Tags.TP)//check that is not null and correct tag
												targetPost.objectReferenceValue = null;//set to null if not a correct tag
					
										GUI.color = Color.white;//set the colour back to white
					
					//buttons need the max width calculation to make them as small as possible, as can't be done using flexible space
										if (GUILayout.Button (new GUIContent ("Find post", "Attempts to find the trade post if one is within the close distance value"), EditorStyles.miniButtonLeft, GUILayout.MaxWidth (GUI.skin.button.CalcSize (new GUIContent ("Find post")).x))) {
												bool find = false;//bool so that if not close enough, can display a message
												for (int p = 0; p<posts.Length; p++) {//go through all posts
														if (Vector3.Distance (traderNormal.transform.position, posts [p].transform.position) <= closeDistance.floatValue) {//check close enough
								
																#if API
								Undo.RegisterUndo ((Trader)target, "TradeSys Trader");
																#endif
								
																GameObject post = posts [p];
																targetPost.objectReferenceValue = post;//set the target
																traderSO.ApplyModifiedProperties ();//apply the modified properties so can then move the trader to the location
																traderNormal.transform.position = post.transform.position;//move to the position of the trade post
																find = true;//set to true because have found a trade post
																break;//break bevause found a trade post
														}//end if close enough
												}//end for all posts
												if (!find)//check to see if a post was found
														EditorUtility.DisplayDialog ("No posts found", "No trade posts were found close enough. Try moving the trader closer, or increase the close distance value and try again.", "Ok");
										}//end find post pressed
										GUI.enabled = traderNormal.target != null;
										if (GUILayout.Button (new GUIContent ("Set location", "Set the location of the trader to that of the selected target post"), EditorStyles.miniButtonRight, GUILayout.MaxWidth (GUI.skin.button.CalcSize (new GUIContent ("Set location")).x))) {
						
												#if API
						Undo.RegisterUndo ((Trader)target, "TradeSys Trader");
												#endif
						
												traderNormal.gameObject.transform.position = traderNormal.target.transform.position;
										}//end if set location pressed
										GUI.enabled = true;
										EditorGUILayout.EndHorizontal ();
					
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (closeDistance, new GUIContent ("Close distance", "If the trader is within this distance to a trade post or an item, it will register as being there"));
										EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
										if (closeDistance.floatValue < 0)
												closeDistance.floatValue = 0;
					
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (cargoSpace, new GUIContent ("Cargo space", "This is the amount of space available to the trader"));
										EditorGUILayout.PropertyField (cash, new GUIContent ("Credits", "The amount of money that the trader has in order to purchase goods. Make sure that the trader has enough money to be able to make purchases!"));
										EditorGUILayout.EndHorizontal ();
										if (cargoSpace.floatValue < 0.000001f)
												cargoSpace.floatValue = 0.000001f;
					
										if (cash.intValue < 0)
												cash.intValue = 0;
												}//end showing options
												EditorGUILayout.EndVertical();
					#endregion
					#region factions
										if (controllerNormal.factions.enabled)//check that the factions have been enabled before showing anything
												GUITools.TGF (controllerNormal, controllerSO, "expandedT", new GUIContent ("Factions", "Select which factions the trader belongs to. In order to be able to trade with a trade post, they both have to have a faction in common"), factions, true, new string[]{}, "factions", "factions");
					#endregion
										break;
					#endregion
					
					#region items
								case 1:
										EditorGUI.indentLevel = 0;
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.LabelField ("Allow item trade", EditorStyles.boldLabel);
										GUILayout.FlexibleSpace ();
										GUITools.ExpandCollapse (controllerGoods, "expandedT", false);
										EditorGUILayout.EndHorizontal ();
					
										GUITools.HorizVertOptions (controllerSO.FindProperty ("showHoriz"));//show display options
					
										scrollPos.TG = GUITools.StartScroll (scrollPos.TG, smallScroll);
					
										if (GUITools.AnyGoods (item, "items")){
												EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
					
										for (int g = 0; g<controllerNormal.goods.Count; g++) {
												SerializedProperty itemGroup = item.GetArrayElementAtIndex (g).FindPropertyRelative ("items");
												if (itemGroup.arraySize > 0) {//dont show anything if nothing in group
														EditorGUI.indentLevel = 0;
							
														SerializedProperty currentGroup = controllerGoods.GetArrayElementAtIndex (g);
														SerializedProperty expanded = currentGroup.FindPropertyRelative ("expandedT");
							
														if (expanded.boolValue)
																EditorGUILayout.BeginHorizontal ();
							
														if (GUITools.TitleButton (new GUIContent (currentGroup.FindPropertyRelative ("name").stringValue), expanded, "BoldLabel")) {//if foldout for goods group open											

																GUITools.EnableDisable (itemGroup, "enabled", true);
								
																GUITools.HorizVertDisplay (controllerNormal.allNames [g], itemGroup, "enabled", controllerSO.FindProperty ("showHoriz").boolValue, 1, true);
														}//end if group open
														EditorGUI.indentLevel = 0;
														EditorGUILayout.LabelField ("", "", "PopupCurveSwatchBackground", GUILayout.MaxHeight (0f));
												}//end if something in group
										}//end for all groups
										}else//end if something to show
						EditorGUILayout.HelpBox("No goods have been added. Add goods in the controller first.", MessageType.Info);
										break;
					#endregion
					
					#region manufacturing
								case 2:
										//scrollPos.TM = GUITools.StartScroll (scrollPos.TM, smallScroll);
										scrollPos.TM = GUITools.PTMan(manufacturing, scrollPos.TM, smallScroll, controllerSO.FindProperty("manufacture"), controllerNormal, traderSO, false);
										break;
					#endregion
								}//end switch	
					
								if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
										EditorGUILayout.EndScrollView ();							
		
								traderSO.ApplyModifiedProperties ();
								controllerSO.ApplyModifiedProperties ();
								controllerNormal.selected.T = sel;
						}//end else not a prefab
				}//end OnInspectorGUI
		}//end TraderEditor
}//end namespace