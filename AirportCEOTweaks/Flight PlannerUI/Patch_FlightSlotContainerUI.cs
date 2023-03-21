using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(FlightSlotContainerUI))]
    static class Patch_FlightSlotContainerUI
    {
        [HarmonyPatch("ConfirmFlightPlan")] //Reschedule flight applies to all subsiquent if shift is held
        public static bool Prefix(ref bool rescheduleAborted, ref FlightSlotContainerUI __instance) //PlannerChangesMod
        {
            bool firstSchedule = !__instance.rescheduleInProgress;
            
            if (rescheduleAborted && (AirportCEOTweaksConfig.plannerChanges | AirportCEOTweaksConfig.fixes))
            {
                __instance.rowStandReferenceID = __instance.flight.assignedStandReferenceID;
                return true;
            }
            if (__instance.flight == null || AirportCEOTweaksConfig.plannerChanges == false)
            {
                return true;
            }
            if (((Input.GetKey(KeyCode.LeftShift)) || (Input.GetKey(KeyCode.RightShift)) || (Input.GetKey(AirportCEOTweaksConfig.overloadShiftBind))) || firstSchedule)
            {
                List<CommercialFlightModel> allFlights = __instance.flight.Airline.flightListObjects;
                HashSet<CommercialFlightModel> flightsToChange = new HashSet<CommercialFlightModel>();
                flightsToChange.Add(__instance.flight);

                foreach (CommercialFlightModel f in allFlights)
                {
                    if ( (f.departureFlightNbr == __instance.flight.departureFlightNbr) && ((f.arrivalTimeDT.CompareTo(__instance.flight.arrivalTimeDT) == 1) || firstSchedule ))
                    {
                        flightsToChange.Add(f);
                        
                    }
                    
                }
                int i = 0;
                foreach (CommercialFlightModel f in flightsToChange)
                {
                    if (f != null)
                    {
                        Singleton<ModsController>.Instance.GetExtensions(f, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                        ecfm.TurnaroundPlayerBiasMins = (int)Singleton<ModsController>.Instance.TurnaroundPlayerBiasMins.RoundToNearest(1);
                        ecfm.DetermineTurnaroundTime();

                        StandModel standByReferenceID = Singleton<BuildingController>.Instance.GetObjectByReferenceID<StandModel>(__instance.rowStandReferenceID);
                        DateTime arrivalDT = Statics_FlightSlotContainerUI.ArrivalDTFromContainer(__instance,i);
                        f.turnaroundTime = ecfm.TurnaroundTimeSetRawGetTrue;
                        DateTime departureDT = arrivalDT.Add(f.TurnaroundTime);

                        f.AllocateFlight(arrivalDT, departureDT, standByReferenceID);
                    }
                    i++;
                }
                return !firstSchedule;
            }	

            return true;
        } 

        [HarmonyPatch("CancelFlightPlan")]//Cancel Flight applies to all subsiquent if shift is held
        public static bool Prefix(ref FlightSlotContainerUI __instance) //PlannerChangesMod
        {
            if (__instance.flight == null || AirportCEOTweaksConfig.plannerChanges == false)
            {
                return true;
            }

            if ( (Input.GetKey(KeyCode.LeftShift)) || (Input.GetKey(KeyCode.RightShift)) || (Input.GetKey(AirportCEOTweaksConfig.overloadShiftBind)))
            {
                List<CommercialFlightModel> allFlights = __instance.flight.Airline.flightListObjects;
                HashSet<CommercialFlightModel> flightsToChange = new HashSet<CommercialFlightModel>();
                flightsToChange.Add(__instance.flight);

                foreach (CommercialFlightModel f in allFlights)
                {
                    if ((f.departureFlightNbr == __instance.flight.departureFlightNbr) && (f.arrivalTimeDT.CompareTo(__instance.flight.arrivalTimeDT) == 1))
                    {
                        flightsToChange.Add(f);
                    }
                }
                foreach (CommercialFlightModel f in flightsToChange)
                {
                    if (f != null)
                    {
                        f.CancelFlight(false);
                    }
                }
            }

            return true;
        }

        [HarmonyPatch("IsContainerPositionLegal")]//Changes to legal slot positions
        [HarmonyPrefix]
        public static bool Prefix_IsPositionLegal(ref bool __result, ref FlightSlotContainerUI __instance) //PlannerChangesMod
        {
            if (AirportCEOTweaksConfig.plannerChanges == false) { return true; }

            __result = false; //Flight position is illigal unless exonerated
            //__instance.SetMessageDisplay("Defaulted Illegal?!");
            __instance.ResetMessageDisplay();

            //Emergency
            CommercialFlightModel f = __instance.flight;
            if (f == null || f.isEmergency)
            {
                //__instance.SetMessageDisplay("[something is wrong] being auto-planned?!");
                //__instance.SetMessageDisplay("Debug: Flight is null or Emegency");
                __instance.ResetMessageDisplay();
                return true;
            }

            //Null checks
            StandModel stand;
            FlightScheduleRowContainerUI nearestRow = FlightScheduleDisplayController.Instance.GetNearestSnappingRow(__instance.transform.position);
            if (nearestRow == null || (stand = nearestRow.assignedStand) == null) 
            {
                //__instance.SetMessageDisplay("Debug: Stand is null");
                __instance.ResetMessageDisplay();
                return true; 
            }

            //Internatonal
            if (GameSettingManager.RealisticInternationalStands && !FlightScheduleDisplayController.Instance.IsFlightNationalityCompatible(nearestRow.assignedStand, f, out bool flag))
            {
                __instance.SetMessageDisplay("International?!");

                return false;
            }

            //Weight
            if (((int)f.weightClass) > ((int)stand.objectSize))
            {
                __instance.SetMessageDisplay("Aircraft is too Large for this Stand!");
                return false;
            }

            //TimeofDay
            DateTime cTime = Singleton<TimeController>.Instance.GetCurrentContinuousTime();
            DateTime fTime = Statics_FlightSlotContainerUI.ArrivalDTFromContainer(__instance, 0);
            DateTime weekday = ModsController.NextWeekday(FlightPlannerPanelUI.Instance.selectedWeekday, 0);
            DateTime fDTime = fTime + f.turnaroundTime;

            //The Past
            if (fTime < cTime && !AirportCEOTweaksConfig.permisivePlanner)
            {
                __instance.SetMessageDisplay("Cannot Schedule for the Past!");
                return false;
            }
            //Night Flight w/out Reasearch
            if ((fTime.Hour < AirportController.restrictedTimeEnd.Hours || fDTime.Hour >= AirportController.restrictedTimeStart.Hours) && Singleton<ProgressionManager>.Instance.GetProjectCompletionStatus(Enums.SpecificProjectType.NightFlights) == false && !AirportCEOTweaksConfig.permisivePlanner)
            {
                __instance.SetMessageDisplay("Must Unlock Night Flights!");
                return false;
            }
            //Not Today?
            if (fTime.Day != weekday.Day)
            {
                __instance.SetMessageDisplay("Cannot Schedule For Different Day!");
                return false;
            }
            //Overlap
            if (!AirTrafficController.Instance.CanAllocateFlightSerie(new DateTime[] { fTime }, f.TurnaroundTime, stand.ReferenceID, Singleton<AirportController>.Instance.AirportData.flightSeparatorMinutes))
            {
                __instance.SetMessageDisplay("Too Much Overlap!");
                return false;
            }
            //Arrives too soon
            if (f.GetTimeUntilArrival() < 1 && __instance.rescheduleInProgress && !AirportCEOTweaksConfig.permisivePlanner)
            {
                __instance.SetMessageDisplay("Flight Due Too Soon! (Or In The Past)");
                return false;
            }

            //Onto possibility of changed outcome...

            //If activated or in flight...

            //

            DateTime takeoffTime = FlightModelUtils.TakeoffTime(f, out TimeSpan flightTime, 3, 24);

            if (f.isActivated)
            {
                TimeSpan maxEarly = TimeSpan.FromSeconds(Utils.Clamp((cTime - takeoffTime).TotalSeconds*.1,300,3600));
                TimeSpan maxLate = TimeSpan.FromSeconds(Utils.Clamp(flightTime.TotalSeconds * .1, 1800, 3600));
                TimeSpan diffMins = fTime - f.arrivalTimeDT; //negative when moving forward
             
                if (diffMins < -maxEarly)
                {
                    __instance.SetMessageDisplay("Flight En-Route, Cannot Arrive This Soon!");
                    return false;
                }
                else if (diffMins > maxLate)
                {
                    __instance.SetMessageDisplay("Flight En-Route, Cannot Delay This Long!");
                    return false;
                }
                else
                {
                    __result = true;
                    __instance.ResetMessageDisplay();
                    __instance.transform.Find("FlightTimeDisplay").transform.gameObject.AttemptEnableDisableGameObject(true);
                    return false;
                }
            }
            //Not Activated...
            else
            {
                bool all = !__instance.rescheduleInProgress || (Input.GetKey(KeyCode.LeftShift)) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(AirportCEOTweaksConfig.overloadShiftBind); //are we re-scheduling one or all?
                if (!Singleton<AirTrafficController>.Instance.CanAllocateFlightSerie(f.GetAllOccuringFlightDates(fTime, all), f.TurnaroundTime, stand.ReferenceID, 0))
                {
                    __instance.SetMessageDisplay("Flights Overlap!");
                    return false;
                }
                if (f.weightClass == Enums.ThreeStepScale.Small && stand.hasJetway)
                {
                    __instance.SetMessageDisplay("Aircraft is Too Small For Jetway Access!");
                    return false;
                }
                if (fTime-flightTime-flightTime<cTime && !AirportCEOTweaksConfig.permisivePlanner)
                {
                    __instance.SetMessageDisplay("Cannot Schedule New Flight This Soon!");
                    return false;
                }
                __result = true;
                __instance.transform.Find("FlightTimeDisplay").transform.gameObject.AttemptEnableDisableGameObject(true);
                __instance.ResetMessageDisplay();
                return false;
            }
        }

        [HarmonyPatch("OnDrag")]
        [HarmonyPostfix]
        public static void Postfix_ChangeTurnaroundTime(ref FlightSlotContainerUI __instance)
        {
            try
            {
                Singleton<ModsController>.Instance.GetExtensions(__instance.flight, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                Singleton<ModsController>.Instance.TurnaroundBiasFromBuffer();
                ecfm.TurnaroundPlayerBiasMins = (int)Singleton<ModsController>.Instance.TurnaroundPlayerBiasMins.RoundToNearest(1);
                ecfm.DetermineTurnaroundTime();
                //Debug.LogError("ACEO Tweaks | Info: FlightSlotContainerUI OnDrag Patch!");
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | ERROR: FlightSlotContainerUI OnDrag Patch!");
            }

        }

        [HarmonyPatch("OnBeginDrag")]
        [HarmonyPostfix]
        public static void Postfix_PresetTurnaroundBias(ref FlightSlotContainerUI __instance)
        {
            try 
            {
                Singleton<ModsController>.Instance.GetExtensions(__instance.flight, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                Singleton<ModsController>.Instance.turnaroundPlayerBiasBufferMins = ecfm.TurnaroundPlayerBiasMins;
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | ERROR: FlightSlotContainerUI OnBeginDrag Patch!");
            }
        }

        [HarmonyPatch("SetContainerValues")]
        [HarmonyPostfix]
        public static void PostfixFlightValue(ref FlightSlotContainerUI __instance, ref TextMeshProUGUI ___paymentPerFlight)
        {
            if (!AirportCEOTweaksConfig.flightTypes)
            {
                return;
            }
            
            Singleton<ModsController>.Instance.GetExtensions(__instance.flight, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            
            float val = eam.PaymentPerFlight(ecfm, __instance.flight.Airline.GetPaymentPerFlight(__instance.flight.weightClass)) * ecfm.GetPaymentPercentAndReportRating(false);
            ___paymentPerFlight.text = Utils.GetCurrencyFormat(val, "C0"); 
        }

    }
    public static class Statics_FlightSlotContainerUI
    {
        public static DateTime ArrivalDTFromContainer(FlightSlotContainerUI slot, int offsetDays = 0)
        {
            if (slot == null)
            { return Singleton<TimeController>.Instance.GetCurrentContinuousTime(); }

            DateTime arrivalDT;
            TimeSpan minutes;
            DateTime weekday;
            float rounding = (Input.GetKey(KeyCode.LeftAlt) | Input.GetKey(KeyCode.RightAlt)) ? 5f : 15f;

            minutes = FlightScheduleDisplayController.Instance.GetMinuteValueFromRectTransform((float)((int)Utils.RoundToNearestGivenNumber(slot.GetContainerEdgePosition(Enums.DirectionNonCompass.Left), rounding)));
            weekday = ModsController.NextWeekday(FlightPlannerPanelUI.Instance.selectedWeekday, offsetDays);

            arrivalDT = weekday + minutes;
            return arrivalDT;
        }
    }

}