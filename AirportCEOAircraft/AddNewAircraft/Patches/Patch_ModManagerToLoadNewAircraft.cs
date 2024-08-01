using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AirportCEOAircraft
{
	[HarmonyPatch(typeof(ModManager))]
	static class Patch_ModManagerToLoadNewAircraft
	{
		[HarmonyPostfix]
		[HarmonyPatch("QueueMods")]
		public static void Patch_AddPrefabs(string path)
		{
			//Debug.Log("ACEO Tweaks | Log : postfic on quemods running...");
			List<string> directories = Directory.GetDirectories(path).ToList();
			List<string> moreDirectories = new List<string>();

			for (int i = 0; i < 3; i++) //search to a given depth
			{
				foreach (string directory in directories)
				{
					moreDirectories.AddRange(Directory.GetDirectories(directory));
				}
				directories = directories.Union(moreDirectories).ToList();
			}

			for (int i = 0; i < directories.Count; i++)
			{
				if (directories[i].SafeSubstring(directories[i].Length - 8, 8).Equals("Aircraft"))
				{
					if (AirportCEOAircraft.aircraftPaths.AddIfNotContains(path))
					{
						Debug.Log("ACEO Tweaks | Log : added path " + path + " to List<> aircraftPaths");
					}
				}
			}
		}
	}
}
