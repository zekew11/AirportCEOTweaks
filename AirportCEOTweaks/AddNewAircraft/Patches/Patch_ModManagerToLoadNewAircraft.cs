using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UModFramework.API;

namespace AirportCEOTweaks
{
	[HarmonyPatch(typeof(ModManager))]
	static class Patch_ModManagerToLoadNewAircraft
	{
		[HarmonyPostfix]
		[HarmonyPatch("QueueMods")]
		public static void Patch_AddPrefabs(string path)
		{
			//Debug.Log("ACEO Tweaks | Log : postfic on quemods running...");
			string[] directories = Directory.GetDirectories(path);
			for (int i = 0; i < directories.Length; i++)
			{
				if (directories[i].SafeSubstring(directories[i].Length - 8, 8).Equals("Aircraft"))
				{
					AirportCEOTweaks.aircraftPaths.Add(path);
					Debug.Log("AirportCEOTweaks v" + UMFMod.GetModVersion().ToString());
					Debug.Log("ACEO Tweaks | Log : added path "+path+" to List<> aircraftPaths");
				}
			}
		}
	}
}
