using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CallumP.TagManagement
{//use namespace to stop any name conflicts
		[CustomEditor(typeof(ObjectTags)), CanEditMultipleObjects]
		public class TagEditor : Editor
		{
				private SerializedObject tagObj;
				ObjectTags tagNormal;
				private SerializedObject connected;
				private ReorderableList list;
				private SerializedProperty tagList;
				bool locked;
				TagManager[] managers;
				bool sort;
				bool con;//whether connected so dont need to run method every time
		
				void OnEnable ()
				{
						tagObj = serializedObject;
						tagList = tagObj.FindProperty ("tags");
		
						locked = tagObj.FindProperty ("locked").boolValue;

						//if not recieved the updates, get required information now
						if (Connected ()) {
								//needs to update and apply properties otherwise wont actually make any changes
								tagObj.Update ();
								Sort ();
								tagObj.ApplyModifiedProperties ();
						}//end if connected
			

						list = new ReorderableList (tagObj, tagList, false, true, false, false);

						list.drawHeaderCallback = (Rect rect) => {//make the header
								Vector2 all = EditorStyles.miniButtonLeft.CalcSize (new GUIContent ("Select all", ""));
								Vector2 none = EditorStyles.miniButtonRight.CalcSize (new GUIContent ("Select none", ""));
				
								Rect textRect = rect;
								textRect.width = rect.width - all.x - none.x - 5;
								if (locked)
										EditorGUI.LabelField (textRect, connected.FindProperty ("groupName").stringValue, EditorStyles.boldLabel);
								else {//else not locked so can select a different group
										GUI.color = SameGroup () ? Color.red : Color.white;
										int index = System.Array.IndexOf (managers, tagObj.FindProperty ("connected").objectReferenceValue);
										int curSelOrig = Mathf.Max (0, index);//get current group, or group 0 if not selected
								
										int curSel = EditorGUI.Popup (textRect, curSelOrig, managers.Select (i => i.groupName).ToArray ());//get selected group
				
										tagObj.FindProperty ("connected").objectReferenceValue = managers [curSel];//apply selected group
										sort = curSel != curSelOrig || index == -1;//sort if changed or just added
										GUI.color = Color.white;
								}		
								if (GUI.Button (new Rect (textRect.width + 25, rect.y, all.x, all.y), new GUIContent ("Select all", ""), EditorStyles.miniButtonLeft))
										SelectAllNone (true);			
								if (GUI.Button (new Rect (textRect.width + 25 + all.x, rect.y, none.x, none.y), new GUIContent ("Select none", ""), EditorStyles.miniButtonRight))
										SelectAllNone (false);
						};
						
						list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {//draw the names
								SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex (index);
								rect.y += 2;
								string name = element.FindPropertyRelative ("tagName").stringValue;
								if (name == "")
										name = " ";
								EditorGUI.PropertyField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), 
							element.FindPropertyRelative ("selected"), new GUIContent (name, ""));
			
						};
				}//end OnEnable
	
				public override void OnInspectorGUI ()
				{
						tagObj.Update ();
						if (!con) { //if not connected to a group
								EditorGUILayout.HelpBox ("No managers to connect to. Add a TagManager to begin.", MessageType.Error);
								Connected ();
						} else {
								if (AllSameGroup ())//if all the selected objects are for the same group
										list.DoLayoutList ();//show the list
								else
										EditorGUILayout.HelpBox ("Not all of the selected objects have the same managers. Make sure they are connected to the same before editing multiple objects", MessageType.Warning);
						}//end else connected to a group
			
						if (sort) {//if the connected group has changed
								Connected ();
								tagList.ClearArray ();
								Sort ();
						}//end if changed
			
						tagObj.ApplyModifiedProperties ();
				}//end OnInspectorGUI
		
				bool Connected ()
				{//check has connected to a group
						managers = (TagManager[])FindObjectsOfType (typeof(TagManager));//get all managers
						SerializedProperty connectedObject = tagObj.FindProperty ("connected");
						
						if (connectedObject.objectReferenceValue == null) {//if null
								if (managers.Length > 0)//if can set
										connectedObject.objectReferenceValue = managers [0];//set to the first one
				else
										con = false;
						} else {//end if null
								connected = new SerializedObject (connectedObject.objectReferenceValue);//set the connected
								con = true;
								Sort ();
						}
						return con;
				}//end Connected
		
		
				void Sort ()
				{//sort out the number of tags and their names
						if (!AllSameGroup ())//if not all same group, cant sort so return
								return;
						SerializedProperty conNames = connected.FindProperty ("nameCol");
						tagList.arraySize += conNames.arraySize - tagList.arraySize;//increase or decrease the array size
		
						for (int i = 0; i<tagList.arraySize; i++) {
								string name = conNames.GetArrayElementAtIndex (i).FindPropertyRelative ("tagName").stringValue;//get the name
								tagList.GetArrayElementAtIndex (i).FindPropertyRelative ("tagName").stringValue = name;	//set the name
				
								if (name == "Default")//if default enable
										tagList.GetArrayElementAtIndex (i).FindPropertyRelative ("selected").boolValue = true;
						}//end for sorting all names
				}//end Sort

				bool SameGroup ()
				{//if there are any other linked to the same group on this gameobject
						ObjectTags thisGroup = ((ObjectTags)target);
						ObjectTags[] allObjectTags = thisGroup.GetComponents<ObjectTags> ();//get all group components
						for (int g= 0; g<allObjectTags.Length; g++) {//for all groups
								if (allObjectTags [g] != thisGroup)//if not current group
								if (allObjectTags [g].connected == thisGroup.connected)//if connected to the same group
										return true;
						}//end for all groups
						return false;//not already connected, so return false
				}//end SameGroup
				
				bool AllSameGroup ()
				{//check that all of the selected objects point to the same manager
						Object[] all = tagObj.targetObjects;
						TagManager first = ((ObjectTags)all [0]).connected;
				
						if (all.Length > 1) {//if more than one selected, check
								for (int t = 1; t<all.Length; t++)
										if (((ObjectTags)all [t]).connected != first)//if not connected to the same
												return false;//return
						}//end if more than one
						return true;
				}//end AllSameGroup
		
				void SelectAllNone (bool select)
				{//select all or none of the items in the list
						SerializedProperty itemArray = list.serializedProperty;
						for (int i = 0; i<itemArray.arraySize; i++)
								itemArray.GetArrayElementAtIndex (i).FindPropertyRelative ("selected").boolValue = select;
		
				}//end SelectAllNone
		}//end TagEditor
}//end namespace