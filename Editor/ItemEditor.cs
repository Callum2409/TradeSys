using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys
{//use namespace to stop any name conflicts
		[CanEditMultipleObjects, CustomEditor(typeof(Item))]
		public class ItemEditor : Editor
		{
				private Controller controller;
				private SerializedObject itemSO;
				private Item itemNormal;
				private SerializedProperty groupID, itemID, number;
				private SerializedProperty traderCollect;

				void OnEnable ()
				{
						controller = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
			
						itemSO = new SerializedObject (targets);
						itemNormal = (Item)target;
						itemNormal.tag = Tags.I;
			
						groupID = itemSO.FindProperty ("groupID");
						itemID = itemSO.FindProperty ("itemID");
						number = itemSO.FindProperty ("number");
			
						traderCollect = itemSO.FindProperty ("traderCollect");
				}//end OnEnable
	
				public override void OnInspectorGUI ()
				{
						if (PrefabUtility.GetPrefabType (itemNormal.gameObject) == PrefabType.Prefab)//if is prefab, show info to why no options can be set
								EditorGUILayout.HelpBox ("Nothing here can be edited because this is a prefab.\nAll of the detals will be set by the controller when it is created.", MessageType.Info);
						else {//else show options because is not a prefab
						
								itemSO.Update ();
				
								#region get goods names
								List<string> allNames = new List<string> ();//list containing all of the goods names
								int[] lengths = new int[controller.goods.Count];//an array containing the lengths of all of the groups
						
								for (int g = 0; g<controller.goods.Count; g++) {//go through groups
										lengths [g] = controller.goods [g].goods.Count;
						
										for (int i = 0; i<lengths[g]; i++) {//go through items
												string name = "";
												if (controller.showGN)
														name = controller.goods [g].name + " ";
												name += controller.goods [g].goods [i].name;
										
												allNames.Add (name);
										}//end for all items
								}//end for groups
								#endregion
						
								GUI.enabled = !Application.isPlaying;//disable any changes if is playing
								EditorGUILayout.BeginHorizontal ();
						
								int selected = ConvertToSelected (lengths);
								selected = EditorGUILayout.Popup (selected, allNames.ToArray (), "DropDownButton");
								ConvertFromSelected (lengths, selected);
			
								EditorGUILayout.PropertyField (number, new GUIContent ("Number", "The number of this item in this crate"));
			
								if (number.intValue < 1)
										number.intValue = 1;			
								EditorGUILayout.EndHorizontal ();
								GUI.enabled = true;//enable GUI again
								
								EditorGUILayout.PropertyField (traderCollect, new GUIContent ("Trader collect", "Allow traders to collect this item"));
						
								itemSO.ApplyModifiedProperties ();
						}//end else show details as is not a prefab
				}//end OnInspectorGUI
				
				int ConvertToSelected (int[] lengths)
				{//convert groupID and itemID to a selected int for the dropdown
						if (itemID.intValue == -1)
								return -1;
				
						int selected = 0;
						for (int g = 0; g<groupID.intValue; g++)
								selected += lengths [g];
						selected += itemID.intValue;
				
						return selected;
				}//end ConvertToSelected
				
				void ConvertFromSelected (int[] lengths, int selected)
				{//convert the selected int into group and item IDs
						if (selected == -1) {
								itemID.intValue = groupID.intValue = -1;
								return;//no need to go through the rest, so return
						}//end if -1
			
						int groupNo = 0;
						while (selected >= lengths[groupNo]) {
								selected -= lengths [groupNo];
								groupNo++;
						}
						itemID.intValue = selected;
						groupID.intValue = groupNo;
				}//end ConvertFromSelected
		}//end ItemEditor
}//end namespace