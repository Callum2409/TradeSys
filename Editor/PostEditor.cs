using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(TradePost)), CanEditMultipleObjects]
public class PostEditor : Editor
{
	Controller controller;
	TradePost post;
	
	void Awake ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		post = (TradePost)target;
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
			EditorGUILayout.BeginVertical ("HelpBox");
			controller.showPG = EditorGUILayout.Foldout (controller.showPG, "Groups");
			if (controller.showPG) {
				EditorGUI.indentLevel = 1;
				if (controller.allowGroups && controller.groups.Count > 0 && post.groups.Count > 0) {
					post.groups [0].allow = true;
					for (int g = 0; g< post.groups.Count; g++) {
						if (g == 0 || post.groups [g - 1].allow) {
							EditorGUILayout.BeginHorizontal ();
							post.groups [g].allow = EditorGUILayout.Toggle ("Enable level " + (g + 1) + " groups", post.groups [g].allow);
							if (post.groups [g].allow) {
								post.groups [g].selection = EditorGUILayout.Popup ("Group " + (g + 1), post.groups [g].selection, controller.groups.ToArray (), "DropDownButton");
								if (g == post.groups.Count - 1) {
									post.groups.Add (new Group{allow = false, selection = 0});
								}
							}
							EditorGUILayout.EndHorizontal ();
						} else {
							post.groups.RemoveAt (g);
							g--;
						}
					}
				} else//else not enabled, show help box
					EditorGUILayout.HelpBox ("There are no available options because grouping has not been enabled in the controller, or there are no possible groups", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginVertical ("HelpBox");
			controller.showPE = EditorGUILayout.Foldout (controller.showPE, "Enable items");
			if (controller.showPE) {
				EditorGUI.indentLevel = 1;
				EditorGUILayout.BeginHorizontal ();
				controller.showHoriz = EditorGUILayout.Toggle (new GUIContent ("Show items vertically", "When enabling or disabling " +
			"items at trade posts and spawners, show ascending vertically"), controller.showHoriz, "Radio");
				controller.showHoriz = !EditorGUILayout.Toggle (new GUIContent ("Show items horizontally", "When enabling or disabling " +
			"items at trade posts and spawners, show ascending horizontally"), !controller.showHoriz, "Radio");
				EditorGUILayout.EndHorizontal ();
				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Allow item at this post", EditorStyles.boldLabel);
				if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft)) {
					Undo.RegisterUndo ((TradePost)target, "Select all items");
					for (int s = 0; s<post.stock.Count; s++) {
						post.stock [s].allow = true;
					}
				}
				if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
					Undo.RegisterUndo ((TradePost)target, "Select no items");
					for (int s = 0; s<post.stock.Count; s++) {
						post.stock [s].allow = false;
					}
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
	}
	
	bool Check (List<NeedMake> mnfctr)
	{
		for (int i = 0; i<mnfctr.Count; i++) {
			if (!post.stock [mnfctr [i].item].allow)
				return false;
		}
		return true;
	}
}