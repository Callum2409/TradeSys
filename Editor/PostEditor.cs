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
		EditorGUI.indentLevel = 0;
		controller.showAG = EditorGUILayout.Foldout (controller.showAG, "Settings");
		if (controller.showAG) {
			EditorGUI.indentLevel = 1;
			EditorGUILayout.BeginHorizontal ();
			controller.showHoriz = EditorGUILayout.Toggle (new GUIContent ("Show items vertically", "When enabling or disabling " +
			"items at trade posts, show ascending vertically"), controller.showHoriz);
			controller.showHoriz = !EditorGUILayout.Toggle (new GUIContent ("Show items horizontally", "When enabling or disabling " +
			"items at trade posts, show ascending horizontally"), !controller.showHoriz);
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Allow item at this post", EditorStyles.boldLabel);
			if (GUILayout.Button ("Select all", EditorStyles.miniButtonLeft)) {
				for (int s = 0; s<post.stock.Count; s++) {
					post.stock [s].allow = true;
				}
			}
			if (GUILayout.Button ("Select none", EditorStyles.miniButtonRight)) {
				for (int s = 0; s<post.stock.Count; s++) {
					post.stock [s].allow = false;
				}
			}
			EditorGUILayout.EndHorizontal ();
			
			if (!controller.showHoriz) {
				for (int a = 0; a<post.stock.Count-1; a = a+2) {
					EditorGUILayout.BeginHorizontal ();
					post.stock [a].allow = EditorGUILayout.Toggle (post.stock [a].name, post.stock [a].allow);
					post.stock [a + 1].allow = EditorGUILayout.Toggle (post.stock [a + 1].name, post.stock [a + 1].allow);
					EditorGUILayout.EndHorizontal ();
				}
				if (post.stock.Count % 2 == 1)
					post.stock [post.stock.Count - 1].allow = EditorGUILayout.Toggle (post.stock [post.stock.Count - 1].name, post.stock [post.stock.Count - 1].allow);
			} else {
				int half = Mathf.FloorToInt (post.stock.Count / 2);
				if (post.stock.Count % 2 == 1)
					half ++;
				
				for (int a = 0; a< half; a++) {
					EditorGUILayout.BeginHorizontal ();
					post.stock [a].allow = EditorGUILayout.Toggle (post.stock [a].name, post.stock [a].allow);
					if (half + a < post.stock.Count)	
						post.stock [half + a].allow = EditorGUILayout.Toggle (post.stock [half + a].name, post.stock [half + a].allow);
					EditorGUILayout.EndHorizontal ();
				}
			}
		}
		
		EditorGUI.indentLevel = 0;
		controller.showS = EditorGUILayout.Foldout (controller.showS, "Stock");
		if (controller.showS) {
			controller.showP = EditorGUILayout.Toggle ("Show prices", controller.showP);
			for (int s = 0; s<post.stock.Count; s++) {
				EditorGUI.indentLevel = 1;
				
				if (post.stock [s].allow) {
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (post.stock [s].name, EditorStyles.boldLabel);
				
					post.stock [s].number = EditorGUILayout.IntField ("Number", post.stock [s].number);
					EditorGUILayout.EndHorizontal ();
				
					if (controller.showP) {				
						EditorGUILayout.BeginHorizontal ();
						EditorGUI.indentLevel = 2;
						EditorGUILayout.LabelField ("Price", post.stock [s].price.ToString ());
						EditorGUILayout.EndHorizontal ();
					}				
					post.stock [s].number = (int)Mathf.Clamp (post.stock [s].number, 0, Mathf.Infinity);
				}
			}
		}
		
		EditorGUI.indentLevel = 0;
		controller.showM = EditorGUILayout.Foldout (controller.showM, "Manufacturing");
		if (controller.showM) {
			for (int m = 0; m<post.manufacture.Count; m++) {
				if (Check (controller.manufacturing [m].needing) && Check (controller.manufacturing [m].making)) {
					EditorGUI.indentLevel = 1;
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (controller.manufacturing [m].name, EditorStyles.boldLabel);
					post.manufacture [m].yesNo = EditorGUILayout.Toggle ("Enable manufacture", post.manufacture [m].yesNo);
					EditorGUILayout.EndHorizontal ();
				
					if (post.manufacture [m].yesNo) {
						EditorGUI.indentLevel = 2;
						EditorGUILayout.BeginHorizontal ();
						post.manufacture [m].seconds = EditorGUILayout.IntField ("Seconds to create", post.manufacture [m].seconds);
						EditorGUILayout.LabelField ("");
						EditorGUILayout.EndHorizontal ();
						post.manufacture [m].seconds = (int)Mathf.Clamp (post.manufacture [m].seconds, 1, Mathf.Infinity);
					}
				} else {
					post.manufacture [m].yesNo = false;
				}
			}
		}
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