using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOAircraft
{
    public class AircraftScaleManager : MonoBehaviour
    {
        //public const float inBuiltScale = 0.75f;
        //public float globalScale = 0.75f; //planes are downscaled
        public bool init = false; //only do Init() once
        public float length;   // x size in small grid meters
        public float wingspan; // y size in small grid meters
        public float forcedScale = 0f; 
        public float scale = 1f; // the result of either caluating the right rescale factor or just doing the override
        public bool rescalingEnabled = true; //should any rescale happen ever at all
        public void Start() //Occurs After Init()
        {
            if (rescalingEnabled == false)
            {
                Destroy(this);
            }
            DoRescale();
        }
        public void Init() //Here we calculate the right rescale factor
        {
            if (rescalingEnabled == false)
            {
                Destroy(this);
            }
            if (init)
            {
                return;
            }

            if (forcedScale != 0f) //is scale forced?
            {
                scale = forcedScale;
            }
            else
            {
                scale = 1f;
                rescalingEnabled = false;
            }

            init = true; //don't go through this again
        }
        public void DoRescale(float scale = 0f, bool logs = false)
        {
            if (scale <= .01f) //I don't ever actually want to scale to 0.
            {
                scale = this.scale;
            }
            if (logs)
            {
                Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " setting scale factor = " + scale);
            }

            gameObject.transform.localScale = new Vector3(scale,scale,1f); //using a new Vector3 seems much more reliable than pointing to anything else (eg localScale.Set() doesn't work)

            if (logs)
            {
                Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " set scale factor = " + gameObject.transform.localScale.ToString());
            }
        }
    }
}
