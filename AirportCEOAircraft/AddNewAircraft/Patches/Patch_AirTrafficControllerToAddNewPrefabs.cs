using System;
using System.Collections;
using UnityEngine;
using HarmonyLib;

namespace AirportCEOAircraft
{
	[HarmonyPatch(typeof(SaveLoadGameDataController))]
	static class Patch_SaveLoadGameDataControllerToAddNewPrefabs
	{
		private static AircraftAdder adder;

		[HarmonyPostfix]
		[HarmonyPatch("Awake")]
		public static void Patch_AddPrefabs(SaveLoadGameDataController __instance)
		{
			adder = Singleton<AirTrafficController>.Instance.gameObject.AddComponent<AircraftAdder>();
			//adder.Initilize();
			Debug.Log("ACEO Tweaks | Log : added aircraft adder");
		}

		//[HarmonyPatch(typeof(SaveLoadGameDataController), "LoadGameDataCoroutine")]
		//static void Postfix(ref IEnumerator __result)
		//{
		//	Action prefixAction = adder.Initilize;
		//
		//	var myEnumerator = new NewLoadGameDataCoroutine()
		//	{
		//		prefixAction = prefixAction
		//	};
		//
		//	__result = myEnumerator.GetEnumerator();
		//}
		[HarmonyPrefix]
		[HarmonyPatch("LoadGameData")]
		[HarmonyPatch(new Type[] {typeof(bool)})]
		public static bool Patch_LoadGameData_ToAddAircraftFirst(bool isMod)
        {
			if (adder.working)
			{
				adder.StartCoroutine(adder.Initilize(isMod));
				return false;
			}
			else
            {
				return true;
            }
        }

		class NewLoadGameDataCoroutine : IEnumerable
		{
			public IEnumerator enumerator;
			public Action prefixAction;//, postfixAction;
			//public Action<object> preItemAction, postItemAction;
			//public Func<object, object> itemAction;
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
			public IEnumerator GetEnumerator()
			{
				prefixAction();
				while (enumerator.MoveNext())
				{
					var item = enumerator.Current;
					//preItemAction(item);
					yield return item;
					//postItemAction(item);
				}
				//postfixAction();
			}
		}
	}
}