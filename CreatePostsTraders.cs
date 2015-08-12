using UnityEngine;
using System.Collections;

namespace TradeSys {//use namespace to stop any name conflicts
	public class CreatePostsTraders : MonoBehaviour {
	//This is an example script demonstrating how trade posts and traders can be created via code. It is fully commented, so should hopefully help
	//with editing it in order to get it to suit your needs
	
		public int seed;//set seed so is the same each time scene is generated, so can perform fair tests
		public int numberOfPosts;//set the number of posts
		public float sphereRadius;//set how far away each post can be
		public int numberOfTraders;//the number of traders
		public int min = 0, max = 30;//the min and max number of items
	
		Controller controller;//the controller
	
		// Use this for initialization
		void Start () {
			controller = GameObject.FindGameObjectWithTag (Tags.C).GetComponent<Controller> ();
			Random.seed = seed;//set the seed
			#region posts
			for (int n = 0; n<numberOfPosts; n++) {//add new posts, setting them up   
				TradePost newPost = GameObject.CreatePrimitive (PrimitiveType.Sphere).AddComponent<TradePost> ();//create the sphere
				newPost.tag = Tags.TP;//set the tag so the controller can see it
				newPost.transform.position = Random.insideUnitSphere * sphereRadius;//set the position
				newPost.transform.parent = GameObject.Find ("Posts").transform;//set the parent, so doesnt fill hierarchy
				newPost.name = "Trade Post " + (n + 1);//set the name so is easier to find any later
				controller.SortTradePost(newPost);//sort the new post information
			
				for (int g = 0; g<controller.goods.Count; g++) {//go through all groups
					for (int s = 0; s<controller.goods[g].goods.Count; s++)//go through all goods
						newPost.stock [g].stock [s].number = Random.Range (min, max);//set the number of items. dont need to add because is sorted in the trade post script
				}//end for groups
			
				for (int m = 0; m<controller.manufacture.Count; m++) {//go through all manufacture groups
					for (int p = 0; p<controller.manufacture[m].manufacture.Count; p++) {//go through all processes
						int random = Random.Range (-50, 20);//create a random number which can be negative so may not be enabled
						if (random > 0) {//if greater than 0, enable this manufacture process
							newPost.manufacture [m].manufacture [p].enabled = true;
							newPost.manufacture [m].manufacture [p].create = random;//set the create time to the random time just generated
							newPost.manufacture [m].manufacture [p].cooldown = Random.Range (0, 30);//generate another random time for cooldown
						}//end if > 0
					}//end for processes
				}//end for manufacture groups
			
				if (controller.groups.enabled) {//if groups enabled, need to generate the groups
					for (int g = 0; g<controller.groups.names.Count; g++) {//go through all groups
						newPost.groups [g] = System.Math.Round (Random.value, System.MidpointRounding.AwayFromZero) == 0 ? true : false;
						//generate a random value between 0 and 1, round it and use that to decide whether or not a group is selected
					}//end for groups			
				}//end if groups enabled
				if (controller.factions.enabled) {//if factions enabled, need to generate the factions
					for (int f = 0; f<controller.factions.factions.Count; f++) {//go through all factions
						newPost.factions[f] = System.Math.Round (Random.value, System.MidpointRounding.AwayFromZero) == 0 ? true : false;
						//generate a random value between 0 and 1, round it and use that to decide whether or not a faction is selected
					}//end for groups			
				}//end if groups enabled
			}//end for new posts
			#endregion
			controller.GetPostScripts ();
			#region traders
			for (int n = 0; n<numberOfTraders; n++) {//add new traders, setting them up
				Trader newTrader = GameObject.CreatePrimitive (PrimitiveType.Cube).AddComponent<Trader> ();//create the cube
				newTrader.gameObject.AddComponent<TSTraderAI>();//add the trader AI
				newTrader.tag = Tags.T;//set the tag so the controller can see it
				int random = Random.Range (0, controller.tradePosts.Length);//select the starting trade post from all of the trade posts
				GameObject targetPost = controller.tradePosts [random];//set the start post
				newTrader.transform.position = targetPost.transform.position;//set the position to the start trade post
				newTrader.target = targetPost;//set the target of the trader to the starting trade post
				newTrader.transform.parent = GameObject.Find ("Traders").transform;//set the parent, so doesnt fill hierarchy
				newTrader.name = "Trader " + (n + 1);//set the name so is easier to find any later
			
				newTrader.closeDistance = 0.3f;//set the close distance
				newTrader.transform.localScale = new Vector3 (0.25f, 0.25f, 0.25f);//scale the cubes so they are smaller than the trade post spheres
			
				if (controller.factions.enabled) {//if factions enabled, need to generate the factions
					for (int f = 0; f<controller.factions.factions.Count; f++) {//go through all factions
						newTrader.factions[f] = System.Math.Round (Random.value, System.MidpointRounding.AwayFromZero) == 0 ? true : false;
						//generate a random value between 0 and 1, round it and use that to decide whether or not a faction is selected
						//NOTE: This may cause TradeSys to tell you that the trader cannot do anything because the faction of the starting post is not the same
						//so some code would need to be added to check that the factions are similar, and if not then either select a similar faction or a new 
						//start post
					}//end for groups			
				}//end if groups enabled
			}//end for new traders
			#endregion

			controller.GenerateDistances ();//added all the posts and traders, now needs to get distances
			//without the line above, TradeSys will not do anything!
			
		}//end Start
	}//end CreatePostsTraders
}//end namespace