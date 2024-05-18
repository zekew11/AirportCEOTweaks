using UnityEngine;
using HarmonyLib;
using System;
using System.Reflection;

namespace Tweaks_PerformanceCEO.RAMReducer
{

    public static class Tweaks_RAMReducerManager
    {
        public static bool TweaksAircraftCall = false;
    }


    [HarmonyPatch(typeof(Sprite), new Type[] { typeof(Texture2D), typeof(Rect), typeof(Vector2), typeof(float), typeof(uint), typeof(SpriteMeshType)})]
    [HarmonyPatch("Create")]
    static class Patch_RAMReducerApplier
    {
        public static void Prefix(ref Texture2D texture)
        {
            if ( Tweaks_RAMReducerManager.TweaksAircraftCall == false || !texture.isReadable )
            {
                return;
            }

            try
            {
                // This does prevent editing of the sprite later on, shouldn't be an issue
                texture.Apply(false, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ACEO Tweaks | ERROR: Error occured while reducing RAM usage (Patch_RAMReducerApplier). Error: {ex.Message}");
            }
        }
    }  
}