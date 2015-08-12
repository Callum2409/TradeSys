#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define API
#endif
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace TradeSys
{//use namespace to stop any name conflicts
		[CanEditMultipleObjects, CustomEditor(typeof(Spawner))]
		public class SpawnerEditor : Editor
		{	
				TSGUI GUITools = new TSGUI ();//extra gui methods which are used by all TradeSys scripts
		
		#region options
				private int sel;
				private ScrollPos scrollPos;
		#endregion
		
		#region varibles
				private SerializedObject controllerSO;
				private Controller controllerNormal;
				private SerializedObject spawnerSO;
				private Spawner spawnerNormal;
				private SerializedProperty smallScroll;
				private SerializedProperty minTime, maxTime;
				private SerializedProperty min, max, tot, diff, countCrates;
				private SerializedProperty maxDist;
				private SerializedProperty specifySeed, seed;
				private SerializedProperty traderCollect;
				string[] shapes = new string[]{"Sphere", "Circle", "Cube", "Square"};
				#endregion
		
				void OnEnable ()
				{
						controllerNormal = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
						controllerSO = new SerializedObject (controllerNormal);
			
						spawnerSO = new SerializedObject (targets);
						spawnerNormal = (Spawner)target;
						spawnerNormal.tag = Tags.S;
			
						sel = controllerNormal.selected.S;
						smallScroll = controllerSO.FindProperty ("smallScroll");
			
						minTime = spawnerSO.FindProperty ("minTime");
						maxTime = spawnerSO.FindProperty ("maxTime");
			
						min = spawnerSO.FindProperty ("min");
						max = spawnerSO.FindProperty ("max");
						tot = spawnerSO.FindProperty ("maxSpawn");
						diff = spawnerSO.FindProperty ("diffItems");
						countCrates = spawnerSO.FindProperty("countCrates");
						
						maxDist = spawnerSO.FindProperty ("maxDist");
						
						specifySeed = spawnerSO.FindProperty("specifySeed");
						seed = spawnerSO.FindProperty("seed");
						
						traderCollect = spawnerSO.FindProperty("traderCollect");
			
						scrollPos = controllerNormal.scrollPos;
			
						GUITools.GetNames (controllerNormal);
						
			if(!Application.isPlaying)//only do this if it isnt playin
						controllerNormal.SortAll ();
				}//end OnEnable
	
				public override void OnInspectorGUI ()
				{
						#if !API
						Undo.RecordObject (spawnerNormal, "TradeSys Spawner");
						EditorGUIUtility.fieldWidth = 30f;
						#endif	
			
						spawnerSO.Update ();
						controllerSO.Update ();
			
						sel = GUITools.Toolbar (sel, new string[] {
				"Settings",
				"Items"
			});//show a toolbar
			
						switch (sel) {
				#region settings
						case 0:
								EditorGUI.indentLevel = 0;
				
								scrollPos.SS = GUITools.StartScroll (scrollPos.SS, smallScroll);
				
				#region time
								if (GUITools.TitleGroup (new GUIContent ("Time", "Set the spawn time options"), controllerSO.FindProperty ("sTi"), false)) {//if showing time
				
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (minTime, new GUIContent ("Min", "The minimum time the spawner waits before the next spawn (inclusive)"));
										EditorGUILayout.PropertyField (maxTime, new GUIContent ("Max", "The maximum time the spawner waits before the next spawn (inclusive)"));
										EditorGUILayout.EndHorizontal ();
				
										//make sure the numbers stay in range
										if (minTime.floatValue < 0)
												minTime.floatValue = 0;
										else if (minTime.floatValue > maxTime.floatValue)
												minTime.floatValue = maxTime.floatValue;
										if (maxTime.floatValue < 0)
												maxTime.floatValue = 0;
								}//end if showing time
								EditorGUILayout.EndVertical ();
				#endregion
				#region number
								if (GUITools.TitleGroup (new GUIContent ("Number", "Set the number options"), controllerSO.FindProperty ("sNo"), false)) {//if showing number
				
										EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.PropertyField (min, new GUIContent ("Min", "The minumum number to spawn of an item at once (inclusive). Number spawned may be less due to collision checking"));
										EditorGUILayout.PropertyField (max, new GUIContent ("Max", "The maximum number to spawn of an item at once (inclusive)"));
										EditorGUILayout.EndHorizontal ();
				
										EditorGUILayout.BeginHorizontal ();
										EditorGUILayout.PropertyField (tot, new GUIContent ("Total", "The maximum number to be at the spawner. Set to 0 for infinite"));
										EditorGUILayout.LabelField ("");
										EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal();
										EditorGUILayout.PropertyField (diff, new GUIContent ("Separate", "If the number is greater than 1, make each item have a separate crate. Unchecked means that the items in each spawn will all be in one crate."));
				if(!diff.boolValue)
				EditorGUILayout.PropertyField(countCrates, new GUIContent("Count crates", "Enable so that the number of crates spawned is counted. Disable so that the total number of items is counted as some spawned crates may contain multiple of that item"));
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(specifySeed, new GUIContent("Specify seed", "Specify the seed number used. If disabled, will use a random seed."));
				if(specifySeed.boolValue)
				EditorGUILayout.PropertyField(seed, new GUIContent("Seed", "Set the seed number used"));
				EditorGUILayout.EndHorizontal();
				
				//make sure values are allowed
										if (min.intValue < 1)
												min.intValue = 1;
										else if (min.intValue > max.intValue)
												min.intValue = max.intValue;
										if (max.intValue < 1)
												max.intValue = 1;
				
										if (tot.intValue < 0)
												tot.intValue = 0;
								}//end if showing number
								EditorGUILayout.EndVertical ();
				#endregion
				#region shape
								if (GUITools.TitleGroup (new GUIContent ("Spawn", "Set the spawn options"), controllerSO.FindProperty ("sSp"), false)) {//if showing shape options
										EditorGUILayout.BeginHorizontal ();
										int selected = spawnerNormal.shapeOption;
										selected = EditorGUILayout.Popup (selected, shapes, "DropDownButton");
					
					//the info for the different options. Done here so that the name of the shape can be used
										GUIContent[] maxDistInfo = new GUIContent[] {
												new GUIContent ("Radius", string.Format ("The radius of the {0} to spawn in.", shapes [selected].ToLower ())),
												new GUIContent ("Side length", string.Format ("The length of a side of the {0} to spawn in.", shapes [selected].ToLower ()))
										};
					
										EditorGUILayout.PropertyField (maxDist, maxDistInfo [selected / 2]);//the info is selected / 2 as use each one twice for the 2d and 3d shape
										EditorGUILayout.EndHorizontal ();
										
										spawnerNormal.shapeOption = selected;
				
										if (maxDist.floatValue < 0)
												maxDist.floatValue = 0;
												
										EditorGUILayout.PropertyField(traderCollect, new GUIContent("Trader collect", "Select whether a trader is allowed to collect items created at this spawner"));
								}//end if showing shape
								EditorGUILayout.EndVertical ();
				#endregion
								break;
				#endregion
				
				#region items
				case 1:
				scrollPos.SG = GUITools.EnableDisableItems("Allow item spawn", "expandedT", scrollPos.SG, controllerSO, spawnerSO.FindProperty ("items"), smallScroll, controllerNormal);
				break;
				#endregion
						}//end switch
			
			if (smallScroll.boolValue)//if small scroll enabled, then end the scroll view
				EditorGUILayout.EndScrollView ();
			
						spawnerSO.ApplyModifiedProperties ();
						controllerSO.ApplyModifiedProperties ();
						controllerNormal.selected.S = sel;
						
						if(GUI.changed)
						EditorUtility.SetDirty(target);
				}//end OnInspectorGUI
				
		void OnSceneGUI()
		{
			Handles.color = Color.green;
			Handles.matrix = Matrix4x4.TRS (spawnerNormal.transform.position, spawnerNormal.transform.rotation, spawnerNormal.transform.lossyScale);
		
		if(spawnerNormal.shapeOption == 1)//if is a circle
		Handles.DrawWireDisc(Vector3.zero, Vector3.forward, maxDist.floatValue);
		}//end OnSceneGUI
		}//end SpawnerEditor
}//end namespace