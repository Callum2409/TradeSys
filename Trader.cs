using UnityEngine;
using System.Collections;

public class Trader : MonoBehaviour {
	
	public GameObject targetPost;
	
	internal int quantity = -1;
	internal string type;
	internal bool onCall = false;
	public float stopTime = 2.0f;
	bool allowGo = false;
	
	public float speedMultiplier = .5f;
	
	Controller controller;
	
	void Start ()
	{
		controller = GameObject.Find ("Controller").GetComponent<Controller> ();
		controller.TradeCall ();
		PauseGo ();
	}
	
	public void NewTrader (GameObject post)
	{//only called if the trader is created in-game
		this.gameObject.tag = "Trader";
		targetPost = post;
		Start ();
		controller.traders.Add (gameObject);
		controller.TradeCall ();
		//optional set the stopTime
	}
	
	void Update ()
	{
		this.transform.LookAt (targetPost.transform);
		if (targetPost != null && onCall && allowGo)
			this.transform.Translate (Vector3.forward * speedMultiplier);
		
		if (Vector3.Distance (this.transform.position, targetPost.transform.position) <= 0.5 && allowGo) 
			AtLocation ();
	}
	
	IEnumerator Pause () {
		yield return new WaitForSeconds(stopTime);
		allowGo = true;
	}
	
	public void PauseGo () {
		StartCoroutine (Pause ());
	}
	
	void AtLocation () {
			if (quantity > 0) {
				TradePost tP = targetPost.GetComponent<TradePost> ();
				tP.stock [controller.goods.FindIndex (x => x.name == type)].number += quantity;
				tP.UpdatePrice();
				controller.ongoing.RemoveAt (controller.ongoing.FindIndex (x => x.buyPost == targetPost && x.type == type));
			} else if (quantity == 0) {
				controller.moving.RemoveAt (controller.moving.FindIndex (x => x.postB == targetPost));
				
			} 
			quantity = -1;
			allowGo = false;
			onCall = false;
			controller.TradeCall ();
	}
}