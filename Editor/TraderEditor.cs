using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CallumP.TagManagement;

namespace CallumP.TradeSys
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
				private SerializedProperty targetPost;
				private SerializedProperty cargoSpace;
				private SerializedProperty cash;
				private SerializedProperty closeDistance;
				private SerializedProperty collect;
				private SerializedProperty item;
				private SerializedProperty manufacturing;
				private SerializedProperty dropCargo;
				private SerializedProperty dropSingle;
				GameObject[] posts;
				bool expendable;
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
		
						targetPost = traderSO.FindProperty ("target");
						cargoSpace = traderSO.FindProperty ("cargoSpace");
						cash = traderSO.FindProperty ("cash");
						closeDistance = traderSO.FindProperty ("closeDistance");
						collect = traderSO.FindProperty ("allowCollect");
		
						posts = GameObject.FindGameObjectsWithTag (Tags.TP);
		
						manufacturing = traderSO.FindProperty ("manufacture");
						
						dropCargo = traderSO.FindProperty ("dropCargo");
						dropSingle = traderSO.FindProperty ("dropSingle");
	
						GUITools.GetNames (controllerNormal);
						GUITools.ManufactureInfo (controllerNormal);
						
						expendable = controllerNormal.expTraders.enabled;
				
						if (!Application.isPlaying)//only do this if it isnt playin
								controllerNormal.SortAll ();
				}//end OnEnable
	
				public override void OnInspectorGUI ()
				{
						Undo.RecordObject (traderNormal, "TradeSys Trader");
						EditorGUIUtility.fieldWidth = 30f;
						
						traderSO.Update ();
						controllerSO.Update ();
						
			traderNormal.factions = controllerNormal.SortTags(traderNormal.gameObject, true);//make sure factions there
			
						sel = GUITools.Toolbar (sel, new string[] {
										"Settings",
										"Items",
										"Manufacturing"
								});//show a toolbar
				
						switch (sel) {
					#region settings
						case 0:
								EditorGUI.indentLevel = 0;
								scrollPos.TS = GUITools.StartScroll (scrollPos.TS, smallScroll);
					
					#region options
								if (GUITools.TitleGroup (new GUIContent ("Options", "Set general information for the trader which will affect trading"), controllerSO.FindProperty ("opT"), false)) {//if showing options
												
										if (!expendable) {//if not expendable, then show target post
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
												if (GUILayout.Button (new GUIContent ("Set location", "Set the location of the trader to that of the selected target post"), EditorStyles.miniButtonRight, GUILayout.MaxWidth (GUI.skin.button.CalcSize (new GUIContent ("Set location")).x)))
traderNormal.gameObject.transform.position = traderNormal.target.transform.position;
												GUI.enabled = true;
												EditorGUILayout.EndHorizontal ();
										}//end if not expendable
					
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (closeDistance, new GUIContent ("Close distance", "If the trader is within this distance to a trade post or an item, it will register as being there"));
										EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
										if (closeDistance.floatValue < 0)
												closeDistance.floatValue = 0;
					
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (cargoSpace, new GUIContent ("Cargo space", "This is the amount of space available to the trader"));
												
										if (!expendable)//if not expendable, then show credits
												EditorGUILayout.PropertyField (cash, new GUIContent ("Credits", "The amount of money that the trader has in order to purchase goods. Make sure that the trader has enough money to be able to make purchases!"));
										else
												EditorGUILayout.LabelField ("");
												
										EditorGUILayout.EndHorizontal ();
										if (cargoSpace.floatValue < 0.000001f)
												cargoSpace.floatValue = 0.000001f;
					
										if (cash.floatValue < 0)
												cash.floatValue = 0;
														
										if (controllerNormal.pickUp)//only need to show this if is enabled in the controller
												EditorGUILayout.PropertyField (collect, new GUIContent ("Allow collection", "If enabled, will allow this trader to collect dropped items"));
						
										if (!expendable) {
												GUIContent[] types = new GUIContent[] {
																new GUIContent ("Standard", "The standard trade type where the trader will go from place to place"),
																new GUIContent ("Depot - backhaul", "The trader will return to a starting post when it has reached its destination. Will try and take cargo back"),
																new GUIContent ("Depot - no backhaul", "The trader will return to a starting post when it has reached its destination. Will not try and take cargo back")
														};//end different types
														
												traderNormal.tradeType = EditorGUILayout.Popup (new GUIContent ("Trade type", "Select the trading type"), traderNormal.tradeType, types, "DropDownButton");
										}//end if not expendable
						
										if (controllerNormal.pickUp) {//only need to show this if is enabled in the controller
												if (dropCargo.boolValue)
														EditorGUILayout.BeginHorizontal ();
						
												EditorGUILayout.PropertyField (dropCargo, new GUIContent ("Drop cargo", "When the trader is destroyed (see manual), drop the cargo in cargo crates"));
						
												if (dropCargo.boolValue) {
														EditorGUILayout.PropertyField (dropSingle, new GUIContent ("Same crate", "If selected, will drop all of a single item in the same crate. If disabled, each individual item will have its own crate"));
														EditorGUILayout.EndHorizontal ();
												}//end if drop cargo selected
										}//end if allow pickup
						
								}//end showing options
								EditorGUILayout.EndVertical ();
					#endregion
								break;
					#endregion
					
					#region items
						case 1:
								scrollPos.TG = GUITools.EnableDisableItems ("Allow item trade", "expandedT", scrollPos.TG, controllerSO, traderSO.FindProperty ("items"), smallScroll, controllerNormal);
								break;
					#endregion
					
					#region manufacturing
						case 2:
								scrollPos.TM = GUITools.PTMan (manufacturing, scrollPos.TM, smallScroll, controllerSO.FindProperty ("manufacture"), controllerNormal, traderSO, false);
								break;
					#endregion
						}//end switch	
					
						if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
								EditorGUILayout.EndScrollView ();							
		
						traderSO.ApplyModifiedProperties ();
						controllerSO.ApplyModifiedProperties ();
						controllerNormal.selected.T = sel;
				}//end OnInspectorGUI
		}//end TraderEditor
}//end namespace