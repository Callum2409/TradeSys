using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
	public float minTime = 5, maxTime = 20;
	public int maxNo = 50;
	public float sphereRadius = 10;
	public List<bool> allowSpawn = new List<bool>();//used for editor
	public List<int> toSpawn = new List<int>();//used to spawn
	public bool showAllow;
	Controller controller;
	
	void Start ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		StartCoroutine (Pause (Random.Range (minTime, maxTime)));
		for (int s = 0; s<allowSpawn.Count; s++) {
			if(allowSpawn[s])
				toSpawn.Add (s);
		}
	}
	
	IEnumerator Pause (float time)
	{
		yield return new WaitForSeconds(time);
		Spawn ();
	}
	
	void Spawn ()
	{
		if (toSpawn.Count > 0) {
			if (gameObject.transform.GetChildCount () < maxNo) {
				int no = Random.Range (0, toSpawn.Count);
				GameObject item = (GameObject)Object.Instantiate (controller.goodsArray [toSpawn[no]].itemCrate, this.transform.position + (Random.insideUnitSphere * sphereRadius), Random.rotation);
				item.transform.parent = gameObject.transform;
				item.tag = "Item";
				controller.spawned.Add(new Spawned{spawner = this.gameObject, item = item, goodID = toSpawn[no]});
				controller.UpdateAverage (toSpawn [no], 1, 0);
			}
			StartCoroutine (Pause (Random.Range (minTime, maxTime)));
		}
	}
	
	void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (transform.position, sphereRadius);
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
				allowSpawn[productID] = false;
			}
		}
	}
}