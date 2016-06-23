using UnityEngine;
using System.Collections;
using Devdog.InventorySystem;
using System.Linq;

namespace CallumP.TradeSys
{
    public class TS_InvPro : MonoBehaviour
    {
        public bool test;//used to call on validate easily
        public bool pushPull;//bool to push or pull the data from inventory pro
        public InventoryItemDatabase database;//the database of the items to push or pull to

        Controller controller;
    }//end class
}//end namespace