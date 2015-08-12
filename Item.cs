using UnityEngine;
using System.Collections;

namespace TradeSys
{//use namespace to stop any name conflicts
	[AddComponentMenu("TradeSys/Make Item")]
	//add to component menu
		public class Item : MonoBehaviour
		{
				public int groupID, itemID, number;//details of item + number of item in crate
				public bool traderCollect;//whether a trader can collect the item or not
				public bool spawned = false;//whether this item was made at a spawner or not
				
		void Awake()
		{
			GameObject.FindGameObjectWithTag(Tags.C).GetComponent<Controller>().UpdateAverage(groupID, itemID, number, 0);//add this item to the controller info
			tag = Tags.I;//make sure the tag has been set
		}//end Awake
		
		public void Collected()
		{//used to tell the spawner to decrease the count
		if(spawned)//only tell the spawner to update the count if it was made at a spawner
			transform.parent.GetComponent<Spawner>().ChangeCount(number, false);//tell the spawner that this item has been collected
			
			Destroy(gameObject);
		}//end Collected
		}//end class TSItem
}//end namespace