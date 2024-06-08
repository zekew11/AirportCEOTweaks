using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using HarmonyLib;


namespace AirportCEOAircraft.AddNewAircraft.Patches
{
    [HarmonyPatch(typeof(AircraftEffectsManager))]
    public static class Patch_AircraftEffectsManager
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPrefix]
        public static bool DontUseArraysKids (ref AircraftEffectsManager __instance, 
											  Func<Transform,Transform> effectPositionAdjustmentCallback,
											  Enums.ThreeStepScale size, ref int ___engineCount, ref int ___wheelCount,
											  ref Func<Transform, Transform> ___effectPositionAdjustmentCallback,
											  ref CustomParticleEffect[] ___engineExhaustEffects,
											  ref CustomParticleEffect[] ___wheelEffects,
											  ref CustomParticleEffect[] ___engineHeatHazeEffects
											 )
        {
            ___engineCount = 0;
            ___wheelCount = 0;

			___effectPositionAdjustmentCallback = effectPositionAdjustmentCallback;

			List<CustomParticleEffect> engineEffectsList = new List<CustomParticleEffect>();
			List<CustomParticleEffect> wheelEffectsList = new List<CustomParticleEffect>();
			List<CustomParticleEffect> heatHazeEffectsList = new List<CustomParticleEffect>();

			for (int i = 0; i < __instance.transform.childCount; i++)
			{
				if (__instance.transform.GetChild(i).name.ToLower().Contains("engine"))
				{
					Transform childEngine = __instance.transform.GetChild(i);
					CustomParticleEffect workingEffect = default(CustomParticleEffect);
					workingEffect.Set(childEngine.GetComponent<ParticleSystem>(), childEngine);
					workingEffect.particleSystem.TogglePrewarm(true);
					workingEffect.mainModule.startLifetime = 1.25f;
					___engineCount++;
					engineEffectsList.Add(workingEffect);
				}
				else if (__instance.transform.GetChild(i).name.ToLower().Contains("touchdown"))
				{
					Transform childWheel = __instance.transform.GetChild(i);
					CustomParticleEffect workingEffect = default(CustomParticleEffect);
					workingEffect.Set(childWheel.GetComponent<ParticleSystem>(), childWheel);
					workingEffect.particleSystem.TogglePrewarm(true);
					workingEffect.mainModule.startLifetime = 0.5f;
					___wheelCount++;
					wheelEffectsList.Add(workingEffect);
				}
			}
			___engineExhaustEffects = engineEffectsList.ToArray();
			___wheelEffects = wheelEffectsList.ToArray();

			if (__instance.spawnHeatHaze && Utils.GetOperatingSystem() == Enums.OperatingSystem.Windows)
			{
				for (int j = 0; j < ___engineCount; j++)
				{
					ParticleSystem component = UnityEngine.Object.Instantiate<GameObject>(SingletonNonDestroy<DataPlaceholder>.Instance.heatHazeEffect, __instance.transform).GetComponent<ParticleSystem>();
					CustomParticleEffect workingEffect = default(CustomParticleEffect);
					workingEffect.Set(component, component.transform);
					workingEffect.transform.position = ___engineExhaustEffects[j].transform.position;
					workingEffect.mainModule.startSpeed = (float)((size == Enums.ThreeStepScale.Large) ? 13 : 10);
					workingEffect.mainModule.startSize = ((size == Enums.ThreeStepScale.Large) ? 1.5f : 1f);
					heatHazeEffectsList.Add(workingEffect);
				}

				___engineHeatHazeEffects = heatHazeEffectsList.ToArray();
			}
			return false;
		}
	}
}
