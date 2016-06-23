using UnityEngine;
using UnityEditor;
using System.Collections;
using Devdog.InventorySystem;
using Devdog.InventorySystem.Models;
using System.Linq;

namespace CallumP.TradeSys
{
    [CustomEditor(typeof(TS_InvPro))]
    public class TS_InvProEditor : Editor
    {
        Controller controller;
        InventoryItemDatabase database;

        void OnEnable()
        {
            controller = GameObject.FindGameObjectWithTag(Tags.C).GetComponent<Controller>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("database"));
            database = (InventoryItemDatabase)serializedObject.FindProperty("database").objectReferenceValue;

            if (database != null && GUILayout.Button("Push")) { }
            if (database != null && GUILayout.Button("Pull")) PullData();

            serializedObject.ApplyModifiedProperties();
        }//end OnInspectorGUI

        void PullData()
        {
            //need to get currencies first
            controller.currencies.Clear();//clear and then add
            controller.currencyExchange.Clear();

            for(int c = 0; c<database.currencies.Length; c++)
            {
                InventoryCurrency currency = database.currencies[c];
                controller.currencies.Add(new Currency { decimals = 0, formatString = currency.stringFormat.Replace("{3}", "{1}"), plural = currency.pluralName, single = currency.singleName });//add the currency

                foreach (InventoryCurrencyConversionLookup exchange in currency.currencyConversions)
                {
                    if (!ExchangeExists(c, System.Array.FindIndex(database.currencies, x => x.ID == exchange.currencyID), exchange.factor))
                    { //if exchange cna be reversed, is reversed
                        //if not, add
                        controller.currencyExchange.Add(new CurrencyExchange { IDA = c, IDB = System.Array.FindIndex(database.currencies, x => x.ID == exchange.currencyID), numberA = 1, numberB = exchange.factor, multiplier = exchange.factor });
                    }//end if not already added
                }//end foreach exchange
            }//end for all currencies

            while (controller.goods.Count < database.itemCategories.Length)//while not enough
                controller.goods.Add(new GoodsTypes());
            while (controller.goods.Count > database.itemCategories.Length)//while too many
                controller.goods.RemoveAt(controller.goods.Count - 1);

            for (int g = 0; g < controller.goods.Count; g++)
            {//go through
                controller.goods[g].name = database.itemCategories[g].name;//set the name
                controller.goods[g].goods.Clear();//clear all as dont know where item would be in the list
            }

            foreach (InventoryItemBase item in database.items)
            {
                //add each item in the correct group
                controller.goods[(int)item._category].goods.Add(new Goods { name = item.name, minPrice = item.sellPrice.amount, maxPrice = item.buyPrice.amount, currencyID = (int)item.buyPrice.currency.ID, mass = item.weight });//add the item
            }
            TSGUI gui = new TSGUI();
            gui.GetNames(controller);

            while (controller.manufacture.Count < database.craftingCategories.Length)//while not enough
                controller.manufacture.Add(new MnfctrTypes());
            while (controller.manufacture.Count > database.craftingCategories.Length)//while too many
                controller.manufacture.RemoveAt(controller.manufacture.Count - 1);

            for (int m = 0; m < controller.manufacture.Count; m++)
            {
                controller.manufacture[m].name = database.craftingCategoriesStrings[m];
                controller.manufacture[m].manufacture.Clear();

                foreach (InventoryCraftingBlueprint process in database.craftingCategories[m].blueprints)
                {
                    Mnfctr toAdd = new Mnfctr() { name = process.name };
                    foreach (InventoryItemAmountRow req in process.requiredItems)
                    {
                        int g = (int)req.item.category.ID;
                        toAdd.needing.Add(new NeedMake() { groupID = g, itemID = controller.goods[g].goods.FindIndex(i => i.name == req.item.name), number = (int)req.amount });
                    }

                    foreach (InventoryItemAmountRow res in process.resultItems)
                    {
                        int g = (int)res.item.category.ID;
                        toAdd.making.Add(new NeedMake() { groupID = g, itemID = controller.goods[g].goods.FindIndex(i => i.name == res.item.name), number = (int)res.amount });
                    }

                    controller.manufacture[m].manufacture.Add(toAdd);
                }//end foreach process
            }//end for all manufacture groups


            gui.ManufactureInfo(controller);
        }//end PullData

        bool ExchangeExists(int IDB, int IDA, float factor)
        {
            foreach (CurrencyExchange exchange in controller.currencyExchange)
            {
                Debug.Log(exchange.IDB + " " + IDB + "\n" + exchange.IDA + " " + IDA + "\n" + exchange.multiplier + " " + 1 / factor);
                if (exchange.IDB == IDB && exchange.IDA == IDA && exchange.multiplier.Equals(1 / factor))
                {
                    Debug.Log("YAA");
                    exchange.reverse = true;//this is just the reverse so allow it
                    return true;
                }//end if
            }//end foreach
            return false;
        }//end ExchangeExists
    }//end class
}//end namespace