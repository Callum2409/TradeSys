using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Trader))]
public class TraderEditor : Editor {
	
	Controller controller;
	Trader trader;
	GameObject[] posts;
	
	void Awake ()
	{
		controller = GameObject.Find("Controller").GetComponent<Controller>();
		trader = (Trader)target;
		posts = GameObject.FindGameObjectsWithTag ("Trade Post");
	}
	
	public override void OnInspectorGUI ()
	{
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginHorizontal ();
		trader.targetPost = (GameObject)EditorGUILayout.ObjectField ("Target post", trader.targetPost, typeof(GameObject), true);
		
		if (GUILayout.Button ("Find post", EditorStyles.miniButtonLeft)) {
			bool find = false;
			for (int p = 0; p< posts.Length; p++) {
				if (Vector3.Distance (posts [p].transform.position, trader.transform.position) < 1) {
					Undo.RegisterUndo ((Trader)target, "Find post");
					trader.targetPost = posts [p];
					trader.transform.position = trader.targetPost.transform.position;
					find = true;
					break;
				}
			}
			if (!find)
				Debug.LogWarning ("Could not find a trade post nearby.\nMake sure that the co-ordinates of the trader " +
					"have been set to a trade post, or select the trade post and press set location.");
		}
		if (trader.targetPost == null)
			GUI.enabled = false;
		else
			GUI.enabled = true;
		if (GUILayout.Button ("Set location", EditorStyles.miniButtonRight)) {
			Undo.RegisterUndo ((Trader)target, "Set location");
			trader.transform.position = trader.targetPost.transform.position;
		}
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal ();
		
		if (trader.targetPost == null)
			EditorGUILayout.HelpBox ("Please select a trade post or press find post.", MessageType.Warning);
		else if (trader.targetPost.tag != "Trade Post" || trader.targetPost.GetComponent<TradePost> () == null)
			EditorGUILayout.HelpBox ("The target post selected is not a trade post or has been " +
				"incorrectly set up.\nMake sure that the target post has the TradePost script attached and the " +
				"tag set to Trade Post.", MessageType.Error);
		
		EditorGUILayout.BeginHorizontal ();
		trader.stopTime = EditorGUILayout.FloatField ("Stop time", trader.stopTime);
		
		string unit = "";
		for (int u = 0; u<controller.units.Count; u++) {
			if (trader.cargoSpace >= controller.units [u].min && trader.cargoSpace < controller.units [u].max)
				unit = " (" + controller.units [u].suffix + ")";
		}
		trader.cargoSpace = EditorGUILayout.FloatField ("Cargo space" + unit, trader.cargoSpace);
		
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		trader.speedMultiplier = EditorGUILayout.FloatField ("Speed multiplier", trader.speedMultiplier);
		EditorGUILayout.LabelField ("");
		EditorGUILayout.EndHorizontal ();
		
		trader.stopTime = Mathf.Max (trader.stopTime, 0);
		trader.cargoSpace = Mathf.Max (trader.cargoSpace, 0.000001f);
		trader.speedMultiplier = Mathf.Max (trader.speedMultiplier, 0);
	}
}