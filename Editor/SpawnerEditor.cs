using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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
		EditorGUILayout.LabelField ("", "");
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		spawner.sphereRadius = EditorGUILayout.FloatField (new GUIContent ("Sphere Radius", "Items will be spawned within the radius of this sphere"), spawner.sphereRadius);
		EditorGUILayout.LabelField ("", "");
		EditorGUILayout.EndHorizontal ();
		spawner.sphereRadius = Mathf.Max (0, spawner.sphereRadius);
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
				Undo.RegisterUndo ((Spawner)target, "Select all items");
				for (int s = 0; s<spawner.allowSpawn.Count; s++) {
					spawner.allowSpawn [s] = true;
				}
			}
			if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
				Undo.RegisterUndo ((Spawner)target, "Select no items");
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
	}
}