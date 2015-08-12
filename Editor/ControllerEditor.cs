using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{
	Controller controller;
	GameObject[] posts;
	TradePost[] postScripts;
	GameObject[] spawners;
	string[] directories;
	List<GameObject> traders = new List<GameObject> ();
	Trader[] traderScripts;
	int traderCount;
	bool reloading;
	
	void Awake ()
	{
		posts = GameObject.FindGameObjectsWithTag ("Trade Post");
		postScripts = new TradePost[posts.Length];
		spawners = GameObject.FindGameObjectsWithTag ("Spawner");
		traders = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Trader"));
		controller = (Controller)target;
		directories = AssetDatabase.GetAllAssetPaths ();//get all assets so item crate finder works
		
		if (!controller.directories.SequenceEqual (directories) && !reloading && (controller.loadTraderPrefabs || controller.expendable) && !Application.isPlaying) {//check if directories are the same, so dont have to reload		
			controller.directories = directories;
			if (!ProgressBar ()) {
				Debug.LogWarning("Trader prefab loading was not allowed to complete. Load trader prefabs and expendable traders have been disabled.");
				controller.loadTraderPrefabs = false;
				controller.expendable = false;
			}
		} else//end same directories check
			traders.AddRange (controller.traderPrefabs);
		traderScripts = new Trader[traders.Count];
		
		while (controller.showSmallG.Count != controller.goods.Count) {
			if (controller.showSmallG.Count > controller.goods.Count) {
				controller.showSmallG.RemoveAt (controller.showSmallG.Count - 1);
			} else {
				controller.showSmallG.Add (false);
			}
		}//while showing each good not correct length
		while (controller.showSmallM.Count != controller.manufacturing.Count) {
			if (controller.showSmallM.Count > controller.manufacturing.Count) 
				controller.showSmallM.RemoveAt (controller.showSmallM.Count - 1);
			else {
				controller.showSmallM.Add (false);
			}
		}//while showing each manufacturing not correct length
		for (int p = 0; p<posts.Length; p++) {
			TradePost post = postScripts [p] = posts [p].GetComponent<TradePost> ();
			while (post.stock.Count != controller.goods.Count) {
				if (post.stock.Count > controller.goods.Count) {
					post.stock.RemoveAt (post.stock.Count - 1);
				} else {
					post.stock.Add (new Stock{name = "Name", allow = true});
				}
			}//end while not correct number of items in posts
			CheckManufacturingLists (p);
			while (post.groups.Count != controller.groups.Count) {
				if (post.groups.Count > controller.groups.Count) {
					post.groups.RemoveAt (post.groups.Count - 1);
				} else {
					post.groups.Add (false);
				}
			}//while not correct number of groups
			while (post.factions.Count != controller.factions.Count) {
				if (post.factions.Count > controller.factions.Count) {
					post.factions.RemoveAt (post.factions.Count - 1);
				} else {
					post.factions.Add (false);
				}
			}//while not correct number of factions
		}//end for posts
		for (int s = 0; s<spawners.Length; s++) {
			Spawner spawner = spawners [s].GetComponent<Spawner> ();
			while (spawner.allowSpawn.Count != controller.goods.Count) {
				if (spawner.allowSpawn.Count > controller.goods.Count) {
					spawner.allowSpawn.RemoveAt (spawner.allowSpawn.Count - 1);
				} else {
					spawner.allowSpawn.Add (true);
				}
			}//end while not correct number of items in spawner
		}//end for spawners
		for (int t = 0; t<traders.Count; t++) {
			traderScripts [t] = traders [t].GetComponent<Trader> ();
			Trader trader = traderScripts [t];
			while (trader.factions.Count != controller.factions.Count) {
				if (trader.factions.Count > controller.factions.Count) {
					trader.factions.RemoveAt (trader.factions.Count - 1);
				} else {
					trader.factions.Add (false);
				}
			}
		}
	}
	
	public override void OnInspectorGUI ()
	{
		EditorGUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		if (Application.isPlaying)
			controller.selC = GUILayout.Toolbar (controller.selC, new string[]{"Settings", "Goods", "Manufacturing", "Overview", "Extra info"});
		else
			controller.selC = GUILayout.Toolbar (controller.selC, new string[]{"Settings", "Goods", "Manufacturing", "Overview"});
		GUILayout.FlexibleSpace ();
		EditorGUILayout.EndHorizontal ();
		
		switch (controller.selC) {
		#region settings				
		case 0:	
			EditorGUI.indentLevel = 0;
			
			EditorGUILayout.BeginHorizontal ();
			controller.updateInterval = Mathf.Max (EditorGUILayout.FloatField (new GUIContent ("Update interval", "This is the time between updates of the prices at trade posts and possible trades"), controller.updateInterval), 0.01f);
			
			if (!controller.expendable) {//can only disable loading if expendable traders is disabled
				bool lTPB = controller.loadTraderPrefabs;
				controller.loadTraderPrefabs = EditorGUILayout.Toggle (new GUIContent ("Load trader prefabs", "If enabled, when an asset is added or removed, the trader prefabs will be reloaded to ensure that all of the trader prefabs get updated. Disabling will mean that it is not reloaded."), controller.loadTraderPrefabs);
			
				if (!lTPB && controller.loadTraderPrefabs)
				if (!reloading && !ProgressBar ())
					controller.loadTraderPrefabs = false;
			} else
				EditorGUILayout.LabelField ("");
			EditorGUILayout.EndHorizontal ();
			
			controller.allowPickup = EditorGUILayout.Toggle (new GUIContent ("Allow item pickup", "Allow the pickup of items from spawners or dropped items. Cannot be disabled if there are spawners, trader pickup is enabled individually"), controller.allowPickup);
			
			#region show trade routes
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginHorizontal ();
			controller.showR = EditorGUILayout.Toggle ("Show trade routes", controller.showR);
			if (controller.showR)
				controller.showRP = EditorGUILayout.Toggle (new GUIContent ("Show routes on post", "Show trade routes while editing trade posts"), controller.showRP);
			EditorGUILayout.EndHorizontal ();
			#endregion
			
			#region pause options
			EditorGUILayout.BeginVertical ("HelpBox");
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginHorizontal ();
			controller.pauseOption = EditorGUILayout.Popup ("Pause option", controller.pauseOption, controller.pauseOptions, "DropDownButton");
			switch (controller.pauseOption) {
			case 0:
				controller.pauseTime = EditorGUILayout.FloatField ("Pause time", controller.pauseTime);
				break;
			case 2:
				string unit = "1";
				for (int u = 0; u<controller.units.Count; u++)
					if (controller.units [u].min <= 1 && controller.units [u].max > 1) {
						unit = controller.units [u].suffix;
						break;
					}
				controller.pauseTime = EditorGUILayout.FloatField ("Pause time per " + unit, controller.pauseTime);
				break;
			}
			controller.pauseTime = Mathf.Max (0f, controller.pauseTime);
			if (controller.pauseOption == 0)
				for (int t = 0; t<traderScripts.Length; t++)
					traderScripts [t].stopTime = controller.pauseTime;
			if (controller.pauseOption == 2)
				for (int g = 0; g<controller.goods.Count; g++)
					controller.goods [g].pausePerUnit = controller.pauseTime * controller.goods [g].mass;
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			string en = "Pause on entry";
			string ex = "Pause on exit";
			string enTo = "Pause when a trader enters a trade post.";
			string exTo = "Pause when a trader exits a trade post.";
			if (controller.pauseOption > 1) {
				en = "Pause for unloading";
				ex = "Pause for loading";
				enTo = "Pause when a trader unloads cargo.";
				exTo = "Pause when a trader loads cargo.";
			}
			controller.pauseOnEnter = EditorGUILayout.Toggle (new GUIContent (en, enTo), controller.pauseOnEnter);
			controller.pauseOnExit = EditorGUILayout.Toggle (new GUIContent (ex, exTo), controller.pauseOnExit);
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();
			#endregion
			
			#region expendable traders
			if (controller.expendable)
				EditorGUILayout.BeginVertical ("HelpBox");
			bool eB = controller.expendable;
			controller.expendable = EditorGUILayout.Toggle ("Expendable traders", controller.expendable);
			if(Application.isPlaying)
				controller.expendable = eB;
			if (!eB && controller.expendable && !controller.loadTraderPrefabs) {
				if (!reloading && !ProgressBar ())
					controller.expendable = false;
			}
			
			if (controller.expendable) {
				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginHorizontal ();				
				controller.maxNoTraders = EditorGUILayout.IntField ("Max no. traders", controller.maxNoTraders);
				controller.maxNoTraders = (int)Mathf.Clamp (controller.maxNoTraders, 0, Mathf.Infinity);
				EditorGUILayout.LabelField ("");
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Trader types: " + controller.expendableT.Count);
				if (GUILayout.Button ("Add", EditorStyles.miniButtonLeft)) {
					Undo.RegisterUndo ((Controller)target, "Add new trader type");
					controller.expendableT.Add (null);
					controller.showE = true;
				}
				if (GUILayout.Button ("Load all", EditorStyles.miniButtonRight)) {
					Undo.RegisterUndo ((Controller)target, "Load all trader prefabs");
					controller.expendableT = new List<GameObject> (controller.traderPrefabs);
					controller.showE = true;
				}
				EditorGUILayout.EndHorizontal ();
				
				controller.showE = EditorGUILayout.Foldout (controller.showE, "Expendable trader types");
				if (controller.showE) {
					if (controller.expendableT.Count > 0) {
						EditorGUILayout.BeginVertical ("HelpBox");
						EditorGUI.indentLevel = 0;
						for (int e = 0; e<controller.expendableT.Count; e++) {
							String name;
							if (controller.expendableT [e] == null)
								name = "Trader " + e;
							else
								name = controller.expendableT [e].name;
							EditorGUILayout.BeginHorizontal ();
							controller.expendableT [e] = (GameObject)EditorGUILayout.ObjectField (name, controller.expendableT [e], typeof(GameObject), false);
							EditorGUILayout.BeginVertical (GUILayout.MaxWidth (18f));
							GUILayout.Space (3f);
							if (GUILayout.Button ("", "OL Minus")) {
								Undo.RegisterUndo ((Controller)target, "Remove trader type");
								controller.expendableT.RemoveAt (e);
							}
							EditorGUILayout.EndVertical ();
							EditorGUILayout.EndHorizontal ();
						}
						GUILayout.EndVertical ();
					}
				}
			}
			if (controller.expendable)
				GUILayout.EndVertical ();
			GUI.enabled = true;
			#endregion
			#region groups
			EditorGUI.indentLevel = 0;
			if (controller.allowGroups)
				EditorGUILayout.BeginVertical ("HelpBox");
			EditorGUI.indentLevel = 0;
			controller.allowGroups = EditorGUILayout.Toggle (new GUIContent ("Enable Groups", "Groups can be used to allow trading between select trade posts"), controller.allowGroups);
			if (controller.allowGroups) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Number of groups:", "" + controller.groups.Count);
				if (GUILayout.Button ("Add", EditorStyles.miniButton)) {
					Undo.RegisterUndo ((Controller)target, "Add new group");
					controller.groups.Add ("Group " + controller.groups.Count);
					controller.showG = true;
					for (int p = 0; p<postScripts.Length; p++)
						postScripts [p].groups.Add (false);
				}
				EditorGUILayout.EndHorizontal ();
				controller.showG = EditorGUILayout.Foldout (controller.showG, "Groups");
				if (controller.showG) {
					if (controller.groups.Count > 0) {
						EditorGUILayout.BeginVertical ("HelpBox");
						for (int g = 0; g<controller.groups.Count; g++) {
							EditorGUILayout.BeginHorizontal ();
							if (controller.groups [g] == "")
								controller.groups [g] = "Element " + controller.groups.Count;
							controller.groups [g] = EditorGUILayout.TextField (controller.groups [g], controller.groups [g]);
							EditorGUILayout.BeginVertical (GUILayout.MaxWidth (18f));
							GUILayout.Space (3f);
							if (GUILayout.Button ("", "OL Minus")) {
								Undo.RegisterUndo ((Controller)target, "Delete group");
								for (int p = 0; p<postScripts.Length; p++) {
									postScripts [p].groups.RemoveAt (g);
								}
								controller.groups.RemoveAt (g);
							}
							EditorGUILayout.EndVertical ();
							EditorGUILayout.EndHorizontal ();
						}
						EditorGUILayout.EndVertical ();
					}
				}	
			}
			if (controller.allowGroups)
				GUILayout.EndVertical ();
			#endregion
			#region factions
			EditorGUI.indentLevel = 0;
			if (controller.allowFactions)
				EditorGUILayout.BeginVertical ("HelpBox");
			EditorGUI.indentLevel = 0;
			controller.allowFactions = EditorGUILayout.Toggle (new GUIContent ("Enable Factions", "Factions are used to allow trading between posts, and with only specific traders"), controller.allowFactions);
			if (controller.allowFactions) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Number of factions:", "" + controller.factions.Count);
				if (GUILayout.Button ("Add", EditorStyles.miniButton)) {
					Undo.RegisterUndo ((Controller)target, "Add new faction");
					controller.factions.Add (new Faction{name = "Faction " + controller.factions.Count, color = Color.green});
					controller.showF = true;
					for (int p = 0; p<postScripts.Length; p++)
						postScripts [p].factions.Add (false);
					for (int t = 0; t<traderScripts.Length; t++)
						traderScripts [t].factions.Add (false);
				}
				EditorGUILayout.EndHorizontal ();
				controller.showF = EditorGUILayout.Foldout (controller.showF, "Factions");
				if (controller.showF) {
					if (controller.factions.Count > 0) {
						EditorGUILayout.BeginVertical ("HelpBox");
						for (int f = 0; f<controller.factions.Count; f++) {
							EditorGUILayout.BeginHorizontal ();
							if (controller.factions [f].name == "")
								controller.factions [f].name = "Element " + controller.factions.Count;
							controller.factions [f].name = EditorGUILayout.TextField (controller.factions [f].name, controller.factions [f].name);
							EditorGUILayout.BeginVertical (GUILayout.MaxWidth (80f));
							GUILayout.Space (3f);
							controller.factions [f].color = EditorGUILayout.ColorField (controller.factions [f].color, GUILayout.MaxWidth (80f));
							EditorGUILayout.EndVertical ();
							EditorGUILayout.BeginVertical (GUILayout.MaxWidth (18f));
							GUILayout.Space (3f);
							if (GUILayout.Button ("", "OL Minus")) {
								Undo.RegisterUndo ((Controller)target, "Delete faction");
								for (int p = 0; p<postScripts.Length; p++) 
									postScripts [p].factions.RemoveAt (f);
								for (int t = 0; t<traderScripts.Length; t++)
									traderScripts [t].factions.RemoveAt (f);
								controller.factions.RemoveAt (f);
							}
							EditorGUILayout.EndVertical ();
							EditorGUILayout.EndHorizontal ();
						}
						EditorGUILayout.EndVertical ();
					}
				}	
			}
			if (controller.allowFactions)
				GUILayout.EndVertical ();
			#endregion
			#region units
			EditorGUI.indentLevel = 0;
			GUILayout.BeginVertical ("HelpBox");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Number of units: ", "" + controller.units.Count);
			if (GUILayout.Button ("Add", EditorStyles.miniButton)) {
				Undo.RegisterUndo ((Controller)target, "Add new unit");
				controller.units.Add (new Unit{suffix = "New unit", max = Mathf.Infinity, min = 0.000001f});
				controller.showAU = true;
			}
			EditorGUILayout.EndHorizontal ();
			controller.showAU = EditorGUILayout.Foldout (controller.showAU, "Units");
			if (controller.showAU) {
				for (int u = 0; u<controller.units.Count; u++) {
					Unit cur = controller.units [u];
					EditorGUI.indentLevel = 1;
					
					EditorGUILayout.BeginHorizontal ();
					cur.suffix = EditorGUILayout.TextField ("Unit suffix", cur.suffix);
					EditorGUILayout.BeginVertical (GUILayout.MaxWidth (18f));
					GUILayout.Space (3f);
					if (GUILayout.Button ("", "OL Minus")) {
						Undo.RegisterUndo ((Controller)target, "Remove unit");
						controller.units.RemoveAt (u);
					}
					EditorGUILayout.EndVertical ();
					EditorGUILayout.EndHorizontal ();
					
					EditorGUI.indentLevel = 2;
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
				EditorGUI.indentLevel = 0;
				if (CheckOverlap ()) 
					EditorGUILayout.HelpBox ("There is overlap between some of the units.\nMake sure that there are " +
							"no unis with an overlap.", MessageType.Warning);
				if (!CheckInfinity () && controller.units.Count > 0)
					EditorGUILayout.HelpBox ("None of your units extend to ininity. As a result, some items may not have " +
						"any units. To extend to infinity, type infinity into the max field.", MessageType.Warning);
			}
			GUILayout.EndVertical ();
			
		#endregion
			break;
		#endregion
		#region goods
		case 1:		
			EditorGUILayout.BeginHorizontal ();
			EditorGUI.indentLevel = 0;
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
				EditorGUI.indentLevel = 0;
				string name = controller.goods [g].name;
				if (name == "")
					name = "Name";
				controller.showSmallG [g] = EditorGUILayout.Foldout (controller.showSmallG [g], name);
	
				if (controller.showSmallG [g]) {
					EditorGUILayout.BeginVertical ("HelpBox");
					EditorGUI.indentLevel = 0;
					#region goods buttons
					EditorGUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Add before", EditorStyles.miniButtonLeft)) {
						Undo.RegisterUndo ((Controller)target, "Add new type before " + controller.allNames [g]);
						controller.goods.Insert (g, new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 1});
						controller.showSmallG.Insert (g, true);
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
							postScripts [p].stock.RemoveAt (g);
						}
						for (int s = 0; s< spawners.Length; s++) {
							spawners [s].GetComponent<Spawner> ().allowSpawn.RemoveAt (g);
						}
						break;
					}
					if (GUILayout.Button ("Add after", EditorStyles.miniButtonRight)) {
						Undo.RegisterUndo ((Controller)target, "Add new type after " + controller.allNames [g]);
						controller.goods.Insert (g + 1, new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 1});
						controller.showSmallG.Insert (g + 1, true);
						controller.allNames.Insert (g + 1, "Name");
						ChangeFromPoint (g, true);
						AddStock (g + 1);
					}
					EditorGUILayout.EndHorizontal ();
					#endregion
					EditorGUILayout.BeginHorizontal ();
					name = EditorGUILayout.TextField ("Name", name);
					string unit = "";
					if (controller.units.Count > 0 && controller.goods [g].unit < controller.units.Count)
						unit = " (" + controller.units [controller.goods [g].unit].suffix + ")";
					controller.goods [g].mass = Mathf.Clamp (EditorGUILayout.FloatField ("Mass" + unit, controller.goods [g].mass), 0.000001f, Mathf.Infinity);
					controller.goods [g].name = name;
					EditorGUILayout.EndHorizontal ();
					if (controller.pauseOption == 3) {
						EditorGUILayout.BeginHorizontal ();
						controller.goods [g].pausePerUnit = Mathf.Max (EditorGUILayout.FloatField ("Unload time", controller.goods [g].pausePerUnit), 0);
						EditorGUILayout.LabelField ("");
						EditorGUILayout.EndHorizontal ();
					}
					if (controller.allowPickup) {
						EditorGUILayout.BeginHorizontal ();
						controller.goods [g].itemCrate = (GameObject)EditorGUILayout.ObjectField (new GUIContent ("Item crate", 
							"This is what the item looks like when you see it in the game, so is likely to be in a box or crate"), controller.goods [g].itemCrate, typeof(GameObject), false);
						if (GUILayout.Button (new GUIContent ("Find crate", "Find an item crate. The name of the GameObject " +
							"needs to be exactly the same as the name of the item."), EditorStyles.miniButton)) {
							Undo.RegisterUndo ((Controller)target, "Find item crate");
							for (int d = 0; d<directories.Length; d++) {
								if (EditorUtility.DisplayCancelableProgressBar ("Searching", "Searching for GameObject called " + controller.goods [g].name, d / (directories.Length * 1f)))
									break;
								GameObject asset = (GameObject)AssetDatabase.LoadAssetAtPath (directories [d], typeof(GameObject));
								if (asset != null && asset.name == controller.goods [g].name) {
									controller.goods [g].itemCrate = asset;
									break;
								} 
							}
							EditorUtility.ClearProgressBar ();
							if (controller.goods [g].itemCrate == null)
								Debug.LogWarning ("Could not find an item crate.\nPlease make sure that the item crate has the same name as the item.");
						}
						
						EditorGUILayout.EndHorizontal ();
					}

					EditorGUILayout.LabelField ("Prices", EditorStyles.boldLabel);
					
					EditorGUI.indentLevel = 1;
					EditorGUILayout.BeginHorizontal ();
					controller.goods [g].basePrice = EditorGUILayout.IntField ("Base", controller.goods [g].basePrice);
					EditorGUILayout.LabelField ("");
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
					GUILayout.EndVertical ();
				}
				
			}//end for all
			if (controller.goods.Count == 0) {
				if (GUILayout.Button ("Add", EditorStyles.miniButton)) {
					Undo.RegisterUndo ((Controller)target, "Add new good");
					controller.showSmallG.Add (true);
					controller.goods.Add (new Goods{name = "Name", basePrice = 0, minPrice = 0, maxPrice = 0, mass = 1});
					controller.allNames.Add ("Name");
					AddStock (0);
				}
			}
			break;	
		#endregion
		#region manufacturing
		case 2:
			EditorGUI.indentLevel = 0;
			
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
				controller.showSmallM.Add (true);
				if (controller.goods.Count == 0) 
					Debug.LogError ("There are no possible types to manufacture\nAdd some types to goods");
				controller.manufacturing.Add (new Items{name = "", needing = new List<NeedMake> (), making = new List<NeedMake> ()});
			}
			
			GUIEnable (controller.manufacturing.Count);
			if (GUILayout.Button ("Check", EditorStyles.miniButtonRight)) {				
				float[] inf = new float[controller.goods.Count];
				float total = 0;
				for (int p = 0; p<posts.Length; p++) {
					EditorUtility.DisplayProgressBar ("Checking", "Checking manufacturing", p / (posts.Length * 1f));
					for (int m = 0; m<controller.manufacturing.Count; m++) {
						TradePost post = postScripts [p];
						if (post.manufacture [m].allow) {
							for (int i = 0; i<controller.manufacturing[m].needing.Count; i++) 
								inf [controller.manufacturing [m].needing [i].item] -= (controller.manufacturing [m].needing [i].number / ((post.manufacture [m].create+post.manufacture[m].cooldown) * 1f));
							for (int i = 0; i<controller.manufacturing[m].making.Count; i++)
								inf [controller.manufacturing [m].making [i].item] += (controller.manufacturing [m].making [i].number / ((post.manufacture [m].create+post.manufacture[m].cooldown) * 1f));
						}
					}
				}
				EditorUtility.ClearProgressBar ();
				string show = "NOTE: This can only act as a guide because there may be pauses in manufacturing if there " +
					"are not enough items, so there will be some variances.\n\n" +
					"It is useful to give an idea of whether the number of each item is expected to increase, decrease " +
					"or stay the same as time tends to infinity. A larger number means that it will increase or decrease faster.\n";
				for (int g = 0; g<controller.goods.Count; g++) {
					show += "\n" + controller.goods [g].name + " ";
					if (inf [g] > 0)
						show += "increase (" + inf [g].ToString ("f2") + ")";
					else if (inf [g] == 0)
						show += "same";
					else
						show += "decrease (" + Mathf.Abs (inf [g]).ToString ("f2") + ")";
					total += inf [g];
				}
				show += "\nGlobal change: ";
				
				if (total > 0)
					show += "increase (" + total.ToString ("f2") + ")";
				else if (total == 0)
					show += "same";
				else
					show += "decrease (" + Mathf.Abs (total).ToString ("f2") + ")";
				
				EditorUtility.DisplayDialog ("Checking manufacturing", show, "Ok");	
			}
			EditorGUILayout.EndHorizontal ();
			GUI.enabled = true;
			
			for (int m = 0; m < controller.manufacturing.Count; m++) {
				EditorGUI.indentLevel = 0;
				string name = controller.manufacturing [m].name;
				if (name == "")
					name = "Element " + m;
				controller.showSmallM [m] = EditorGUILayout.Foldout (controller.showSmallM [m], name);
				if (controller.showSmallM [m]) {
					EditorGUILayout.BeginVertical ("HelpBox");
					EditorGUI.indentLevel = 0;
					
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
					
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Needing", EditorStyles.boldLabel);
					if (GUILayout.Button ("Add")) {
						Undo.RegisterUndo ((Controller)target, "Add needing element to " + controller.manufacturing [m].name);
						controller.manufacturing [m].needing.Add (new NeedMake{number = 1});
						CheckManufacturing ();
					}
					EditorGUILayout.EndHorizontal ();
					
					for (int n = 0; n<controller.manufacturing[m].needing.Count; n++) {
						EditorGUI.indentLevel = 1;
						EditorGUILayout.BeginHorizontal ();
						controller.manufacturing [m].needing [n].item = EditorGUILayout.Popup ("Type", controller.manufacturing [m].needing [n].item, controller.allNames.ToArray (), "DropDownButton");
						controller.manufacturing [m].needing [n].number = EditorGUILayout.IntField ("Quantity", controller.manufacturing [m].needing [n].number);
						EditorGUILayout.BeginVertical (GUILayout.MaxWidth (18f));
						GUILayout.Space (3f);
						if (GUILayout.Button ("", "OL Minus")) {
							Undo.RegisterUndo ((Controller)target, "Remove needing item from " + controller.manufacturing [m].name);
							controller.manufacturing [m].needing.RemoveAt (n);
							break;
						}
						EditorGUILayout.EndVertical ();
						EditorGUILayout.EndHorizontal ();
						controller.manufacturing [m].needing [n].number = (int)Mathf.Clamp (controller.manufacturing [m].needing [n].number, 0, Mathf.Infinity);
					}
					
					EditorGUI.indentLevel = 0;
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Making", EditorStyles.boldLabel);
					if (GUILayout.Button ("Add")) {
						Undo.RegisterUndo ((Controller)target, "Add making element to " + controller.manufacturing [m].name);
						controller.manufacturing [m].making.Add (new NeedMake{number = 1});
						CheckManufacturing ();
					}
					EditorGUILayout.EndHorizontal ();
					
					for (int n = 0; n<controller.manufacturing[m].making.Count; n++) {
						EditorGUI.indentLevel = 1;
						EditorGUILayout.BeginHorizontal ();
						controller.manufacturing [m].making [n].item = EditorGUILayout.Popup ("Type", controller.manufacturing [m].making [n].item, controller.allNames.ToArray (), "DropDownButton");
						controller.manufacturing [m].making [n].number = EditorGUILayout.IntField ("Quantity", controller.manufacturing [m].making [n].number);
						EditorGUILayout.BeginVertical (GUILayout.MaxWidth (18f));
						GUILayout.Space (3f);
						if (GUILayout.Button ("", "OL Minus")) {
							Undo.RegisterUndo ((Controller)target, "Remove making item from " + controller.manufacturing [m].name);
							controller.manufacturing [m].making.RemoveAt (n);
							break;
						}
						EditorGUILayout.EndVertical ();
						EditorGUILayout.EndHorizontal ();
						controller.manufacturing [m].making [n].number = (int)Mathf.Clamp (controller.manufacturing [m].making [n].number, 0, Mathf.Infinity);
					}
					GUILayout.EndVertical ();
				}
			}//end for all manufacturing
			break;
		#endregion
		#region Overview
		case 3:
			EditorGUI.indentLevel = 0;
			
			EditorGUILayout.BeginHorizontal ();
			controller.showHoriz = EditorGUILayout.Toggle (new GUIContent ("Show items vertically", "This will show the items ascending vertically"), controller.showHoriz, "Radio");
			controller.showHoriz = !EditorGUILayout.Toggle (new GUIContent ("Show items horizontally", "This will show the items ascending h"), !controller.showHoriz, "Radio");
			EditorGUILayout.EndHorizontal ();
			#region groups
			{
				if (!controller.allowGroups) {
					EditorGUI.indentLevel = 0;
					EditorGUILayout.BeginVertical ("HelpBox");
					EditorGUILayout.LabelField ("Groups", EditorStyles.boldLabel);
					EditorGUI.indentLevel = 1;
					EditorGUILayout.HelpBox ("Grouping has not been enabled, so all posts can trade with each other", MessageType.Info);
					EditorGUILayout.EndVertical ();
				} else {
					int[] count = new int[controller.groups.Count];
					for (int p = 0; p<postScripts.Length; p++) {
						for (int g  = 0; g<controller.groups.Count; g++)
							if (postScripts [p].groups [g])
								count [g]++;	
					}
					EditorGUI.indentLevel = 0;
					EditorGUILayout.BeginVertical ("HelpBox");
					EditorGUILayout.LabelField ("Groups", EditorStyles.boldLabel);
					EditorGUI.indentLevel = 1;
					if (!controller.showHoriz) {
						for (int g = 0; g<controller.groups.Count; g=g+2) {
							EditorGUILayout.BeginHorizontal ();
							EditorGUILayout.LabelField (new GUIContent (controller.groups [g]), new GUIContent ("" + count [g], "This is the number of posts that are in this group"));
							if (g < controller.groups.Count - 1)
								EditorGUILayout.LabelField (new GUIContent (controller.groups [g + 1]), new GUIContent ("" + count [g + 1], "This is the number of posts that are in this group"));
							EditorGUILayout.EndHorizontal ();
						}
					} else {
						int half = Mathf.CeilToInt (controller.groups.Count / 2f);
						for (int g = 0; g<half; g++) {
							EditorGUILayout.BeginHorizontal ();
							EditorGUILayout.LabelField (new GUIContent (controller.groups [g]), new GUIContent ("" + count [g], "This is the number of posts that are in this group"));
							if (half + g < controller.groups.Count)
								EditorGUILayout.LabelField (new GUIContent (controller.groups [half + g]), new GUIContent ("" + count [half + g], "This is the number of posts that are in this group"));
							EditorGUILayout.EndHorizontal ();
						}
					}
					EditorGUILayout.EndVertical ();
				}//groups have been enabled
			}
			#endregion
			#region factions
			{
				if (!controller.allowFactions) {
					EditorGUI.indentLevel = 0;
					EditorGUILayout.BeginVertical ("HelpBox");
					EditorGUILayout.LabelField ("Factions", EditorStyles.boldLabel);
					EditorGUI.indentLevel = 1;
					EditorGUILayout.HelpBox ("Factions have not been enabled, so all posts and traders can trade with each other", MessageType.Info);
					EditorGUILayout.EndVertical ();
				} else {
					int[] countP = new int[controller.factions.Count];
					int[] countT = new int[controller.factions.Count];
					for (int p = 0; p<postScripts.Length; p++) {
						for (int f  = 0; f<controller.factions.Count; f++)
							if (postScripts [p].factions [f])
								countP [f]++;	
					}
					GameObject[] tradersInScene = GameObject.FindGameObjectsWithTag ("Trader");
					for (int t = 0; t<tradersInScene.Length; t++) {
						for (int f  = 0; f<controller.factions.Count; f++)
							if (tradersInScene [t].GetComponent<Trader> ().factions [f])
								countT [f]++;	
					}
					EditorGUI.indentLevel = 0;
					EditorGUILayout.BeginVertical ("HelpBox");
					EditorGUILayout.LabelField ("Factions", EditorStyles.boldLabel);
					EditorGUI.indentLevel = 1;
					string text = "Left number is number of posts in the faction. Right number is the number of traders in the faction";
					if (!controller.showHoriz) {
						for (int f = 0; f<controller.factions.Count; f=f+2) {
							EditorGUILayout.BeginHorizontal ();
							EditorGUILayout.LabelField (new GUIContent (controller.factions [f].name), new GUIContent ("" + countP [f] + ", " + countT [f], text));
							if (f < controller.factions.Count - 1)
								EditorGUILayout.LabelField (new GUIContent (controller.factions [f + 1].name), new GUIContent ("" + countP [f + 1] + ", " + countT [f], text));
							EditorGUILayout.EndHorizontal ();
						}
					} else {
						int half = Mathf.CeilToInt (controller.factions.Count / 2f);
						for (int f = 0; f<half; f++) {
							EditorGUILayout.BeginHorizontal ();
							EditorGUILayout.LabelField (new GUIContent (controller.factions [f].name), new GUIContent ("" + countP [f] + ", " + countT [f], text));
							if (half + f < controller.factions.Count)
								EditorGUILayout.LabelField (new GUIContent (controller.factions [half + f].name), new GUIContent ("" + countP [half + f] + ", " + countT [half + f], text));
							EditorGUILayout.EndHorizontal ();
						}
					}
					EditorGUILayout.EndVertical ();
				}//groups have been enabled
			}
			#endregion
			int totalLinks = 0;
			for (int p1 = 0; p1<postScripts.Length; p1++) {
				for (int p2 = p1 +1; p2<postScripts.Length; p2++) {
					if (controller.CheckGroupsFactions (postScripts [p1], postScripts [p2])) {
						totalLinks++;
					}
				}
			}
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField (new GUIContent ("Total number of links", "This is the total number of  unique trade links (Does not include links going in the opposite direction to that already counted)"), 
						new GUIContent ("" + totalLinks, "This is the total number of  unique trade links (Does not include links going in the opposite direction to that already counted)"));
					
			#region item totals
			{
				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginVertical ("HelpBox");
				EditorGUILayout.LabelField ("Item totals", EditorStyles.boldLabel);
				EditorGUI.indentLevel = 1;
				string text = "Left number is the number of posts the item is available at. Right number is the total number of the item.";
				int[] total = new int[controller.goods.Count];
				int[] count = new int[controller.goods.Count];
				for (int g = 0; g<controller.goods.Count; g++) {
					if (!Application.isPlaying) {
						for (int p = 0; p<posts.Length; p++) { 
							if (postScripts [p].stock [g].allow) {
								total [g] += postScripts [p].stock [g].number;
								count [g]++;
							}
						}
					} else {
						total [g] = Mathf.RoundToInt (controller.goods [g].average * controller.goods [g].postCount);
						count [g] = controller.goods [g].postCount;
					}
				}//end for get totals
				if (!controller.showHoriz) {
					for (int g = 0; g<controller.goods.Count; g=g+2) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (new GUIContent (controller.goods [g].name, ""), new GUIContent ("" + count [g] + ", " + total [g], text));
						if (g < controller.goods.Count - 1)
							EditorGUILayout.LabelField (new GUIContent (controller.goods [g + 1].name, ""), new GUIContent ("" + count [g + 1] + ", " + total [g + 1], text));
						EditorGUILayout.EndHorizontal ();
					}
				} else {
					int half = Mathf.CeilToInt (controller.goods.Count / 2f);
					for (int g = 0; g<half; g++) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (new GUIContent (controller.goods [g].name, ""), new GUIContent ("" + count [g] + ", " + total [g], text));
						if (half + g < controller.goods.Count)
							EditorGUILayout.LabelField (new GUIContent (controller.goods [half + g].name, ""), new GUIContent ("" + count [half + g] + ", " + total [half + g], text));
						EditorGUILayout.EndHorizontal ();
					}
				}
				long totalAll = 0;
				for (int t = 0; t<total.Length; t++)
					totalAll += total [t];
				EditorGUI.indentLevel = 0;
				EditorGUILayout.LabelField (new GUIContent ("Total number of items: ", "This is the total number of items available in game."), new GUIContent ("" + totalAll, "This is the total number of items available in game."));
				GUILayout.EndVertical ();
			}			
			#endregion
			#region manufacturing totals
			{
				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginVertical ("HelpBox");
				EditorGUILayout.LabelField ("Manufacturing totals", EditorStyles.boldLabel);
				EditorGUI.indentLevel = 1;
				int[] count = new int[controller.manufacturing.Count];
				string text = "This is the number of posts where the manufacturing process has been enabled";
				for (int m = 0; m<controller.manufacturing.Count; m++) {
					for (int p = 0; p<posts.Length; p++)
						if (postScripts [p].manufacture [m].allow)
							count [m]++;
				}//end for get totals
				if (!controller.showHoriz) {
					for (int m = 0; m<controller.manufacturing.Count; m=m+2) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (new GUIContent (controller.manufacturing [m].name), new GUIContent ("" + count [m], text));
						if (m < controller.manufacturing.Count - 1)
							EditorGUILayout.LabelField (new GUIContent (controller.manufacturing [m + 1].name), new GUIContent ("" + count [m + 1], text));
						EditorGUILayout.EndHorizontal ();
					}
				} else {
					int half = Mathf.CeilToInt (controller.manufacturing.Count / 2f);
					for (int m = 0; m<half; m++) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (new GUIContent (controller.manufacturing [m].name), new GUIContent ("" + count [m], text));
						if (half + m < controller.manufacturing.Count)
							EditorGUILayout.LabelField (new GUIContent (controller.manufacturing [half + m].name), new GUIContent ("" + count [half + m], text));
						EditorGUILayout.EndHorizontal ();
					}
				}
			}
			GUILayout.EndVertical ();
			#endregion
			break;
		#endregion
		#region Extra info
		case 4:
			EditorGUI.indentLevel = 0;
			
			GUILayout.BeginVertical ("HelpBox");
			if (GUILayout.Button ("Please see the manual for what each category is showing.\nClick here to open", "LODRendererAddButton"))
				Application.OpenURL ((Application.dataPath) + "/TradeSys/Manual.pdf");
			GUILayout.EndVertical ();
			
			EditorGUILayout.BeginHorizontal ();
			controller.showHoriz = EditorGUILayout.Toggle ("Show items vertically", controller.showHoriz, "Radio");
			controller.showHoriz = !EditorGUILayout.Toggle ("Show items horizontally", !controller.showHoriz, "Radio");
			EditorGUILayout.EndHorizontal ();
			
			#region list totals
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginVertical ("HelpBox");
			EditorGUILayout.LabelField ("List totals", EditorStyles.boldLabel);
			EditorGUI.indentLevel = 1;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Sell list", "" + controller.sell.Count);
			EditorGUILayout.LabelField ("Buy list", "" + controller.buy.Count);
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.LabelField ("Compare list", "" + controller.compare.Count);
			EditorGUILayout.EndVertical ();
			#endregion
			
			#region item totals
			{		
				EditorGUI.indentLevel = 0;
				EditorGUILayout.BeginVertical ("HelpBox");
				EditorGUILayout.LabelField ("Item totals", EditorStyles.boldLabel);
				EditorGUI.indentLevel = 1;
				int[] total = new int[controller.goods.Count];
				int[] count = new int[controller.goods.Count];
				for (int g = 0; g<controller.goods.Count; g++) {
					if (!Application.isPlaying) {
						for (int p = 0; p<posts.Length; p++) { 
							if (postScripts [p].stock [g].allow) {
								total [g] += postScripts [p].stock [g].number;
								count [g]++;
							}
						}
					} else {
						total [g] = Mathf.RoundToInt (controller.goods [g].average * controller.goods [g].postCount);
						count [g] = controller.goods [g].postCount;
					}
				}//end for get totals
				if (!controller.showHoriz) {
					for (int g = 0; g<controller.goods.Count; g=g+2) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (controller.goods [g].name, "" + count [g] + ", " + total [g] + ", " + controller.goods [g].average.ToString ("F2"));
						if (g < controller.goods.Count - 1)
							EditorGUILayout.LabelField (controller.goods [g + 1].name, "" + count [g + 1] + ", " + total [g + 1] + ", " + controller.goods [g + 1].average.ToString ("F2"));
						EditorGUILayout.EndHorizontal ();
					}
				} else {
					int half = Mathf.CeilToInt (controller.goods.Count / 2f);
					for (int g = 0; g<half; g++) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (controller.goods [g].name, "" + count [g] + ", " + total [g] + ", " + controller.goods [g].average.ToString ("F2"));
						if (half + g < controller.goods.Count)
							EditorGUILayout.LabelField (controller.goods [half + g].name, "" + count [half + g] + ", " + total [half + g] + ", " + controller.goods [half + g].average.ToString ("F2"));
						EditorGUILayout.EndHorizontal ();
					}
				}
				long totalAll = 0;
				for (int t = 0; t<total.Length; t++)
					totalAll += total [t];
				EditorGUI.indentLevel = 0;
				EditorGUILayout.LabelField ("Total number of items: ", "" + totalAll);
				GUILayout.EndVertical ();
			}
			#endregion
			
			#region trading
			EditorGUI.indentLevel = 0;
			EditorGUILayout.BeginVertical ("HelpBox");
			EditorGUILayout.LabelField ("Trading totals", EditorStyles.boldLabel);
			EditorGUI.indentLevel = 1;
			{
				int[] total = new int[controller.goods.Count];
				int[] count = new int[controller.goods.Count];
				
				for (int o = 0; o<controller.ongoing.Count; o++) {
					total [controller.ongoing [o].typeID] += controller.ongoing [o].number;
					count [controller.ongoing [o].typeID]++;
				}
				
				if (!controller.showHoriz) {
					for (int g = 0; g<controller.goods.Count; g=g+2) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (controller.goods [g].name, "" + count [g] + ", " + total [g]);
						if (g < controller.goods.Count - 1)
							EditorGUILayout.LabelField (controller.goods [g + 1].name, "" + count [g + 1] + ", " + total [g + 1]);
						EditorGUILayout.EndHorizontal ();
					}
				} else {
					int half = Mathf.CeilToInt (controller.goods.Count / 2f);
					for (int g = 0; g<half; g++) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (controller.goods [g].name, "" + count [g] + ", " + total [g]);
						if (half + g < controller.goods.Count)
							EditorGUILayout.LabelField (controller.goods [half + g].name, "" + count [half + g] + ", " + total [half + g]);
						EditorGUILayout.EndHorizontal ();
					}
				}
			}
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField ("Total trades", "" + controller.ongoing.Count);
			EditorGUILayout.EndVertical ();
			#endregion
			#region spawned
			if (spawners.Length > 0) {
				EditorGUI.indentLevel = 0; 
				EditorGUILayout.BeginVertical ("HelpBox");
				EditorGUILayout.LabelField ("Spawned totals", EditorStyles.boldLabel);
				EditorGUI.indentLevel = 1;
				int[] count = new int[controller.goods.Count];
				for (int s = 0; s<controller.spawned.Count; s++)
					count [controller.spawned [s].goodID]++;
				
				if (!controller.showHoriz) {
					for (int g = 0; g<controller.goods.Count; g=g+2) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (controller.goods [g].name, "" + count [g]);
						if (g < controller.goods.Count - 1)
							EditorGUILayout.LabelField (controller.goods [g + 1].name, "" + count [g + 1]);
						EditorGUILayout.EndHorizontal ();
					}
				} else {
					int half = Mathf.CeilToInt (controller.goods.Count / 2f);
					for (int g = 0; g<half; g++) {
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (controller.goods [g].name, "" + count [g]);
						if (half + g < controller.goods.Count)
							EditorGUILayout.LabelField (controller.goods [half + g].name, "" + count [half + g]);
						EditorGUILayout.EndHorizontal ();
					}
				}
				EditorGUI.indentLevel = 0;
				EditorGUILayout.LabelField ("Total spawned", "" + controller.spawned.Count);
				EditorGUILayout.EndVertical ();
			}//end if spawners in scene
			#endregion
			#region trader totals
			{
				EditorGUI.indentLevel = 0; 
				EditorGUILayout.BeginVertical ("HelpBox");
				EditorGUILayout.LabelField ("Traders", EditorStyles.boldLabel);
				
				if (controller.expendable) {
					traders = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Trader"));
					traderScripts = new Trader[traders.Count];
					for (int t = 0; t<traders.Count; t++)
						traderScripts [t] = traders [t].GetComponent<Trader> ();
				}
				EditorGUILayout.LabelField ("Trader count", "" + traderCount);
				EditorGUI.indentLevel = 1;
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				EditorGUILayout.BeginVertical ();
				
				EditorStyles.label.alignment = TextAnchor.MiddleCenter;
				EditorStyles.label.fontStyle = FontStyle.Bold;
				EditorGUILayout.BeginHorizontal ();
				
				EditorGUILayout.PrefixLabel ("Trader name");
				EditorGUILayout.PrefixLabel ("Current target");
				EditorGUILayout.PrefixLabel ("Final post");
				EditorGUILayout.EndHorizontal ();
				EditorStyles.label.fontStyle = FontStyle.Normal;
				
				
				traderCount = 0;
				for (int t = 0; t<traderScripts.Length; t++) {
					if (traders [t].activeInHierarchy && traderScripts [t].expendable == controller.expendable) {
						traderCount++;
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.PrefixLabel (traderScripts [t].name);
						if (traderScripts [t].target != null)
							EditorGUILayout.PrefixLabel (traderScripts [t].target.name);
						else
							EditorGUILayout.PrefixLabel (" - ");
						if (traderScripts [t].finalPost != null)
							EditorGUILayout.PrefixLabel (traderScripts [t].finalPost.name);
						else
							EditorGUILayout.PrefixLabel (" - ");
						EditorGUILayout.EndHorizontal ();
					}
				}
				EditorGUILayout.EndVertical ();
				GUILayout.FlexibleSpace ();
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
				EditorStyles.label.alignment = TextAnchor.UpperLeft;
			}
			#endregion
			break;
			#endregion
		default:
			controller.selC = 3;
			break;
		}//end switch
		#region GUI changed
		if (GUI.changed) {//get changes so can update other scripts
			for (int p = 0; p<posts.Length; p++) {
				TradePost post = postScripts [p];
				
				for (int x = 0; x<controller.goods.Count; x++) 
					post.stock [x].name = controller.goods [x].name;
				CheckManufacturingLists (p);
			}//end for posts
			
			for (int x = 0; x<controller.goods.Count; x++)
				controller.allNames [x] = controller.goods [x].name;
			
			if (!controller.allowPickup && GameObject.FindGameObjectsWithTag ("Spawner").Length > 0)
				controller.allowPickup = true;
			EditorUtility.SetDirty (controller);
		}//end if GUI changed
		#endregion	
	}//end OnInspectorGUI
	
	void CheckManufacturingLists (int p)
	{
		TradePost post = postScripts [p];
		while (post.manufacture.Count != controller.manufacturing.Count) {
			if (post.manufacture.Count > controller.manufacturing.Count)
				post.manufacture.RemoveAt (post.manufacture.Count - 1);
			else
				post.manufacture.Add (new Mnfctr{allow = false, create = 1, cooldown = 0});
		}//end while not correct number of manufacturing
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
			postScripts [p].stock.Insert (point, new Stock{name = "Name", allow = true});
		}
		for (int s = 0; s< spawners.Length; s++) {
			spawners [s].GetComponent<Spawner> ().allowSpawn.Insert (point, true);
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
	
	void GUIEnable (int count)
	{
		if (count > 0)
			GUI.enabled = true;
		else
			GUI.enabled = false;
	}
	
	void OnSceneGUI ()
	{
		if (controller.showR) {
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
	
	bool ProgressBar ()
	{
		controller.traderPrefabs.Clear ();	
		reloading = true;
		bool cancelled = false;
		for (int d = 0; d<directories.Length; d++) {
			if (EditorUtility.DisplayCancelableProgressBar ("Reloading", "Reloading trader prefabs", d / (directories.Length * 1f))) {
				cancelled = true;
				break;
			}
			GameObject asset = (GameObject)AssetDatabase.LoadAssetAtPath (directories [d], typeof(GameObject));
			if (asset != null && asset.tag == "Trader") {
				traders.Add (asset);
				controller.traderPrefabs.Add (asset);
			} 
		}
		EditorUtility.ClearProgressBar ();
		reloading = false;
		return !cancelled;
	}
}