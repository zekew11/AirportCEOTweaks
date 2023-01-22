using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOTweaks
{
    public class AircraftScaleManager : MonoBehaviour
    {
        public float globalScale = 0.7f; //planes are downscaled
        public bool init = false; //only do Init() once
        public float length;   // x size in small grids
        public float wingspan; // y size in small grids
        public float forcedScale = 0f; //override
        public float scale; // the result of either caluating the right rescale factor or just doing the override
        public bool fixShadow = false; // should I bother doing anything to the shadow
        public void Start() //After Init()
        {
            DoRescale();
            
            //FixShadow(fixShadow); //not ready
        }
        public void Init() //Here we calculate the right rescale factor
        {
            if (init)
            {
                return;
            }

            Vector3 oldScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z); // were going to set the scale to 1,1,1 for calculating things; cache the old scale so we can revert
            Bounds bounds = GetMyBounds(); //Important funtion do have a look
            gameObject.transform.localScale = oldScale; // revert to whatever the scale was before we did calcs


            // This block is for baking in the global recalse of aircraft; safe to mostly ignore

            Vector3 boundsSizeCorrected = bounds.size;
            boundsSizeCorrected.x *= 1f / globalScale;
            boundsSizeCorrected.y *= 1f / globalScale;
            boundsSizeCorrected.z = 1f;


            //here you'll use the x & y size straight from json instead of boundsSizeCorrected

            float scaleFactorL =  length / boundsSizeCorrected.x;
            float scaleFactorW = wingspan / boundsSizeCorrected.y;


            int misMatch = (Math.Abs(scaleFactorW - scaleFactorL) * 100).RoundToIntLikeANormalPerson(); //this tells us if we;ve calculated a differnt scale multiplier in x vs y, which is probably wrong.
            if (misMatch >= 10 && forcedScale == 0f)
            {
                Debug.LogWarning("ACEO Tweaks | Warning: Gameobject " + gameObject.name + " calculated scale factors in length and wingspan mismatch by" + misMatch + "%. This may be due to excessive alpha padding or innacurate wing assignment. Considerusing a forcedReScale.");
            }

            if (forcedScale != 0f) //do we just ignore all that and get the scale out of the json?
            {
                scale = forcedScale;
                Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " forced scale = " + scale);
            }
            else
            {
                scale = scaleFactorL;
                Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " calculated scale = " + scale);
            }

            init = true; //don't go through this again
            Start(); // do the start action even if I'm inactive for some reason
        }
        public Bounds GetMyBounds()
        {
            // This function will tell us how big, in grid units, we are at 1,1,1 scale; this will later let us calculate an x,y,z scale to strech to whatever we need.
            // Bounds is a rectanlge that contains all the pixels of the sprite(s). This includes empty/alpha pixels. Unity engine class, well documented on unity website.

            DoRescale(1f); // Make the scale 1,1,1 so that we don't have to correct for it later

            var renderers = gameObject.GetComponentsInChildren<SpriteRenderer>(); //get all sprites
            if (renderers.Length == 0) return new Bounds(gameObject.transform.position, Vector3.zero); //no sprites?


            var b = renderers[0].bounds; //initial value
            foreach (Renderer r in renderers)
            {
                string parentName = r.gameObject.GetComponentInParent<Transform>().gameObject.name;
                if (parentName.ToLower() == "toggleablesprites" | r.gameObject.name.ToLower() == "shadow")  //for the aircraft, the toggleables might be outside the wing span (cones). We don't want that included in calculated wingspan.
                {
                    continue;
                }
                b.Encapsulate(r.bounds);
            }
            DoRescale(); //DoRescale with no value provided does the rescale according to whatever this class's "scale" parameter is set to.
            return b;
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
        private void FixShadow(bool fix) // Don't actually know if this works yet. If it does its probably a more concise version of all of the above.
        {
            if (!fix)
            {
                return;
            }

            Bounds bounds = GetMyBounds();
            GameObject shadow = gameObject.GetComponentInChildren<ShadowHandler>(true).gameObject;
            
            if (shadow == null)
            {
                return;
            }

            bool active = shadow.active;
            shadow.SetActive(true);
            shadow.transform.position = bounds.center;

            Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " shadow old scale factor = " + shadow.transform.localScale.ToString());

            shadow.transform.localScale.Set(1,1,1);
            Bounds shadowBounds = shadow.GetComponent<SpriteRenderer>().bounds;

            shadow.transform.localScale.Set(bounds.size.x/shadowBounds.size.x,bounds.size.y/shadowBounds.size.y,1);

            Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " shadow requested new scale factor = " + bounds.size.x / shadowBounds.size.x + ", " + bounds.size.y / shadowBounds.size.y);
            Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " shadow new scale factor = " + shadow.transform.localScale.ToString());
            shadow.SetActive(active);
        }
    }
}
