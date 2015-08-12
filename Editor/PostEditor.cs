using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(TradePost))]
public class PostEditor : Editor {
	
	Controller controller;
	TradePost post;
	
	void Awake () {
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		post = (TradePost)target;
	}
	
	public override void OnInspectorGUI () {
		EditorGUI.indentLevel = 0;
		post.showS = EditorGUILayout.Foldout (post.showS, "Stock");
		if (post.showS) {
			for (int s = 0; s<post.stock.Count; s++) {
				EditorGUI.indentLevel = 1;
				EditorGUILayout.LabelField (post.stock [s].name, EditorStyles.boldLabel);
				
				EditorGUILayout.BeginHorizontal ();
				EditorGUI.indentLevel = 2;
				post.stock [s].number = EditorGUILayout.IntField ("Number", post.stock [s].number);
				EditorGUILayout.LabelField ("Price", post.stock [s].price.ToString ());
				EditorGUILayout.EndHorizontal ();
				
				post.stock [s].number = (int)Mathf.Clamp (post.stock [s].number, 0, Mathf.Infinity);
			}
		}
		
		EditorGUI.indentLevel = 0;
		post.showM = EditorGUILayout.Foldout (post.showM, "Manufacturing");
		if (post.showM) {
			for (int m = 0; m<post.manufacture.Count; m++) {
				EditorGUI.indentLevel = 1;
				EditorGUILayout.LabelField (controller.manufacturing [m].name, EditorStyles.boldLabel);
				
				EditorGUILayout.BeginHorizontal ();
				EditorGUI.indentLevel = 2;
				post.manufacture [m].yesNo = EditorGUILayout.Toggle ("Enable manufacture", post.manufacture [m].yesNo);
				
				GUI.enabled = post.manufacture [m].yesNo;
				post.manufacture [m].seconds = EditorGUILayout.IntField ("Seconds to create", post.manufacture [m].seconds);
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal ();
				post.manufacture [m].seconds = (int)Mathf.Clamp (post.manufacture [m].seconds, 1, Mathf.Infinity);
			}
		}
		
		if (GUI.changed) {
			EditorUtility.SetDirty (post);
		}
		Repaint ();
	}
}