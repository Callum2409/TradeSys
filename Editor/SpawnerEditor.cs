#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define API
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys{//uses TradeSys namespace to prevent any conflicts

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
	Controller controller;
	Spawner spawner;
	
	void Awake ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		spawner = (Spawner)target;
		controller.allowPickup = true;
	}
	
	public override void OnInspectorGUI ()
	{
		#if !API
		Undo.RecordObject(controller, "TradeSys Spawner");
		#endif
		
		EditorGUILayout.BeginVertical ("HelpBox");
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal ();
		spawner.minTime = EditorGUILayout.FloatField (new GUIContent ("Min time", "This is the min time to wait before spawning a new item"), spawner.minTime);
		spawner.maxTime = EditorGUILayout.FloatField (new GUIContent ("Max time", "This is the max time to wait before spawning a new item"), spawner.maxTime);
		EditorGUILayout.EndHorizontal ();
		spawner.minTime = Mathf.Clamp (spawner.minTime, 0, spawner.maxTime);
		spawner.maxTime = Mathf.Max (spawner.minTime, spawner.maxTime);
		
		EditorGUILayout.BeginHorizontal ();
		spawner.maxNo = Mathf.Max (EditorGUILayout.IntField (new GUIContent ("Max total", "This is the maximum number of items that can be in the sphere at any time"), spawner.maxNo), 1);
		EditorGUILayout.LabelField ("");
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		spawner.option = EditorGUILayout.Popup ("Spawn area", spawner.option, spawner.options, "DropDownButton");
		
		string rd = "";
		if (spawner.option == 0 || spawner.option == 2)
			rd = " radius";
		else
			rd = " width";
		
		spawner.radius = EditorGUILayout.FloatField (new GUIContent (spawner.options [spawner.option] + rd, "Items will be spawned within the radius of this sphere"), spawner.radius);
		EditorGUILayout.EndHorizontal ();
		
		spawner.randomRotation = EditorGUILayout.Toggle (new GUIContent ("Random rotation", "If selected, an item will be spawned with a random rotation. If disabled, items will be spawned that lie flat along the axis."), spawner.randomRotation);
		if (!spawner.randomRotation)
			spawner.specifiedRotation = EditorGUILayout.Vector3Field ("Rotation", spawner.specifiedRotation);

		
		spawner.radius = Mathf.Max (0, spawner.radius);
		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical ("HelpBox");
		spawner.showAllow = EditorGUILayout.Foldout (spawner.showAllow, new GUIContent ("Allow spawn", "Select which items you want to be spawned at this spawner"));
		if (spawner.showAllow) {
			EditorGUI.indentLevel = 1;
			EditorGUILayout.BeginHorizontal ();
			controller.showHoriz = EditorGUILayout.Toggle (new GUIContent ("Show items vertically", "When enabling or disabling " +
			"items at trade posts and spawners, show ascending vertically"), controller.showHoriz, "Radio");
			controller.showHoriz = !EditorGUILayout.Toggle (new GUIContent ("Show items horizontally", "When enabling or disabling " +
			"items at trade posts and spawners, show ascending horizontally"), !controller.showHoriz, "Radio");
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Allow item to be spawned", EditorStyles.boldLabel);
			if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft)) {
				#if API
				Undo.RegisterUndo ((Spawner)target, "Select all items");
				#endif
				
				for (int s = 0; s<spawner.allowSpawn.Count; s++) {
					spawner.allowSpawn [s] = true;
				}
			}
			if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
				#if API
				Undo.RegisterUndo ((Spawner)target, "Select no items");
				#endif
				
				for (int s = 0; s<spawner.allowSpawn.Count; s++) {
					spawner.allowSpawn [s] = false;
				}
			}
			EditorGUILayout.EndHorizontal ();

			if (!controller.showHoriz) {
				for (int a = 0; a<spawner.allowSpawn.Count; a = a+2) {
					EditorGUILayout.BeginHorizontal ();
					spawner.allowSpawn [a] = EditorGUILayout.Toggle (controller.allNames [a], spawner.allowSpawn [a]);
					if (a < spawner.allowSpawn.Count - 1)
						spawner.allowSpawn [a + 1] = EditorGUILayout.Toggle (controller.allNames [a + 1], spawner.allowSpawn [a + 1]);
					EditorGUILayout.EndHorizontal ();
				}
			} else {
				int half = Mathf.CeilToInt (spawner.allowSpawn.Count / 2f);
				
				for (int a = 0; a< half; a++) {
					EditorGUILayout.BeginHorizontal ();
					spawner.allowSpawn [a] = EditorGUILayout.Toggle (controller.allNames [a], spawner.allowSpawn [a]);
					if (half + a < spawner.allowSpawn.Count)	
						spawner.allowSpawn [half + a] = EditorGUILayout.Toggle (controller.allNames [half + a], spawner.allowSpawn [half + a]);
					EditorGUILayout.EndHorizontal ();
				}
			}
		}
		EditorGUILayout.EndVertical ();
		
		if (GUI.changed)
			EditorUtility.SetDirty (spawner);
	}
	
	void OnSceneGUI ()
	{
		if (spawner.option == 2) {
			Handles.color = Color.green;
			Handles.DrawWireDisc (spawner.transform.position, spawner.transform.up, spawner.radius);
		}
		if (spawner.option == 0 || spawner.option == 2) {
			if (!spawner.randomRotation) {
				Handles.color = new Color (1, 0, 0, .3f);
				Handles.matrix = Matrix4x4.TRS (spawner.transform.position, spawner.transform.rotation * Quaternion.Euler (spawner.specifiedRotation), spawner.transform.lossyScale);
				Handles.DrawSolidDisc (Vector3.zero, Vector3.up, spawner.radius/2);
				Handles.color = Color.red;
				Handles.DrawWireDisc (Vector3.zero, Vector3.up, spawner.radius / 2);
			}
		}
	}
}
}//end namespace