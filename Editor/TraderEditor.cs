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
		trader.target = (GameObject)EditorGUILayout.ObjectField (new GUIContent ("Target post", "This is the post that the trader starts at. This will change when playing to the post that the trader is going to."), trader.target, typeof(GameObject), true);
		
		if (GUILayout.Button (new GUIContent ("Find post", "This will find the post that the trader is next to. Needs to be within 1 unit."), EditorStyles.miniButtonLeft)) {
			Undo.RegisterUndo ((Trader)target, "Find post");
			bool find = false;
			for (int p = 0; p< posts.Length; p++) {
				if (Vector3.Distance (posts [p].transform.position, trader.transform.position) < 3) {
					Undo.RegisterUndo ((Trader)target, "Find post");
					trader.target = posts [p];
					trader.transform.position = trader.target.transform.position;
					find = true;
					break;
				}
			}
			if (!find)
				Debug.LogWarning ("Could not find a trade post close enough.\nMake sure that the co-ordinates of the trader " +
					"have been set to a trade post, or select the trade post and press set location.");
		}
		if (trader.target == null)
			GUI.enabled = false;
		else
			GUI.enabled = true;
		if (GUILayout.Button (new GUIContent ("Set location", "This will set the location of the trader to be at the location of the selected target post"), EditorStyles.miniButtonRight)) {
			Undo.RegisterUndo ((Trader)target, "Set location");
			trader.transform.position = trader.target.transform.position;
		}
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal ();
		
		if (trader.target == null)
			EditorGUILayout.HelpBox ("Please select a trade post or press find post.", MessageType.Warning);
		else if ((trader.target.tag != "Trade Post" || trader.target.GetComponent<TradePost> () == null) && !Application.isPlaying)
			EditorGUILayout.HelpBox ("The target post selected is not a trade post or has been " +
				"incorrectly set up.\nMake sure that the target post has the TradePost script attached and the " +
				"tag set to Trade Post.", MessageType.Error);
		
		EditorGUILayout.BeginHorizontal ();
		trader.stopTime = EditorGUILayout.FloatField (new GUIContent ("Stop time", "This is the time that the trader will stop for before leaving a trade post"), trader.stopTime);
		
		string unit = "";
		for (int u = 0; u<controller.units.Count; u++) {
			if (trader.cargoSpace >= controller.units [u].min && trader.cargoSpace < controller.units [u].max)
				unit = " (" + controller.units [u].suffix + ")";
		}
		trader.cargoSpace = EditorGUILayout.FloatField (new GUIContent ("Cargo space" + unit, "This is the amount of cargo space available."), trader.cargoSpace);
		
		EditorGUILayout.EndHorizontal ();
		
		if (controller.allowPickup) {
			bool before = trader.allowTraderPickup;
			trader.allowTraderPickup = EditorGUILayout.Toggle (new GUIContent ("Allow trader collection", "Allow current trader to collect items from spawners or dropped items"), trader.allowTraderPickup);
			if (before != trader.allowTraderPickup)
				trader.transform.Translate (Vector3.zero);
			if (trader.allowTraderPickup) {
				EditorGUILayout.BeginHorizontal ();
				trader.radarDistance = EditorGUILayout.FloatField (new GUIContent ("Radar distance", "This is how far the radar of " +
			"the trader reaches. It is used to so that if the traders can collect items, and there is an item within the radar, " +
			"the trader will go and pick it up."), trader.radarDistance);
				trader.droneTime = EditorGUILayout.FloatField (new GUIContent ("Drone stop time", "This is the amount of time that the trader has to stop for if it is picking up an item"), trader.droneTime);
				EditorGUILayout.EndHorizontal ();
			}
		}
		
		trader.stopTime = Mathf.Max (trader.stopTime, 0);
		trader.cargoSpace = Mathf.Max (trader.cargoSpace, 0.000001f);
		trader.radarDistance = Mathf.Max (trader.radarDistance, 0);
		trader.droneTime = Mathf.Max (trader.droneTime, 0);
	}
}