using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

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
			EditorGUILayout.LabelField ("Allow item at this post", EditorStyles.boldLabel);
			for (int a = 0; a<post.allowGoods.Count-1; a = a+2) {
				EditorGUILayout.BeginHorizontal ();
				post.allowGoods [a] = EditorGUILayout.Toggle (post.stock [a].name, post.allowGoods [a]);
				post.allowGoods [a + 1] = EditorGUILayout.Toggle (post.stock [a + 1].name, post.allowGoods [a + 1]);
				EditorGUILayout.EndHorizontal ();
			}
			if (post.allowGoods.Count % 2 == 1)
				post.allowGoods [post.allowGoods.Count - 1] = EditorGUILayout.Toggle (post.stock [post.allowGoods.Count - 1].name, post.allowGoods [post.allowGoods.Count - 1]);
		}
		
		EditorGUI.indentLevel = 0;
		controller.showS = EditorGUILayout.Foldout (controller.showS, "Stock");
		if (controller.showS) {
			controller.showP = EditorGUILayout.Toggle ("Show prices", controller.showP);
			for (int s = 0; s<post.stock.Count; s++) {
				EditorGUI.indentLevel = 1;
				
				if (post.allowGoods [s]) {
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
			if (!post.allowGoods [mnfctr [i].item])
				return false;
		}
		return true;
	}
}