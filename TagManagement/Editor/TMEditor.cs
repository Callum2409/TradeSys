using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;

namespace CallumP.TagManagement
{//use namespace to stop any name conflicts
[CustomEditor(typeof(TagManager))]
public class TMEditor : Editor
{
		private SerializedObject tagObj;
		TagManager tagMan;
		private SerializedProperty groupName;
		private SerializedProperty shortName;
		private SerializedProperty singName;
		private SerializedProperty minNo;
		private SerializedProperty showCol;
		private ReorderableList list;
		bool locked;
		bool settings;
		int selected;
	
		void OnEnable ()
		{
				tagObj = new SerializedObject (target);
				tagMan = (TagManager)target;
				groupName = tagObj.FindProperty ("groupName");
				singName = tagObj.FindProperty ("singular");
				shortName = tagObj.FindProperty ("shortName");
				minNo = tagObj.FindProperty ("minNumber");
				showCol = tagObj.FindProperty("colours");
				
				locked = tagObj.FindProperty ("locked").boolValue;
				
				list = new ReorderableList (tagObj, tagObj.FindProperty ("nameCol"), true, true, true, true);

				list.drawHeaderCallback = (Rect rect) => {//make the header
						GUI.color = (CheckDup () && !locked) ? Color.red : Color.white;//if duplicate name and is unlocked, make red
						EditorGUI.LabelField (rect, groupName.stringValue, EditorStyles.boldLabel);
						
						string disp = "Number of " + shortName.stringValue + ": " + list.count;//work out what is shown so size can be calculated
						float len = GUI.skin.label.CalcSize (new GUIContent (disp, "")).x;//calculate the size so can set correct positions
			
						EditorGUI.LabelField (new Rect (rect.width - len + (locked ? 20 : -50), rect.y, len, EditorGUIUtility.singleLineHeight), disp);
						
						if (!locked)//if locked so cant change any settings
								settings = GUI.Button (new Rect (rect.width - 40, rect.y, 60, EditorGUIUtility.singleLineHeight), "Settings");
						GUI.color = Color.white;
				};//end drawHeader
				list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {//draw the names
						SerializedProperty element = tagObj.FindProperty ("nameCol").GetArrayElementAtIndex (index);
				SerializedProperty elementName = element.FindPropertyRelative("tagName");
				rect.y += 2;
				
				float width = Mathf.Min(120, rect.width/3)-20;//work out the width for the colour selector
				if(showCol.boolValue)//if has a colour option too
				rect.width -= width;

						EditorGUI.BeginChangeCheck ();
						EditorGUI.PropertyField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), elementName, new GUIContent (elementName.stringValue, "Name of the item in this tag group"));
				
				
						if (elementName.stringValue == "") {
					elementName.stringValue = singName.stringValue + " " + index;
								tagMan.NameChanger (index, elementName.stringValue);
						}

						if (EditorGUI.EndChangeCheck ())
								tagMan.NameChanger (index, elementName.stringValue);
								
				if(showCol.boolValue)//if has a colour option too
				EditorGUI.PropertyField(new Rect(rect.width +40, rect.y, width-10, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("colour"), GUIContent.none);
				
			};//end drawElement
				list.onSelectCallback = (ReorderableList l) => {//get the currently selected element
						selected = l.index;
				};
				list.onReorderCallback = (ReorderableList l) => {//when reordered, needs to sort all
						tagMan.Reorder (selected, l.index);
				};
				list.onAddCallback = (ReorderableList l) => {//update all scripts of new element
						AddItem (l);
				};//end addCallback
				list.onRemoveCallback = (ReorderableList l) => {//update scripts when removed an element
						l.serializedProperty.DeleteArrayElementAtIndex (l.index);//remove the item
						tagMan.AddRemove (l.index, false, "");
				};//end removeCallback
				list.onCanRemoveCallback = (ReorderableList l) => {//if the button has more than the minimum, enable it
						return l.count > tagMan.minNumber;
				};//end canRemoveCallback
		}//end OnEnable
	
		public override void OnInspectorGUI ()
		{
				tagObj.Update ();

				if (settings || groupName.stringValue == "") {
						settings = true;
						Color col = (CheckDup () && !locked) ? Color.red : Color.white;//if duplicate name and is unlocked, make red
						
						GUI.color = col;
						EditorGUILayout.PropertyField (groupName, new GUIContent ("Name", "The name of the tag group"));
						GUI.color = Color.white;
						
						EditorGUILayout.PropertyField (singName, new GUIContent ("Singular name", "The singular case of the name"));
						EditorGUILayout.PropertyField (shortName, new GUIContent ("Short name", "Shorter name to be used"));
						EditorGUILayout.PropertyField (minNo, new GUIContent ("Min number", "The minimum number allowed"));
				EditorGUILayout.PropertyField(showCol, new GUIContent("Colour selector", "Show a colour selector option for each tag"));
				
				if(minNo.intValue<0)//cant have min < 0, so make 0 if it is
				minNo.intValue = 0;
				
						if (groupName.stringValue == "")
								groupName.stringValue = "New Group";
						
						if (singName.stringValue == "")
								singName.stringValue = groupName.stringValue;
		
						if (shortName.stringValue == "")
								shortName.stringValue = groupName.stringValue;				
						shortName.stringValue = shortName.stringValue.ToLower ();//make this lower case as required
				
						EditorGUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace ();
						
						GUI.color = col;
						if (GUILayout.Button ("Done")) {//if done pressed
								settings = false;//now close the settings
				
								while (list.count < tagMan.minNumber)//if not enough items
										AddItem (list);//then add some
						}//end if done pressed
						GUI.color = Color.white;
						
						GUILayout.FlexibleSpace ();
						EditorGUILayout.EndHorizontal ();
				} else
						list.DoLayoutList ();//show the list
	
				tagObj.ApplyModifiedProperties ();
		}//end OnInspectorGUI
		
		void AddItem (ReorderableList l)
		{//add an item to the list
				l.serializedProperty.arraySize++;//increase the array size
				string newName = singName.stringValue + " " + (l.count - 1);//get the new neame
				l.serializedProperty.GetArrayElementAtIndex (l.count - 1).FindPropertyRelative("tagName").stringValue = newName;//set the name
				tagMan.AddRemove (l.count - 1, true, newName);//push updates to everything else
		}//end AddItem
	
		bool CheckDup ()
		{//check that there are no other tag managers with the same name
				//get all of the tag managers in the scene
				TagManager[] allManagers = (TagManager[])FindObjectsOfType (typeof(TagManager));
				TagManager thisMan = (TagManager)target;//current manager
		
				for (int g=0; g<allManagers.Length; g++) {
						if (allManagers [g] != thisMan)//if not current manager
						if (allManagers [g].groupName == thisMan.groupName)//if has same name
								return true;
				}
		
				return false;
		}//end CheckDup
}//end TMEditor
}//end namespace