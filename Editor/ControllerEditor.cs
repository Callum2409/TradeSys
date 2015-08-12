using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{
	Controller controller;
	GameObject[] posts;
	
	void Awake ()
	{
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
		for (int p = 0; p<posts.Length; p++) {
			TradePost post = posts[p].GetComponent<TradePost>();
			while (post.stock.Count != controller.goods.Count) {
				if (post.stock.Count > controller.goods.Count) {
					post.stock.RemoveAt (post.stock.Count - 1);
				} else {
					post.stock.Add (new Stock{name = "Name", allow = true});
				}
			}
		}
	}
	
	public override void OnInspectorGUI ()
	{
		#region settings
		controller.settings = EditorGUILayout.Foldout (controller.settings, "Settings");		
		EditorGUI.indentLevel = 1;
		
		if (controller.settings) {		
		#region expendable traders
			controller.expendable = EditorGUILayout.Toggle ("Expendable traders", controller.expendable);
			
			if (controller.expendable) {
				EditorGUI.indentLevel = 2;
				EditorGUILayout.BeginHorizontal ();
				controller.pauseBeforeStart = EditorGUILayout.Toggle ("Pause before leave", controller.pauseBeforeStart);				
				controller.maxNoTraders = EditorGUILayout.IntField ("Max no. traders", controller.maxNoTraders);
				controller.maxNoTraders = (int)Mathf.Clamp (controller.maxNoTraders, 1, Mathf.Infinity);
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Trader types: " + controller.expendableT.Count);
				if (GUILayout.Button ("Add", EditorStyles.miniButton)) {
					Undo.RegisterUndo ((Controller)target, "Add new trader type");
					controller.expendableT.Add (null);
					controller.showE = true;
				}
				EditorGUILayout.EndHorizontal ();

				controller.showE = EditorGUILayout.Foldout (controller.showE, "Expendable trader types");
				if (controller.showE) {
					EditorGUI.indentLevel = 3;
					for (int e = 0; e<controller.expendableT.Count; e++) {
						String name;
						if (controller.expendableT [e] == null)
							name = "Trader" + e;
						else
							name = controller.expendableT [e].name;
						EditorGUILayout.BeginHorizontal ();
						controller.expendableT [e] = (GameObject)EditorGUILayout.ObjectField (name, controller.expendableT [e], typeof(GameObject), true);
						if (GUILayout.Button ("X", EditorStyles.miniButton, GUILayout.MaxWidth (20f))) {
							Undo.RegisterUndo ((Controller)target, "Remove trader type");
							controller.expendableT.RemoveAt (e);
						}
						EditorGUILayout.EndHorizontal ();
					}
				}
			}
			GUI.enabled = true;
			#endregion
			#region units
			EditorGUI.indentLevel = 1;
			controller.showAllU = EditorGUILayout.Foldout (controller.showAllU, "Units");
			if (controller.showAllU) {
				if (GUILayout.Button ("Add", EditorStyles.miniButton)) {
					Undo.RegisterUndo ((Controller)target, "Add new unit");
					controller.units.Add (new Unit{suffix = "New unit", max = Mathf.Infinity, min = 0.000001f});
				}
				for (int u = 0; u<controller.units.Count; u++) {
					Unit cur = controller.units [u];
					EditorGUI.indentLevel = 2;
					
					EditorGUILayout.BeginHorizontal ();
					cur.suffix = EditorGUILayout.TextField ("Unit suffix", cur.suffix);
					if (GUILayout.Button ("X", EditorStyles.miniButton, GUILayout.MaxWidth (20f))) {
						Undo.RegisterUndo ((Controller)target, "Remove unit");
						controller.units.RemoveAt (u);
					}
					EditorGUILayout.EndHorizontal ();
					
					EditorGUI.indentLevel = 3;
					EditorGUILayout.BeginHorizontal ();
					cur.min = EditorGUILayout.FloatField ("Min", cur.min);
					cur.max = EditorGUILayout.FloatField ("Max", cur.max);
					EditorGUILayout.EndHorizontal ();
					
					if (cur.suffix.Length == 0)
						cur.suffix = "New unit";
					cur.min = Mathf.Max (cur.min, 0.000001f);
					cur.max = Mathf.Max (cur.max, 0.000001f, cur.min);
					
					for (int g = 0; g< controller.goods.Count; g++)
						SetUnit (g);
				}
				EditorGUI.indentLevel = 2;
				if (CheckOverlap ()) 
					EditorGUILayout.HelpBox ("There is overlap between some of the units.\nMake sure that there are " +
							"no unis with an overlap.", MessageType.Warning);
				if (!CheckInfinity () && controller.units.Count > 0)
					EditorGUILayout.HelpBox ("None of your units extend to ininity. As a result, some items may not have " +
						"any units. To extend to infinity, type infinity into the max field.", MessageType.Warning);
			}
		}
		#endregion
		#endregion
		#region goods
		EditorGUI.indentLevel = 0;
		controller.showAllG = EditorGUILayout.Foldout (controller.showAllG, "Goods");
		if (controller.showAllG) {			
			EditorGUILayout.BeginHorizontal ();
			EditorGUI.indentLevel = 1;
			EditorGUILayout.LabelField ("Number of types", controller.goods.Count.ToString ());
			
			GUIEnable (controller.goods.Count);
			if (GUILayout.Button ("Expand all", EditorStyles.miniButtonLeft)) {
				ExpandCollapse (controller.showSmallG, true);
			}
			if (GUILayout.Button ("Collapse all", EditorStyles.miniButtonRight)) {
				ExpandCollapse (controller.showSmallG, false);
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal ();
			
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
						controller.goods.Insert (g, new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 1});
						controller.showSmallG.Insert (g, false);
						controller.allNames.Insert (g, "Name");	
						ChangeFromPoint (g - 1, true);
						AddStock (g);
					}
					if (GUILayout.Button ("Remove", EditorStyles.miniButtonMid)) {
						Undo.RegisterUndo ((Controller)target, "Remove " + controller.allNames [g]);
						controller.goods.RemoveAt (g);
						controller.showSmallG.RemoveAt (g);
						controller.allNames.RemoveAt (g);
						ChangeFromPoint (g - 1, false);
						CheckManufacturing ();
						for (int p = 0; p< posts.Length; p++) {
							posts [p].GetComponent<TradePost> ().stock.RemoveAt (g);
						}
						break;
					}
					if (GUILayout.Button ("Add after", EditorStyles.miniButtonRight)) {
						Undo.RegisterUndo ((Controller)target, "Add new type after " + controller.allNames [g]);
						controller.goods.Insert (g + 1, new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 1});
						controller.showSmallG.Insert (g + 1, false);
						controller.allNames.Insert (g + 1, "Name");
						ChangeFromPoint (g, true);
						AddStock (g + 1);
					}
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					name = EditorGUILayout.TextField ("Name", name);
					string unit = "";
					if (controller.units.Count > 0 && controller.goods [g].unit < controller.units.Count)
						unit = " (" + controller.units [controller.goods [g].unit].suffix + ")";
					controller.goods [g].mass = Mathf.Clamp (EditorGUILayout.FloatField ("Mass" + unit, controller.goods [g].mass), 0.000001f, Mathf.Infinity);
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
					
					SetUnit (g);
				}
				
			}//end for all
			if (controller.goods.Count == 0) {
				if (GUILayout.Button ("Add")) {
					controller.showSmallG.Add (true);
					controller.goods.Add (new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 1});
					controller.allNames.Add ("Name");
					AddStock(0);
				}
			}
				
		}//end if showing goods
		#endregion
		#region manufacturing
		EditorGUI.indentLevel = 0;
		controller.showAllM = EditorGUILayout.Foldout (controller.showAllM, "Manufacturing");
		if (controller.showAllM) {
			EditorGUI.indentLevel = 1;
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Number of types", controller.manufacturing.Count.ToString ());
			
			GUIEnable (controller.manufacturing.Count);
			if (GUILayout.Button ("Expand all", EditorStyles.miniButtonLeft)) {
				ExpandCollapse (controller.showSmallM, true);
			}
			if (GUILayout.Button ("Collapse all", EditorStyles.miniButtonRight)) {
				ExpandCollapse (controller.showSmallM, false);
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Add", EditorStyles.miniButtonLeft)) {
				Undo.RegisterUndo ((Controller)target, "Add new manufacturing process");
				controller.showSmallM.Add (false);
				if (controller.manufacturing.Count == 0)
					controller.showSmallM [0] = true;
				if (controller.goods.Count == 0) 
					Debug.LogError ("There are no possible types to manufacture\nAdd some types to goods");
				controller.manufacturing.Add (new Items{name = "", needing = new List<NeedMake> (), making = new List<NeedMake> ()});
			}
			
			GUIEnable (controller.manufacturing.Count);
			if (GUILayout.Button ("Check", EditorStyles.miniButtonRight)) {				
				float[] inf = new float[controller.goods.Count];
				for (int p = 0; p<posts.Length; p++) {
					for (int m = 0; m<controller.manufacturing.Count; m++) {
						TradePost post = posts [p].GetComponent<TradePost> ();
						if (post.manufacture [m].yesNo) {
							for (int i = 0; i<controller.manufacturing[m].needing.Count; i++) 
								inf [controller.manufacturing [m].needing [i].item] -= (controller.manufacturing [m].needing [i].number / (post.manufacture [m].seconds * 1f));
							for (int i = 0; i<controller.manufacturing[m].making.Count; i++) 
								inf [controller.manufacturing [m].making [i].item] += (controller.manufacturing [m].making [i].number / (post.manufacture [m].seconds * 1f));
						}
					}
				}
				string show = "NOTE: This can only act as a guide because there may be pauses in manufacturing if there " +
					"are not enough items, so there will be some variances.\n\n" +
					"It is useful to give an idea of whether the number of each item is expected to increase, decrease " +
					"or stay the same as time tends to infinity. A larger number means that it will increase or decrease faster.\n";
				for (int g = 0; g<controller.goods.Count; g++) {
					show += "\n" + controller.goods [g].name + " ";
					if (inf [g] > 0)
						show += "increase ("+inf[g].ToString("f2")+")";
					else if (inf [g] == 0)
						show += "same";
					else
						show += "decrease ("+Mathf.Abs(inf[g]).ToString("f2")+")";
				}
				EditorUtility.DisplayDialog ("Checking manufacturing", show, "Ok");	
			}
			EditorGUILayout.EndHorizontal ();
			GUI.enabled = true;
			
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
						controller.manufacturing [m].needing.Add (new NeedMake{number = 1});
						CheckManufacturing ();
					}
					EditorGUILayout.EndHorizontal ();
					
					for (int n = 0; n<controller.manufacturing[m].needing.Count; n++) {
						EditorGUI.indentLevel = 3;
						EditorGUILayout.BeginHorizontal ();
						controller.manufacturing [m].needing [n].item = EditorGUILayout.Popup ("Type", controller.manufacturing [m].needing [n].item, controller.allNames.ToArray ());
						controller.manufacturing [m].needing [n].number = EditorGUILayout.IntField ("Quantity", controller.manufacturing [m].needing [n].number);
						if (GUILayout.Button ("X", EditorStyles.miniButton, GUILayout.MaxWidth (20f))) {
							Undo.RegisterUndo ((Controller)target, "Remove needing item from " + controller.manufacturing [m].name);
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
						controller.manufacturing [m].making.Add (new NeedMake{number = 1});
						CheckManufacturing ();
					}
					EditorGUILayout.EndHorizontal ();
					
					for (int n = 0; n<controller.manufacturing[m].making.Count; n++) {
						EditorGUI.indentLevel = 3;
						EditorGUILayout.BeginHorizontal ();
						controller.manufacturing [m].making [n].item = EditorGUILayout.Popup ("Type", controller.manufacturing [m].making [n].item, controller.allNames.ToArray ());
						controller.manufacturing [m].making [n].number = EditorGUILayout.IntField ("Quantity", controller.manufacturing [m].making [n].number);
						if (GUILayout.Button ("X", EditorStyles.miniButton, GUILayout.MaxWidth (20f))) {
							Undo.RegisterUndo ((Controller)target, "Remove making item from " + controller.manufacturing [m].name);
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
		#region GUI changed
		if (GUI.changed) {//get changes so can update other scripts
			for (int p = 0; p<posts.Length; p++) {
				TradePost post = posts [p].GetComponent<TradePost> ();
				
				while (post.manufacture.Count != controller.manufacturing.Count) {
					if (post.manufacture.Count > controller.manufacturing.Count)
						post.manufacture.RemoveAt (post.manufacture.Count - 1);
					else
						post.manufacture.Add (new Mnfctr{yesNo = false, seconds = 1});
				}
				
//				Debug.Log("post: "+posts[p].name+"\nstock length: "+post.stock.Count+"\ncontroller goods length: "+controller.goods.Count);
				for (int x = 0; x<controller.goods.Count; x++) {
					post.stock [x].name = controller.goods [x].name;
					controller.allNames [x] = controller.goods [x].name;
				}
			}//end for posts
		}//end if GUI changed
		#endregion	
	}

	void ChangeFromPoint (int point, bool increase)
	{
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
	}
	
	void AddStock (int point)
	{
		for (int p = 0; p< posts.Length; p++) {
			posts [p].GetComponent<TradePost> ().stock.Insert (point, new Stock{name = "Name", allow = true});
		}
	}
	
	void CheckManufacturing ()
	{
		for (int m = 0; m<controller.manufacturing.Count; m++) {
			if (controller.goods.Count == 0) {
				Debug.LogError ("There are no possible types to manufacture\nAdd some types to goods");
				break;
			}
			
			if (CheckNM (controller.manufacturing [m].needing)) {
				Debug.LogError ("Some items in manufacturing " + controller.manufacturing [m].name + " are undefined" +
						"\nPlease ensure that all types have been selected");
				break;
			}
				
			if (CheckNM (controller.manufacturing [m].making)) {
				Debug.LogError ("Some items in manufacturing " + controller.manufacturing [m].name + " are undefined" +
						"\nPlease ensure that all types have been selected");
				break;
			}
		}
	}
	
	bool CheckNM (List<NeedMake> list)
	{
		for (int i = 0; i<list.Count; i++) {
			if (list [i].item == -1) {
				return true;
			}					
		}
		return false;
	}
	
	void ExpandCollapse (List<bool> list, bool ec)
	{
		for (int l = 0; l<list.Count; l++) {
			list [l] = ec;
		}
	}
	
	bool CheckOverlap ()
	{
		for (int u = 0; u<controller.units.Count; u++) {
			for (int v = 0; v<controller.units.Count; v++) {
				if (u != v && controller.units [u].min < controller.units [v].max && controller.units [u].min >= controller.units [v].min)
					return true;
			}
		}
		return false;
	}
	
	bool CheckInfinity ()
	{
		for (int u = 0; u<controller.units.Count; u++) {
			if (controller.units [u].max == Mathf.Infinity)
				return true;
		}
		return false;
	}
	
	void SetUnit (int goodID)
	{
		for (int u = 0; u<controller.units.Count; u++) {
			if (controller.goods [goodID].mass >= controller.units [u].min && controller.goods [goodID].mass < controller.units [u].max)
				controller.goods [goodID].unit = u;
		}
	}
	
	void GUIEnable(int count){
		if (count > 0)
			GUI.enabled = true;
		else
			GUI.enabled = false;
	}
}