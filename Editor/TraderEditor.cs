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
		if (trader.gameObject.activeInHierarchy) {
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
	}//end if in hierarchy

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
		
		#region factions
		EditorGUI.indentLevel = 0;
		EditorGUILayout.BeginVertical ("HelpBox");
		controller.showTF = EditorGUILayout.Foldout (controller.showTF, "Factions");
		if (controller.showTF) {
			EditorGUILayout.BeginHorizontal ();
			controller.showHoriz = EditorGUILayout.Toggle (new GUIContent ("Show items vertically", "This will show the items ascending vertically"), controller.showHoriz, "Radio");
			controller.showHoriz = !EditorGUILayout.Toggle (new GUIContent ("Show items horizontally", "This will show the items ascending h"), !controller.showHoriz, "Radio");
			EditorGUILayout.EndHorizontal ();
			
			EditorGUI.indentLevel = 0;
			if (controller.allowFactions && controller.factions.Count > 0) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Select factions", EditorStyles.boldLabel);
				if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft)) {
					Undo.RegisterUndo ((Trader)target, "Select all factions");
					for (int f = 0; f<trader.factions.Count; f++)
						trader.factions [f] = true;
				}
				if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
					Undo.RegisterUndo ((Trader)target, "Select no factions");
					for (int f = 0; f<trader.factions.Count; f++)
						trader.factions [f] = false;
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUI.indentLevel = 1;
					
				if (!controller.showHoriz) {
					for (int a = 0; a<trader.factions.Count; a = a+2) {
						EditorGUILayout.BeginHorizontal ();
						trader.factions [a] = EditorGUILayout.Toggle (controller.factions [a].name, trader.factions [a]);
						if (a < trader.factions.Count - 1)
							trader.factions [a + 1] = EditorGUILayout.Toggle (controller.factions [a + 1].name, trader.factions [a + 1]);
						EditorGUILayout.EndHorizontal ();
					}
				} else {
					int half = Mathf.CeilToInt (trader.factions.Count / 2f);
				
					for (int a = 0; a< half; a++) {
						EditorGUILayout.BeginHorizontal ();
						trader.factions [a] = EditorGUILayout.Toggle (controller.factions [a].name, trader.factions [a]);
						if (half + a < trader.factions.Count)	
							trader.factions [half + a] = EditorGUILayout.Toggle (controller.factions [half + a].name, trader.factions [half + a]);
						EditorGUILayout.EndHorizontal ();
					}
				}
					
			} else//else not enabled, show help box
				EditorGUILayout.HelpBox ("There are no available options because factions have not been enabled in the controller, or there are no possible factions", MessageType.Info);
		}
		EditorGUILayout.EndVertical ();
		#endregion
		if (!Application.isPlaying && trader.gameObject.activeInHierarchy && controller.allowFactions && !CheckFaction () && controller.factions.Count > 0) {
			EditorGUI.indentLevel = 0;
			EditorGUILayout.HelpBox ("The target post and trader are not in the same faction.\nMake sure they are in the same faction so that the trader can make trades.", MessageType.Error);
		}
	}
	
	bool CheckFaction ()
	{//checking that post is not of same faction
		for (int f = 0; f<trader.factions.Count; f++) {
			if (trader.factions [f] && trader.target.GetComponent<TradePost> ().factions [f])
				return true;
		}	
		return false;
	}
}