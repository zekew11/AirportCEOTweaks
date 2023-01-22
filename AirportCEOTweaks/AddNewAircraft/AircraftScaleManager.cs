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
        public float globalScale = 0.7f;
        public bool init = false;
        public float length;
        public float wingspan;
        public float forcedScale = 0f;
        public float scale;
        public bool fixShadow = false;
        public void Start()
        {
            DoRescale();
            //RescaleLivery();
            //FixShadow(fixShadow);
        }
        public void LateUpdate()
        {
            //DoRescale(false);
            //FixShadow(fixShadow);
        }
        public void Init()
        {
            if (init)
            {
                return;
            }

            Vector3 oldScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            Bounds bounds = GetMyBounds(); //normalizes to x1 scale for you
            gameObject.transform.localScale = oldScale;

            Vector3 boundsSizeCorrected = bounds.size;
            boundsSizeCorrected.x *= 1f / globalScale;
            boundsSizeCorrected.y *= 1f / globalScale;
            boundsSizeCorrected.z = 1f;

            float scaleFactorL =  length / boundsSizeCorrected.x;
            float scaleFactorW = wingspan / boundsSizeCorrected.y;

            int misMatch = (Math.Abs(scaleFactorW - scaleFactorL) * 100).RoundToIntLikeANormalPerson();
            if (misMatch >= 10 && forcedScale == 0f)
            {
                Debug.LogWarning("ACEO Tweaks | Warning: Gameobject " + gameObject.name + " calculated scale factors in length and wingspan mismatch by" + misMatch + "%. This may be due to excessive alpha padding or innacurate wing assignment. Considerusing a forcedReScale.");
            }

            if (forcedScale != 0f)
            {
                scale = forcedScale;
                Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " forced scale = " + scale);
            }
            else
            {
                scale = scaleFactorL;
                Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " calculated scale = " + scale);
            }
            init = true;
            Start();
        }
        public Bounds GetMyBounds()
        {
            DoRescale(1f);
            var renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0) return new Bounds(gameObject.transform.position, Vector3.zero);

            var b = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                string parentName = r.gameObject.GetComponentInParent<Transform>().gameObject.name;
                if (parentName.ToLower() == "toggleablesprites" | r.gameObject.name.ToLower() == "shadow")
                {
                    continue;
                }
                b.Encapsulate(r.bounds);
            }
            DoRescale();
            return b;
        }
        public void DoRescale(float scale = 0f, bool logs = false)
        {
            if (scale <= .01f)
            {
                scale = this.scale;
            }
            if (logs)
            {
                Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " setting scale factor = " + scale);
            }

            gameObject.transform.localScale = new Vector3(scale,scale,1f);

            if (logs)
            {
                Debug.Log("ACEO Tweaks | Log: Gameobject " + gameObject.name + " set scale factor = " + gameObject.transform.localScale.ToString());
            }
        }
        private void FixShadow(bool fix)
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
