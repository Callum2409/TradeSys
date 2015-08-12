using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TradeSys{//uses TradeSys namespace to prevent any conflicts

		public class Spawner : MonoBehaviour
		{
				public float minTime = 5, maxTime = 20;
				public int maxNo = 50;
				public float radius = 10;
				public List<bool> allowSpawn = new List<bool> ();//used for editor
				public List<int> toSpawn = new List<int> ();//used to spawn
				public bool showAllow;
				Controller controller;
				public string[] options = new string[]{"Sphere", "Cube", "Circle", "Square"};
				public int option = 0;
				public bool randomRotation;
				public Vector3 specifiedRotation = new Vector3 ();
	
				void Start ()
				{
						controller = GameObject.Find ("Controller").GetComponent<Controller> ();
						StartCoroutine (Pause (Random.Range (minTime, maxTime)));
						for (int s = 0; s<allowSpawn.Count; s++) {
								if (allowSpawn [s])
										toSpawn.Add (s);
						}
				}
	
				IEnumerator Pause (float time)
				{
						yield return new WaitForSeconds (time);
						Spawn ();
				}
	
				void Spawn ()
				{
						if (toSpawn.Count > 0) {
								if (gameObject.transform.childCount < maxNo) {
					
										int no = Random.Range (0, toSpawn.Count);
										GameObject crate = controller.goodsArray [toSpawn [no]].itemCrate;
										Vector3 position = SpawnPosition ();
										Collider[] colliders = Physics.OverlapSphere (position, crate.renderer.bounds.size.magnitude / 2);
										if (colliders.Length == 0 || (colliders.Length == 1 && colliders [0].name == "Terrain")) {
												Quaternion rotation = Quaternion.Euler (specifiedRotation) * Quaternion.LookRotation (Vector3.up) * transform.rotation;
												if (randomRotation)
														rotation = Random.rotation;
												GameObject item = (GameObject)Object.Instantiate (crate, position, rotation);
												item.transform.parent = gameObject.transform;
												item.tag = "Item";
												controller.spawned.Add (new Spawned{spawner = this.gameObject, item = item, goodID = toSpawn [no]});
												controller.UpdateAverage (toSpawn [no], 1, 0);
										}//if item to be spawned is colliding with another, dont spawn
								}
						}
						StartCoroutine (Pause (Random.Range (minTime, maxTime)));
				}
	
				void OnDrawGizmosSelected ()
				{
						Gizmos.color = Color.green;
						Gizmos.matrix = Matrix4x4.TRS (transform.position, transform.rotation, transform.lossyScale);
						switch (option) {
						case 0://sphere
								Gizmos.DrawWireSphere (Vector3.zero, radius);	
								break;
						case 1://cube
								Gizmos.DrawWireCube (Vector3.zero, Vector3.one * radius);
								break;
						//case 2 is circle, drawn in editor script
						case 3://square
								Gizmos.DrawWireCube (Vector3.zero, new Vector3 (radius, 0, radius));
								break;
						}
						if (option == 1 || option == 3) {
								if (!randomRotation) {
										Gizmos.color = new Color (1, 0, 0, .3f);
										Gizmos.matrix = Matrix4x4.TRS (transform.position, transform.rotation * Quaternion.Euler (specifiedRotation), transform.lossyScale);
										Gizmos.DrawCube (Vector3.zero, new Vector3 (radius / 2, 0, radius / 2));
										Gizmos.color = Color.red;
										Gizmos.DrawWireCube (Vector3.zero, new Vector3 (radius / 2, 0, radius / 2));
								}
						}
				}
	
				public void SpawnEnableDisable (bool enable, int productID)
				{
						if (productID > allowSpawn.Count)
								Debug.LogError ("The productID is greater than the number of items available.\nMake sure that the productID is correct.");
						else {
								int index = toSpawn.Find (x => x == productID);
								if (enable) {
										if (index == -1) {
												toSpawn.Add (productID);
												allowSpawn [productID] = true;
										}
								} else {
										if (index != -1)
												toSpawn.RemoveAt (index);
										allowSpawn [productID] = false;
								}
						}
				}
	
				Vector3 SpawnPosition ()
				{
						switch (option) {
						case 0://sphere
								return this.transform.position + (Random.insideUnitSphere * radius);
						case 1://cube
								return this.transform.position + this.transform.rotation * (new Vector3 (Random.Range (-radius * .5f, radius * .5f), Random.Range (-radius * .5f, radius * .5f), Random.Range (-radius * .5f, radius * .5f)));
						case 2://circle
								Vector2 location = Random.insideUnitCircle * radius;
								return this.transform.position + this.transform.rotation * (new Vector3 (location.x, 0, location.y));
						case 3://square
								return this.transform.position + this.transform.rotation * (new Vector3 (Random.Range (-radius * .5f, radius * .5f), 0, Random.Range (-radius * .5f, radius * .5f)));
						default:
								return Vector3.zero;
						}
				}
		}
}//end namespace