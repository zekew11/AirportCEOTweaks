using System;
using System.Collections;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace AirportCEOAircraft
{
	[HarmonyPatch(typeof(GameController))]
	static class Patch_GameControllerToAddNewPrefabs
	{
		private static AircraftAdder adder;
		private static int iteration = 0;
		private static IEnumerator item;

		[HarmonyPostfix]
		[HarmonyPatch("Awake")]
		public static void Patch_AddPrefabs(GameController __instance)
		{
			adder = Singleton<AirTrafficController>.Instance.gameObject.AddComponent<AircraftAdder>();

			Debug.Log("ACEO Tweaks | Log : added aircraft adder");
		}


		[HarmonyPostfix]
		[HarmonyPatch("LaunchGame")]
		//static void Patch_InitializeGameSession_ToAddAircraftFirst(ref GameController __instance, ref IEnumerator __result)
        //{
		//	//if (adder.working && __result == Singleton<TemplateController>.Instance.LoadQueuedObjects())
		//	if (adder.working)
		//	{
		//		__result = adder.packagedEnumerator(__result);
		//		//IEnumerator _coroutine = adder.Initilize();
		//		//while (adder.working)
        //        //{
		//		//	_coroutine.MoveNext();
        //        //}
		//		////adder.StartCoroutine(adder.Initilize());
		//		//return true;
		//		////return false;
		//	}
		//	//else
        //    //{
		//	//	return true;
        //    //}
        //}
		static void Postfix(ref IEnumerator __result)
		{
			Action prefixAction = () =>  { Debug.Log("--> before LaunchGame"); };
			Action postfixAction = () => { Debug.Log("--> after LaunchGame"); };
			Action<object> preItemAction = (item) =>  { ; };
			Action<object> postItemAction = (item) => { iteration++; };
			Func<object, object> itemAction = (item) =>
			{
				//var newItem = item + "+";
				//Console.WriteLine($"--> item {item} => {newItem}");
				//return newItem;
				if (iteration == 0)
				{
					return adder.packagedEnumerator(item);
				}
				else
                {
					return item;
                }
			};
			var myEnumerator = new SimpleEnumerator()
			{
				enumerator = __result,
				prefixAction = prefixAction,
				postfixAction = postfixAction,
				preItemAction = preItemAction,
				postItemAction = postItemAction,
				itemAction = itemAction
			};
			__result = myEnumerator.GetEnumerator();
		}


		class SimpleEnumerator : IEnumerable
		{
			public IEnumerator enumerator;
			public Action prefixAction, postfixAction;
			public Action<object> preItemAction, postItemAction;
			public Func<object, object> itemAction;
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
			public IEnumerator GetEnumerator()
			{
				prefixAction();
				while (enumerator.MoveNext())
				{
					var currentItem = enumerator.Current;
					preItemAction(currentItem);
					yield return itemAction(currentItem);
					postItemAction(currentItem);
				}
				postfixAction();
			}

		}
	}
}