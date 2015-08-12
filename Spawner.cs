using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TradeSys
{//use namespace to stop any name conflicts
		[AddComponentMenu("TradeSys/Make Spawner")]
		//add to component menu	
		public class Spawner : MonoBehaviour
		{
				//Controller controller;
				public List<ItemGroup> items = new List<ItemGroup> ();//the selected items
				public float minTime = 1, maxTime = 5;// the min / max time between spawns
				public int maxSpawn = 10, min = 1, max = 10, shapeOption;
				//max that can be spawned, and the min / max to spawn at once, shape option selected
				public float maxDist = 5;//max dist from the spawner to spawn - is dist due to different shapes
				public bool countCrates;//whether is counting the number of crates or the number of items
				public bool diffItems;//if selected, will spawn multiple different rather than increasing count
				public bool specifySeed;//allow the seed to be set specifically
				public int seed;//means that it is possible for it to be the same each time
				
				Controller controller;
				int count;//the number of items currently at the spawner
				List<List<NeedMake>> canSpawn = new List<List<NeedMake>> ();//an array of items which can be spawned
				//uses NeedMake as need groupID, itemID and number
	
				void Awake ()
				{
						controller = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
						
						//get all of the items which can be spawned to make random numbers easier
						for (int g = 0; g<items.Count; g++) {//for all groups
								List<NeedMake> groupAllow = new List<NeedMake> ();//list containing everything allowed in current group
								for (int i = 0; i<items[g].items.Count; i++) {//for all items
										if (items [g].items [i].enabled)//if item is enabled
												groupAllow.Add (new NeedMake{groupID=g, itemID=i, number = items[g].items[i].number});//add item to groupAllow
								}//end for all items
								if (groupAllow.Count > 0)//only need to add group if something is enabled
										canSpawn.Add (groupAllow);
						}//end for all groups
						
						if (specifySeed)//if specific seed set, then make seed. if not, then will use a random seed
								Random.seed = seed;
						
						StartCoroutine (Spawn ());
				}//end Awake
	
				IEnumerator Spawn ()
				{//spawn an item
						yield return new WaitForSeconds (Random.Range (minTime, maxTime));//pause for a time before spawning
						if (count < maxSpawn || maxSpawn == 0) {//check can spawn a new item
								
								int cG = Random.Range (0, canSpawn.Count);//the chosen group
								NeedMake chosen = canSpawn [cG] [Random.Range (0, canSpawn [cG].Count)];
								
								GameObject crate = controller.goods [chosen.groupID].goods [chosen.itemID].itemCrate;
								
								if (crate == null)//check that there is a crate
										Debug.LogError (controller.goods [chosen.groupID].goods [chosen.itemID].name + " does not have a crate and no default crate has been specified!");
								else {//only create if crate exists
										int toSpawn = Random.Range (min, max + 1);//spawn a number of items
								
										if (diffItems) {
												for (int s = 0; s<toSpawn; s++)
														Create (chosen, crate, 1);//spawn each item individually
										} else //else all in one
												Create (chosen, crate, toSpawn);//else can increase the number
								}//end creating item								
						}//end number check
						StartCoroutine (Spawn ());//spawn a new item
				}//end Spawn
				
				void Create (NeedMake chosen, GameObject crate, int number)
				{//create the spawned items
								
						GameObject spawned = (GameObject)GameObject.Instantiate (crate);
						
						switch (shapeOption) {//start a switch with the different spawn area options
						case 0://sphere
								spawned.transform.position = Random.insideUnitSphere * maxDist;
								break;
						case 1://circle
								spawned.transform.position = Random.insideUnitCircle * maxDist;
								break;
						case 2://cube
								spawned.transform.position = new Vector3 (RandomLength (maxDist), RandomLength (maxDist), RandomLength (maxDist));
								break;
						case 3://square
								spawned.transform.position = new Vector3 (RandomLength (maxDist), RandomLength (maxDist), 0);
								break;
						}//end switch
						
						spawned.transform.position = this.transform.position + this.transform.rotation * Vector3.Scale(spawned.transform.position, this.transform.lossyScale);
						//sort out position relating to spawner position and rotation
				
						spawned.transform.parent = this.transform;//set the spawned item to be under the spawner
						spawned.name = controller.goods [chosen.groupID].goods [chosen.itemID].name + "\u00D7" + number.ToString ();
			
						Item spawnedScript = spawned.GetComponent<Item> ();//get the item script
			
						//need to set the group and item IDs as some items may share a crate
						spawnedScript.groupID = chosen.groupID;//set the groupID
						spawnedScript.itemID = chosen.itemID;//set the itemID	
						spawnedScript.number = number;//set the number of this item
						spawnedScript.traderCollect = true;//let traders collect this item
						
						ChangeCount(number, true);
						
						controller.UpdateAverage (chosen.groupID, chosen.itemID, number, 0);//update the item averages
				}//end Create
				
				float RandomLength (float maxDist)
				{//generate a distance
						float hmd = maxDist / 2;
						return Random.Range (-hmd, hmd);
				}//end RandomLength
				
				void OnDrawGizmosSelected ()
				{//draw the shapes of the spawn areas				
						Gizmos.color = Color.green;
						Gizmos.matrix = Matrix4x4.TRS (this.transform.position, this.transform.rotation, this.transform.lossyScale);
				
						switch (shapeOption) {//use a switch for the different spawn areas
						case 0://sphere
								Gizmos.DrawWireSphere (Vector3.zero, maxDist);
								break;
						//circle drawn in editor script
						case 2://cube
								Gizmos.DrawWireCube (Vector3.zero, Vector3.one * maxDist);
								break;
						case 3://square
								Gizmos.DrawWireCube (Vector3.zero, new Vector3 (maxDist, maxDist, 0));
								break;
						}//end switch
				}//end OnDrawGizmosSelected
				
				public void ChangeCount(int change, bool increase)
				{//used to change the count of items at the spawner
				int multiplier = increase?1:-1;//have a multiplier so will be 1 when increase, -1 for decrease				
			
			if (countCrates && !diffItems)
				count+=multiplier;//if counting crates when having items all in one, change by 1
			else//else increase by the number
				count += change*multiplier;
				}//end ChangeCount
		}//end Spawner
}//end namespace