using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor {
	
	Controller controller;
	GameObject[] posts;
	
	void Awake () {
		posts = GameObject.FindGameObjectsWithTag ("Trade Post");
		controller = (Controller)target;
		
		while (controller.showSmallG.Count != controller.goods.Count) {
			if (controller.showSmallG.Count > controller.goods.Count) {
				controller.showSmallG.RemoveAt (controller.showSmallG.Count - 1);
			} else {
				controller.showSmallG.Add (false);
			}
		}
		while (controller.showSmallM.Count != controller.manufacturing.Count) {
			if (controller.showSmallM.Count > controller.manufacturing.Count) 
				controller.showSmallM.RemoveAt (controller.showSmallM.Count - 1);
			else {
				controller.showSmallM.Add (false);
			}
		}
	}
	
	public override void OnInspectorGUI () {
//		DrawDefaultInspector ();
		#region show goods
		EditorGUI.indentLevel = 0;
		controller.showAllG = EditorGUILayout.Foldout (controller.showAllG, "Goods");
		if (controller.showAllG) {
			EditorGUILayout.LabelField ("Number of types", controller.goods.Count.ToString ());
			for (int g = 0; g < controller.goods.Count; g++) {
				EditorGUI.indentLevel = 1;
				string name = controller.goods [g].name;
				if (name == "")
					name = "Name";
				controller.showSmallG [g] = EditorGUILayout.Foldout (controller.showSmallG [g], name);
	
				if (controller.showSmallG [g]) {
					EditorGUI.indentLevel = 3;
					
					EditorGUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Add before", EditorStyles.miniButtonLeft)) {
						Undo.RegisterUndo ((Controller)target, "Add new type before " + controller.allNames [g]);
						controller.goods.Insert (g, new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 0});
						controller.showSmallG.Insert (g, false);
						controller.allNames.Insert (g, "Name");	
						ChangeFromPoint (g - 1, true);
					}
					if (GUILayout.Button ("Remove", EditorStyles.miniButtonMid)) {
						Undo.RegisterUndo ((Controller)target, "Remove " + controller.allNames [g]);
						controller.goods.RemoveAt (g);
						controller.showSmallG.RemoveAt (g);
						controller.allNames.RemoveAt (g);
						ChangeFromPoint (g - 1, false);
						CheckManufacturing ();
						break;
					}
					if (GUILayout.Button ("Add after", EditorStyles.miniButtonRight)) {
						Undo.RegisterUndo ((Controller)target, "Add new type after " + controller.allNames [g]);
						controller.goods.Insert (g + 1, new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 0});
						controller.showSmallG.Insert (g + 1, false);
						controller.allNames.Insert (g + 1, "Name");
						ChangeFromPoint (g, true);
					}
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					name = EditorGUILayout.TextField ("Name", name);
					controller.goods [g].mass = EditorGUILayout.FloatField ("Mass", controller.goods [g].mass);
					controller.goods [g].name = name;
					EditorGUILayout.EndHorizontal ();
					
					EditorGUI.indentLevel = 2;
					EditorGUILayout.LabelField ("Prices", EditorStyles.boldLabel);
					
					EditorGUI.indentLevel = 3;
					EditorGUILayout.BeginHorizontal ();
					controller.goods [g].basePrice = EditorGUILayout.IntField ("Base", controller.goods [g].basePrice);
					EditorGUILayout.LabelField ("", "");
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					controller.goods [g].minPrice = EditorGUILayout.IntField ("Min", controller.goods [g].minPrice);
					controller.goods [g].maxPrice = EditorGUILayout.IntField ("Max", controller.goods [g].maxPrice);		
					EditorGUILayout.EndHorizontal ();
					
					controller.goods [g].mass = Mathf.Clamp (controller.goods [g].mass, 0, Mathf.Infinity);
					controller.goods [g].basePrice = (int)Mathf.Clamp (controller.goods [g].basePrice, 0, Mathf.Infinity);
					controller.goods [g].minPrice = (int)Mathf.Clamp (controller.goods [g].minPrice, 0, controller.goods [g].basePrice);
					controller.goods [g].maxPrice = (int)Mathf.Clamp (controller.goods [g].maxPrice, controller.goods [g].basePrice, Mathf.Infinity);
					
				}
				
			}//end for all
			if (controller.goods.Count == 0) {
				if (GUILayout.Button ("Add")) {
					controller.showSmallG.Add (true);
					controller.goods.Add (new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 0});
					controller.allNames.Add ("Name");
				}
			}
				
		}//end if showing goods
		#endregion
		#region show manufacturing
		EditorGUI.indentLevel = 0;
		controller.showAllM = EditorGUILayout.Foldout (controller.showAllM, "Manufacturing");
		if (controller.showAllM) {
			if (GUILayout.Button ("Add")) {
				Undo.RegisterUndo ((Controller)target, "Add new manufacturing process");
				controller.showSmallM.Add (false);
				if (controller.manufacturing.Count == 0)
					controller.showSmallM [0] = true;
				controller.manufacturing.Add (new Items{name = "", needing = new List<NeedMake> (), making = new List<NeedMake> ()});
			}
			
			for (int m = 0; m < controller.manufacturing.Count; m++) {
				EditorGUI.indentLevel = 1;
				string name = controller.manufacturing [m].name;
				if (name == "")
					name = "Element " + m;
				controller.showSmallM [m] = EditorGUILayout.Foldout (controller.showSmallM [m], name);
				if (controller.showSmallM [m]) {
					EditorGUI.indentLevel = 3;
					
					EditorGUILayout.BeginHorizontal ();
					name = EditorGUILayout.TextField ("Name", name);
					controller.manufacturing [m].name = name;
					
					if (GUILayout.Button ("Remove")) {
						Undo.RegisterUndo ((Controller)target, "Remove manufacturing process " + controller.manufacturing [m].name);
						controller.manufacturing.RemoveAt (m);
						break;
					}
					EditorGUILayout.EndHorizontal ();
					
					controller.manufacturing [m].name = name;
					
					EditorGUI.indentLevel = 2;
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Needing", EditorStyles.boldLabel);
					if (GUILayout.Button ("Add")) {
						Undo.RegisterUndo ((Controller)target, "Add needing element to " + controller.manufacturing [m].name);
						controller.manufacturing [m].needing.Add (new NeedMake{});
					}
					EditorGUILayout.EndHorizontal ();
					
					for (int n = 0; n<controller.manufacturing[m].needing.Count; n++) {
						EditorGUI.indentLevel = 3;
						EditorGUILayout.BeginHorizontal ();
						controller.manufacturing [m].needing [n].item = EditorGUILayout.Popup ("Type", controller.manufacturing [m].needing [n].item, controller.allNames.ToArray ());
						controller.manufacturing [m].needing [n].number = EditorGUILayout.IntField ("Quantity", controller.manufacturing [m].needing [n].number);
						if (GUILayout.Button ("X", EditorStyles.miniButton, GUILayout.MaxWidth (20f))) {
							Undo.RegisterUndo ((Controller)target, "Remove needing " + 
								controller.allNames [controller.manufacturing [m].needing [n].item] + " from " + controller.manufacturing [m].name);
							controller.manufacturing [m].needing.RemoveAt (n);
							break;
						}
						EditorGUILayout.EndHorizontal ();
						controller.manufacturing [m].needing [n].number = (int)Mathf.Clamp (controller.manufacturing [m].needing [n].number, 0, Mathf.Infinity);
					}
					
					EditorGUI.indentLevel = 2;
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Making", EditorStyles.boldLabel);
					if (GUILayout.Button ("Add")) {
						Undo.RegisterUndo ((Controller)target, "Add making element to " + controller.manufacturing [m].name);
						controller.manufacturing [m].making.Add (new NeedMake{});
					}
					EditorGUILayout.EndHorizontal ();
					
					for (int n = 0; n<controller.manufacturing[m].making.Count; n++) {
						EditorGUI.indentLevel = 3;
						EditorGUILayout.BeginHorizontal ();
						controller.manufacturing [m].making [n].item = EditorGUILayout.Popup ("Type", controller.manufacturing [m].making [n].item, controller.allNames.ToArray ());
						controller.manufacturing [m].making [n].number = EditorGUILayout.IntField ("Quantity", controller.manufacturing [m].making [n].number);
						if (GUILayout.Button ("X", EditorStyles.miniButton, GUILayout.MaxWidth (20f))) {
							Undo.RegisterUndo ((Controller)target, "Remove making " + 
								controller.allNames [controller.manufacturing [m].making [n].item] + " from " + controller.manufacturing [m].name);
							controller.manufacturing [m].making.RemoveAt (n);
							break;
						}
						EditorGUILayout.EndHorizontal ();
						controller.manufacturing [m].making [n].number = (int)Mathf.Clamp (controller.manufacturing [m].making [n].number, 0, Mathf.Infinity);
					}
					
				}
			}//end for all manufacturing
		}//end if showing manufacturing
		#endregion
		if (GUI.changed) {//get changes so can update other scripts
			for (int p = 0; p<posts.Length; p++) {
				TradePost post = posts [p].GetComponent<TradePost> ();
					
				while (post.stock.Count != controller.goods.Count) {
					if (post.stock.Count > controller.goods.Count)
						post.stock.RemoveAt (post.stock.Count - 1);
					else
						post.stock.Add (new Stock{});
				}
				
				while (post.manufacture.Count != controller.manufacturing.Count) {
					if (post.manufacture.Count > controller.manufacturing.Count)
						post.manufacture.RemoveAt (post.manufacture.Count - 1);
					else
						post.manufacture.Add (new Mnfctr{yesNo = false, seconds = 1});
				}
				
				for (int x = 0; x<controller.goods.Count; x++) {
					post.stock [x].name = controller.goods [x].name;
					controller.allNames [x] = controller.goods [x].name;
				}
			}//end for posts
		}//end if GUI changed
		this.Repaint ();	
	}

	void ChangeFromPoint (int point, bool increase) {
		int increment = 1;
		if (!increase)
			increment = -1;
		for (int m = 0; m < controller.manufacturing.Count; m++) {
			for (int c = 0; c<controller.manufacturing[m].needing.Count; c++)
				if (controller.manufacturing [m].needing [c].item > point)
					controller.manufacturing [m].needing [c].item += increment;
			for (int c = 0; c<controller.manufacturing[m].making.Count; c++)
				if (controller.manufacturing [m].making [c].item > point)
					controller.manufacturing [m].making [c].item += increment;
		}
		
		for (int p = 0; p < posts.Length; p++) {
			if (increase)
				posts [p].GetComponent<TradePost> ().stock.Insert (point + 1, new Stock{name = "Name"});
			else
				posts [p].GetComponent<TradePost> ().stock.RemoveAt (point + 1);
		}
	}
	
	void CheckManufacturing () {
		for (int m = 0; m<controller.manufacturing.Count; m++) {
			for (int i = 0; i<controller.manufacturing[m].needing.Count; i++) {
				if (controller.manufacturing [m].needing [i].item == -1) {
					Debug.LogError ("Some items in manufacturing " + controller.manufacturing [m].name + " are undefined" +
						"\nPlease ensure that all types have been selected");
					break;
				}
			}
		}
	}
}