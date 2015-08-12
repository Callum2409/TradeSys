using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NoType
{
	public int number;
	public int goodID;
}

public class Trader : MonoBehaviour
{
	public GameObject targetPost;
	internal List<NoType> trading = new List<NoType> ();
	internal bool onCall = false;
	public float stopTime = 2.0f;
	public float cargoSpace = 1;
	internal float spaceRemaining;
	internal bool allowGo = false;
	Controller controller;
	
	public float speedMultiplier = .5f;
	
	void Awake ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();	
		CheckPause();
		cargoSpace = Mathf.Clamp (cargoSpace, 0, Mathf.Infinity);
		spaceRemaining = cargoSpace;
	}
	
	public void NewTrader (GameObject post, float space /*REMOVEME , float stop REMOVE */)
	{//only called if the trader is created in-game
		this.gameObject.tag = "Trader";
		targetPost = post;
		Awake ();
		controller.traders.Add (gameObject);
		cargoSpace = space;
		//REMOVEME stopTime = stop;
		
		//to enable the stopTime to be set, remove the comments with REMOVEME before
	}
	
	void Update ()
	{
		if (allowGo && onCall && targetPost != null) {
			if (onCall) {
				this.transform.Translate (Vector3.forward * speedMultiplier * Time.timeScale);
				this.transform.LookAt (targetPost.transform);
			}
			if (Vector3.Distance (this.transform.position, targetPost.transform.position) <= 0.5) 
				AtLocation ();
		}
	}
	
	IEnumerator Pause ()
	{
		yield return new WaitForSeconds(stopTime);
			allowGo = true;
	}
	
	void CheckPause ()
	{
		if (controller.expendable && !controller.pauseBeforeStart) 
			allowGo = true;
		else
			StartCoroutine (Pause ());
	}

	
	void AtLocation ()
	{
		if (trading.Count > 0) {
			TradePost tP = targetPost.GetComponent<TradePost> ();
			for (int t = 0; t<trading.Count; t++) {
				tP.stock [trading[t].goodID].number += trading [t].number;
				tP.UpdatePrice ();
				controller.ongoing.RemoveAt (t);
			}
			spaceRemaining = cargoSpace;
			trading.Clear ();
			if (controller.expendable)
				Destroy (this.gameObject);
		} else if (trading.Count == 0 && onCall) {
			controller.moving.RemoveAt (controller.moving.FindIndex (x => x.postB == targetPost));
				
		} 
		allowGo = false;
		onCall = false;
		CheckPause();
	}
}