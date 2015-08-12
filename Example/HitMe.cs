using UnityEngine;
using System.Collections;

public class HitMe : MonoBehaviour {

	void OnMouseDown ()
	{//When a player clicks on an object, set the focus of the information to be shown
		//The line below would be edited depending on the name etc, or could be used in a different way
		//for example, called when the GameObject has been targetted, so information is shown.
		GameObject.Find ("Player").GetComponent<Player> ().focus = this.gameObject;
	}
}
