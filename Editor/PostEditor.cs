using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(TradePost))]
public class PostEditor : Editor
{
	Controller controller;
	TradePost post;
	GameObject[] posts;
	TradePost[] postScripts;
	
	void Awake ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		post = (TradePost)target;
		posts = GameObject.FindGameObjectsWithTag ("Trade Post");
		postScripts = new TradePost[posts.Length];
		for (int p = 0; p<posts.Length; p++)
			postScripts[p] = posts[p].GetComponent<TradePost>();
	}
	
	public override void OnInspectorGUI ()
	{
		EditorGUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		controller.selP = GUILayout.Toolbar (controller.selP, new string[]{"Settings", "Stock", "Manufacturing"});
		GUILayout.FlexibleSpace ();
		EditorGUILayout.EndHorizontal ();
		
		#region settings
		if (controller.selP == 0) {
			EditorGUI.indentLevel = 0;
			
			if (controller.showR) 
				controller.showRP = EditorGUILayout.Toggle ("Show trade routes", controller.showRP);
			
			EditorGUILayout.BeginHorizontal ();
			controller.showHoriz = EditorGUILayout.Toggle (new GUIContent ("Show items vertically", "When enabling or disabling " +
			"items at trade posts and spawners, show ascending vertically"), controller.showHoriz, "Radio");
			controller.showHoriz = !EditorGUILayout.Toggle (new GUIContent ("Show items horizontally", "When enabling or disabling " +
			"items at trade posts and spawners, show ascending horizontally"), !controller.showHoriz, "Radio");
			EditorGUILayout.EndHorizontal ();
			#region groups
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginVertical ("HelpBox");
			controller.showPG = EditorGUILayout.Foldout (controller.showPG, "Groups");
			if (controller.showPG) {
				EditorGUI.indentLevel = 0;
				if (controller.allowGroups && controller.groups.Count > 0) {
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Allow trade with group", EditorStyles.boldLabel);
					if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft)) {
						Undo.RegisterUndo ((TradePost)target, "Select all groups");
						for (int g = 0; g<post.groups.Count; g++)
							post.groups [g] = true;
					}
					if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
						Undo.RegisterUndo ((TradePost)target, "Select no groups");
						for (int g = 0; g<post.groups.Count; g++)
							post.groups [g] = false;
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUI.indentLevel = 1;
					
					if (!controller.showHoriz) {
						for (int a = 0; a<post.groups.Count; a = a+2) {
							EditorGUILayout.BeginHorizontal ();
							post.groups [a] = EditorGUILayout.Toggle (controller.groups [a], post.groups [a]);
							if (a < post.groups.Count - 1)
								post.groups [a + 1] = EditorGUILayout.Toggle (controller.groups [a + 1], post.groups [a + 1]);
							EditorGUILayout.EndHorizontal ();
						}
					} else {
						int half = Mathf.CeilToInt (post.groups.Count / 2f);
				
						for (int a = 0; a< half; a++) {
							EditorGUILayout.BeginHorizontal ();
							post.groups [a] = EditorGUILayout.Toggle (controller.groups [a], post.groups [a]);
							if (half + a < post.groups.Count)	
								post.groups [half + a] = EditorGUILayout.Toggle (controller.groups [half + a], post.groups [half + a]);
							EditorGUILayout.EndHorizontal ();
						}
					}
					
				} else//else not enabled, show help box
					EditorGUILayout.HelpBox ("There are no available options because grouping has not been enabled in the controller, or there are no possible groups", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();
			#endregion
			#region factions
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginVertical ("HelpBox");
			controller.showPF = EditorGUILayout.Foldout (controller.showPF, "Factions");
			if (controller.showPF) {
				EditorGUI.indentLevel = 0;
				if (controller.allowFactions && controller.factions.Count > 0) {
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Select factions", EditorStyles.boldLabel);
					if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft)) {
						Undo.RegisterUndo ((TradePost)target, "Select all factions");
						for (int f = 0; f<post.factions.Count; f++)
							post.factions [f] = true;
					}
					if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
						Undo.RegisterUndo ((TradePost)target, "Select no factions");
						for (int f = 0; f<post.factions.Count; f++)
							post.factions [f] = false;
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUI.indentLevel = 1;
					
					if (!controller.showHoriz) {
						for (int a = 0; a<post.factions.Count; a = a+2) {
							EditorGUILayout.BeginHorizontal ();
							post.factions [a] = EditorGUILayout.Toggle (controller.factions [a].name, post.factions [a]);
							if (a < post.factions.Count - 1)
								post.factions [a + 1] = EditorGUILayout.Toggle (controller.factions [a + 1].name, post.factions [a + 1]);
							EditorGUILayout.EndHorizontal ();
						}
					} else {
						int half = Mathf.CeilToInt (post.factions.Count / 2f);
				
						for (int a = 0; a< half; a++) {
							EditorGUILayout.BeginHorizontal ();
							post.factions [a] = EditorGUILayout.Toggle (controller.factions [a].name, post.factions [a]);
							if (half + a < post.factions.Count)	
								post.factions [half + a] = EditorGUILayout.Toggle (controller.factions [half + a].name, post.factions [half + a]);
							EditorGUILayout.EndHorizontal ();
						}
					}
					
				} else//else not enabled, show help box
					EditorGUILayout.HelpBox ("There are no available options because factions have not been enabled in the controller, or there are no possible factions", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();
			#endregion
			#region enable items
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginVertical ("HelpBox");
			controller.showPE = EditorGUILayout.Foldout (controller.showPE, "Enable items");
			if (controller.showPE) {				
				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Allow item at this post", EditorStyles.boldLabel);
				if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft)) {
					Undo.RegisterUndo ((TradePost)target, "Select all items");
					for (int s = 0; s<post.stock.Count; s++)
						post.stock [s].allow = true;
				}
				if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
					Undo.RegisterUndo ((TradePost)target, "Select no items");
					for (int s = 0; s<post.stock.Count; s++) 
						post.stock [s].allow = false;
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUI.indentLevel = 1;
				if (!controller.showHoriz) {
					for (int a = 0; a<post.stock.Count; a = a+2) {
						EditorGUILayout.BeginHorizontal ();
						post.stock [a].allow = EditorGUILayout.Toggle (post.stock [a].name, post.stock [a].allow);
						if (a < post.stock.Count - 1)
							post.stock [a + 1].allow = EditorGUILayout.Toggle (post.stock [a + 1].name, post.stock [a + 1].allow);
						EditorGUILayout.EndHorizontal ();
					}
				} else {
					int half = Mathf.CeilToInt (post.stock.Count / 2f);
				
					for (int a = 0; a< half; a++) {
						EditorGUILayout.BeginHorizontal ();
						post.stock [a].allow = EditorGUILayout.Toggle (post.stock [a].name, post.stock [a].allow);
						if (half + a < post.stock.Count)	
							post.stock [half + a].allow = EditorGUILayout.Toggle (post.stock [half + a].name, post.stock [half + a].allow);
						EditorGUILayout.EndHorizontal ();
					}
				}
			}
			EditorGUILayout.EndVertical ();
			#endregion
		}
		#endregion
		#region stock
		if (controller.selP == 1) {
			controller.showP = EditorGUILayout.Toggle ("Show prices", controller.showP);
			for (int s = 0; s<post.stock.Count; s++) {
				EditorGUI.indentLevel = 0;
				
				if (post.stock [s].allow) {
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (post.stock [s].name, EditorStyles.boldLabel);
				
					post.stock [s].number = EditorGUILayout.IntField ("Number", post.stock [s].number);
					EditorGUILayout.EndHorizontal ();
				
					if (controller.showP) {
						EditorGUI.indentLevel = 1;
						EditorGUILayout.LabelField ("Price", post.stock [s].price.ToString ());
					}				
					post.stock [s].number = (int)Mathf.Clamp (post.stock [s].number, 0, Mathf.Infinity);
				}
			}
		}
		#endregion
		#region manufacturing
		if (controller.selP == 2) {
			for (int m = 0; m<post.manufacture.Count; m++) {
				if (Check (controller.manufacturing [m].needing) && Check (controller.manufacturing [m].making)) {
					EditorGUI.indentLevel = 0;
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (controller.manufacturing [m].name, EditorStyles.boldLabel);
					post.manufacture [m].allow = EditorGUILayout.Toggle ("Enable manufacture", post.manufacture [m].allow);
					EditorGUILayout.EndHorizontal ();
				
					if (post.manufacture [m].allow) {
						EditorGUI.indentLevel = 1;
						EditorGUILayout.BeginHorizontal ();
						post.manufacture [m].seconds = EditorGUILayout.IntField ("Seconds to create", post.manufacture [m].seconds);
						EditorGUILayout.LabelField ("");
						EditorGUILayout.EndHorizontal ();
						post.manufacture [m].seconds = (int)Mathf.Clamp (post.manufacture [m].seconds, 1, Mathf.Infinity);
					}
				} else {
					post.manufacture [m].allow = false;
				}
			}
		}
		#endregion
		if (GUI.changed)
			EditorUtility.SetDirty(post);
	}
	
	bool Check (List<NeedMake> mnfctr)
	{
		for (int i = 0; i<mnfctr.Count; i++) {
			if (!post.stock [mnfctr [i].item].allow)
				return false;
		}
		return true;
	}
	
	void OnSceneGUI ()
	{
		if (controller.showR && controller.showRP) {
			for (int p1 = 0; p1<postScripts.Length; p1++) {
				for (int p2 = p1 +1; p2<postScripts.Length; p2++) {
					if (controller.CheckGroupsFactions (postScripts [p1], postScripts [p2])) {
						if (!controller.allowFactions) {
							Handles.color = Color.green;
							Handles.DrawLine (posts [p1].transform.position, posts [p2].transform.position);
						} else {
							List<Color> colors = new List<Color> ();
							for (int f = 0; f<controller.factions.Count; f++) {
								if (postScripts [p1].factions [f] && postScripts [p2].factions [f])
									colors.Add (controller.factions [f].color);
							}
							for (int c = 0; c<colors.Count; c++) {
								Handles.color = colors [c];
								//set the line color and split the distance up so that can be multi colored
								Handles.DrawLine (((posts [p2].transform.position - posts [p1].transform.position) / colors.Count) * c + posts [p1].transform.position,
									((posts [p2].transform.position - posts [p1].transform.position) / colors.Count) * (c + 1) + posts [p1].transform.position);
							}//end for colors
						}//end else factions enabled
					}//end check
				}//end 2nd post
			}//end 1st post
		}//end show routes
	}
}