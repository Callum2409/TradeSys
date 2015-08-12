using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys {//use namespace to stop any name conflicts
	public class CheckManufacturingWindow : EditorWindow {
		Controller controller;
		public float[][] perItem;//the per item changes
		public float total;//the total changes
		public string[][] allNamesG;//names of goods
		public string[][] pricing;//the manufacturing processes which make losses
		public string[][] allNamesM;//names of manufacturing
		public string[][] tooltipsM;//manufacturing tooltips
		public string[] goodsTitles;//the names of the goods groups
		public string[] manTitles;//the names of the manufacturing groups
	
		bool showHoriz;
		Vector2 scrollPosN, scrollPosP;
		string[][] info;//the item changes strings
		string totalInfo;
		TSGUI GUITools = new TSGUI ();
		bool first = true;
		int selection;
	
		void SetInfo () {//called before any GUI starts. This is so that the information can be received
			controller = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
			showHoriz = controller.showHoriz;//everything will be shown with the horizontal or vertical
		
			info = new string[allNamesG.Length][];//an array containing all of the strings to display
		
			for (int x = 0; x<info.Length; x++) {//go through all goods, saying increase, decrease or stay the same and give a value
				info [x] = new string[allNamesG [x].Length];
				for (int i = 0; i<info[x].Length; i++)
					info [x] [i] = SetChange (perItem [x] [i]);
			}
		
			totalInfo = SetChange (total);//add a change string to the total change
				
			first = false;
		}//end SetInfo
	
		string SetChange (float change) {//check to see if increase, decrease or the same
			if (change > 0)
				return "increase (" + change.ToString ("f2") + ")";
			if (change < 0)
				return "decrease (" + Mathf.Abs (change).ToString ("f2") + ")";
			return "same";
		}//end SetChange
	
		void OnGUI () {
			if (first)
				SetInfo ();
			
			EditorGUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			selection = GUILayout.Toolbar (selection, new string[]{"Item numbers", "Item pricing"});
			GUILayout.FlexibleSpace ();
			EditorGUILayout.EndHorizontal ();
		
			switch (selection) {
				#region item numbers
				case 0:
					EditorGUI.indentLevel = 0;
					EditorGUILayout.LabelField ("NOTE: This can only be used as a guide because there may be pauses and greater times between " +
						"manufacturing processes if items are not available.\n\nAs a result, there will be some variances, but will still " +
						"be useful to give an idea of whether the numbers of an item is expected to increase, decrease or stay the same.\n\n" +
						"A larger number means that this change is faster.", EditorStyles.wordWrappedLabel);//the text which is always displayed
		
					EditorGUILayout.LabelField ("", "", "ShurikenLine", GUILayout.MaxHeight (1f));//draw a separating line
					scrollPosN = EditorGUILayout.BeginScrollView (scrollPosN);
			
					EditorGUILayout.BeginVertical ("HelpBox");
			
					for (int g = 0; g<goodsTitles.Length; g++) {
						EditorGUI.indentLevel = 0;
						EditorGUILayout.LabelField (goodsTitles [g], EditorStyles.boldLabel);
						GUITools.HorizVertDisplay (allNamesG [g], info [g], new string[info [g].Length], showHoriz, 1);
					}
					
					EditorGUI.indentLevel = 0;
					EditorGUILayout.Space ();//space between all the items and the total change
					EditorGUILayout.LabelField ("Total change", totalInfo);
					EditorGUILayout.EndVertical ();
					GUILayout.EndScrollView ();
					break;
				#endregion
		
				#region pricing
				case 1:
					EditorGUI.indentLevel = 0;
					EditorGUILayout.LabelField ("This is showing the profit per time the manufacturing process occurs. This is a best-case scenario, where the cost of items purchased to manufacture are at their lowest, and the items made are sold at the highest. As a result, the profits are likely to be lower than this.\n\nAny process that shows a negative value here will always have a loss.\n\nThis assumes that the item prices are set automatically", EditorStyles.wordWrappedLabel);
		
					EditorGUILayout.LabelField ("", "", "ShurikenLine", GUILayout.MaxHeight (1f));//draw a separating line
					scrollPosP = EditorGUILayout.BeginScrollView (scrollPosP);
		
					EditorGUILayout.BeginVertical ("HelpBox");
					for (int m = 0; m<manTitles.Length; m++) {
						EditorGUI.indentLevel = 0;
						EditorGUILayout.LabelField (manTitles [m], EditorStyles.boldLabel);
						GUITools.HorizVertDisplay (allNamesM [m], pricing [m], tooltipsM [m], showHoriz, 1);
					}
					EditorGUILayout.EndVertical ();
			
					EditorGUILayout.EndScrollView ();
					break;
				#endregion
			}//end switch
		}//end OnGUI
	}//end CheckManufacturingWindow
}//end namespace