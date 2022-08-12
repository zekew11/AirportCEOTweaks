using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft;




namespace AirportCEOTweaks
{
    public class NightLightHide : MonoBehaviour
    {
        public GameObject sprite;
        public GameObject nightWindows;



        void Start()
        {
            GameObject gameObject;
            int children=transform.childCount;


            for (int i=0;  i<children; i++)
            {
                
                try
                {
                    gameObject = transform.GetChild(i).gameObject;
                    if (gameObject.name == "Sprite")
                        {
                        sprite = gameObject;
                        break;
                        }
                }
                catch
                {

                }
            }
            children = sprite.transform.childCount;
            for (int i = 0; i < children; i++)
            {

                try
                {
                    gameObject = sprite.transform.GetChild(i).gameObject;
                    if (gameObject.name == "NightWindows")
                    {
                        nightWindows = gameObject;
                        break;
                    }
                }
                catch
                {

                }
            }
            children = nightWindows.transform.childCount;
            for (int i = 0; i < children; i++)
            {

                try
                {
                    gameObject = nightWindows.transform.GetChild(i).gameObject;
                    if (Math.Abs(gameObject.transform.localPosition.y) > 0.05f)
                    {
                        gameObject.AttemptEnableDisableGameObject(false);
                    }
                }
                catch
                {

                }
            }

            Destroy(this);

        }



    }
}
