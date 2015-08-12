using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CallumP.TagManagement
{//use namespace to stop any name conflicts
		[System.Serializable]
		public class SC
		{
				public string tagName;
				public Color colour;
		}

	[AddComponentMenu("CallumP/TagManagement/TagManager")]
	//add to component menu	
		public class TagManager : MonoBehaviour
		{
				public bool locked;//whether the name and details are locked
				public int minNumber = 0;//the minimum number of items that can be in the list
				public string groupName;//the name of the tag group
				public string singular;//the singular case of the name
				public string shortName;//short, lower case name
				public bool colours;//have a colour associated with each tag
				public List<SC> nameCol = new List<SC> ();//the list of names to display
		
				public void Init (string groupNameIn, string singularIn, string shortNameIn, bool lockedIn, bool coloursIn, int minNumberIn)
				{//initialise without names
						Init (groupNameIn, singularIn, shortNameIn, lockedIn, coloursIn, minNumberIn, new List<SC> ());
				}//end Init without names
		
				public void Init (string groupNameIn, string singularIn, string shortNameIn, bool lockedIn, bool coloursIn, int minNumberIn, SC[] namesIn)
				{//initialise with names in array
						Init (groupNameIn, singularIn, shortNameIn, lockedIn, coloursIn, minNumberIn, namesIn.ToList ());
				}//end Init with names in array
	
				public void Init (string groupNameIn, string singularIn, string shortNameIn, bool lockedIn, bool coloursIn, int minNumberIn, List<SC> namesIn)
				{//initialise with names in list
						//set up all values
						groupName = groupNameIn;
						singular = singularIn;
						shortName = shortNameIn;
						locked = lockedIn;
						colours = coloursIn;
						minNumber = minNumberIn;
						nameCol = namesIn;
				}//end Init with names in list
	
				public void Sort (ObjectTags toSort)
				{//sort the info for a single group
						List<SB> tagList = toSort.tags;
						int difference = tagList.Count - nameCol.Count;//if not enough

						if (difference > 0)	
								tagList.RemoveRange (tagList.Count - difference, difference);
						else {//else add new elements
								for (int a = 0; a<Mathf.Abs(difference); a++)
										tagList.Add (new SB ());
						}//end else add
				
						for (int i = 0; i<tagList.Count; i++) {//go through tag list
								tagList [i].tagName = nameCol [i].tagName;//set the name of the item
								if (nameCol [i].tagName == "Default")//if default, enable
										tagList [i].selected = true;
						}//end going through tag list
				}//end if connected to get info
		
				public ObjectTags[] GetAllScripts ()
				{//return all of the scripts that are connected to this manager
						ObjectTags[] all = (ObjectTags[])FindObjectsOfType (typeof(ObjectTags));
						return all.Where (g => g.connected == this).ToArray ();
				}//end GetAllScripts
		
				public void Reorder (int origin, int loc)
				{//move the elements in the given group when reordered
						ObjectTags[] groups = GetAllScripts ();
		
						foreach (ObjectTags group in groups) {//go through each script
								SB tagToMove = group.tags [origin];//get the tag to move
								group.tags.RemoveAt (origin);
								group.tags.Insert (loc, tagToMove);
						}//end for
				}//end Reorder
		
				public void NameChanger (int index, string name)
				{//update any changes in the names of items
						ObjectTags[] groups = GetAllScripts ();
		
						foreach (ObjectTags group in groups)//go through each script
								group.tags [index].tagName = name;
				}//end NameChanger
		
				public void AddRemove (int index, bool add, string newName)
				{//add or remove an element
						ObjectTags[] groups = GetAllScripts ();
						foreach (ObjectTags group in groups) {//go through each script
								if (add) 					
										group.tags.Insert (nameCol.Count, new SB (){tagName = newName, selected = false});
								else
										group.tags.RemoveAt (index);
						}//end foreach
				}//end AddRemove
		
		#region static methods		
				public static TagManager GetManager (GameObject curObj, string groupName)
				{//return the group manager of the name given
						TagManager[] managers = curObj.GetComponents<TagManager> ().Where (m => m.groupName == groupName).ToArray ();

						if (managers.Length == 0)//if not found
								return null;//return null
						return managers [0];//otherwise return what should be the only one
				}//end GetManager
				
				public static bool ShareEnabled (GameObject obj1, GameObject obj2, string groupName)
				{//get the groups of the GameObjects and then compare
						ObjectTags g1 = ObjectTags.GetTagComponent (obj1, groupName);
			ObjectTags g2 = ObjectTags.GetTagComponent (obj2, groupName);
		
						if (g1 != null && g2 != null)//only do this if not null
								return ShareEnabled (g1, g2, groupName);
						return false;//return false if not connected to correct
				}//end ShareEnabled for GameObjects and string
	
				public static bool ShareEnabled (GameObject obj1, GameObject obj2, TagManager manager)
				{//get the groups of the GameObjects and then compare
						return ShareEnabled (obj1, obj2, manager.groupName);
				}//end ShareEnabled for GameObjects and TagManager
	
				public static bool ShareEnabled (ObjectTags group1, ObjectTags group2, string groupName)
				{//work out if obj1 & 2 share the same group and then compare
						if (group1.connected.groupName == groupName && group2.connected.groupName == groupName)
						//if match the groupName
								return CompareEnabled (group1, group2);//compare
						//show the errors
						DispError (group1, groupName);
						DispError (group2, groupName);
			
						return false;
				}//end ShareEnabled for ObjectTags and string	
	
				public static bool ShareEnabled (ObjectTags group1, ObjectTags group2, TagManager manager)
				{//work out if obj1 & 2 share the same group and then compare
						if (group1.connected == manager && group2.connected == manager)
						//if match the groupManager
								return CompareEnabled (group1, group2);//compare
						//show the errors
						DispError (group1, manager.groupName);
						DispError (group2, manager.groupName);
						
						return false;
				}//end ShareEnabled for ObjectTags and TagManager
		
				static void DispError (ObjectTags group, string groupName)
				{//show an error if not the same groupName
						if (group.connected.groupName != groupName)
								Debug.LogError (group.gameObject.name + " has no groups connected to " + groupName);
				}//end DispError
	
				static bool CompareEnabled (ObjectTags group1, ObjectTags group2)
				{//compare the items in the two groups to see if any enabled match
						if (group1.tags.Count != group2.tags.Count)
								Debug.LogError ("The items in each group are not the same length! Attempting to match...");
						for (int i = 0; i<Mathf.Min(group1.tags.Count, group2.tags.Count); i++)
								if (group1.tags [i].selected && group2.tags [i].selected)//if both enabled, will return true
										return true;
						return false;//if gone through complete list and not both true, return false
				}//end CompareEnabled
		#endregion
		}//end TagManager
}//end namespace