using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CallumP.TagManagement
{//use namespace to stop any name conflicts
		[System.Serializable]
		public class SB
		{
				public string tagName;
				public bool selected;
		}

	[AddComponentMenu("CallumP/TagManagement/ObjectTags")]
	//add to component menu	
		public class ObjectTags : MonoBehaviour
		{
	
				public bool locked;//whether the connected group can be changed or not
				public TagManager connected;//the connected tag manager
				public List<SB> tags = new List<SB> ();//the list of tags with names and bool
				
				public void Init (bool lockedIn, string managerGroupName)
				{//initialise with string in
						TagManager[] allManagers = (TagManager[])FindObjectsOfType (typeof(TagManager));//get all managers
						allManagers = allManagers.Where (a => a.groupName == managerGroupName).ToArray ();
				
						if (allManagers.Length == 0) {//if not found
								Debug.LogError ("Could not find manager specified");
								return;
						}
						Init (lockedIn, allManagers [0]);//else can initialise
				}//end Init for string
				
				public void Init (bool lockedIn, TagManager connectedManager)
				{//initialise with TagManager
						locked = lockedIn;
						connected = connectedManager;
					
						connected.Sort (this);//now sort the information required
				}//end Init for TagManager
		
				public static ObjectTags GetTagComponent (GameObject curObj, string connectedName)
				{//get the tag component with the required name
						ObjectTags[] allObjectTags = curObj.GetComponents<ObjectTags> ();//get all scripts where connected name is correct
		
						if (allObjectTags.Length == 0)//if not found
								return null;//return null
								
						allObjectTags = allObjectTags.Where (t => t.connected != null && t.connected.groupName == connectedName).ToArray ();
			
						if (allObjectTags.Length == 0)//if not found
								return null;//return null
				
						return allObjectTags [0];//return what should be the only ObjectTag
				}//end GetTagComponent
				
				public static List<bool> GetTagsList (GameObject curObj, string connectedName)
				{//get the list of bools of the tags
						return GetTagsList (GetTagComponent (curObj, connectedName));
				}//end GetTagsList
				
				public static List<bool> GetTagsList (ObjectTags tagObj)
				{
						if (tagObj == null)
								return new List<bool> ();//if null, return an empty list
			
						return tagObj.tags.Select (t => t.selected).ToList ();//return the selected values
				}//end GetTagsList
				
				public static bool GetTag (GameObject curObj, string connectedName, string tagName)
				{
						return GetTag (GetTagComponent (curObj, connectedName), tagName);
				}
				
				public static bool GetTag (GameObject curObj, string connectedName, int tagNumber)
				{
						return GetTag (GetTagComponent (curObj, connectedName), tagNumber);
				}
				
				public static bool GetTag (ObjectTags tagObj, string tagName)
				{
						if (tagObj.connected == false)
								return false;
				
						return GetTag (tagObj, GetTagNumber (tagObj, tagName));
				}
				
				public static bool GetTag (ObjectTags tagObj, int tagNumber)
				{
						if (tagObj == null || tagObj.tags.Count < tagNumber || tagNumber == -1)
								return false;
						return tagObj.tags [tagNumber].selected;
				}
				
				public static int GetTagNumber (ObjectTags tagObj, string tagName)
				{
						return tagObj.tags.FindIndex (t => t.tagName == tagName);
				}
				
				public static void SetTag (GameObject curObj, string connectedName, string tagName, bool selected)
				{
						SetTag (GetTagComponent (curObj, connectedName), tagName, selected);
				}

				public static void SetTag (GameObject curObj, string connectedName, int tagNumber, bool selected)
				{
						SetTag (GetTagComponent (curObj, connectedName), tagNumber, selected);
				}

				public static void SetTag (ObjectTags tagObj, string tagName, bool selected)
				{
						SetTag (tagObj, GetTagNumber (tagObj, tagName), selected);
				}

				public static void SetTag (ObjectTags tagObj, int tagNumber, bool selected)
				{
						tagObj.tags [tagNumber].selected = selected;
				}
		}//end ObjectTags
}//end namespace