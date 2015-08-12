#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define API
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TradeSys{//uses TradeSys namespace to prevent any conflicts

[CustomEditor(typeof(Trader))]
public class TraderEditor : Editor
{
	
	Controller controller;
	Trader trader;
	GameObject[] posts;
	
	void Awake ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		trader = (Trader)target;
		posts = GameObject.FindGameObjectsWithTag ("Trade Post");
	}
	
	public override void OnInspectorGUI ()
	{
		#if !API
		Undo.RecordObject(controller, "TradeSys Trader");
		#endif
	
		if (!controller.loadTraderPrefabs && !InHierarchy () && !controller.expendable) {
			EditorGUILayout.HelpBox ("Nothing here can be set because:\n - The trader is not in the hierarchy\n - Expendable traders is disabled\n - Loading trader prefabs is off", MessageType.Info);
		} else {
			if (!InHierarchy ())
				GUI.enabled = false;
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginHorizontal ();
			trader.target = (GameObject)EditorGUILayout.ObjectField (new GUIContent ("Target post", "This is the post that the trader starts at. This will change when playing to the post that the trader is going to."), trader.target, typeof(GameObject), true);
		
			if (GUILayout.Button (new GUIContent ("Find post", "This will find the post that the trader is next to. Needs to be within 1 unit."), EditorStyles.miniButtonLeft)) {
				#if API
				Undo.RegisterUndo ((Trader)target, "Find post");
				#endif
				
				bool find = false;
				for (int p = 0; p< posts.Length; p++) {
					if (Vector3.Distance (posts [p].transform.position, trader.transform.position) <= trader.closeDistance) {
						#if API
						Undo.RegisterUndo ((Trader)target, "Find post");
						#endif
						
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
				#if API
				Undo.RegisterUndo ((Trader)target, "Set location");
				#endif
				
				trader.transform.position = trader.target.transform.position;
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal ();
			
			if (InHierarchy ()) {//only show help box if in the hierarchy
				if (trader.target == null)
					EditorGUILayout.HelpBox ("Please select a trade post or press find post.", MessageType.Warning);
				else if ((trader.target.tag != "Trade Post" || trader.target.GetComponent<TradePost> () == null) && !Application.isPlaying)
					EditorGUILayout.HelpBox ("The target post selected is not a trade post or has been " +
				"incorrectly set up.\nMake sure that the target post has the TradePost script attached and the " +
				"tag set to Trade Post.", MessageType.Error);
			} else {//end if not in hierarchy
				//if in hierarchy, display message
				EditorGUILayout.HelpBox ("The trader is not in the hierarchy, so no target post can be set because it is " +
				"set by the controller when expendable traders have been enabled and the trader added.", MessageType.Info);
			}
			
			EditorGUILayout.BeginHorizontal ();
			trader.closeDistance = Mathf.Max (0, EditorGUILayout.FloatField (new GUIContent("Close distance", "If the trader is within this distance to the target, then it will trade or collect the item."), trader.closeDistance));
			EditorGUILayout.LabelField ("");
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			string unit = "";
			for (int u = 0; u<controller.units.Count; u++) {
				if (trader.cargoSpace >= controller.units [u].min && trader.cargoSpace < controller.units [u].max)
					unit = " (" + controller.units [u].suffix + ")";
			}
			trader.cargoSpace = EditorGUILayout.FloatField (new GUIContent ("Cargo space" + unit, "This is the amount of cargo space available."), trader.cargoSpace);
		
			if (controller.pauseOption == 1)
				trader.stopTime = EditorGUILayout.FloatField (new GUIContent ("Stop time", "This is the time that the trader will stop for before leaving a trade post"), trader.stopTime);
			else
				EditorGUILayout.LabelField ("");
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
			if (controller.allowFactions) {
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
							#if API
							Undo.RegisterUndo ((Trader)target, "Select all factions");
							#endif
							
							for (int f = 0; f<trader.factions.Count; f++)
								trader.factions [f] = true;
						}
						if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
							#if API
							Undo.RegisterUndo ((Trader)target, "Select no factions");
							#endif
							
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
						EditorGUILayout.HelpBox ("There are no available options because there are no possible factions", MessageType.Info);
				}
				EditorGUILayout.EndVertical ();
			}
		#endregion
			if (!Application.isPlaying && trader.gameObject.activeInHierarchy && controller.allowFactions && !CheckFaction () && controller.factions.Count > 0 && trader.target != null && trader.target.tag == "Trade Post") {
				EditorGUI.indentLevel = 0;
				EditorGUILayout.HelpBox ("The target post and trader are not in the same faction.\nMake sure they are in the same faction so that the trader can make trades.", MessageType.Error);
			}
		}
	}
	
	bool CheckFaction ()
	{//checking that post is not of same faction
		if (trader.target != null && trader.target.tag == "Trade Post") {
			for (int f = 0; f<trader.factions.Count; f++) {
				if (trader.factions [f] && trader.target.GetComponent<TradePost> ().factions [f])
					return true;
			}
		}
		return false;
	}
	
	bool InHierarchy ()
	{
		if (trader.gameObject.activeSelf) {//if active, then can easily say if in hierarchy
			return trader.gameObject.activeInHierarchy;
		} else {//in not active, then needs to be enabled first to find out if in hierarchy
			trader.gameObject.SetActive (true);
			bool returning = trader.gameObject.activeInHierarchy;
			trader.gameObject.SetActive (false);
			return returning;
		}
	}
}
}//end namespace